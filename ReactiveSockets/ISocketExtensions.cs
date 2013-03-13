namespace ReactiveSockets
{
    using System.Net.Sockets;

    /// <summary>
    /// Provides easy configuration of socket options via properties.
    /// </summary>
    public static class ISocketExtensions
    {
        /// <summary>
        /// Retrieves the IP options available for the socket.
        /// </summary>
        public static ISocketOptions IpOptions(this ISocket socket)
        {
            return new IpSocketOptions(socket);
        }

        /// <summary>
        /// Exposes the well-known options that can be set for a socket.
        /// </summary>
        public interface ISocketOptions
        {
            /// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Net.Sockets.Socket" /> allows Internet Protocol (IP) datagrams to be fragmented.</summary>
            /// <returns>true if the <see cref="T:System.Net.Sockets.Socket" /> allows datagram fragmentation; otherwise, false. The default is true.</returns>
            bool DontFragment { get; set; }

            /// <returns>Returns <see cref="T:System.Boolean" />.</returns>
            bool DualMode { get; set; }

            /// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Net.Sockets.Socket" /> can send or receive broadcast packets.</summary>
            /// <returns>true if the <see cref="T:System.Net.Sockets.Socket" /> allows broadcast packets; otherwise, false. The default is false.</returns>
            bool EnableBroadcast { get; set; }

            /// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Net.Sockets.Socket" /> allows only one process to bind to a port.</summary>
            /// <returns>true if the <see cref="T:System.Net.Sockets.Socket" /> allows only one socket to bind to a specific port; otherwise, false. The default is true for Windows Server 2003 and Windows XP Service Pack 2, and false for all other versions.</returns>
            bool ExclusiveAddressUse { get; set; }

            /// <summary>Gets or sets a value that specifies whether the <see cref="T:System.Net.Sockets.Socket" /> will delay closing a socket in an attempt to send all pending data.</summary>
            /// <returns>A <see cref="T:System.Net.Sockets.LingerOption" /> that specifies how to linger while closing a socket.</returns>
            LingerOption LingerState { get; set; }

            /// <summary>Gets or sets a value that specifies whether outgoing multicast packets are delivered to the sending application.</summary>
            /// <returns>true if the <see cref="T:System.Net.Sockets.Socket" /> receives outgoing multicast packets; otherwise, false.</returns>
            bool MulticastLoopback { get; set; }

            /// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the stream <see cref="T:System.Net.Sockets.Socket" /> is using the Nagle algorithm.</summary>
            /// <returns>false if the <see cref="T:System.Net.Sockets.Socket" /> uses the Nagle algorithm; otherwise, true. The default is false.</returns>
            bool NoDelay { get; set; }

            /// <summary>Gets or sets a value that specifies the size of the receive buffer of the <see cref="T:System.Net.Sockets.Socket" />.</summary>
            /// <returns>An <see cref="T:System.Int32" /> that contains the size, in bytes, of the receive buffer. The default is 8192.</returns>
            int ReceiveBufferSize { get; set; }

            /// <summary>Gets or sets a value that specifies the amount of time after which a synchronous <see cref="T:System.Net.Sockets.Socket.Receive" /> call will time out.</summary>
            /// <returns>The time-out value, in milliseconds. The default value is 0, which indicates an infinite time-out period. Specifying -1 also indicates an infinite time-out period.</returns>
            int ReceiveTimeout { get; set; }

            /// <summary>Gets or sets a value that specifies the size of the send buffer of the <see cref="T:System.Net.Sockets.Socket" />.</summary>
            /// <returns>An <see cref="T:System.Int32" /> that contains the size, in bytes, of the send buffer. The default is 8192.</returns>
            int SendBufferSize { get; set; }

            /// <summary>Gets or sets a value that specifies the amount of time after which a synchronous <see cref="T:System.Net.Sockets.Socket.Send" /> call will time out.</summary>
            /// <returns>The time-out value, in milliseconds. If you set the property with a value between 1 and 499, the value will be changed to 500. The default value is 0, which indicates an infinite time-out period. Specifying -1 also indicates an infinite time-out period.</returns>
            int SendTimeout { get; set; }

            /// <summary>Gets or sets a value that specifies the Time To Live (TTL) value of Internet Protocol (IP) packets sent by the <see cref="T:System.Net.Sockets.Socket" />.</summary>
            /// <returns>The TTL value.</returns>
            short Ttl { get; set; }
        }

        private class IpSocketOptions : ISocketOptions
        {
            private ISocket socket;

            public IpSocketOptions(ISocket socket)
            {
                this.socket = socket;
            }

            public bool DontFragment
            {
                get
                {
                    return (int)socket.GetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment) != 0;
                }
                set
                {
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, value ? 1 : 0);
                }
            }

            public bool DualMode
            {
                get
                {
                    return (int)socket.GetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only) == 0;
                }
                set
                {
                    socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, value ? 0 : 1);
                }
            }

            public bool EnableBroadcast
            {
                get
                {
                    return (int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast) != 0;
                }
                set
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, value ? 1 : 0);
                }
            }

            public bool ExclusiveAddressUse
            {
                get
                {
                    return (int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse) != 0;
                }
                set
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, value ? 1 : 0);
                }
            }

            public LingerOption LingerState
            {
                get
                {
                    return (LingerOption)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger);
                }
                set
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, value);
                }
            }

            public bool MulticastLoopback
            {
                get
                {
                    return (int)socket.GetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback) != 0;
                }
                set
                {
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, value ? 1 : 0);
                }
            }

            public bool NoDelay
            {
                get
                {
                    return (int)socket.GetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug) != 0;
                }
                set
                {
                    socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.Debug, value ? 1 : 0);
                }
            }

            public int ReceiveBufferSize
            {
                get
                {
                    return (int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer);
                }
                set
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, value);
                }
            }

            public int ReceiveTimeout
            {
                get
                {
                    return (int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
                }
                set
                {
                    // Consistent with Socket.ReceiveTimeout behavior.
                    if (value == -1)
                        value = 0;

                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value);
                }
            }

            public int SendBufferSize
            {
                get
                {
                    return (int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer);
                }
                set
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, value);
                }
            }

            public int SendTimeout
            {
                get
                {
                    return (int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
                }
                set
                {
                    // Consistent with Socket.SendTimeout.
                    if (value == -1)
                        value = 0;

                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value);
                }
            }

            public short Ttl
            {
                get
                {
                    return (short)socket.GetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress);
                }
                set
                {
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, (int)value);
                }
            }
        }
    }
}
