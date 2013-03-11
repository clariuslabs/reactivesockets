namespace ReactiveSockets
{
    using System;
    using System.Linq;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using ReactiveSockets.Properties;

    public class TcpSocket : ISocket, IDisposable
    {
        private bool disposed;
        private TcpClient client;
        private ReaderWriterLockSlim syncLock = new ReaderWriterLockSlim();
        private CancellationTokenSource cancellation;

        private ISubject<byte> receiver = new Subject<byte>();
        private ISubject<byte> sender = new Subject<byte>();

        internal TcpSocket(TcpClient client)
        {
            Tracer.Log.TcpSocketCreated();
            Connect(client);
        }

        protected TcpSocket() { }

        public event EventHandler Connected = (sender, args) => { };
        public event EventHandler Disconnected = (sender, args) => { };
        public event EventHandler Disposed = (sender, args) => { };

        public bool IsConnected { get { return client != null && client.Connected; } }

        public IObservable<byte> Receiver { get { return receiver; } internal set { receiver = (ISubject<byte>)value; } }
        public IObservable<byte> Sender { get { return sender; } internal set { sender = (ISubject<byte>)value; } }

        protected void Connect(TcpClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            if (disposed)
            {
                Tracer.Log.TcpSocketReconnectDisposed();
                throw new ObjectDisposedException(this.ToString());
            }

            if (!client.Connected)
            {
                Tracer.Log.TcpSocketReceivedDisconnectedTcpClient();
                throw new InvalidOperationException("Client must be connected");
            }

            // We're connecting an already connected client.
            if (this.client == client && client.Connected)
            {
                Tracer.Log.TcpSocketAlreadyConnected();
                return;
            }

            // We're switching to a new client?
            if (this.client != null && this.client != client)
            {
                Tracer.Log.TcpSocketSwitchingUnderlyingClient();
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
                .Subscribe(b => receiver.OnNext(b), cancellation.Token);

            Connected(this, EventArgs.Empty);

            Tracer.Log.TcpSocketConnected();
        }

        protected void Disconnect()
        {
            if (!IsConnected)
                throw new InvalidOperationException(Strings.TcpClientSocket.DisconnectingNotConnected);

            Disconnect(false);
        }

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
                Tracer.Log.TcpSocketDisconnected();
            }

            client = null;

            Disconnected(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Disconnect(true);

            syncLock.Dispose();

            Tracer.Log.TcpSocketDisposed();

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
                        Tracer.Log.TcpSocketReadFailed(e);

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

        public Task SendAsync(byte[] bytes)
        {
            return SendAsync(bytes, CancellationToken.None);
        }

        public Task SendAsync(byte[] bytes, CancellationToken cancellation)
        {
            if (disposed)
            {
                Tracer.Log.TcpSocketSendDisposed();
                throw new ObjectDisposedException(this.ToString());
            }

            if (!IsConnected)
            {
                Tracer.Log.TcpSocketSendDisconnected();
                throw new InvalidOperationException("Not connected");
            }

            return Task.Run(() =>
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
    }
}
