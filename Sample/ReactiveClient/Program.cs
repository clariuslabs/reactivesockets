namespace ReactiveClient
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using ReactiveProtocol;
    using ReactiveSockets;

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                    throw new ArgumentException("Usage: reactiveclient host [port]");

                var host = args[0];
                var port = 1055;

                if (args.Length > 1)
                    port = int.Parse(args[1]);

                var client = new ReactiveClient(host, port);
                var protocol = new StringChannel(client);

                protocol.Receiver.SubscribeOn(TaskPoolScheduler.Default).Subscribe(
                    s => Console.Write(s),
                    e => Console.WriteLine(e),
                    () => Console.WriteLine("Socket receiver completed"));

                client.ConnectAsync().Wait();

                string line = null;

                while ((line = Console.ReadLine()) != "")
                {
                    if (line == "r")
                    {
                        Console.WriteLine("Reconnecting...");
                        client.Disconnect();
                        client.ConnectAsync().Wait();
                        Console.WriteLine("IsConnected = {0}", client.IsConnected);
                    }
                    else
                    {
                        Console.WriteLine("Sending");
                        protocol.SendAsync(line);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed: {0}", e);
            }
        }
    }
}
