// -----------------------------------------------------------------------------
// <summary>
// The end-user table interface.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using DeltaLake.Table;
using CancellationToken = System.Threading.CancellationToken;

namespace DeltaLake.Interfaces
{
    /// <summary>
    /// Table operations.
    /// </summary>
    public interface ITable : IDisposable
    {
        #region Properties

        /// <summary>
        /// Retrieves the current table files.
        /// </summary>
        /// <returns>The list of physical files representing the table.</returns>
        string[] Files();

        /// <summary>
        /// Retrieves the current table file URIs/paths.
        /// </summary>
        /// <returns>The list of physical file URIs/paths representing the table.</returns>
        string[] FileUris();

        /// <summary>
        /// Retrieves the current table location.
        /// </summary>
        /// <returns>The table location.</returns>
        string Location();

        /// <summary>
        /// Retrieves the current table version.
        /// </summary>
        /// <returns>The table version.</returns>
        ulong Version();

        /// <summary>
        /// Returns the table schema as an Arrow schema.
        /// </summary>
        /// <returns>An Arrow <see cref="Schema"/>.</returns>
        Schema Schema();

        /// <summary>
        /// Returns the table metadata.
        /// </summary>
        /// <returns>A <see cref="DeltaLake.Table.TableMetadata"/>.</returns>
        TableMetadata Metadata();

        /// <summary>
        /// Returns minimum reader and writer versions.
        /// </summary>
        /// <returns>A <see cref="DeltaLake.Table.ProtocolInfo"/>.</returns>
        ProtocolInfo ProtocolVersions();

        /// <summary>
        /// Retrieves the table commit history.
        /// </summary>
        /// <param name="limit">Optional maximum amount of history to return.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>An array of <see cref="CommitInfo"/>.</returns>
        Task<CommitInfo[]> HistoryAsync(ulong? limit, CancellationToken cancellationToken);

        #endregion Properties

        #region Version Operations

        /// <summary>
        /// Asynchronously loads a table at the desired version.
        /// </summary>
        /// <param name="version">Desired version</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A <see cref="Task"/>representing the version load operation.</returns>
        Task LoadVersionAsync(ulong version, CancellationToken cancellationToken);

        /// <summary>
        /// Loads table at a specific point in time.
        /// </summary>
        /// <param name="timestamp">A desired point in time to load the table.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A <see cref="Task"/>representing the version load operation.</returns>
        Task LoadDateTimeAsync(DateTimeOffset timestamp, CancellationToken cancellationToken);

        /// <summary>
        /// Loads table at a specific point in time.
        /// </summary>
        /// <param name="timestampMilliseconds">A desired point in time in elapsed milliseconds to load the table.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A <see cref="Task"/>representing the version load operation.</returns>
        Task LoadDateTimeAsync(long timestampMilliseconds, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the table to specific or latest version.
        /// </summary>
        /// <param name="maxVersion">Maximum version; optional, defaults to latest.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A <see cref="Task"/>representing the update operation.</returns>
        Task UpdateIncrementalAsync(long? maxVersion, CancellationToken cancellationToken);

        /// <summary>
        /// Restores a table using the options provided.
        /// </summary>
        /// <param name="options">The restore options.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A <see cref="Task"/>representing the restore operation.</returns>
        Task RestoreAsync(RestoreOptions options, CancellationToken cancellationToken);

        #endregion Version Operations

        #region Read Operations

        /// <summary>
        /// Issue a SELECT query against the table and returns an Arrow <see cref="RecordBatch"/>.
        /// </summary>
        /// <param name="query">A SELECT query.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A <see cref="IAsyncEnumerable{RecordBatch}"/> collection of record batches representing the query results.</returns>
        IAsyncEnumerable<RecordBatch> QueryAsync(SelectQuery query, [EnumeratorCancellation] CancellationToken cancellationToken);

        /// <summary>
        /// Read the delta table and return as <see cref="Apache.Arrow.Table"/>.
        /// </summary>
        Apache.Arrow.Table ReadAsArrowTable();

        /// <summary>
        /// Read the delta table and return as a <see cref="string"/> representation,
        /// only displays the first <see cref="RecordBatch"/>.
        /// </summary>
        string ReadAsString();

        #endregion Read Operations

        #region Transaction Operations

        /// <summary>
        /// Inserts a collection of records into the table based on the provided options.
        /// </summary>
        /// <param name="records">A collection of <see cref="RecordBatch"/> records to insert.</param>
        /// <param name="schema">The associated <see cref="Schema"/> for the <paramref name="records"/>.</param>
        /// <param name="options">The <see cref="InsertOptions"/>.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A <see cref="Task"/>representing the insert operation.</returns>
        Task InsertAsync(IReadOnlyCollection<RecordBatch> records, Schema schema, InsertOptions options, CancellationToken cancellationToken);

        /// <summary>
        /// Inserts a stream of records into the table based on the provided options.
        /// </summary>
        /// <param name="records">A collection of <see cref="IArrowArrayStream"/> records to insert.</param>
        /// <param name="options">The <see cref="InsertOptions"/>.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A <see cref="Task"/>representing the insert operation.</returns>
        Task InsertAsync(IArrowArrayStream records, InsertOptions options, CancellationToken cancellationToken);

        /// <summary>
        /// Merges a collection of <see cref="RecordBatch"/> into the table.
        /// </summary>
        /// <param name="query">A MERGE sql statement.</param>
        /// <param name="records">A collection of record batches.</param>
        /// <param name="schema">The associated <see cref="Schema"/> for the <paramref name="records"/>.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A <see cref="Task"/>representing the merge operation.</returns>
        Task MergeAsync(string query, IReadOnlyCollection<RecordBatch> records, Schema schema, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the table based on the provided query.
        /// </summary>
        /// <param name="query">A sql statement.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>JSON serialized statistics about the update operation.</returns>
        Task<string> UpdateAsync(string query, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes matching items from the table based on a predicate.
        /// </summary>
        /// <param name="predicate">The criteria for deletion; eg. "color = 'Blue' AND ts > 10000".</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A <see cref="Task"/>representing the delete operation.</returns>
        Task DeleteAsync(string predicate, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes all records from the table.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A <see cref="Task"/>representing the delete operation.</returns>
        Task DeleteAsync(CancellationToken cancellationToken);

        #endregion Transaction Operations

        #region Metadata Operations

        /// <summary>
        /// Adds constraints and custom metadata to a table.
        /// </summary>
        /// <param name="constraints">A collection of key and values representing columns and constraints.</param>
        /// <param name="customMetadata">A collection of key and values representing columns and metadata.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A <see cref="Task"/>representing applying the constraint operation.</returns>
        Task AddConstraintsAsync(IReadOnlyDictionary<string, string> constraints, IReadOnlyDictionary<string, string>? customMetadata, CancellationToken cancellationToken);

        /// <summary>
        /// Adds constraints to a table.
        /// </summary>
        /// <param name="constraints">A collection of key and values representing columns and constraints.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A <see cref="Task"/>representing applying the constraint operation.</returns>
        Task AddConstraintsAsync(IReadOnlyDictionary<string, string> constraints, CancellationToken cancellationToken);

        #endregion Metadata Operations
    }
}
