namespace SpotifyDaily.Worker.Helpers
{
    public static class DateTimeHelpers
    {
        public static TimeSpan CalculateNextRunDelay(this DateTime dateTime)
        {
            var nextRun = new DateTime(DateOnly.FromDateTime(dateTime).AddDays(1), TimeOnly.MinValue);
            return nextRun - dateTime;
        }
    }
}
