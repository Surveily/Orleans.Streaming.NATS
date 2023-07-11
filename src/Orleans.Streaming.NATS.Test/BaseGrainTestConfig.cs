// <copyright file="BaseGrainTestConfig.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Streaming.NATS.Hosting;
using Orleans.Streaming.NATS.Streams;
using Orleans.TestingHost;
#pragma warning disable CS0618

namespace Orleans.Streaming.NATS.Test
{
    public abstract class BaseGrainTestConfig : ISiloConfigurator, IClientBuilderConfigurator
    {
        public abstract void Configure(IServiceCollection services);

        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.ConfigureServices(Configure)
                       .AddMemoryGrainStorageAsDefault()
                       .AddMemoryGrainStorage("PubSubStore")
                       .AddPersistentStreams("Default", NatsQueueAdapterFactory.Create, config => config.Configure<NatsOptions>(options =>
                       {
                           options.Configure(x => x.ConnectionString = "nats://nats:4222");
                       }));
        }

        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
            clientBuilder.AddPersistentStreams("Default", NatsQueueAdapterFactory.Create, config => config.Configure<NatsOptions>(options =>
                         {
                             options.Configure(x => x.ConnectionString = "nats://nats:4222");
                         }));
        }
    }
}
#pragma warning restore CS0618