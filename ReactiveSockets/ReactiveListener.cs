namespace ReactiveSockets
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Net = System.Net.Sockets;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;
    using System.Reactive.Disposables;
    using Diagnostics;

    /// <summary>
    /// Implements a TCP listener.
    /// </summary>
    public class ReactiveListener : IDisposable, IReactiveListener
    {
        private static readonly ITracer tracer = Tracer.Get<ReactiveListener>();

        // Active connections kept so we can close on dispose.
        private List<ReactiveSocket> connections = new List<ReactiveSocket>();
        // Notifies subscribers of new connections.
        private Subject<ReactiveSocket> observable = new Subject<ReactiveSocket>();
        // The underlying .NET TCP listener.
        private Net.TcpListener listener;
        // This is the subscription to the AcceptTcpClientAsync
        private IDisposable listenerSubscription;
        private int port;
        private bool disposed;
        private CompositeDisposable socketDisposable;

        /// <summary>
        /// Initializes the listener with the given port.
        /// </summary>
        public ReactiveListener(int port)
        {
            this.port = port;
            tracer.ReactiveListenerCreated(port);

            this.socketDisposable = new CompositeDisposable();
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
            this.socketDisposable.Dispose();
            connections.ForEach(socket => socket.Dispose());
            observable.OnCompleted();
        }

        /// <summary>
        /// Starts accepting connections.
        /// </summary>
        public void Start()
        {
            if (disposed)
                throw new ObjectDisposedException(this.ToString());

            // This is equivalent to the behavior of TcpListener.Create in .NET 4.5.
            listener = new Net.TcpListener(IPAddress.Any, port);
            //listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);

            listener.Start();

            tracer.ReactiveListenerStarted(port);

            listenerSubscription = Observable
                .FromAsync(() => 
                    {
                        tracer.ReactiveListenerAwaitingNewTcpConnection();
                        return Task.Factory.FromAsync<TcpClient>(listener.BeginAcceptTcpClient, listener.EndAcceptTcpClient, TaskCreationOptions.AttachedToParent);
                    })
                .Repeat()
                .Select(client => new ReactiveSocket(client))
                .Subscribe(socket =>
                {
                    connections.Add(socket);
                    observable.OnNext(socket);

                    IDisposable disposeSubscription = Observable.FromEventPattern<EventHandler, EventArgs>(
                            h => socket.Disposed += h, h => socket.Disposed -= h)
                        .FirstAsync().Subscribe(x =>
                        {
                            tracer.ReactiveListenerRemovingDisposedSocket();
                            connections.Remove(socket);
                        });

                    this.socketDisposable.Add(disposeSubscription);
                });
        }
    }
}
