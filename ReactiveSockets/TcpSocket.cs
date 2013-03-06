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

    public class TcpSocket : IDisposable
    {
        private bool disposed;
        private ReaderWriterLockSlim syncLock = new ReaderWriterLockSlim();
        private CancellationTokenSource readCancellation = new CancellationTokenSource();

        private Subject<byte> receiver = new Subject<byte>();
        private Subject<byte> sender = new Subject<byte>();

        internal TcpSocket(TcpClient client)
        {
            Initialize(client);
        }

        protected TcpSocket() { }

        public IObservable<byte> Receiver { get { return receiver; } }
        public IObservable<byte> Sender { get { return sender; } }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            TcpClient.Close();
            TcpClient = null;
            syncLock.Dispose();
            readCancellation.Cancel();
        }

        public Task SendAsync(byte[] bytes)
        {
            return SendAsync(bytes, CancellationToken.None);
        }

        public Task SendAsync(byte[] bytes, CancellationToken cancellation)
        {
            if (disposed)
                throw new ObjectDisposedException(this.ToString());
            if (TcpClient == null)
                throw new InvalidOperationException("Not connected");

            return Task.Run(() =>
            {
                syncLock.EnterWriteLock();
                try
                {
                    TcpClient.GetStream()
                        .WriteAsync(bytes, 0, bytes.Length, cancellation)
                        .Wait(cancellation);

                    foreach (var b in bytes)
                        sender.OnNext(b);
                }
                finally
                {
                    syncLock.ExitWriteLock();
                }
            }, cancellation);
        }

        protected TcpClient TcpClient { get; private set; }

        protected void Initialize(TcpClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            TcpClient = client;

            Observable.Create<byte>(obs => Task.Run(async () =>
            {
                while (!readCancellation.IsCancellationRequested)
                {
                    var buffer = new byte[1];
                    try
                    {
                        var count = await client.GetStream().ReadAsync(buffer, 0, 1, readCancellation.Token);
                        if (count != 0)
                            obs.OnNext(buffer[0]);
                    }
                    catch (Exception e)
                    {
                        if (!readCancellation.IsCancellationRequested)
                        {
                            // We cancel the reads on the first error.
                            readCancellation.Cancel();
                            obs.OnError(e);
                        }
                        else
                        {
                            // This is the scenario where the read error happened 
                            // because Close/Dispose was called, meaning we have 
                            // to signal the end for subscribers.
                            obs.OnCompleted();
                        }
                    }
                }

                return readCancellation;
            }))
            .Subscribe(b => receiver.OnNext(b), readCancellation.Token);
        }
    }
}
