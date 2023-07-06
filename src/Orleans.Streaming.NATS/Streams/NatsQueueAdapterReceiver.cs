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
        private readonly string _stream;

        private readonly IJetStream _jetStream;

        private readonly Serializer<NatsBatchContainer> _serializationManager;

        private TimeSpan _timeout;

        private long _lastReadMessage;

        private IJetStreamPullSubscription? _subscription;

        public NatsQueueAdapterReceiver(Serializer<NatsBatchContainer> serializationManager, IJetStream jetStream, string stream)
        {
            if (stream == null)
            {
                throw new ArgumentException(nameof(stream));
            }

            if (jetStream == null)
            {
                throw new ArgumentException(nameof(jetStream));
            }

            _stream = stream;
            _jetStream = jetStream;
            _timeout = TimeSpan.FromSeconds(1);
            _serializationManager = serializationManager;
        }

        public Task<IList<IBatchContainer>> GetQueueMessagesAsync(int maxCount)
        {
            const int MaxNumberOfMessagesToPeek = 256;

            IList<IBatchContainer> result = new List<IBatchContainer>();

            int count = maxCount < 0 || maxCount == QueueAdapterConstants.UNLIMITED_GET_QUEUE_MSG ?
                   MaxNumberOfMessagesToPeek : Math.Min(maxCount, MaxNumberOfMessagesToPeek);

            var fetched = _subscription!.Fetch(count, (int)_timeout.TotalMilliseconds);

            foreach (var message in fetched)
            {
                result.Add(NatsBatchContainer.FromNatsMessage(_serializationManager, message, _lastReadMessage++));
            }

            return Task.FromResult(result);
        }

        public Task Initialize(TimeSpan timeout)
        {
            var cc = Nats.GetConsumer(_stream);
            var options = PullSubscribeOptions.Builder()
                                              .WithConfiguration(cc)
                                              .Build();

            _timeout = timeout;
            _subscription = _jetStream.PullSubscribe($"{_stream}.request", options);

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
            if (_subscription != null)
            {
                _subscription.Dispose();
            }

            return Task.CompletedTask;
        }
    }
}