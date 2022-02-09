// <copyright file="Nats.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using NATS.Client.JetStream;

namespace Orleans.Streaming.NATS
{
    /// <summary>
    /// Utility class for generating NATS configuration.
    /// </summary>
    public static class Nats
    {
        /// <summary>
        /// Generate Stream configuration by name.
        /// </summary>
        /// <param name="stream">Name of the stream.</param>
        /// <param name="storageType">Keep it in file or memory.</param>
        /// <returns>Pregenerated stream configuration.</returns>
        public static StreamConfiguration GetStream(string stream, StorageType storageType)
        {
            return StreamConfiguration.Builder()
                                      .WithName(stream)
                                      .WithStorageType(storageType)
                                      .WithRetentionPolicy(RetentionPolicy.WorkQueue)
                                      .WithSubjects($"{stream}.*")
                                      .Build();
        }

        /// <summary>
        /// Generate Consumer configuration by name.
        /// </summary>
        /// <param name="stream">Name of the stream to consume.</param>
        /// <returns>Pregenerated consumer configuration.</returns>
        public static ConsumerConfiguration GetConsumer(string stream)
        {
            return ConsumerConfiguration.Builder()
                                        .WithDurable($"{stream}")
                                        .WithFilterSubject($"{stream}.request")
                                        .Build();
        }
    }
}