// <copyright file="NatsQueueAdapter.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using NATS.Client.JetStream;
using Orleans.Serialization;
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
        /// Logger factory object.
        /// </summary>
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Serialization manager object.
        /// </summary>
        private readonly SerializationManager serializationManager;

        /// <summary>
        /// Queue mapper object.
        /// </summary>
        private readonly IConsistentRingStreamQueueMapper streamQueueMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NatsQueueAdapter"/> class.
        /// </summary>
        /// <param name="serializationManager">Serialization manager object.</param>
        /// <param name="jetStream">Connection object.</param>
        /// <param name="streamQueueMapper">Stream queue mapper object.</param>
        /// <param name="loggerFactory">Logger factory object.</param>
        public NatsQueueAdapter(SerializationManager serializationManager, IConsistentRingStreamQueueMapper streamQueueMapper, ILoggerFactory loggerFactory, IJetStream jetStream)
        {
            this.jetStream = jetStream;
            this.loggerFactory = loggerFactory;
            this.streamQueueMapper = streamQueueMapper;
            this.serializationManager = serializationManager;
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
        public StreamProviderDirection Direction => StreamProviderDirection.ReadWrite;

        /// <summary>
        /// Go ahead and create a Queue consumer.
        /// </summary>
        /// <param name="queueId">What is the identifier of the queue in question.</param>
        /// <returns>A NATS adapter.</returns>
        public IQueueAdapterReceiver CreateReceiver(QueueId queueId)
        {
            return new NatsQueueAdapterReceiver(this.serializationManager, this.jetStream, queueId.ToString());
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
        public async Task QueueMessageBatchAsync<T>(Guid streamGuid, string streamNamespace, IEnumerable<T> events, StreamSequenceToken token, Dictionary<string, object> requestContext)
        {
            var builder = PublishOptions.Builder()
                                        .WithTimeout(1000)
                                        .WithStream(streamNamespace)
                                        .WithMessageId(Guid.NewGuid().ToString());

            var ack = await this.jetStream.PublishAsync($"{streamNamespace}.request", null, builder.Build());
        }
    }
}