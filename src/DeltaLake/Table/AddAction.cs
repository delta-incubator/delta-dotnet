using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeltaLake.Table
{
    /// <summary>
    /// Represents a file add action for log-only commits.
    /// Describes a pre-written Parquet data file to be registered in the Delta log.
    /// </summary>
    public record AddAction
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        /// <summary>
        /// Relative path to the data file from the table root.
        /// Example: "year=2024/month=01/part-00000-abc123.snappy.parquet"
        /// </summary>
        [JsonPropertyName("path")]
        public string Path { get; init; } = string.Empty;

        /// <summary>
        /// Size of the data file in bytes.
        /// </summary>
        [JsonPropertyName("size")]
        public long Size { get; init; }

        /// <summary>
        /// Partition column values. Keys are partition column names,
        /// values are the partition values (null for null partitions).
        /// Null or empty for non-partitioned tables.
        /// </summary>
        [JsonPropertyName("partitionValues")]
        public IReadOnlyDictionary<string, string?>? PartitionValues { get; init; }

        /// <summary>
        /// Modification time of the file in milliseconds since Unix epoch.
        /// </summary>
        [JsonPropertyName("modificationTime")]
        public long ModificationTime { get; init; }

        /// <summary>
        /// Whether this action represents a data change. True for new data writes.
        /// </summary>
        [JsonPropertyName("dataChange")]
        public bool DataChange { get; init; } = true;

        /// <summary>
        /// Serializes this add action to a JSON string matching the Delta protocol format.
        /// </summary>
        /// <returns>A JSON string representation of this add action.</returns>
        public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

        /// <summary>
        /// Deserializes a JSON string to an <see cref="AddAction"/> instance.
        /// </summary>
        /// <param name="json">A JSON string in the Delta protocol add action format.</param>
        /// <returns>A deserialized <see cref="AddAction"/> instance.</returns>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static AddAction FromJson(string json) =>
            JsonSerializer.Deserialize<AddAction>(json, JsonOptions)
                ?? throw new JsonException("Failed to deserialize AddAction from JSON");
    }
}
