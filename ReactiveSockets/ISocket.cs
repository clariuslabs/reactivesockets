namespace ReactiveSockets
{
    using System.Net.Sockets;

    /// <summary>
    /// Exposes the core SetSocketOption method from .NET sockets.
    /// </summary>
    public interface ISocket
    {
        /// <summary>See <see cref="T:System.Net.Sockets.Socket.GetSocketOption(SocketOptionLevel, SocketOptionName)" />.</summary>
        object GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName);

        /// <summary>See <see cref="T:System.Net.Sockets.Socket.GetSocketOption(SocketOptionLevel, SocketOptionName, byte[])" />.</summary>
        void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);

        /// <summary>See <see cref="T:System.Net.Sockets.Socket.GetSocketOption(SocketOptionLevel, SocketOptionName, int)" />.</summary>
        byte[] GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionLength);

        /// <summary>See <see cref="T:System.Net.Sockets.Socket.SetSocketOption(SocketOptionLevel, SocketOptionName, bool)" />.</summary>
        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue);

        /// <summary>See <see cref="T:System.Net.Sockets.Socket.SetSocketOption(SocketOptionLevel, SocketOptionName, byte[])" />.</summary>
        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);

        /// <summary>See <see cref="T:System.Net.Sockets.Socket.SetSocketOption(SocketOptionLevel, SocketOptionName, int)" />.</summary>
        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue);

        /// <summary>See <see cref="T:System.Net.Sockets.Socket.SetSocketOption(SocketOptionLevel, SocketOptionName, object)" />.</summary>
        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue);
    }
}
