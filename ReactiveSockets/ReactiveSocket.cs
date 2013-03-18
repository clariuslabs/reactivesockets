namespace ReactiveSockets
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Net.Sockets;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using ReactiveSockets.Properties;

    /// <summary>
    /// Implements the reactive socket base class, which is used 
    /// on the <see cref="IReactiveListener"/> for accepted connections, 
    /// as well as a base class for the <see cref="ReactiveClient"/>.
    /// </summary>
    public class ReactiveSocket : IReactiveSocket, IDisposable
    {
        private bool disposed;
        private TcpClient client;
        // This allows us to write to the underlying socket in a 
        // single-threaded fashion.
        private ReaderWriterLockSlim syncLock = new ReaderWriterLockSlim();
        private CancellationTokenSource cancellation;

        // This allows protocols to be easily built by consuming 
        // bytes from the stream using Rx expressions.
        private BlockingCollection<byte> received = new BlockingCollection<byte>();

        // The receiver created from the above blocking collection.
        private IObservable<byte> receiver;
        // Subject used to pub/sub sent bytes.
        private ISubject<byte> sender = new Subject<byte>();

        /// <summary>
        /// Initializes the socket with a previously accepted TCP 
        /// client connection. This overload is used by the <see cref="ReactiveListener"/>.
        /// </summary>
        internal ReactiveSocket(TcpClient client)
            : this()
        {
            Tracer.Log.ReactiveSocketCreated();
            Connect(client);
        }

        /// <summary>
        /// Protected constructor used by <see cref="ReactiveClient"/> 
        /// client.
        /// </summary>
        protected internal ReactiveSocket() 
        {
            receiver = received.GetConsumingEnumerable().ToObservable(TaskPoolScheduler.Default);
        }

        /// <summary>
        /// Raised when the socket is connected.
        /// </summary>
        public event EventHandler Connected = (sender, args) => { };

        /// <summary>
        /// Raised when the socket is disconnected.
        /// </summary>
        public event EventHandler Disconnected = (sender, args) => { };

        /// <summary>
        /// Raised when the socket is disposed.
        /// </summary>
        public event EventHandler Disposed = (sender, args) => { };

        /// <summary>
        /// Gets whether the socket is connected.
        /// </summary>
        public bool IsConnected { get { return client != null && client.Connected; } }

        /// <summary>
        /// Observable bytes that are being received by this endpoint. Note that 
        /// subscribing to the receiver blocks until a byte is received, so 
        /// subscribers will typically use the extension method <c>SubscribeOn</c> 
        /// to specify the scheduler to use for subscription.
        /// </summary>
        /// <remarks>
        /// This blocking characteristic also propagates to higher level channels built 
        /// on top of this socket, but it's not necessary to use SubscribeOn 
        /// at more than one level.
        /// </remarks>
        public IObservable<byte> Receiver { get { return receiver; } }

        /// <summary>
        /// Observable bytes that are being sent through this endpoint 
        /// by using the <see cref="SendAsync(byte[])"/> or 
        /// <see cref="SendAsync(byte[], CancellationToken)"/>  methods.
        /// </summary>
        public IObservable<byte> Sender { get { return sender; } }

        /// <summary>
        /// Connects the reactive socket using the given TCP client.
        /// </summary>
        protected internal void Connect(TcpClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            if (disposed)
            {
                Tracer.Log.ReactiveSocketReconnectDisposed();
                throw new ObjectDisposedException(this.ToString());
            }

            if (!client.Connected)
            {
                Tracer.Log.ReactiveSocketReceivedDisconnectedTcpClient();
                throw new InvalidOperationException("Client must be connected");
            }

            // We're connecting an already connected client.
            if (this.client == client && client.Connected)
            {
                Tracer.Log.ReactiveSocketAlreadyConnected();
                return;
            }

            // We're switching to a new client?
            if (this.client != null && this.client != client)
            {
                Tracer.Log.ReactiveSocketSwitchingUnderlyingClient();
                Disconnect();
            }

            this.client = client;

            // Cancel possibly outgoing async work (i.e. reads).
            if (cancellation != null && !cancellation.IsCancellationRequested)
            {
                cancellation.Cancel();
                cancellation.Dispose();
            }

            cancellation = new CancellationTokenSource();

            // Subscribe to the new client with the new token.
            Observable
                .Create<byte>(obs => Read(client, obs, cancellation.Token))
                .Subscribe(b => received.Add(b), cancellation.Token);

            Connected(this, EventArgs.Empty);

            Tracer.Log.ReactiveSocketConnected();
        }

        /// <summary>
        /// Disconnects the reactive socket. Throws if not currently connected.
        /// </summary>
        protected void Disconnect()
        {
            if (!IsConnected)
                throw new InvalidOperationException(Strings.TcpClientSocket.DisconnectingNotConnected);

            Disconnect(false);
        }

        /// <summary>
        /// Disconnects the socket, specifying if this is being called 
        /// from Dispose.
        /// </summary>
        protected void Disconnect(bool disposing)
        {
            if (disposed && !disposing)
                throw new ObjectDisposedException(this.ToString());

            if (cancellation != null)
            {
                cancellation.Cancel();
                cancellation.Dispose();
            }

            if (IsConnected)
            {
                client.Close();
                Tracer.Log.ReactiveSocketDisconnected();
            }

            client = null;

            Disconnected(this, EventArgs.Empty);
        }

        /// <summary>
        /// Disconnects the socket and releases all resources.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Disconnect(true);

            syncLock.Dispose();

            Tracer.Log.ReactiveSocketDisposed();

            Disposed(this, EventArgs.Empty);
        }

        private async Task<IDisposable> Read(TcpClient client, IObserver<byte> obs, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var buffer = new byte[client.Available];
                    var count = await client.GetStream().ReadAsync(buffer, 0, buffer.Length, token);
                    foreach (var b in buffer.Take(count))
                        obs.OnNext(b);
                }
                catch (Exception e)
                {
                    if (!token.IsCancellationRequested)
                    {
                        Tracer.Log.ReactiveSocketReadFailed(e);

                        obs.OnError(e);
                        Disconnect(false);
                    }
                    // Token cancellation was requested: we have two 
                    // scenarios:
                    // - Dispose: requested the cancel as part of a Dispose.
                    //            this is truly the end of this instance.
                    // - Disconnect: requested cancel but we may reconnect.
                    else if (disposed)
                    {
                        obs.OnCompleted();
                    }
                }
            }

            return (IDisposable)cancellation;
        }

        /// <summary>
        /// Sends data asynchronously through this endpoint.
        /// </summary>
        public Task SendAsync(byte[] bytes)
        {
            return SendAsync(bytes, CancellationToken.None);
        }

        /// <summary>
        /// Sends data asynchronously through this endpoint, with support 
        /// for cancellation.
        /// </summary>
        public Task SendAsync(byte[] bytes, CancellationToken cancellation)
        {
            if (disposed)
            {
                Tracer.Log.ReactiveSocketSendDisposed();
                throw new ObjectDisposedException(this.ToString());
            }

            if (!IsConnected)
            {
                Tracer.Log.ReactiveSocketSendDisconnected();
                throw new InvalidOperationException("Not connected");
            }

            return Task.Factory.StartNew(() =>
            {
                syncLock.EnterWriteLock();
                try
                {
                    client.GetStream()
                        .WriteAsync(bytes, 0, bytes.Length, cancellation)
                        .Wait(cancellation);

                    foreach (var b in bytes)
                        sender.OnNext(b);
                }
                catch (Exception)
                {
                    Disconnect();
                    throw;
                }
                finally
                {
                    syncLock.ExitWriteLock();
                }
            }, cancellation);
        }

        #region SocketOptions

        /// <summary>See <see cref="T:System.Net.Sockets.Socket.GetSocketOption(SocketOptionLevel, SocketOptionName)" />.</summary>
        public object GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            return client.Client.GetSocketOption(optionLevel, optionName);
        }

        /// <summary>See <see cref="T:System.Net.Sockets.Socket.GetSocketOption(SocketOptionLevel, SocketOptionName, byte[])" />.</summary>
        public void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            client.Client.GetSocketOption(optionLevel, optionName, optionValue);
        }

        /// <summary>See <see cref="T:System.Net.Sockets.Socket.GetSocketOption(SocketOptionLevel, SocketOptionName, int)" />.</summary>
        public byte[] GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionLength)
        {
            return client.Client.GetSocketOption(optionLevel, optionName, optionLength);
        }

        /// <summary>See <see cref="T:System.Net.Sockets.Socket.SetSocketOption(SocketOptionLevel, SocketOptionName, bool)" />.</summary>
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
        {
            client.Client.SetSocketOption(optionLevel, optionName, optionValue);
        }

        /// <summary>See <see cref="T:System.Net.Sockets.Socket.SetSocketOption(SocketOptionLevel, SocketOptionName, byte[])" />.</summary>
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            client.Client.SetSocketOption(optionLevel, optionName, optionValue);
        }

        /// <summary>See <see cref="T:System.Net.Sockets.Socket.SetSocketOption(SocketOptionLevel, SocketOptionName, int)" />.</summary>
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            client.Client.SetSocketOption(optionLevel, optionName, optionValue);
        }

        /// <summary>See <see cref="T:System.Net.Sockets.Socket.SetSocketOption(SocketOptionLevel, SocketOptionName, object)" />.</summary>
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
        {
            client.Client.SetSocketOption(optionLevel, optionName, optionValue);
        }

        #endregion
    }
}
