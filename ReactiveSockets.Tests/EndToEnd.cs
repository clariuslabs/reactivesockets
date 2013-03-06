namespace ReactiveSockets.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class EndToEnd
    {
        [Fact]
        public void when_writing_then_can_receive_complete_notification()
        {
            var server = new TcpServer(1055);
            server.Start();

            // NOTE: it's THIS easy to implement a protocol ;)
            Func<IObservable<byte>, IObservable<string>> parse =
                socket => from header in socket.Buffer(4)
                          let length = BitConverter.ToInt32(header.ToArray(), 0)
                          let message = socket.Take(length)
                          select Encoding.UTF8.GetString(message.ToEnumerable().ToArray());
            
            Func<string, byte[]> convert = s =>
                {
                    var bytes = Encoding.UTF8.GetBytes(s);
                    // NOTE: I'm doing something stupid here.
                    return BitConverter.GetBytes(bytes.Length).Concat(bytes).ToArray();
                };

            server.Connections.Subscribe(socket =>
            {
                Console.WriteLine("Server socket created: {0}", socket.GetHashCode());

                socket.Receiver.Subscribe(b => Console.WriteLine("Server received byte {0}", b));
                socket.Sender.Subscribe(b => Console.WriteLine("Server sent byte {0}", b));

                parse(socket.Receiver).Subscribe(
                    x => Console.WriteLine("Server received message: " + x),
                    e => Console.WriteLine("Server socket error: {0}", e.Message),
                    () => Console.WriteLine("Server socket completed"));

                socket.SendAsync(convert("Welcome!")).Wait();
            });

            var client = new TcpClientSocket("127.0.0.1", 1055);
            Console.WriteLine("Client socket created: {0}", client.GetHashCode());

            client.Connect();

            client.Receiver.Subscribe(b => Console.WriteLine("Client received byte {0}", b));
            client.Sender.Subscribe(b => Console.WriteLine("Client sent byte {0}", b));

            parse(client.Receiver).Subscribe(
                x => Console.WriteLine("Client received message: {0}", x),
                e => Console.WriteLine("Client socket error: {0}", e.Message),
                () => Console.WriteLine("Client socket completed"));

            client.SendAsync(convert("Hello")).Wait();

            Thread.Sleep(1000);

            server.Dispose();

            Thread.Sleep(5000);
            Thread.Sleep(200);

            client.SendAsync(convert("World"))
                .ContinueWith(t => Assert.False(true, "Should have failed to write"), TaskContinuationOptions.OnlyOnRanToCompletion)
                .ContinueWith(t => Assert.True(true), TaskContinuationOptions.OnlyOnCanceled)
                .Wait();

            Thread.Sleep(200);

            client.Dispose();
        }

        [Fact]
        public void when_connected_then_can_exchange_fixed_length_messages()
        {
            var serverReceives = new List<string>();
            var clientReceives = new List<string>();
            var messageLength = 32;

            var server = new TcpServer(1055);
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

            var client = new TcpClientSocket("127.0.0.1", 1055);
            Console.WriteLine("Client socket created: {0}", client.GetHashCode());

            client.Connect();

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

            Thread.Sleep(200);

            client.SendAsync(convert("World"))
                .ContinueWith(t => Assert.False(true, "Should have failed to write"), TaskContinuationOptions.OnlyOnRanToCompletion)
                .ContinueWith(t => Assert.True(true), TaskContinuationOptions.OnlyOnCanceled)
                .Wait();

            client.Dispose();

            Thread.Sleep(1000);
        }
    }
}
