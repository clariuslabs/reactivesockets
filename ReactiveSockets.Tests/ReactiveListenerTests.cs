using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ReactiveSockets.Tests
{
    public class ReactiveListenerTests
    {
        [Fact]
        public void when_disposed_then_complete_connections_observable()
        {
            var listener = new ReactiveListener(1055);
            listener.Start();

            bool completed = false;

            listener.Connections.Subscribe(x => { }, () => completed = true);

            listener.Dispose();

            Assert.True(completed);
        }
    }
}
