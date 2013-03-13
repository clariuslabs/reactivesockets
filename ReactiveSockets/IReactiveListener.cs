namespace ReactiveSockets
{
    using System;
    
    /// <summary>
    /// Interface implemented by the reactive listeners which can 
    /// accept incoming connections.
    /// </summary>
    public interface IReactiveListener
    {
        /// <summary>
        /// Observable connections that are being accepted by the listener.
        /// </summary>
        IObservable<ReactiveSocket> Connections { get; }

        /// <summary>
        /// Disposes the listener, releasing all resources and closing 
        /// any active connections.
        /// </summary>
        void Dispose();
        
        /// <summary>
        /// Starts accepting connections.
        /// </summary>
        void Start();
    }
}
