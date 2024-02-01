using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Apache.Arrow;
using Apache.Arrow.Ipc;

namespace DeltaLake.Bridge
{
    internal class RecordBatchReader : IArrowArrayStream
    {
        private readonly IEnumerable<RecordBatch> _recordBatches;
        private readonly IEnumerator<RecordBatch> _enumerator;

        public RecordBatchReader(IEnumerable<RecordBatch> recordBatches, Schema schema)
        {
            _recordBatches = recordBatches;
            _enumerator = _recordBatches.GetEnumerator();
            Schema = schema;
        }

        public Schema Schema { get; }

        public void Dispose()
        {
            _enumerator.Dispose();
        }

        public ValueTask<RecordBatch> ReadNextRecordBatchAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            if (_enumerator.MoveNext())
            {
                return ValueTask.FromResult(_enumerator.Current);
            }

            return ValueTask.FromResult<RecordBatch>(default(RecordBatch));
        }
    }
}