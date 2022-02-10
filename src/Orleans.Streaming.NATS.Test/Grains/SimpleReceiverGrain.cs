// <copyright file="SimpleReceiverGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Streaming.NATS.Test.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.NATS.Test.Grains
{
    [ImplicitStreamSubscription("SimpleStream")]
    public class SimpleReceiverGrain : Grain, ISimpleReceiverGrain
    {
        private readonly IProcessor processor;

        private object? subscription;
        private IAsyncStream<SimpleMessage>? input;

        public SimpleReceiverGrain(IProcessor processor)
        {
            this.processor = processor;
        }

        public override async Task OnActivateAsync()
        {
            var streamProvider = this.GetStreamProvider("Default");

            this.input = streamProvider.GetStream<SimpleMessage>(this.GetPrimaryKey(), "SimpleStream");
            this.subscription = await this.input.SubscribeAsync<SimpleMessage>(this.OnNextAsync);

            await base.OnActivateAsync();
        }

        private Task OnNextAsync(SimpleMessage message, StreamSequenceToken token)
        {
            this.processor.Process(message.Text.Value);

            return Task.CompletedTask;
        }
    }
}