namespace ReactiveProtocol
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Text;
    using System.Threading.Tasks;
    using ReactiveSockets;

    public class ProtocolClient
    {
        private Encoding encoding;
        private ISocket socket;

        public ProtocolClient(ISocket socket)
            : this(socket, Encoding.UTF8)
        {
        }

        public ProtocolClient(ISocket socket, Encoding encoding)
        {
            this.socket = socket;
            this.encoding = encoding;

            Receiver = from header in socket.Receiver.Buffer(4)
                       let length = BitConverter.ToInt32(header.ToArray(), 0)
                       let body = socket.Receiver.Take(length)
                       select Encoding.UTF8.GetString(body.ToEnumerable().ToArray());
        }

        public IObservable<string> Receiver { get; private set; }

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
    }
}
