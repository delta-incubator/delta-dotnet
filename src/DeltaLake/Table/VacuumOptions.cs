using System.Collections.Generic;
namespace DeltaLake.Table
{
    /// <summary>
    /// Options for the vacuum operation
    /// </summary>
    public class VacuumOptions
    {
        /// <summary>
        /// True to only simulate
        /// </summary>
        public bool DryRun { get; set; }

        /// <summary>
        /// The number of hours in the retention period. Only enforced if set.
        /// </summary>
        public ulong? RetentionHours { get; set; }

        /// <summary>
        /// Custom metadata to add to the operations
        /// </summary>
        public Dictionary<string, string>? CustomMetadata { get; init; } = new Dictionary<string, string>();
    }
}