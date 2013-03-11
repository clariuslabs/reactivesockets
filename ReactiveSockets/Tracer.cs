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

        public void TcpClientSocketCreated(string host, int port)
        {
            source.TraceInformation("TcpClientSocketCreated: {0}:{1}", host, port);
            //WriteEvent(1);
        }

        public void TcpSocketCreated()
        {
            source.TraceInformation("TcpSocketCreated");
            //WriteEvent(1);
        }

        public void TcpSocketConnected()
        {
            source.TraceInformation("TcpSocketConnected");
            //WriteEvent(2);
        }

        public void TcpSocketReconnectDisposed()
        {
            source.TraceEvent(TraceEventType.Error, 0, "Attempted to reconnect a disposed socket.");
            //WriteEvent(3);
        }

        public void TcpSocketSendDisposed()
        {
            source.TraceEvent(TraceEventType.Error, 0, "Attempted to send data over a disposed socket.");
            //WriteEvent(3);
        }

        public void TcpSocketSendDisconnected()
        {
            source.TraceEvent(TraceEventType.Error, 0, "Attempted to send data over a disconnected socket.");
            //WriteEvent(3);
        }

        public void TcpSocketReceivedDisconnectedTcpClient()
        {
            source.TraceEvent(TraceEventType.Error, 0, "Attempted to initialize the socket with a disconnected TCP client instance.");
        }

        public void TcpSocketAlreadyConnected()
        {
            source.TraceEvent(TraceEventType.Verbose, 0, "Initialized twice with the same instance of connected TCP client.");
        }

        public void TcpSocketSwitchingUnderlyingClient()
        {
            source.TraceInformation("Changing exisiting TCP client with a new one.");
        }

        public void TcpSocketDisconnected()
        {
            source.TraceInformation("Closed connected client.");
        }

        public void TcpSocketReadFailed(Exception e)
        {
            source.TraceEvent(TraceEventType.Warning, 0, "Read failed: {0}", e.Message);
        }

        public void TcpServerCreated(TcpServerSettings settings)
        {
            source.TraceInformation("TcpServer created with settings: {0}", settings);
        }

        public void TcpListenerStarted(int port)
        {
            source.TraceInformation("TCP server listener started on port {0}", port);
        }

        public void TcpServerAwaitingNewTcpConnection()
        {
            source.TraceInformation("Accepting new TCP clients asynchronously");
        }

        public void TcpSocketDisposed()
        {
            source.TraceInformation("Socket was disposed, removing from list of active connections");
        }

        public void TcpServerRemovingDisposedSocket()
        {
            source.TraceInformation("Socket was disposed, removing from list of active connections");
        }
    }
}
