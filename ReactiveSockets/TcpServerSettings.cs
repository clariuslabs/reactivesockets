
namespace ReactiveSockets
{
    public class TcpServerSettings
    {
        public TcpServerSettings(int port)
        {
            this.Port = port;
        }

        public int Port { get; private set; }

        public override string ToString()
        {
            return "Port: " + Port;
        }
    }
}
