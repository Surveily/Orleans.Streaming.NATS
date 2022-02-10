// <copyright file="EmitterGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Concurrency;
using Orleans.Streaming.NATS.Test.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.NATS.Test.Grains
{
    public class EmitterGrain : Grain, IEmitterGrain
    {
        private IAsyncStream<BlobMessage>? blobStream;
        private IAsyncStream<SimpleMessage>? simpleStream;

        public override async Task OnActivateAsync()
        {
            var key = this.GetPrimaryKeyString().Split('/');
            var streamProvider = this.GetStreamProvider("Default");

            var id = Guid.Parse(key[1]);

            this.blobStream = streamProvider.GetStream<BlobMessage>(id, nameof(BlobMessage));
            this.simpleStream = streamProvider.GetStream<SimpleMessage>(id, nameof(SimpleMessage));

            await base.OnActivateAsync();
        }

        public async Task SendAsync(string text)
        {
            if (this.simpleStream != null)
            {
                await this.simpleStream.OnNextAsync(new SimpleMessage
                {
                    Text = new Immutable<string>(text),
                });
            }
        }

        public async Task SendAsync(byte[] data)
        {
            if (this.blobStream != null)
            {
                await this.blobStream.OnNextAsync(new BlobMessage
                {
                    Data = new Immutable<byte[]>(data),
                });
            }
        }
    }
}