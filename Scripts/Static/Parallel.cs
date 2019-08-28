using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Unity parallel namespace
/// </summary>
namespace UnityParallel
{
    /// <summary>
    /// Parallel class
    /// </summary>
    public static class Parallel
    {
        /// <summary>
        /// Default minimum partition size
        /// </summary>
        private static readonly int defaultMinimumPartitionSize = 128;

        /// <summary>
        /// Default parallel options
        /// </summary>
        private static readonly ParallelOptions defaultParallelOptions = new ParallelOptions
        {
            CancellationToken = System.Threading.CancellationToken.None,
            MaxDegreeOfParallelism = SystemInfo.processorCount,
            TaskScheduler = TaskScheduler.Default
        };

        /// <summary>
        /// Partitioned parallel "for" loop
        /// </summary>
        /// <param name="fromInclusive">From (inclusive)</param>
        /// <param name="toExclusive">To (Exclusive)</param>
        /// <param name="minimumPartitionSize">Minimum partition size</param>
        /// <param name="body">Body</param>
        public static void For(int fromInclusive, int toExclusive, int minimumPartitionSize, Action<int> body)
        {
            int length = (toExclusive - fromInclusive);
            if ((body != null) && (length > 0))
            {
                int partition_size = length / SystemInfo.processorCount;
                if ((SystemInfo.processorCount > 1) && (minimumPartitionSize <= partition_size))
                {
                    if (toExclusive > fromInclusive)
                    {
                        System.Threading.Tasks.Parallel.ForEach(Partitioner.Create(fromInclusive, toExclusive, partition_size), defaultParallelOptions, (partition) =>
                        {
                            for (int i = partition.Item1; i < partition.Item2; i++)
                            {
                                body(i);
                            }
                        });
                    }
                }
                else
                {
                    for (int i = fromInclusive; i < toExclusive; i++)
                    {
                        body(i);
                    }
                }
            }
        }

        /// <summary>
        /// Automatically partitioned parallel "for" loop
        /// </summary>
        /// <param name="fromInclusive">From (inclusive)</param>
        /// <param name="toExclusive">To (exclusive)</param>
        /// <param name="body">Body</param>
        public static void For(int fromInclusive, int toExclusive, Action<int> body)
        {
            For(fromInclusive, toExclusive, defaultMinimumPartitionSize, body);
        }

        /// <summary>
        /// Partitioned parallel "foreach" loop
        /// </summary>
        /// <typeparam name="T">Array element type</typeparam>
        /// <param name="array">Array</param>
        /// <param name="minimumPartitionSize">Minimum partition size</param>
        /// <param name="body">Body</param>
        public static void ForEach<T>(IReadOnlyList<T> array, int minimumPartitionSize, Action<T> body)
        {
            if ((array != null) && (body != null))
            {
                For(0, array.Count, minimumPartitionSize, (i) =>
                {
                    body(array[i]);
                });
            }
        }

        /// <summary>
        /// Automatically partitioned parallel "foreach" loop
        /// </summary>
        /// <typeparam name="T">Array element type</typeparam>
        /// <param name="array">Array</param>
        /// <param name="body">Body</param>
        public static void ForEach<T>(IReadOnlyList<T> array, Action<T> body)
        {
            ForEach(array, defaultMinimumPartitionSize, body);
        }
    }
}
