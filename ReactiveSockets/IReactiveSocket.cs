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
        /// Observable bytes that are being received by this endpoint. Note that 
        /// subscribing to the receiver blocks until a byte is received, so 
        /// subscribers will typically use the extension method <c>SubscribeOn</c> 
        /// to specify the scheduler to use for subscription.
        /// </summary>
        /// <remarks>
        /// This blocking characteristic also propagates to higher level channels built 
        /// on top of this socket, but it's not necessary to use SubscribeOn 
        /// at more than one level.
        /// </remarks>
        IObservable<byte> Receiver { get; }

        /// <summary>
        /// Observable bytes that are being sent through this endpoint 
        /// by using the <see cref="SendAsync(byte[])"/> or 
        /// <see cref="SendAsync(byte[], CancellationToken)"/>  methods. 
        /// Non-blocking.
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
