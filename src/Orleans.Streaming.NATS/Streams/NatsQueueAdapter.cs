// <copyright file="NatsQueueAdapter.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using NATS.Client.JetStream;
using Orleans.Serialization;
using Orleans.Streams;

namespace Orleans.Streaming.NATS.Streams
{
    /// <summary>
    /// The queue adapter for NATS.
    /// </summary>
    public class NatsQueueAdapter : IQueueAdapter
    {
        private readonly IJetStream jetStream;

        private readonly ILoggerFactory loggerFactory;

        private readonly SerializationManager serializationManager;

        private readonly IConsistentRingStreamQueueMapper streamQueueMapper;

        public NatsQueueAdapter(SerializationManager serializationManager, IConsistentRingStreamQueueMapper streamQueueMapper, ILoggerFactory loggerFactory, IJetStream jetStream)
        {
            this.jetStream = jetStream;
            this.loggerFactory = loggerFactory;
            this.streamQueueMapper = streamQueueMapper;
            this.serializationManager = serializationManager;
        }

        public string Name => nameof(NatsQueueAdapter);

        public bool IsRewindable => false;

        public StreamProviderDirection Direction => StreamProviderDirection.ReadWrite;

        public IQueueAdapterReceiver CreateReceiver(QueueId queueId)
        {
            return new NatsQueueAdapterReceiver(this.serializationManager, this.jetStream, queueId.ToString());
        }

        public async Task QueueMessageBatchAsync<T>(Guid streamGuid, string streamNamespace, IEnumerable<T> events, StreamSequenceToken token, Dictionary<string, object> requestContext)
        {
            var queueId = this.streamQueueMapper.GetQueueForStream(streamGuid, streamNamespace);
            var message = NatsBatchContainer.ToMessage(this.serializationManager, streamGuid, streamNamespace, events, requestContext);
            var builder = PublishOptions.Builder()
                                        .WithTimeout(1000)
                                        .WithStream(queueId.ToString())
                                        .WithMessageId(Guid.NewGuid().ToString());

            var ack = await this.jetStream.PublishAsync($"{queueId}.request", message.Data, builder.Build());
        }
    }
}