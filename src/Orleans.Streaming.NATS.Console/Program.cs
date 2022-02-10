// <copyright file="Program.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Text;
using Nats;
using NATS.Client;

if (args.Length > 0)
{
    Console.WriteLine($"arg: {args[0]}");
}

var stream = "TEST";
var subTasks = new List<Task>();
var cf = new ConnectionFactory();
var qr = new QueueRepository(cf.CreateConnection("nats://nats:4222"));

qr.Prepare(stream);

if (args.Length == 0 || args[0] == "s")
{
    subTasks.Add(Task.Run(() =>
    {
        var sub = qr.Subscribe(stream);

        while (true)
        {
            sub.PullNoWait(100);

            var list = sub.Fetch(1, (int)TimeSpan.FromSeconds(1).TotalMilliseconds);

            foreach (var msg in list)
            {
                var text = Encoding.Default.GetString(msg.Data);
                Console.WriteLine($"[{DateTime.UtcNow.Ticks}] {text}");
                msg.Ack();
            }
        }
    }));

    if (args.Length > 0 && args[0] == "s")
    {
        await Task.Delay(-1);
    }
}

if (args.Length == 0 || args[0] == "p")
{
    for (var i = 0; i < 10000; i++)
    {
        await qr.PublishAsync(stream, "Test Message");
    }
}

if (args.Length == 0)
{
    await Task.Delay(TimeSpan.FromSeconds(1));
}