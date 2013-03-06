using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveSockets
{
    public class TcpServerSettings
    {
        public TcpServerSettings(int port)
        {
            this.Port = port;
        }

        public int Port { get; private set; }
    }
}
