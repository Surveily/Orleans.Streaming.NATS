// <copyright file="NatsQueueAdapterReceiver.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using NATS.Client.JetStream;
using Orleans.Streams;

namespace Orleans.Streaming.NATS
{
    /// <summary>
    /// The receiver class for NATS.
    /// </summary>
    public class NatsQueueAdapterReceiver : IQueueAdapterReceiver
    {
        /// <summary>
        /// Connection object.
        /// </summary>
        private readonly IJetStream jetStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="NatsQueueAdapterReceiver"/> class.
        /// </summary>
        /// <param name="jetStream">Connection object.</param>
        public NatsQueueAdapterReceiver(IJetStream jetStream)
        {
            this.jetStream = jetStream;
        }

        /// <summary>
        /// Pull a batch of messages.
        /// </summary>
        /// <param name="maxCount">The number of anticipated messages.</param>
        /// <returns>List of messages.</returns>
        public Task<IList<IBatchContainer>> GetQueueMessagesAsync(int maxCount)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create the receiver.
        /// </summary>
        /// <param name="timeout">Creation timeout.</param>
        /// <returns>The task that creates the receiver.</returns>
        public Task Initialize(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Acknowledge the messages.
        /// </summary>
        /// <param name="messages">Data to ack.</param>
        /// <returns>The task that acknowledges the messages.</returns>
        public Task MessagesDeliveredAsync(IList<IBatchContainer> messages)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shut down the receiver.
        /// </summary>
        /// <param name="timeout">Shutdown timeout.</param>
        /// <returns>The task that shuts down the receiver.</returns>
        public Task Shutdown(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }
}