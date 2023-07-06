// <copyright file="SiloNatsStreamConfigurator.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.Streams.Common;

namespace Orleans.Streaming.NATS.Streams
{
    public class SiloNatsStreamConfigurator : SiloPersistentStreamConfigurator
    {
        public SiloNatsStreamConfigurator(string name, Action<Action<IServiceCollection>> configureServicesDelegate)
            : base(name, configureServicesDelegate, NatsQueueAdapterFactory.Create)
        {
            ConfigureDelegate(services =>
            {
                services.ConfigureNamedOptionForLogging<NatsOptions>(name)
                        .ConfigureNamedOptionForLogging<SimpleQueueCacheOptions>(name)
                        .ConfigureNamedOptionForLogging<HashRingStreamQueueMapperOptions>(name);
            });
        }

        public SiloNatsStreamConfigurator ConfigureNats(Action<OptionsBuilder<NatsOptions>> configureOptions)
        {
            this.Configure(configureOptions);
            return this;
        }

        public SiloNatsStreamConfigurator ConfigureCache(int cacheSize = SimpleQueueCacheOptions.DEFAULT_CACHE_SIZE)
        {
            this.Configure<SimpleQueueCacheOptions>(ob => ob.Configure(options => options.CacheSize = cacheSize));
            return this;
        }

        public SiloNatsStreamConfigurator ConfigurePartitioning(int numOfparitions = HashRingStreamQueueMapperOptions.DEFAULT_NUM_QUEUES)
        {
            this.Configure<HashRingStreamQueueMapperOptions>(ob => ob.Configure(options => options.TotalQueueCount = numOfparitions));
            return this;
        }
    }

    public class ClusterClientNatsStreamConfigurator : ClusterClientPersistentStreamConfigurator
    {
        public ClusterClientNatsStreamConfigurator(string name, IClientBuilder builder)
            : base(name, builder, NatsQueueAdapterFactory.Create)
        {
            builder.ConfigureServices(services =>
                   {
                       services.ConfigureNamedOptionForLogging<NatsOptions>(name)
                               .ConfigureNamedOptionForLogging<HashRingStreamQueueMapperOptions>(name);
                   });
        }

        public ClusterClientNatsStreamConfigurator ConfigureNats(Action<OptionsBuilder<NatsOptions>> configureOptions)
        {
            this.Configure(configureOptions);
            return this;
        }

        public ClusterClientNatsStreamConfigurator ConfigurePartitioning(int numOfparitions = HashRingStreamQueueMapperOptions.DEFAULT_NUM_QUEUES)
        {
            this.Configure<HashRingStreamQueueMapperOptions>(ob => ob.Configure(options => options.TotalQueueCount = numOfparitions));
            return this;
        }
    }
}