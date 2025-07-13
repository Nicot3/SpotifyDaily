namespace SpotifyDaily.Worker.Exceptions
{
    public class ClientTokenExpiredException : Exception
    {
        public ClientTokenExpiredException() : base()
        {
            
        }

        public ClientTokenExpiredException(string message) : base(message)
        {

        }
    }
}
