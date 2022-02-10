// <copyright file="NatsQueueAdapterFactory.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client.JetStream;
using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Serialization;
using Orleans.Streams;

namespace Orleans.Streaming.NATS
{
    /// <summary>
    /// The factory for NATS queue adapters.
    /// </summary>
    public class NatsQueueAdapterFactory : IQueueAdapterFactory
    {
        private readonly string name;

        private readonly IJetStream jetStream;

        private readonly StorageType storageType;

        private readonly ILoggerFactory loggerFactory;

        private readonly IQueueAdapterCache adapterCache;

        private readonly IServiceProvider serviceProvider;

        private readonly SimpleQueueCacheOptions cacheOptions;

        private readonly IOptions<ClusterOptions> clusterOptions;

        private readonly SerializationManager serializationManager;

        private readonly HashRingBasedStreamQueueMapper streamQueueMapper;

        public NatsQueueAdapterFactory(string name, IJetStream jetStream, HashRingStreamQueueMapperOptions queueMapperOptions, SimpleQueueCacheOptions cacheOptions, IServiceProvider serviceProvider, IOptions<ClusterOptions> clusterOptions, SerializationManager serializationManager, ILoggerFactory loggerFactory, StorageType storageType)
        {
            this.name = name;
            this.jetStream = jetStream;
            this.storageType = storageType;
            this.cacheOptions = cacheOptions;
            this.loggerFactory = loggerFactory;
            this.clusterOptions = clusterOptions;
            this.serviceProvider = serviceProvider;
            this.serializationManager = serializationManager;
            this.streamQueueMapper = new HashRingBasedStreamQueueMapper(queueMapperOptions, this.name);
            this.adapterCache = new SimpleQueueAdapterCache(cacheOptions, this.name, this.loggerFactory);
        }

        public Task<IQueueAdapter> CreateAdapter()
        {
            var adapter = new NatsQueueAdapter(this.serializationManager, this.streamQueueMapper, this.loggerFactory, this.jetStream);

            return Task.FromResult<IQueueAdapter>(adapter);
        }

        public Task<IStreamFailureHandler> GetDeliveryFailureHandler(QueueId queueId)
        {
            return Task.FromResult<IStreamFailureHandler>(new NoOpStreamDeliveryFailureHandler());
        }

        public IQueueAdapterCache GetQueueAdapterCache()
        {
            return this.adapterCache;
        }

        public IStreamQueueMapper GetStreamQueueMapper()
        {
            return this.streamQueueMapper;
        }
    }
}