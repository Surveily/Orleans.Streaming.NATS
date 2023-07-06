// <copyright file="BlobReceiverGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Runtime;
using Orleans.Streaming.NATS.Test.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.NATS.Test.Grains
{
    [ImplicitStreamSubscription(nameof(BlobMessage))]
    public class BlobReceiverGrain : Grain, IBlobReceiverGrain
    {
        private readonly IProcessor _processor;

        public BlobReceiverGrain(IProcessor processor)
        {
            _processor = processor;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");
            var streamId = StreamId.Create(nameof(BlobMessage), this.GetPrimaryKey());
            var stream = streamProvider.GetStream<BlobMessage>(streamId);

            await stream.SubscribeAsync(OnNextAsync);
        }

        private Task OnNextAsync(BlobMessage message, StreamSequenceToken token)
        {
            _processor.Process(message.Data.Value);

            return Task.CompletedTask;
        }
    }
}