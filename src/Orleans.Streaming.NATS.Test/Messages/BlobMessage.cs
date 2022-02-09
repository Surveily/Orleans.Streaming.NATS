// <copyright file="BlobMessage.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Concurrency;

namespace Orleans.Streaming.NATS.Test.Messages
{
    public class BlobMessage
    {
        public Immutable<byte[]> Data { get; set; }
    }
}