using System.Collections;

namespace DeltaLake.Tests.Table;
public partial class LoadTests
{
    public class AllTablesEnumerable : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator() => TableHelpers.ValidTables.Select(x => new object[] {
            x
        }).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}