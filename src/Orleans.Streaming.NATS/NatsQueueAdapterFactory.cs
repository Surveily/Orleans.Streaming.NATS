// <copyright file="NatsQueueAdapterFactory.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using NATS.Client.JetStream;
using Orleans.Streams;

namespace Orleans.Streaming.NATS
{
    /// <summary>
    /// The factory for NATS queue adapters.
    /// </summary>
    public class NatsQueueAdapterFactory : IQueueAdapterFactory
    {
        /// <summary>
        /// Connection object.
        /// </summary>
        private readonly IJetStream jetStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="NatsQueueAdapterFactory"/> class.
        /// </summary>
        /// <param name="jetStream">Connection object.</param>
        public NatsQueueAdapterFactory(IJetStream jetStream)
        {
            this.jetStream = jetStream;
        }

        /// <summary>
        /// Create an adapter.
        /// </summary>
        /// <returns>An adapter.</returns>
        public Task<IQueueAdapter> CreateAdapter()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the failure handler.
        /// </summary>
        /// <param name="queueId">Identifier of the queue.</param>
        /// <returns>The object that handles failures.</returns>
        public Task<IStreamFailureHandler> GetDeliveryFailureHandler(QueueId queueId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the cache of adapters.
        /// </summary>
        /// <returns>The cache of adapters.</returns>
        public IQueueAdapterCache GetQueueAdapterCache()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the stream queue mapper.
        /// </summary>
        /// <returns>The stream queue mapper.</returns>
        public IStreamQueueMapper GetStreamQueueMapper()
        {
            throw new NotImplementedException();
        }
    }
}