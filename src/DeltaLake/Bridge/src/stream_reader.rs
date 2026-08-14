use crate::error::{DeltaTableError, DeltaTableErrorCode};
use crate::runtime::Runtime;
use arrow::datatypes::SchemaRef;
use arrow::ffi_stream::ArrowArrayStreamReader;
use arrow::record_batch::RecordBatchReader;
use deltalake::datafusion::catalog::streaming::StreamingTable;
use deltalake::datafusion::common::internal_err;
use deltalake::datafusion::datasource::provider_as_source;
use deltalake::datafusion::execution::{SendableRecordBatchStream, TaskContext};
use deltalake::datafusion::logical_expr::{LogicalPlan, LogicalPlanBuilder};
use deltalake::datafusion::physical_plan::stream::RecordBatchReceiverStreamBuilder;
use deltalake::datafusion::physical_plan::streaming::PartitionStream;
use std::sync::{Arc, Mutex};
use tokio::sync::oneshot;

/// A DataFusion PartitionStream backed by an FFI ArrowArrayStreamReader
#[derive(Debug)]
struct FfiPartitionStream {
    schema: SchemaRef,
    /// The reader, paired with a sender that is dropped along with it to signal
    /// that the FFI stream has been released.
    reader: Mutex<Option<(ArrowArrayStreamReader, oneshot::Sender<()>)>>,
}

impl PartitionStream for FfiPartitionStream {
    fn schema(&self) -> &SchemaRef {
        &self.schema
    }

    fn execute(&self, _ctx: Arc<TaskContext>) -> SendableRecordBatchStream {
        // The reader can only be consumed once, so take it out of the option:
        let reader = self.reader.lock().unwrap().take();
        let mut builder = RecordBatchReceiverStreamBuilder::new(self.schema.clone(), 2);
        let tx = builder.tx();
        builder.spawn_blocking(move || {
            // Take the reader and oneshot sender together. The sender will be dropped once the
            // reader is also dropped, signalling that it's OK to dispose the FFI stream.
            let Some((reader, _released_tx)) = reader else {
                return internal_err!("FFI stream was already consumed");
            };
            for batch in reader {
                let failed = batch.is_err();
                if tx.blocking_send(batch.map_err(Into::into)).is_err() {
                    break; // receiver dropped
                }
                if failed {
                    // Don't continue reading after receiving an error
                    break;
                }
            }
            Ok(())
        });
        builder.build()
    }
}

/// Builds a plan that reads from the FFI stream, along with a receiver that resolves
/// once the reader has been released and the FFI stream is safe to dispose.
pub(crate) fn record_batch_stream_plan(
    runtime: &mut Runtime,
    reader: ArrowArrayStreamReader,
) -> Result<(LogicalPlan, oneshot::Receiver<()>), DeltaTableError> {
    let schema = reader.schema().clone();
    let (released_tx, released_rx) = oneshot::channel();
    let table = StreamingTable::try_new(
        schema.clone(),
        vec![Arc::new(FfiPartitionStream {
            schema,
            reader: Mutex::new(Some((reader, released_tx))),
        })],
    )
    .map_err(|err| {
        DeltaTableError::new(runtime, DeltaTableErrorCode::DataFusion, &err.to_string())
    })?;

    LogicalPlanBuilder::scan(
        "record_batch_stream",
        provider_as_source(Arc::new(table)),
        None,
    )
    .and_then(|plan| plan.build())
    .map(|plan| (plan, released_rx))
    .map_err(|err| DeltaTableError::new(runtime, DeltaTableErrorCode::DataFusion, &err.to_string()))
}
