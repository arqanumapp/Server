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

                var result = await accountService.IsUsernameTakenAsync(request);

                return Ok(result);
            }
            catch
            {
                return BadRequest("An error occurred while checking username availability.");
            }
        }

        [HttpPost("update-fullname")]
        [EnableRateLimiting("update-fullname")]
        public async Task<IActionResult> UpdateFullName()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-Signature", out var signatureHeader))
                    return BadRequest("Missing X-Signature header");

                var signatureBytes = Convert.FromBase64String(signatureHeader);

                using var ms = new MemoryStream();

                await Request.Body.CopyToAsync(ms);

                var rawData = ms.ToArray();

                var (Responce, IsComplete) = await accountService.UpdateFullNameAsync(signatureBytes, rawData);

                if (!IsComplete)
                    return BadRequest("Invalid signature or data");

                return Ok(Responce);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost("update-username")]
        [EnableRateLimiting("update-username")]
        public async Task<IActionResult> UpdateUsername()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-Signature", out var signatureHeader))
                    return BadRequest("Missing X-Signature header");

                var signatureBytes = Convert.FromBase64String(signatureHeader);

                using var ms = new MemoryStream();

                await Request.Body.CopyToAsync(ms);

                var rawData = ms.ToArray();

                var (Responce, IsComplete) = await accountService.UpdateUsernameAsync(signatureBytes, rawData);

                if (!IsComplete)
                    return BadRequest("Invalid signature or data");

                return Ok(Responce);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost("update-bio")]
        [EnableRateLimiting("update-bio")]
        public async Task<IActionResult> UpdateBio()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-Signature", out var signatureHeader))
                    return BadRequest("Missing X-Signature header");

                var signatureBytes = Convert.FromBase64String(signatureHeader);

                using var ms = new MemoryStream();

                await Request.Body.CopyToAsync(ms);

                var rawData = ms.ToArray();

                var (Responce, IsComplete) = await accountService.UpdateBioAsync(signatureBytes, rawData);

                if (!IsComplete)
                    return BadRequest("Invalid signature or data");

                return Ok(Responce);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost("update-avatar")]
        [EnableRateLimiting("update-avatar")]
        public async Task<IActionResult> UpdateAvatar()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-Signature", out var signatureHeader))
                    return BadRequest("Missing X-Signature header");

                var signatureBytes = Convert.FromBase64String(signatureHeader);

                using var ms = new MemoryStream();

                await Request.Body.CopyToAsync(ms);

                var rawData = ms.ToArray();

                var (Responce, IsComplete) = await accountService.UpdateAvatar(signatureBytes, rawData);

                if (!IsComplete)
                    return BadRequest("Invalid signature or data");

                return Ok(Responce);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
