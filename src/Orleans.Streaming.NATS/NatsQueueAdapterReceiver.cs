// <copyright file="NatsQueueAdapterReceiver.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using NATS.Client.JetStream;
using Orleans.Serialization;
using Orleans.Streams;

namespace Orleans.Streaming.NATS
{
    /// <summary>
    /// The receiver class for NATS.
    /// </summary>
    public class NatsQueueAdapterReceiver : IQueueAdapterReceiver
    {
        /// <summary>
        /// Stream name.
        /// </summary>
        private readonly string stream;

        /// <summary>
        /// Connection object.
        /// </summary>
        private readonly IJetStream jetStream;

        /// <summary>
        /// Serialization manager object.
        /// </summary>
        private readonly SerializationManager serializationManager;

        /// <summary>
        /// Timeout object.
        /// </summary>
        private TimeSpan timeout;

        /// <summary>
        /// Counter for read messages.
        /// </summary>
        private long lastReadMessage;

        /// <summary>
        /// Subscription object.
        /// </summary>
        private IJetStreamPullSubscription? subscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="NatsQueueAdapterReceiver"/> class.
        /// </summary>
        /// <param name="serializationManager">Serialization manager object.</param>
        /// <param name="jetStream">Connection object.</param>
        /// <param name="stream">Stream name.</param>
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

        /// <summary>
        /// Pull a batch of messages.
        /// </summary>
        /// <param name="maxCount">The number of anticipated messages.</param>
        /// <returns>List of messages.</returns>
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

        /// <summary>
        /// Create the receiver.
        /// </summary>
        /// <param name="timeout">Creation timeout.</param>
        /// <returns>The task that creates the receiver.</returns>
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

        /// <summary>
        /// Acknowledge the messages.
        /// </summary>
        /// <param name="messages">Data to ack.</param>
        /// <returns>The task that acknowledges the messages.</returns>
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

        /// <summary>
        /// Shut down the receiver.
        /// </summary>
        /// <param name="timeout">Shutdown timeout.</param>
        /// <returns>The task that shuts down the receiver.</returns>
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