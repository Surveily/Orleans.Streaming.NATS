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

        /// <summary>
        /// Connection object.
        /// </summary>
        private readonly IJetStream jetStream;

        /// <summary>
        /// Storage type.
        /// </summary>
        private readonly StorageType storageType;

        /// <summary>
        /// Logger factory object.
        /// </summary>
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Adapter cache object.
        /// </summary>
        private readonly IQueueAdapterCache adapterCache;

        /// <summary>
        /// Service provider object.
        /// </summary>
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Cache options.
        /// </summary>
        private readonly SimpleQueueCacheOptions cacheOptions;

        /// <summary>
        /// Cluster options.
        /// </summary>
        private readonly IOptions<ClusterOptions> clusterOptions;

        /// <summary>
        /// Serialization manager object.
        /// </summary>
        private readonly SerializationManager serializationManager;

        /// <summary>
        /// Queue mapper options.
        /// </summary>
        private readonly HashRingBasedStreamQueueMapper streamQueueMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NatsQueueAdapterFactory"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="jetStream">Connection object.</param>
        /// <param name="queueMapperOptions">Queue mapper options.</param>
        /// <param name="cacheOptions">Cache options.</param>
        /// <param name="serviceProvider">Service provider object.</param>
        /// <param name="clusterOptions">Cluster options.</param>
        /// <param name="serializationManager">Serialization manager object.</param>
        /// <param name="loggerFactory">Logger factory object.</param>
        /// <param name="storageType">Storage type.</param>
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

        /// <summary>
        /// Create an adapter.
        /// </summary>
        /// <returns>An adapter.</returns>
        public Task<IQueueAdapter> CreateAdapter()
        {
            var adapter = new NatsQueueAdapter(this.serializationManager, this.streamQueueMapper, this.loggerFactory, this.jetStream);

            return Task.FromResult<IQueueAdapter>(adapter);
        }

        /// <summary>
        /// Get the failure handler.
        /// </summary>
        /// <param name="queueId">Identifier of the queue.</param>
        /// <returns>The object that handles failures.</returns>
        public Task<IStreamFailureHandler> GetDeliveryFailureHandler(QueueId queueId)
        {
            return Task.FromResult<IStreamFailureHandler>(new NoOpStreamDeliveryFailureHandler());
        }

        /// <summary>
        /// Get the cache of adapters.
        /// </summary>
        /// <returns>The cache of adapters.</returns>
        public IQueueAdapterCache GetQueueAdapterCache()
        {
            return this.adapterCache;
        }

        /// <summary>
        /// Get the stream queue mapper.
        /// </summary>
        /// <returns>The stream queue mapper.</returns>
        public IStreamQueueMapper GetStreamQueueMapper()
        {
            return this.streamQueueMapper;
        }
    }
}