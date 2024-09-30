using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DeltaLake.Table
{
    /// <summary>
    /// CommitInfo for history command
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
    public class CommitInfo
    {
        /// <summary>
        /// Timestamp in millis when the commit was created
        /// </summary>
        [JsonPropertyName("timestamp")]
        public long? Timestamp { get; set; }

        /// <summary>
        /// Id of the user invoking the commit[]
        /// </summary>
        [JsonPropertyName("user_id")]

        public string? UserId { get; set; }
        /// <summary>
        /// Name of the user invoking the commit
        /// </summary>
        [JsonPropertyName("user_name")]
        public string? UserName { get; set; }
        /// <summary>
        /// The operation performed during the
        /// </summary>
        [JsonPropertyName("operation")]
        public string? Operation { get; set; }

        /// <summary>
        /// Parameters used for table operation
        /// </summary>
        [JsonPropertyName("operation_parameters")]

        public Dictionary<string, JsonValue>? OperationParameters { get; set; }

        /// <summary>
        /// Version of the table when the operation was started
        /// </summary>
        [JsonPropertyName("read_version")]
        public long? ReadVersion { get; set; }
        /// <summary>
        /// The isolation level of the commit
        /// </summary>
        [JsonPropertyName("isolation_level")]
        public string? IsolationLevel { get; set; }
        /// <summary>
        /// Undocumented
        /// </summary>
        [JsonPropertyName("is_blind_append")]
        public bool? IsBlindAppend { get; set; }
        /// <summary>
        /// Delta engine which created the commit.
        /// </summary>
        [JsonPropertyName("engine_info")]
        public string? EngineInfo { get; set; }
        /// <summary>
        /// Additional provenance information for the commit
        /// </summary>
        [JsonPropertyName("info")]
        public Dictionary<string, JsonValue>? Info { get; set; }
    }
#pragma warning restore CA2227 // Collection properties should be read only
}