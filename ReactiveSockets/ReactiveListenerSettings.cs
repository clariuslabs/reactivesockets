namespace ReactiveSockets
{
    /// <summary>
    /// Basic listener settings.
    /// </summary>
    public class ReactiveListenerSettings
    {
        /// <summary>
        /// Initializes the listener settings with the given port.
        /// </summary>
        /// <param name="port">Port to listen on.</param>
        public ReactiveListenerSettings(int port)
        {
            this.Port = port;
        }

        /// <summary>
        /// Port to listen on.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Returns a human-readable representation of the settings.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Port: " + Port;
        }
    }
}
