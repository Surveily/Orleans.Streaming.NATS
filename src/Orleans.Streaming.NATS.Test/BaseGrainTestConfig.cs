// <copyright file="BaseGrainTestConfig.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Streaming.NATS.Test.Grains;
using Orleans.TestingHost;
#pragma warning disable CS0618

namespace Orleans.Streaming.NATS.Test
{
    public abstract class BaseGrainTestConfig : ISiloBuilderConfigurator, IClientBuilderConfigurator
    {
        protected string Id => $"{Guid.NewGuid()}";

        public abstract void Configure(HostBuilderContext host, IServiceCollection services);

        public abstract void Configure(HostBuilderContext host, IConfigurationBuilder configuration);

        public void Configure(ISiloHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(this.Configure)
                       .ConfigureAppConfiguration(this.Configure)
                       .Configure<ClusterOptions>(options =>
                       {
                           options.ClusterId = this.Id;
                           options.ServiceId = this.Id;
                       })
                       .Configure<SiloOptions>(options =>
                       {
                           options.SiloName = this.Id;
                       })
                       .Configure<ClientMessagingOptions>(options =>
                       {
                           options.LocalAddress = IPAddress.Parse("127.0.0.1");
                       })
                       .ConfigureEndpoints(IPAddress.Parse("127.0.0.1"), 22222, 30000, listenOnAnyHostAddress: true)
                       .AddMemoryGrainStorageAsDefault()
                       .AddMemoryGrainStorage("PubSubStore")
                       .AddMemoryStreams<DefaultMemoryMessageBodySerializer>("Default");
        }

        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
            clientBuilder.Configure<ClusterOptions>(options =>
                         {
                             options.ClusterId = this.Id;
                             options.ServiceId = this.Id;
                         })
                         .Configure<ClientMessagingOptions>(options =>
                         {
                             options.LocalAddress = IPAddress.Parse("127.0.0.1");
                         });
        }
    }
}
#pragma warning restore CS0618