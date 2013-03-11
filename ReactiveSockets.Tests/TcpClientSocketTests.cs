namespace ReactiveSockets.Tests
{
    using System;
    using System.Diagnostics;
    using Xunit;

    public class TcpClientSocketTests
    {
        [Fact]
        public void when_client_created_then_it_is_disconnected()
        {
            var client = new TcpClientSocket("127.0.0.1", 1055);

            Assert.False(client.IsConnected);
        }

        [Fact]
        public void when_disconnecting_disconnected_then_throws()
        {
            var client = new TcpClientSocket("127.0.0.1", 1055);

            Assert.Throws<InvalidOperationException>(() => client.Disconnect());
        }

        [Fact]
        public void when_connecting_then_raises_connected()
        {
            var server = new TcpServer(1055);
            var client = new TcpClientSocket("127.0.0.1", 1055);
            var connected = false;
            client.Connected += (sender, args) => connected = true;

            server.Start();
            client.ConnectAsync().Wait();

            Assert.True(client.IsConnected);
            Assert.True(connected);
        }

        [Fact]
        public void when_disconnecting_then_raises_disconnected()
        {
            var server = new TcpServer(1055);
            var client = new TcpClientSocket("127.0.0.1", 1055);
            server.Start();
            client.ConnectAsync().Wait();

            var disconnected = false;
            client.Disconnected += (sender, args) => disconnected = true;


            client.Disconnect();

            Assert.True(disconnected);
            Assert.False(client.IsConnected);
        }

        [Fact(Skip = "Does not work from tests.")]
        public void when_reconnecting_then_raises_connected()
        {
            // Server has to be on another process for the reconnect 
            // behavior to succeed in tests.
            var server = Process.Start(@".\..\..\..\Sample\ReactiveServer\bin\Debug\ReactiveServer.exe");
            try
            {
                var client = new TcpClientSocket("127.0.0.1", 1055);
                client.ConnectAsync().Wait();
                Assert.True(client.IsConnected);

                client.Disconnect();

                var connected = false;
                client.Connected += (sender, args) => connected = true;

                client.ConnectAsync().Wait();

                Assert.True(connected);
                Assert.False(client.IsConnected);
            }
            finally
            {
                server.Kill();
            }
        }
    }
}
