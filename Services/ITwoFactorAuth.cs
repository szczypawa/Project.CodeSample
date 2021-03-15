using Project.Core.Data.Model;
using Project.Core.DTO.Account;
using GoogleAuthenticatorService.Core;
using System.Threading.Tasks;

namespace Project.Core.Interfaces
{
    public interface ITwoFactorAuth
    {
        Task<SetupCode> GetSetupCode(AccountDto account);
        Task<bool> ValidateCode(AccountDto account, string code);

        SetupCode GetSetupCode(AppUser appUser);
        bool ValidateCode(AppUser appUser, string code);
    }
}
