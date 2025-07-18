namespace SpotifyDaily.Worker.Helpers
{
    public static class DateTimeHelpers
    {
        public static TimeSpan CalculateNextRunDelay(this DateTime dateTime, TimeOnly? time = null)
        {
            var nextRun = new DateTime(DateOnly.FromDateTime(dateTime).AddDays(1), time ?? TimeOnly.MinValue);
            return nextRun - dateTime;
        }
    }
}
