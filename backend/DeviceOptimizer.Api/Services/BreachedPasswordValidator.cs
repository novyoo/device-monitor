using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using DeviceOptimizer.Api.Models;

namespace DeviceOptimizer.Api.Services
{
    public class BreachedPasswordValidator : IPasswordValidator<AppUser>
    {
        private readonly HttpClient _httpClient;

        public BreachedPasswordValidator(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string? password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return IdentityResult.Success;
            }

            var hash = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(password)));
            var prefix = hash[..5];
            var suffix = hash[5..];

            try
            {
                var response = await _httpClient.GetStringAsync($"https://api.pwnedpasswords.com/range/{prefix}");
                var isBreached = response
                    .Split('\n')
                    .Any(line => line.StartsWith(suffix, StringComparison.OrdinalIgnoreCase));

                if (isBreached)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "PasswordBreached",
                        Description = "That password has appeared in a known data breach. Please choose a different one."
                    });
                }
            }
            catch
            {
                return IdentityResult.Success;
            }

            return IdentityResult.Success;
        }
    }
}
