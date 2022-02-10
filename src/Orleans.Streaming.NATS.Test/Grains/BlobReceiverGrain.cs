// <copyright file="BlobReceiverGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Streaming.NATS.Test.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.NATS.Test.Grains
{
    [ImplicitStreamSubscription("BlobStream")]
    public class BlobReceiverGrain : Grain, IBlobReceiverGrain
    {
        private object? subscription;
        private IAsyncStream<BlobMessage>? input;

        public override async Task OnActivateAsync()
        {
            var streamProvider = this.GetStreamProvider("Default");

            this.input = streamProvider.GetStream<BlobMessage>(this.GetPrimaryKey(), "BlobStream");
            this.subscription = await this.input.SubscribeAsync<BlobMessage>(this.OnNextAsync);

            await base.OnActivateAsync();
        }

        private Task OnNextAsync(BlobMessage message, StreamSequenceToken token)
        {
            Console.WriteLine(message.Data.Value.Length);

            return Task.CompletedTask;
        }
    }
}