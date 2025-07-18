using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using SpotifyAPI.Web;

namespace SpotifyDaily.Tests.Worker.Attributes;

public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute() : base(() =>
        {
            var fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization
            {
                ConfigureMembers = true,
                GenerateDelegates = true
            });
            fixture.Customize<ISpotifyClient>(sb => sb.FromFactory(() => new SpotifyClient("token")));
            return fixture;
        })
    {
    }
}
