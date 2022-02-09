// <copyright file="QueueRepository.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Text;
using NATS.Client;
using NATS.Client.JetStream;

namespace Nats
{
    public class QueueRepository
    {
        private readonly IJetStream _jetStream;
        private readonly IConnection _connection;
        private readonly IJetStreamManagement _management;

        public QueueRepository(IConnection connection)
        {
            _connection = connection;
            _jetStream = _connection.CreateJetStreamContext();
            _management = _connection.CreateJetStreamManagementContext();
        }

        public void Delete(string stream)
        {
            try
            {
                var info = _management.GetStreamInfo(stream); // this throws if the stream does not exist
                _management.DeleteStream(stream);
                return;
            }
            catch (NATSJetStreamException)
            {
                /* stream does not exist */
            }
        }

        public void Prepare(string stream)
        {
            StreamInfo? streamInfo = null;

            try
            {
                streamInfo = _management.GetStreamInfo(stream);
            }
            catch (NATSJetStreamException)
            {
                /* stream does not exist */
            }

            if (streamInfo == null)
            {
                var sc = GetStream(stream);

                _management.AddStream(sc);
            }

            ConsumerInfo? consumerInfo = null;

            try
            {
                consumerInfo = _management.GetConsumerInfo(stream, $"{stream}");
            }
            catch (NATSJetStreamException)
            {
                /* consumer does not exist */
            }

            if (consumerInfo == null)
            {
                var cc = GetConsumer(stream);

                _management.AddOrUpdateConsumer(stream, cc);
            }
        }

        public IJetStreamPullSubscription Subscribe(string stream)
        {
            var cc = GetConsumer(stream);
            var options = PullSubscribeOptions.Builder()
                                              .WithConfiguration(cc)
                                              .Build();

            return _jetStream.PullSubscribe($"{stream}.request", options);
        }

        public async Task PublishAsync(string stream, string message)
        {
            var builder = PublishOptions.Builder()
                                        .WithTimeout(1000)
                                        .WithStream(stream)
                                        .WithMessageId(Guid.NewGuid().ToString());

            var ack = await _jetStream.PublishAsync($"{stream}.request", Encoding.Default.GetBytes(message), builder.Build());
        }

        private static StreamConfiguration GetStream(string stream)
        {
            return StreamConfiguration.Builder()
                                      .WithName(stream)
                                      .WithStorageType(StorageType.File)
                                      .WithRetentionPolicy(RetentionPolicy.WorkQueue)
                                      .WithSubjects($"{stream}.*")
                                      .Build();
        }

        private static ConsumerConfiguration GetConsumer(string stream)
        {
            return ConsumerConfiguration.Builder()
                                        .WithDurable($"{stream}")
                                        .WithFilterSubject($"{stream}.request")
                                        // .WithDeliverGroup($"{stream}_queue")
                                        // .WithDeliverSubject($"{stream}.request")
                                        .Build();
        }
    }
}