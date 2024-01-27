namespace DeltaLake.Table
{
    /// <summary>
    /// Save mode for the created delta table
    /// </summary>
    public enum SaveMode
    {
        /// <summary>
        ///  Append to the table
        /// </summary>
        Append,
        /// <summary>
        /// The target location will be overwritten.
        /// </summary>
        Overwrite,
        /// <summary>
        /// If files exist for the target, the operation must fail.
        /// </summary>
        ErrorIfExists,
        /// <summary>
        /// /// If files exist for the target, the operation must not proceed or change any data.
        /// </summary>
        Ignore,
    }
}