using Microsoft.AspNetCore.Mvc;
using SpotifyDaily.Worker.Services.Contracts;

namespace SpotifyDaily.Worker.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TokenController(ISpotifyClientService spotifyClientService) : ControllerBase
    {
        [HttpGet("register")]
        public async Task<IActionResult> RegisterTokenAsync([FromQuery] string? code, CancellationToken cancellationToken = default)
        {
            if (code == null)
            {
                return BadRequest();
            }

            try
            {
                await spotifyClientService.ConfigureNewClientAsync(code, cancellationToken);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }
    }
}
