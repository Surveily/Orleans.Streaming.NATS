// <copyright file="SiloBuilderExtensions.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Hosting;

namespace Orleans.Streaming.NATS
{
    public static class SiloBuilderExtensions
    {
        public static ISiloHostBuilder AddNatsStreams(this ISiloHostBuilder builder, string name, Action<NatsOptions> configureOptions)
        {
            builder.AddNatsStreams(name, b => b.ConfigureNats(ob => ob.Configure(configureOptions)));
            return builder;
        }

        public static ISiloBuilder AddNatsStreams(this ISiloBuilder builder, string name, Action<NatsOptions> configureOptions)
        {
            builder.AddNatsStreams(name, b => b.ConfigureNats(ob => ob.Configure(configureOptions)));
            return builder;
        }

        public static ISiloBuilder AddNatsStreams(this ISiloBuilder builder, string name, Action<NatsStreamConfigurator> configure)
        {
            var configurator = new NatsStreamConfigurator(name,
                configureServicesDelegate => builder.ConfigureServices(configureServicesDelegate),
                configureAppPartsDelegate => builder.ConfigureApplicationParts(configureAppPartsDelegate));
            configure?.Invoke(configurator);
            return builder;
        }

        public static ISiloHostBuilder AddNatsStreams(this ISiloHostBuilder builder, string name, Action<NatsStreamConfigurator> configure)
        {
            var configurator = new NatsStreamConfigurator(name,
                configureServicesDelegate => builder.ConfigureServices(configureServicesDelegate),
                configureAppPartsDelegate => builder.ConfigureApplicationParts(configureAppPartsDelegate));
            configure?.Invoke(configurator);
            return builder;
        }
    }
}