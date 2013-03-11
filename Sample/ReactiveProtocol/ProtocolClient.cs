namespace ReactiveProtocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Text;
    using System.Threading.Tasks;
    using ReactiveSockets;

    public class ProtocolClient : IDisposable
    {
        private Encoding encoding;
        private ISocket socket;
        private IDisposable socketSubscription;

        private bool isHeader = true;
        private int length;
        private List<byte> buffer = new List<byte>();
        private Subject<string> messages = new Subject<string>();

        public ProtocolClient(ISocket socket)
            : this(socket, Encoding.UTF8)
        {
        }

        public ProtocolClient(ISocket socket, Encoding encoding)
        {
            this.socket = socket;
            this.encoding = encoding;
            socketSubscription = socket.Receiver.Subscribe(
                Parse,
                e => messages.OnError(e),
                () => messages.OnCompleted());
        }

        public void Dispose()
        {
            socketSubscription.Dispose();
        }

        public IObservable<string> Receiver { get { return messages; } }

        public Task SendAsync(string message)
        {
            return socket.SendAsync(Convert(message));
        }

        internal byte[] Convert(string message)
        {
            var body = encoding.GetBytes(message);
            var header = BitConverter.GetBytes(body.Length);
            var payload = header.Concat(body).ToArray();

            return payload;
        }

        private void Parse(byte b)
        {
            buffer.Add(b);

            if (!isHeader && buffer.Count == length)
            {
                var message = encoding.GetString(buffer.ToArray());
                messages.OnNext(message);
                buffer.Clear();
                length = -1;
                isHeader = true;
            }
            else if (isHeader && buffer.Count == 4)
            {
                length = BitConverter.ToInt32(buffer.ToArray(), 0);
                buffer.Clear();
                isHeader = false;
            }
        }
    }
}
