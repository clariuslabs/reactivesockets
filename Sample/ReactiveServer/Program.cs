namespace ReactiveServer
{
    using System;
    using System.Text;
    using System.Reactive;
    using System.Linq;
    using System.Reactive.Linq;
    using ReactiveProtocol;
    using ReactiveSockets;
    using System.Reactive.Concurrency;

    class Program
    {
        static void Main(string[] args)
        {
            var port = 1055;
            if (args.Length > 0)
                port = int.Parse(args[0]);

            var server = new ReactiveListener(port);

            server.Connections.Subscribe(socket =>
                {
                    Console.WriteLine("New socket connected {0}", socket.GetHashCode());

                    var protocol = new ProtocolClient(socket);

                    // Here we hook the "echo" prototocol
                    protocol.Receiver.Subscribe(
                        s => { Console.Write(s); protocol.SendAsync(s).Wait(); }, 
                        e => Console.WriteLine(e),
                        () => Console.WriteLine("Socket receiver completed"));

                    socket.Disconnected += (sender, e) => Console.WriteLine("Socket disconnected {0}", sender.GetHashCode());
                    socket.Disposed += (sender, e) => Console.WriteLine("Socket disposed {0}", sender.GetHashCode());
                });

            server.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }
    }
}
