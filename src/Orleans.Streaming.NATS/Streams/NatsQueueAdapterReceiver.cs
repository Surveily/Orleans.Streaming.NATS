// <copyright file="NatsQueueAdapterReceiver.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using NATS.Client.JetStream;
using Orleans.Serialization;
using Orleans.Streams;

namespace Orleans.Streaming.NATS.Streams
{
    /// <summary>
    /// The receiver class for NATS.
    /// </summary>
    public class NatsQueueAdapterReceiver : IQueueAdapterReceiver
    {
        private readonly string stream;

        private readonly IJetStream jetStream;

        private readonly SerializationManager serializationManager;

        private TimeSpan timeout;

        private long lastReadMessage;

        private IJetStreamPullSubscription? subscription;

        public NatsQueueAdapterReceiver(SerializationManager serializationManager, IJetStream jetStream, string stream)
        {
            if (stream == null)
            {
                throw new ArgumentException(nameof(stream));
            }

            if (jetStream == null)
            {
                throw new ArgumentException(nameof(jetStream));
            }

            this.stream = stream;
            this.jetStream = jetStream;
            this.timeout = TimeSpan.FromSeconds(1);
            this.serializationManager = serializationManager;
        }

        public Task<IList<IBatchContainer>> GetQueueMessagesAsync(int maxCount)
        {
            var result = new List<IBatchContainer>();
            var fetched = this.subscription!.Fetch(maxCount, (int)this.timeout.TotalMilliseconds);

            foreach (var message in fetched)
            {
                NatsBatchContainer.FromNatsMessage(this.serializationManager, message, this.lastReadMessage++);
            }

            return Task.FromResult(result as IList<IBatchContainer>);
        }

        public Task Initialize(TimeSpan timeout)
        {
            var cc = Nats.GetConsumer(this.stream);
            var options = PullSubscribeOptions.Builder()
                                              .WithConfiguration(cc)
                                              .Build();

            this.timeout = timeout;
            this.subscription = this.jetStream.PullSubscribe($"{this.stream}.request", options);

            return Task.CompletedTask;
        }

        public Task MessagesDeliveredAsync(IList<IBatchContainer> messages)
        {
            foreach (var message in messages.OfType<NatsBatchContainer>())
            {
                if (message.Message != null)
                {
                    message.Message.Ack();
                    message.Message = null;
                }
            }

            return Task.CompletedTask;
        }

        public Task Shutdown(TimeSpan timeout)
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
            }

            return Task.CompletedTask;
        }
    }
}