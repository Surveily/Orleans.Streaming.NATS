// <copyright file="BaseGrainTest.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Diagnostics;
using NUnit.Framework;
using Orleans.Runtime;
using Orleans.TestingHost;
using Polly;
using Polly.Retry;
#pragma warning disable CS0618

namespace Orleans.Streaming.NATS.Test
{
    public abstract class BaseGrainTest<T>
       where T : ISiloBuilderConfigurator, IClientBuilderConfigurator, new()
    {
        private readonly AsyncRetryPolicy retryPolicy;

#pragma warning disable CS8618
        public BaseGrainTest()
        {
            var builder = new TestClusterBuilder(1);

            builder.AddSiloBuilderConfigurator<T>();
            builder.AddClientBuilderConfigurator<T>();

            this.Subject = builder.Build();
            this.retryPolicy = Policy.Handle<OrleansMessageRejectionException>()
                                     .WaitAndRetryAsync(10, f => TimeSpan.FromSeconds(5));
        }
#pragma warning restore CS8618

        public T Config { get; }

        public TestCluster Subject { get; }

        public IServiceProvider Container
        {
            get
            {
                var siloHandle = this.Subject.Primary as Orleans.TestingHost.InProcessSiloHandle;

                if (siloHandle != null)
                {
                    return siloHandle.SiloHost.Services;
                }

                throw new InvalidOperationException("Subject.Primary is not a test host.");
            }
        }

        public virtual void Prepare()
        {
        }

        public abstract Task Act();

        [OneTimeSetUp]
        public async Task SetupAsync()
        {
            await this.retryPolicy.ExecuteAsync(async () =>
            {
                this.Subject.Deploy();

                await this.Subject.WaitForLivenessToStabilizeAsync();
            });

            this.Prepare();

            await this.Act();
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            await this.Subject.StopAllSilosAsync();
        }

        protected async Task WaitFor(Func<object> subject)
        {
            await this.WaitFor(subject, TimeSpan.FromSeconds(3));
        }

        protected async Task WaitFor(Func<object> subject, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                while (subject() == null)
                {
#if DEBUG
                    timeout = TimeSpan.FromMinutes(1);
#endif
                    if (sw.Elapsed > timeout)
                    {
                        throw new TimeoutException($"Timeout while waiting for subject.");
                    }

                    await Task.Delay(100);
                }
            }
            finally
            {
                sw.Stop();
            }
        }
    }
}
#pragma warning restore CS0618