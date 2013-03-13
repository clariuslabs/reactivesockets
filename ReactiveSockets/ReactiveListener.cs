namespace ReactiveSockets
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Net = System.Net.Sockets;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    /// <summary>
    /// Implements a TCP listener.
    /// </summary>
    public class ReactiveListener : IDisposable, IReactiveListener
    {
        // Active connections kept so we can close on dispose.
        private List<ReactiveSocket> connections = new List<ReactiveSocket>();
        // Notifies subscribers of new connections.
        private Subject<ReactiveSocket> observable = new Subject<ReactiveSocket>();
        // The underlying .NET TCP listener.
        private Net.TcpListener listener;
        // This is the subscription to the AcceptTcpClientAsync
        private IDisposable listenerSubscription;
        private ReactiveListenerSettings settings;
        private bool disposed;

        /// <summary>
        /// Initializes the listener with the given port.
        /// </summary>
        public ReactiveListener(int port)
            : this(new ReactiveListenerSettings(port))
        {
        }

        /// <summary>
        /// Initializes the listener with the given settings.
        /// </summary>
        public ReactiveListener(ReactiveListenerSettings settings)
        {
            this.settings = settings;
            Tracer.Log.ReactiveListenerCreated(settings);
        }

        /// <summary>
        /// Observable connections that are being accepted by the listener.
        /// </summary>
        public IObservable<ReactiveSocket> Connections { get { return observable; } }

        /// <summary>
        /// Disposes the listener, releasing all resources and closing 
        /// any active connections.
        /// </summary>
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

        /// <summary>
        /// Starts accepting connections.
        /// </summary>
        public void Start()
        {
            if (disposed)
                throw new ObjectDisposedException(this.ToString());

            // This is equivalent to the behavior of TcpListener.Create in .NET 4.5.
            listener = new Net.TcpListener(IPAddress.IPv6Any, settings.Port);
            listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);

            listener.Start();

            Tracer.Log.ReactiveListenerStarted(settings.Port);

            listenerSubscription = Observable
                .FromAsync(() => 
                    {
                        Tracer.Log.ReactiveListenerAwaitingNewTcpConnection();
                        return listener.AcceptTcpClientAsync();
                    })
                .Repeat()
                .Select(client => new ReactiveSocket(client))
                .Subscribe(socket =>
                {
                    connections.Add(socket);
                    observable.OnNext(socket);

                    socket.Disposed += (sender, args) =>
                    {
                        Tracer.Log.ReactiveListenerRemovingDisposedSocket();
                        connections.Remove(socket);
                    };
                });
        }
    }
}
