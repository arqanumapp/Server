using ArqanumServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ArqanumServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController(IContactService contactService) : ControllerBase
    {
        [HttpPost("find")]
        [EnableRateLimiting("find")]
        public async Task<IActionResult> FindContact()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-Signature", out var signatureHeader))
                    return BadRequest("Missing X-Signature header");

                var signatureBytes = Convert.FromBase64String(signatureHeader);

                using var ms = new MemoryStream();
                await Request.Body.CopyToAsync(ms);
                var rawData = ms.ToArray();

                var contact = await contactService.FindContactAsync(signatureBytes, rawData);
                return Ok(contact);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("add")]
        [EnableRateLimiting("add")]
        public async Task<IActionResult> AddContact()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-Signature", out var signatureHeader))
                    return BadRequest("Missing X-Signature header");

                var signatureBytes = Convert.FromBase64String(signatureHeader);

                using var ms = new MemoryStream();
                await Request.Body.CopyToAsync(ms);
                var rawData = ms.ToArray();

                var contact = await contactService.AddContactRequest(signatureBytes, rawData);
                return Ok(contact);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("confirm")]
        [EnableRateLimiting("confirm")]
        public async Task<IActionResult> ConfirmContact()
        {
            try
            {
                if (!Request.Headers.TryGetValue("X-Signature", out var signatureHeader))
                    return BadRequest("Missing X-Signature header");

                var signatureBytes = Convert.FromBase64String(signatureHeader);

                using var ms = new MemoryStream();
                await Request.Body.CopyToAsync(ms);
                var rawData = ms.ToArray();

                var contact = await contactService.AddContactRequest(signatureBytes, rawData);
                return Ok(contact);
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
