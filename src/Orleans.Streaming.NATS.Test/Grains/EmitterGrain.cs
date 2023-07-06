// <copyright file="EmitterGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Streaming.NATS.Test.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.NATS.Test.Grains
{
    public class EmitterGrain : Grain, IEmitterGrain
    {
        private IAsyncStream<BlobMessage> _blobStream;
        private IAsyncStream<SimpleMessage> _simpleStream;

        public EmitterGrain()
        {
            var key = this.GetPrimaryKeyString().Split('/');
            var id = Guid.Parse(key[1]);

            var streamProvider = this.GetStreamProvider("Default");

            _blobStream = streamProvider.GetStream<BlobMessage>(StreamId.Create(nameof(BlobMessage), id));
            _simpleStream = streamProvider.GetStream<SimpleMessage>(StreamId.Create(nameof(SimpleMessage), id));
        }

        public async Task SendAsync(string text)
        {
            if (_simpleStream != null)
            {
                await _simpleStream.OnNextAsync(new SimpleMessage
                {
                    Text = new Immutable<string>(text),
                });
            }
        }

        public async Task SendAsync(byte[] data)
        {
            if (_blobStream != null)
            {
                await _blobStream.OnNextAsync(new BlobMessage
                {
                    Data = new Immutable<byte[]>(data),
                });
            }
        }
    }
}