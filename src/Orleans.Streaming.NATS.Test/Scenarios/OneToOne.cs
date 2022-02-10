// <copyright file="OneToOne.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Orleans.Hosting;
using Orleans.Streaming.NATS.Test.Grains;

namespace Orleans.Streaming.NATS.Test.Scenarios
{
    public class OneToOne
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

        public class When_Sending_Simple_Message_One_To_One : BaseGrainTest<Config>
        {
            public override void Prepare()
            {
                base.Prepare();
            }

            [Test]
            public override async Task Act()
            {
                var grain = this.Subject.GrainFactory.GetGrain<IEmitterGrain>($"{1}/{Guid.NewGuid()}");

                await grain.SendAsync("text");
            }
        }

        public class When_Sending_Blob_Message_One_To_One : BaseGrainTest<Config>
        {
            public override void Prepare()
            {
                base.Prepare();
            }

            [Test]
            public override async Task Act()
            {
                var grain = this.Subject.GrainFactory.GetGrain<IEmitterGrain>($"{1}/{Guid.NewGuid()}");

                await grain.SendAsync(new byte[1024]);
            }
        }
    }
}