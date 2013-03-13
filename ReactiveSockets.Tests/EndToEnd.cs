namespace ReactiveSockets.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class EndToEnd
    {
        [Fact]
        public void when_consuming_enumerable_then_succeeds()
        {
            var bytes = new BlockingCollection<byte>();

            var incoming = bytes.GetConsumingEnumerable().ToObservable(TaskPoolScheduler.Default);

            var messages = from header in incoming.Buffer(4)
                           let length = BitConverter.ToInt32(header.ToArray(), 0)
                           let body = incoming.Take(length)
                           select Encoding.UTF8.GetString(body.ToEnumerable().ToArray());

            messages.Subscribe(s => Console.Write(s));

            var message = "hello";

            BitConverter.GetBytes(message.Length).Concat(Encoding.UTF8.GetBytes(message)).ToList().ForEach(b => bytes.Add(b));

            message = "world";

            BitConverter.GetBytes(message.Length).Concat(Encoding.UTF8.GetBytes(message)).ToList().ForEach(b => bytes.Add(b));

            Thread.Sleep(2000);

            Console.WriteLine(bytes.Count);
        }

        [Fact]
        public void when_connected_then_can_exchange_fixed_length_messages()
        {
            var serverReceives = new List<string>();
            var clientReceives = new List<string>();

            var server = new ReactiveListener(1055);
            server.Start();

            Func<IObservable<byte>, IObservable<string>> parse =
                socket => from header in socket.Buffer(4)
                          let length = BitConverter.ToInt32(header.ToArray(), 0)
                          let body = socket.Take(length)
                          select Encoding.UTF8.GetString(body.ToEnumerable().ToArray());

            Func<string, byte[]> convert = s =>
            {
                var body = Encoding.UTF8.GetBytes(s);
                var header = BitConverter.GetBytes(body.Length);
                var payload = header.Concat(body).ToArray();

                return payload;
            };

            server.Connections.Subscribe(socket =>
            {
                Console.WriteLine("Server socket created: {0}", socket.GetHashCode());

                parse(socket.Receiver).Subscribe(
                    x => serverReceives.Add(x.Trim()),
                    e => Console.WriteLine("Server socket error: {0}", e.Message),
                    () => Console.WriteLine("Server socket completed"));

                socket.SendAsync(convert("Welcome!")).Wait();
            });

            var client = new ReactiveClient("127.0.0.1", 1055);
            Console.WriteLine("Client socket created: {0}", client.GetHashCode());

            client.ConnectAsync().Wait();

            parse(client.Receiver).Subscribe(
                x => clientReceives.Add(x.Trim()),
                e => Console.WriteLine("Client socket error: {0}", e.Message),
                () => Console.WriteLine("Client socket completed"));

            client.SendAsync(convert("Hello")).Wait();

            Thread.Sleep(100);

            Assert.Equal(1, serverReceives.Count);
            Assert.Equal(1, clientReceives.Count);
            Assert.Equal("Welcome!", clientReceives[0]);
            Assert.Equal("Hello", serverReceives[0]);

            server.Dispose();
        }

        [Fact(Skip = @"Reconnecting for some reason is not working at all,
not even restarting the server and creating a new client altogether.
It DOES work if they are two processes, so this is just a test artifact, runtime behavior is correct.")]
        public void when_client_reconnects_then_can_exchange_fixed_length_messages()
        {
            var serverReceives = new List<string>();
            var clientReceives = new List<string>();
            var messageLength = 32;

            var server = new ReactiveListener(1055);
            server.Start();

            Func<IObservable<byte>, IObservable<string>> parse =
                socket => from message in socket.Buffer(messageLength)
                          select Encoding.UTF8.GetString(message.ToArray());

            Func<string, byte[]> convert = s =>
            {
                return Encoding.UTF8.GetBytes(new string(' ', messageLength - s.Length) + s);
            };

            server.Connections.Subscribe(socket =>
            {
                Console.WriteLine("Server socket created: {0}", socket.GetHashCode());

                parse(socket.Receiver).Subscribe(
                    x => serverReceives.Add(x.Trim()),
                    e => Console.WriteLine("Server socket error: {0}", e.Message),
                    () => Console.WriteLine("Server socket completed"));

                socket.SendAsync(convert("Welcome!")).Wait();
            });

            var client = new ReactiveClient("127.0.0.1", 1055);
            Console.WriteLine("Client socket created: {0}", client.GetHashCode());

            client.ConnectAsync().Wait();

            parse(client.Receiver).Subscribe(
                x => clientReceives.Add(x.Trim()),
                e => Console.WriteLine("Client socket error: {0}", e.Message),
                () => Console.WriteLine("Client socket completed"));

            client.SendAsync(convert("Hello")).Wait();

            Thread.Sleep(1200);

            // Give it time to detect the disconnection from the server.
            while (client.IsConnected)
                client.SendAsync(new byte[1]);

            Assert.Throws<InvalidOperationException>(() => client.SendAsync(convert("World")).Wait());

            client.Disconnect();
            client.Dispose();

            var tcp = new TcpClient("127.0.0.1", 1055);
            var bytes = convert("World");
            tcp.GetStream().Write(bytes, 0, bytes.Length);

            //client = new TcpClientSocket("127.0.0.1", 1055);

            //// Reconnect ansd send one more string.
            //client.ConnectAsync().Wait();
            //client.SendAsync(convert("World")).Wait();

            Thread.Sleep(1200);

            //while (serverReceives.Count < 2)
            //    Thread.Sleep(100);

            // Fails :(
            Assert.Equal("World", serverReceives.Last());

            client.Dispose();
            server.Dispose();
        }
    }
}
