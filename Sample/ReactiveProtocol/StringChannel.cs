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

    /// <summary>
    /// Implements a communication channel over a socket that 
    /// has a fixed length header and a variable length string 
    /// payload.
    /// </summary>
    public class StringChannel : IChannel<string>
    {
        private Encoding encoding;
        private IReactiveSocket socket;

        /// <summary>
        /// Initializes the channel with the given socket, using 
        /// the default UTF8 encoding for messages.
        /// </summary>
        public StringChannel(IReactiveSocket socket)
            : this(socket, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Initializes the channel with the given socket, using 
        /// the given encoding for messages.
        /// </summary>
        public StringChannel(IReactiveSocket socket, Encoding encoding)
        {
            this.socket = socket;
            this.encoding = encoding;

            Receiver = from header in socket.Receiver.Buffer(sizeof(int))
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
