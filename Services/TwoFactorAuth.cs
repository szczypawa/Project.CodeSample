using System;
using Project.Core.Interfaces;
using GoogleAuthenticatorService.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Project.Core.DTO.Account;
using System.Threading.Tasks;
using Project.Core.Helpers;
using Project.Core.Data.Model;

namespace Project.Core.Services
{
    public class TwoFactorAuth : ITwoFactorAuth
    {
        private readonly ILogger<TwoFactorAuth> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAccountService _accountService;

        public TwoFactorAuth(IAccountService accountService, IConfiguration configuration, ILogger<TwoFactorAuth> logger)
        {
            _accountService = accountService;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<SetupCode> GetSetupCode(AccountDto account) 
        {
            var Authenticator = new TwoFactorAuthenticator();
            return Authenticator.GenerateSetupCode(
                                    _configuration["TwoFactorAuth:Issuer"],
                                    account.Email,
                                    await _AccountSecretKey(account), 
                                    180, 
                                    180,
                                    true);
        }
        public async Task<bool> ValidateCode(AccountDto account, string code)
        {
            var Authenticator = new TwoFactorAuthenticator();
            return Authenticator.ValidateTwoFactorPIN(await _AccountSecretKey(account), code);
        }

        private async Task<string> _AccountSecretKey(AccountDto account) 
        {
            var acc = await _accountService.GetAccountById(account.Id);
            return StringHelper.CreateSHA256Hash(acc.Personal2FAuthSecret + _configuration["TwoFactorAuth:Secret"]);
        }

        public SetupCode GetSetupCode(AppUser appUser)
        {
            var Authenticator = new TwoFactorAuthenticator();
            return Authenticator.GenerateSetupCode(
                                    _configuration["TwoFactorAuth:Issuer"],
                                    appUser.Email,
                                    _AccountSecretKey(appUser),
                                    180,
                                    180,
                                    true);
        }
        public bool ValidateCode(AppUser appUser, string code)
        {
            var Authenticator = new TwoFactorAuthenticator();
            return Authenticator.ValidateTwoFactorPIN(_AccountSecretKey(appUser), code);
        }

        private string _AccountSecretKey(AppUser appUser)
        {
            return StringHelper.CreateSHA256Hash(appUser.Personal2FAuthSecret + _configuration["TwoFactorAuth:Secret"]);
        }
    }
}
