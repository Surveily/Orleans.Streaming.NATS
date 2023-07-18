// <copyright file="NatsQueueAdapterFactory.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;
using NATS.Client.JetStream;
using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Orleans.Providers.Streams.Common;
using Orleans.Serialization;
using Orleans.Streams;

namespace Orleans.Streaming.NATS.Streams
{
    /// <summary>
    /// The factory for NATS queue adapters.
    /// </summary>
    public class NatsQueueAdapterFactory : IQueueAdapterFactory
    {
        private readonly string _name;

        private readonly IJetStream _jetStream;

        private readonly ILoggerFactory _loggerFactory;

        private readonly IQueueAdapterCache _adapterCache;

        private readonly IServiceProvider _serviceProvider;

        private readonly Serialization.Serializer _serializer;

        private readonly SimpleQueueCacheOptions _cacheOptions;

        private readonly IOptions<ClusterOptions> _clusterOptions;

        private readonly HashRingBasedStreamQueueMapper _streamQueueMapper;

        public NatsQueueAdapterFactory(string name,
                                       IJetStream jetStream,
                                       HashRingStreamQueueMapperOptions queueMapperOptions,
                                       SimpleQueueCacheOptions cacheOptions,
                                       IServiceProvider serviceProvider,
                                       IOptions<ClusterOptions> clusterOptions,
                                       Serialization.Serializer serializer,
                                       ILoggerFactory loggerFactory)
        {
            _name = name;
            _jetStream = jetStream;
            _serializer = serializer;
            _cacheOptions = cacheOptions;
            _loggerFactory = loggerFactory;
            _clusterOptions = clusterOptions;
            _serviceProvider = serviceProvider;
            _streamQueueMapper = new HashRingBasedStreamQueueMapper(queueMapperOptions, _name);
            _adapterCache = new SimpleQueueAdapterCache(cacheOptions, _name, _loggerFactory);
        }

        public static NatsQueueAdapterFactory Create(IServiceProvider services, string name)
        {
            var clusterOptions = services.GetProviderClusterOptions(name);
            var natsOptions = services.GetOptionsByName<NatsOptions>(name);
            var cacheOptions = services.GetOptionsByName<SimpleQueueCacheOptions>(name);
            var queueMapperOptions = services.GetOptionsByName<HashRingStreamQueueMapperOptions>(name);

            var cf = new ConnectionFactory();
            var qr = cf.CreateConnection(natsOptions.ConnectionString);
            var jetStream = qr.CreateJetStreamContext();

            return ActivatorUtilities.CreateInstance<NatsQueueAdapterFactory>(services, name, jetStream, queueMapperOptions, cacheOptions, services, clusterOptions);
        }

        public Task<IQueueAdapter> CreateAdapter()
        {
            var adapter = new NatsQueueAdapter(_serializer, _streamQueueMapper, _loggerFactory, _jetStream, _name);

            return Task.FromResult<IQueueAdapter>(adapter);
        }

        public Task<IStreamFailureHandler> GetDeliveryFailureHandler(QueueId queueId)
        {
            return Task.FromResult<IStreamFailureHandler>(new NoOpStreamDeliveryFailureHandler());
        }

        public IQueueAdapterCache GetQueueAdapterCache()
        {
            return _adapterCache;
        }

        public IStreamQueueMapper GetStreamQueueMapper()
        {
            return _streamQueueMapper;
        }
    }
}