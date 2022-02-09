// <copyright file="ManyToMany.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;

namespace Orleans.Streaming.NATS.Test.Scenarios
{
    public class ManyToMany
    {
        public class Config : BaseGrainTestConfig, IDisposable
        {
            private bool isDisposed;

            public override void Configure(HostBuilderContext host, IServiceCollection services)
            {
                /* dependency injection code here */
            }

            public override void Configure(HostBuilderContext host, IConfigurationBuilder configuration)
            {
                /* host configuration code here */
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!this.isDisposed)
                {
                    if (disposing)
                    {
                        /* dispose code here */
                    }

                    this.isDisposed = true;
                }
            }
        }
    }
}