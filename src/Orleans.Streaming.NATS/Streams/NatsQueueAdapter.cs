// <copyright file="NatsQueueAdapter.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using NATS.Client.JetStream;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Streams;

namespace Orleans.Streaming.NATS.Streams
{
    /// <summary>
    /// The queue adapter for NATS.
    /// </summary>
    public class NatsQueueAdapter : IQueueAdapter
    {
        private readonly IJetStream _jetStream;

        private readonly string _providerName;

        private readonly ILoggerFactory _loggerFactory;

        private readonly Serializer<NatsBatchContainer> _serializer;

        private readonly IConsistentRingStreamQueueMapper _streamQueueMapper;

        public NatsQueueAdapter(Serializer serializer, IConsistentRingStreamQueueMapper streamQueueMapper, ILoggerFactory loggerFactory, IJetStream jetStream, string providerName)
        {
            _jetStream = jetStream;
            _loggerFactory = loggerFactory;
            _streamQueueMapper = streamQueueMapper;
            _serializer = serializer.GetSerializer<NatsBatchContainer>();
            _providerName = providerName;
        }

        public bool IsRewindable => false;

        public string Name => _providerName;

        public StreamProviderDirection Direction => StreamProviderDirection.ReadWrite;

        public IQueueAdapterReceiver CreateReceiver(QueueId queueId) => new NatsQueueAdapterReceiver(_serializer, _jetStream, queueId.ToString());

        public async Task QueueMessageBatchAsync<T>(StreamId streamId, IEnumerable<T> events, StreamSequenceToken token, Dictionary<string, object> requestContext)
        {
            var queueId = _streamQueueMapper.GetQueueForStream(streamId);
            var message = NatsBatchContainer.ToMessage(_serializer, streamId, events, requestContext);
            var builder = PublishOptions.Builder()
                                        .WithTimeout(5000)
                                        .WithStream(queueId.ToString())
                                        .WithMessageId(Guid.NewGuid().ToString());

            var ack = await _jetStream.PublishAsync($"{queueId}.request", message.Data, builder.Build());
        }
    }
}