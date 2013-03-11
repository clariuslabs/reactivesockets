namespace ReactiveSockets.Tests
{
    using System;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using Moq;
    using ReactiveProtocol;
    using ReactiveSockets;
    using Xunit;

    public class SampleProtocolTests
    {
        [Fact]
        public void when_parsing_bytes_then_raises_messages()
        {
            var subject = new Subject<byte>();
            var socket = Mock.Of<ISocket>(x => x.Receiver == subject);

            var protocol = new ProtocolClient(socket);
            var bytes = new Subject<byte>();
            var message = "";

            protocol.Receiver.Subscribe(s => message = s);

            protocol.Convert("Hello").ToList().ForEach(b => subject.OnNext(b));

            Thread.Sleep(1000);

            Assert.NotNull(message);
            Assert.Equal("Hello", message);
        }
    }
}
