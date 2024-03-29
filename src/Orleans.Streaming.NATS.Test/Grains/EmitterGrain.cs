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

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var id = this.GetPrimaryKey();
            var streamProvider = this.GetStreamProvider("Default");

            _blobStream = StreamFactory.Create<BlobMessage>(streamProvider, id);
            _simpleStream = StreamFactory.Create<SimpleMessage>(streamProvider, id);

            await base.OnActivateAsync(cancellationToken);
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