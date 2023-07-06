// <copyright file="BaseGrainTestConfig.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Streaming.NATS.Hosting;
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
                       .AddNatsStreams("Default", configureOptions: options =>
                       {
                           options.ConnectionString = "nats://nats:4222";
                       });
        }

        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
            clientBuilder.AddNatsStreams("Default", configureOptions: options =>
                       {
                           options.ConnectionString = "nats://nats:4222";
                       });
        }
    }
}
#pragma warning restore CS0618