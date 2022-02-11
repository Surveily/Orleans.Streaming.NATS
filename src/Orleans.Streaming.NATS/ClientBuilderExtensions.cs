// <copyright file="ClientBuilderExtensions.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

namespace Orleans.Streaming.NATS
{
    public static class ClientBuilderExtensions
    {
        public static IClientBuilder AddNatsStreams(this IClientBuilder builder, string name, Action<NatsOptions> configureOptions)
        {
            builder.AddNatsStreams(name, b => b.ConfigureNats(ob => ob.Configure(configureOptions)));
            return builder;
        }

        public static IClientBuilder AddNatsStreams(this IClientBuilder builder, string name, Action<ClusterClientNatsConfigurator> configure)
        {
            var configurator = new ClusterClientNatsConfigurator(name, builder);
            configure?.Invoke(configurator);
            return builder;
        }
    }
}