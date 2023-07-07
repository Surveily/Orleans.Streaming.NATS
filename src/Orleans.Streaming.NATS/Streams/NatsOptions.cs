// <copyright file="NatsOptions.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using NATS.Client.JetStream;

namespace Orleans.Streaming.NATS.Streams
{
    public class NatsOptions
    {
        [Redact]
        public string ConnectionString { get; set; }
    }
}