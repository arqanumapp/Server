using ArqanumServer.Models.Dtos.Account;
using ArqanumServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ArqanumServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IAccountService accountService) : ControllerBase
    {
        [HttpPost("register")]
        [EnableRateLimiting("register")]
        public async Task<IActionResult> Register()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-Signature", out var signatureHeader))
                    return BadRequest("Missing X-Signature header");

                var signatureBytes = Convert.FromBase64String(signatureHeader);

                using var ms = new MemoryStream();
                await Request.Body.CopyToAsync(ms);
                var rawData = ms.ToArray();

                var (IsComplete, AvatarUrl) = await accountService.CreateAccountAsync(signatureBytes, rawData);

                if (!IsComplete)
                    return BadRequest("Invalid signature or data");

                return Ok(AvatarUrl);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost("username-available")]
        [EnableRateLimiting("username-available")]
        public async Task<IActionResult> UsernameAvailable([FromBody] UsernameAvailabilityRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username))
                    return BadRequest("Username is required.");

                var result = await accountService.IsUsernameTakenAsync(request.Username);

                return Ok(result);
            }
            catch
            {
                return BadRequest("An error occurred while checking username availability.");
            }
        }

    }
}
