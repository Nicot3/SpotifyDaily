namespace SpotifyDaily.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRouting();
        builder.Services.AddControllers();
        builder.Services.AddHostedService<Worker>();

        builder.ConfigureSpotifyDaily();

        var app = builder.Build();

        app.MapControllers();

        app.Run();
    }
}