// <copyright file="SimpleReceiverGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Runtime;
using Orleans.Streaming.NATS.Test.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.NATS.Test.Grains
{
    [ImplicitStreamSubscription(nameof(SimpleMessage))]
    public class SimpleReceiverGrain : Grain, ISimpleReceiverGrain
    {
        private readonly IProcessor _processor;

        public SimpleReceiverGrain(IProcessor processor)
        {
            _processor = processor;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");
            var streamId = StreamId.Create(nameof(SimpleMessage), this.GetPrimaryKey());
            var stream = streamProvider.GetStream<SimpleMessage>(streamId);

            await stream.SubscribeAsync(OnNextAsync);
        }

        private Task OnNextAsync(SimpleMessage message, StreamSequenceToken token)
        {
            _processor.Process(message.Text.Value);

            return Task.CompletedTask;
        }
    }
}