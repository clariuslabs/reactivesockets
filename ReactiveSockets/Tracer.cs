namespace ReactiveSockets
{
    using System;
    using System.Diagnostics;

    internal sealed class Tracer // : EventSource // TODO: we'll turn it into a proper ETW source
    {
        internal TraceSource source;

        static Tracer()
        {
            Log = new Tracer();
        }

        public Tracer()
        {
            source = new TraceSource(typeof(Tracer).Namespace);
        }

        public static Tracer Log { get; private set; }

        public void ReactiveClientCreated(string host, int port)
        {
            source.TraceInformation("ReactiveClientCreated: {0}:{1}", host, port);
        }

        public void ReactiveSocketCreated()
        {
            source.TraceInformation("ReactiveSocketCreated");
        }

        public void ReactiveSocketConnected()
        {
            source.TraceInformation("ReactiveSocketConnected");
        }

        public void ReactiveSocketReconnectDisposed()
        {
            source.TraceEvent(TraceEventType.Error, 0, "Attempted to reconnect a disposed socket.");
        }

        public void ReactiveSocketSendDisposed()
        {
            source.TraceEvent(TraceEventType.Error, 0, "Attempted to send data over a disposed socket.");
        }

        public void ReactiveSocketSendDisconnected()
        {
            source.TraceEvent(TraceEventType.Error, 0, "Attempted to send data over a disconnected socket.");
        }

        public void ReactiveSocketReceivedDisconnectedTcpClient()
        {
            source.TraceEvent(TraceEventType.Error, 0, "Attempted to initialize the socket with a disconnected TCP client instance.");
        }

        public void ReactiveSocketAlreadyConnected()
        {
            source.TraceEvent(TraceEventType.Verbose, 0, "Initialized twice with the same instance of connected TCP client.");
        }

        public void ReactiveSocketSwitchingUnderlyingClient()
        {
            source.TraceInformation("Changing exisiting TCP client with a new one.");
        }

        public void ReactiveSocketDisconnected()
        {
            source.TraceInformation("Closed connected client.");
        }

        public void ReactiveSocketReadFailed(Exception e)
        {
            source.TraceEvent(TraceEventType.Warning, 0, "Read failed: {0}", e.Message);
        }

        public void ReactiveListenerCreated(int port)
        {
            source.TraceInformation("TCP server created for port {0}", port);
        }

        public void ReactiveListenerStarted(int port)
        {
            source.TraceInformation("TCP server listener started on port {0}", port);
        }

        public void ReactiveListenerAwaitingNewTcpConnection()
        {
            source.TraceInformation("Accepting new TCP clients asynchronously");
        }

        public void ReactiveSocketDisposed()
        {
            source.TraceInformation("Socket was disposed, removing from list of active connections");
        }

        public void ReactiveListenerRemovingDisposedSocket()
        {
            source.TraceInformation("Socket was disposed, removing from list of active connections");
        }
    }
}
