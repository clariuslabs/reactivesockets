namespace ReactiveSockets
{
    using System.Net.Sockets;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements the <see cref="IReactiveClient"/> over TCP.
    /// </summary>
    public class ReactiveClient : ReactiveSocket, IReactiveClient
    {
        private string hostname;
        private int port;

        /// <summary>
        /// Initializes the reactive client.
        /// </summary>
        /// <param name="hostname">The host name or IP address of the TCP server to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        public ReactiveClient(string hostname, int port)
        {
            this.hostname = hostname;
            this.port = port;
            Tracer.Log.ReactiveClientCreated(hostname, port);
        }

        /// <summary>
        /// Attemps to connect to the TCP server.
        /// </summary>
        public Task ConnectAsync()
        {
            var client = new TcpClient();
            return Task.Factory
                .FromAsync<string, int>(client.BeginConnect, client.EndConnect, hostname, port, null)
                .ContinueWith(_ => Connect(client), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// Disconnects the underlying TCP socket.
        /// </summary>
        public new void Disconnect()
        {
            base.Disconnect();
        }
    }
}
