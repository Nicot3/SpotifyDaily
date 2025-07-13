namespace SpotifyDaily.Worker.Exceptions
{

	public class PlaylistServiceException : Exception
	{
		public PlaylistServiceException() { }
		public PlaylistServiceException(string message) : base(message) { }
		public PlaylistServiceException(string message, Exception inner) : base(message, inner) { }
	}
}
