// <copyright file="NatsQueueAdapter.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using NATS.Client.JetStream;
using Orleans.Streams;

namespace Orleans.Streaming.NATS
{
    /// <summary>
    /// The queue adapter for NATS.
    /// </summary>
    public class NatsQueueAdapter : IQueueAdapter
    {
        /// <summary>
        /// Connection object.
        /// </summary>
        private readonly IJetStream jetStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="NatsQueueAdapter"/> class.
        /// </summary>
        /// <param name="jetStream">Connection object.</param>
        public NatsQueueAdapter(IJetStream jetStream)
        {
            this.jetStream = jetStream;
        }

        /// <summary>
        /// Gets the name of the adapter.
        /// </summary>
        /// <returns>A magical string that is the name of this class.</returns>
        public string Name => nameof(NatsQueueAdapter);

        /// <summary>
        /// Gets a value indicating whether this stream is rewindable.
        /// </summary>
        public bool IsRewindable => false;

        /// <summary>
        /// Gets the direction of the stream.
        /// </summary>
        public StreamProviderDirection Direction => StreamProviderDirection.ReadOnly;

        /// <summary>
        /// Go ahead and create a Queue consumer.
        /// </summary>
        /// <param name="queueId">What is the identifier of the queue in question.</param>
        /// <returns>A NATS adapter.</returns>
        public IQueueAdapterReceiver CreateReceiver(QueueId queueId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Put stuff on the queue.
        /// </summary>
        /// <param name="streamGuid">The unique stream identifier.</param>
        /// <param name="streamNamespace">The namespace where stream is located.</param>
        /// <param name="events">A generic collection of events.</param>
        /// <param name="token">Sequence Token for us to put data in the stream.</param>
        /// <param name="requestContext">Meta information about the context.</param>
        /// <typeparam name="T">Message data class.</typeparam>
        /// <returns>The task of putting data in the queue.</returns>
        public Task QueueMessageBatchAsync<T>(Guid streamGuid, string streamNamespace, IEnumerable<T> events, StreamSequenceToken token, Dictionary<string, object> requestContext)
        {
            throw new NotImplementedException();
        }
    }
}