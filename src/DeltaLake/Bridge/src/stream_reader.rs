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

/// A DataFusion PartitionStream backed by an FFI ArrowArrayStreamReader
#[derive(Debug)]
struct FfiPartitionStream {
    schema: SchemaRef,
    reader: Mutex<Option<ArrowArrayStreamReader>>,
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
            let Some(reader) = reader else {
                return internal_err!("FFI stream was already consumed");
            };
            for batch in reader {
                if tx.blocking_send(batch.map_err(Into::into)).is_err() {
                    break; // receiver dropped
                }
            }
            Ok(())
        });
        builder.build()
    }
}

pub(crate) fn record_batch_stream_plan(
    runtime: &mut Runtime,
    reader: ArrowArrayStreamReader,
) -> Result<LogicalPlan, DeltaTableError> {
    let schema = reader.schema().clone();
    let table = StreamingTable::try_new(
        schema.clone(),
        vec![Arc::new(FfiPartitionStream {
            schema,
            reader: Mutex::new(Some(reader)),
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
    .map_err(|err| DeltaTableError::new(runtime, DeltaTableErrorCode::DataFusion, &err.to_string()))
}
