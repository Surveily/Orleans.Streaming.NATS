// <copyright file="OneToOne.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Orleans.Hosting;
using Orleans.Streaming.NATS.Test.Grains;
using Orleans.Streaming.NATS.Test.Messages;

namespace Orleans.Streaming.NATS.Test.Scenarios
{
    public class OneToOne
    {
        public class Config : BaseGrainTestConfig, IDisposable
        {
            private bool isDisposed;
            private Mock<IProcessor> processor = new Mock<IProcessor>();

            public override void Configure(HostBuilderContext host, IServiceCollection services)
            {
                services.AddSingleton(this.processor);
                services.AddSingleton(this.processor.Object);
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

        public abstract class BaseOneToOneTest : BaseGrainTest<Config>
        {
            protected Mock<IProcessor>? Processor { get; set; }

            public override void Prepare()
            {
                this.Processor = this.Container.GetService<Mock<IProcessor>>();

                base.Prepare();
            }
        }

        public class When_Sending_Simple_Message_One_To_One : BaseOneToOneTest
        {
            private string result;
            private string expected = "text";

            public override void Prepare()
            {
                base.Prepare();

                this.Processor!.Setup(x => x.Process(It.IsAny<string>()))
                               .Callback<string>(x => this.result = x);
            }

            public override async Task Act()
            {
                var grain = this.Subject.GrainFactory.GetGrain<IEmitterGrain>($"{1}/{Guid.NewGuid()}");

                await grain.SendAsync(this.expected);

                await this.WaitFor(() => this.result);
            }

            [Test]
            public void It_Should_Deliver_Text()
            {
                this.Processor!.Verify(x => x.Process(this.expected), Times.Once);
            }
        }

        public class When_Sending_Blob_Message_One_To_One : BaseOneToOneTest
        {
            private byte[] result;
            private byte[] expected = new byte[1024];

            public override void Prepare()
            {
                base.Prepare();

                this.Processor!.Setup(x => x.Process(It.IsAny<byte[]>()))
                               .Callback<byte[]>(x => this.result = x);
            }

            public override async Task Act()
            {
                var grain = this.Subject.GrainFactory.GetGrain<IEmitterGrain>($"{1}/{Guid.NewGuid()}");

                await grain.SendAsync(this.expected);

                await this.WaitFor(() => this.result);
            }

            [Test]
            public void It_Should_Deliver_Data()
            {
                this.Processor!.Verify(x => x.Process(this.expected), Times.Once);
            }
        }
    }
}