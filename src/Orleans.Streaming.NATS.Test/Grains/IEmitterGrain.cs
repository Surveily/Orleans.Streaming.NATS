// <copyright file="IEmitterGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

namespace Orleans.Streaming.NATS.Test.Grains
{
    public interface IEmitterGrain : IGrainWithGuidKey
    {
        Task SendAsync(string text);

        Task SendAsync(byte[] data);
    }
}