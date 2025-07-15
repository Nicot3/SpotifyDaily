namespace SpotifyDaily.Worker.Exceptions;

	public class WorkerException : Exception
	{
		public WorkerException() { }
		public WorkerException(string message) : base(message) { }
		public WorkerException(string message, Exception inner) : base(message, inner) { }
	}
