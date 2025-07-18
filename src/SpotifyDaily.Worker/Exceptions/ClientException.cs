namespace SpotifyDaily.Worker.Exceptions
{
    public class ClientException : Exception
    {
        public ClientException() : base()
        {
            
        }
        public ClientException(string message) : base(message)
        {
        }
        public ClientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
