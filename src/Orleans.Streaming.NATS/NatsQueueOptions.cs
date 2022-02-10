// <copyright file="NatsQueueOptions.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using NATS.Client.JetStream;

namespace Orleans.Streaming.NATS
{
    public class NatsQueueOptions
    {
        [Redact]
        public string? ConnectionString { get; set; }

        [Redact]
        public StorageType StorageType { get; set; }
    }
}