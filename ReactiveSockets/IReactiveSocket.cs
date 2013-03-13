namespace ReactiveSockets
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Low level channel between client and server.
    /// </summary>
    public interface IReactiveSocket : ISocket, IDisposable
    {
        /// <summary>
        /// Raised when the socket is connected.
        /// </summary>
        event EventHandler Connected;

        /// <summary>
        /// Raised when the socket is disconnected.
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        /// Raised when the socket is disposed.
        /// </summary>
        event EventHandler Disposed;

        /// <summary>
        /// Gets whether the socket is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Observable bytes that are being received by this endpoint.
        /// </summary>
        IObservable<byte> Receiver { get; }

        /// <summary>
        /// Observable bytes that are being sent through this endpoint 
        /// by using the <see cref="SendAsync(byte[])"/> or 
        /// <see cref="SendAsync(byte[], CancellationToken)"/>  methods.
        /// </summary>
        IObservable<byte> Sender { get; }

        /// <summary>
        /// Sends data asynchronously through this endpoint.
        /// </summary>
        Task SendAsync(byte[] data);

        /// <summary>
        /// Sends data asynchronously through this endpoint, with support 
        /// for cancellation.
        /// </summary>
        Task SendAsync(byte[] bytes, CancellationToken cancellation);
    }
}
