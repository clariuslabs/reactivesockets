namespace ReactiveSockets
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class TcpServer : IDisposable
    {
        private List<TcpSocket> connections = new List<TcpSocket>();
        private Subject<TcpSocket> observable = new Subject<TcpSocket>();
        private TcpListener listener;
        private IDisposable listenerSubscription;
        private TcpServerSettings settings;
        private bool disposed;

        public TcpServer(int port)
            : this(new TcpServerSettings(port))
        {
        }

        public TcpServer(TcpServerSettings settings)
        {
            this.settings = settings;
        }

        public IObservable<TcpSocket> Connections { get { return observable; } }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            listener.Stop();
            listener = null;
            listenerSubscription.Dispose();
            connections.ForEach(socket => socket.Dispose());
        }

        public void Start()
        {
            if (disposed)
                throw new ObjectDisposedException(this.ToString());

            listener = TcpListener.Create(settings.Port);
            listener.Start();

            listenerSubscription = Observable
                .FromAsync(listener.AcceptTcpClientAsync)
                .Select(client => new TcpSocket(client))
                .Subscribe(socket =>
                {
                    connections.Add(socket);
                    observable.OnNext(socket);
                });
        }
    }
}
