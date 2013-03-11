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
    using System.Reactive.Linq;
    using System.Reactive.Concurrency;
    using System.Collections.Concurrent;

    public class SampleProtocolTests
    {
        [Fact]
        public void when_parsing_bytes_then_raises_messages()
        {
            var bytes = new BlockingCollection<byte>();
            var socket = Mock.Of<ISocket>(x => x.Receiver == bytes.GetConsumingEnumerable().ToObservable(TaskPoolScheduler.Default));

            var protocol = new ProtocolClient(socket);
            var message = "";

            protocol.Receiver.SubscribeOn(TaskPoolScheduler.Default).Subscribe(s => message = s);

            protocol.Convert("Hello").ToList().ForEach(b => bytes.Add(b));

            Thread.Sleep(200);

            Assert.NotNull(message);
            Assert.Equal("Hello", message);
        }
    }
}
