namespace ReactiveSockets
{
    using System;
    using System.Linq;
    using System.Net.Sockets;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;

    public class TcpClientSocket : TcpSocket
    {
        private string hostname;
        private int port;

        public TcpClientSocket(string hostname, int port)
        {
            this.hostname = hostname;
            this.port = port;
        }

        public bool IsConnected { get { return TcpClient != null && TcpClient.Connected; } }

        public void Connect()
        {
            if (TcpClient == null)
            {
                // ctor with credentials. Instantiating the TcpClient 
                // already attempts to connect it.
                Initialize(new TcpClient(hostname, port));
            }
        }
    }
}
