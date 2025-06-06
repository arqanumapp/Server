namespace ArqanumServer.Services.Validators
{
    public interface IHCaptchaValidator
    {
        Task<bool> VerifyAsync(string hcaptchaToken);
    }
    public class HCaptchaValidator(IConfiguration configuration) : IHCaptchaValidator
    {
        public async Task<bool> VerifyAsync(string hcaptchaToken)
        {
            try
            {
                var secret = configuration["hCaptcha:Token"];
                if (string.IsNullOrEmpty(secret))
                {
                    throw new Exception("hCaptcha secret is not configured.");
                }
                using var client = new HttpClient();
                var values = new Dictionary<string, string>
                {
                    { "secret", secret },
                    { "response", hcaptchaToken }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://hcaptcha.com/siteverify", content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic jsonResponse = JObject.Parse(responseString);
                return jsonResponse.success == "true";
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
