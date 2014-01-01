namespace ReactiveSockets
{
    using Diagnostics;
    using System;

    internal static class TracerExtensions
    {
        public static void ReactiveClientCreated(this ITracer tracer, string host, int port)
        {
            tracer.Info("ReactiveClientCreated: {0}:{1}", host, port);
        }

        public static void ReactiveSocketCreated(this ITracer tracer)
        {
            tracer.Info("ReactiveSocketCreated");
        }

        public static void ReactiveSocketConnected(this ITracer tracer)
        {
            tracer.Info("ReactiveSocketConnected");
        }

        public static void ReactiveSocketReconnectDisposed(this ITracer tracer)
        {
            tracer.Error("Attempted to reconnect a disposed socket.");
        }

        public static void ReactiveSocketSendDisposed(this ITracer tracer)
        {
            tracer.Error("Attempted to send data over a disposed socket.");
        }

        public static void ReactiveSocketSendDisconnected(this ITracer tracer)
        {
            tracer.Error("Attempted to send data over a disconnected socket.");
        }

        public static void ReactiveSocketReceivedDisconnectedTcpClient(this ITracer tracer)
        {
            tracer.Error("Attempted to initialize the socket with a disconnected TCP client instance.");
        }

        public static void ReactiveSocketAlreadyConnected(this ITracer tracer)
        {
            tracer.Verbose("Initialized twice with the same instance of connected TCP client.");
        }

        public static void ReactiveSocketSwitchingUnderlyingClient(this ITracer tracer)
        {
            tracer.Info("Changing exisiting TCP client with a new one.");
        }

        public static void ReactiveSocketDisconnected(this ITracer tracer)
        {
            tracer.Info("Closed connected client.");
        }

        public static void ReactiveSocketReadFailed(this ITracer tracer, Exception e)
        {
            tracer.Warn("Read failed: {0}", e.Message);
        }

        public static void ReactiveListenerCreated(this ITracer tracer, int port)
        {
            tracer.Info("TCP server created for port {0}", port);
        }

        public static void ReactiveListenerStarted(this ITracer tracer, int port)
        {
            tracer.Info("TCP server listener started on port {0}", port);
        }

        public static void ReactiveListenerAwaitingNewTcpConnection(this ITracer tracer)
        {
            tracer.Info("Accepting new TCP clients asynchronously");
        }

        public static void ReactiveSocketDisposed(this ITracer tracer)
        {
            tracer.Info("Socket was disposed, removing from list of active connections");
        }

        public static void ReactiveListenerRemovingDisposedSocket(this ITracer tracer)
        {
            tracer.Info("Socket was disposed, removing from list of active connections");
        }
    }
}
