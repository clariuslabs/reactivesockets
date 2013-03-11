namespace ReactiveSockets
{
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public class TcpClientSocket : TcpSocket
    {
        private string hostname;
        private int port;

        public TcpClientSocket(string hostname, int port)
        {
            this.hostname = hostname;
            this.port = port;
            Tracer.Log.TcpClientSocketCreated(hostname, port);
        }

        public async Task ConnectAsync()
        {
            var client = new TcpClient();
            await client.ConnectAsync(hostname, port);
            Connect(client);
        }

        public new void Disconnect()
        {
            base.Disconnect();
        }
    }
}
