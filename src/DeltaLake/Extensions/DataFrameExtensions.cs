// -----------------------------------------------------------------------------
// <summary>
// Extension methods for transforming DataFrames.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Text;
using Microsoft.Data.Analysis;

namespace DeltaLake.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="DataFrame"/>.
    /// </summary>
    public static class DataFrameExtensions
    {
        # region Public methods

        /// <summary>
        /// Converts the <see cref="DataFrame"/> to a formatted <see cref="string"/>.
        /// </summary>
        /// <param name="dataFrame">The DataFrame to convert to formatted string.</param>
        /// <returns>The formatted string representation of the DataFrame.</returns>
        public static string ToFormattedString(this DataFrame dataFrame) => ToStringArray2D(dataFrame).ToMarkdown();

        # endregion Public methods

        #region private methods

        private static string ToPrettyText(this DataFrame df) => ToStringArray2D(df).ToFormattedText();

        private static string[,] ToStringArray2D(DataFrame df)
        {
            string[,] strings = new string[df.Rows.Count + 1, df.Columns.Count];

            for (int i = 0; i < df.Columns.Count; i++)
                strings[0, i] = df.Columns[i].Name;

            for (int i = 0; i < df.Rows.Count; i++)
            for (int j = 0; j < df.Columns.Count; j++)
                strings[i + 1, j] = df[i, j]?.ToString() ?? string.Empty;

            return strings;
        }

        private static int[] GetMaxLengthsByColumn(this string[,] strings)
        {
            int[] maxLengthsByColumn = new int[strings.GetLength(1)];

            for (int y = 0; y < strings.GetLength(0); y++)
            for (int x = 0; x < strings.GetLength(1); x++)
                maxLengthsByColumn[x] = Math.Max(maxLengthsByColumn[x], strings[y, x].Length);

            return maxLengthsByColumn;
        }

        private static string ToFormattedText(this string[,] strings)
        {
            StringBuilder sb = new();
            int[] maxLengthsByColumn = strings.GetMaxLengthsByColumn();

            for (int y = 0; y < strings.GetLength(0); y++)
            {
                for (int x = 0; x < strings.GetLength(1); x++)
                {
                    sb.Append(strings[y, x].PadRight(maxLengthsByColumn[x] + 2));
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string ToMarkdown(this string[,] strings)
        {
            StringBuilder sb = new();
            int[] maxLengthsByColumn = GetMaxLengthsByColumn(strings);

            for (int y = 0; y < strings.GetLength(0); y++)
            {
                for (int x = 0; x < strings.GetLength(1); x++)
                {
                    sb.Append(strings[y, x].PadRight(maxLengthsByColumn[x]));
                    if (x < strings.GetLength(1) - 1)
                        sb.Append(" | ");
                }
                sb.AppendLine();

                if (y == 0)
                {
                    for (int i = 0; i < strings.GetLength(1); i++)
                    {
                        int bars = maxLengthsByColumn[i] + 2;
                        if (i == 0)
                            bars -= 1;
                        sb.Append(new String('-', bars));

                        if (i < strings.GetLength(1) - 1)
                            sb.Append("|");
                    }
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        #endregion private methods
    }
}
