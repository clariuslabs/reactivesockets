namespace ReactiveSockets
{
    using System;
    using System.Threading.Tasks;

    public interface ISocket
    {
        IObservable<byte> Receiver { get; }
        IObservable<byte> Sender { get; }
        Task SendAsync(byte[] data);
    }
}
