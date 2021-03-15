using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Project.Core.Api.V1.ResponseModels;
using Project.Core.Data.Model;
using Project.Core.Domain;
using Project.Core.DTO.Account;

namespace Project.Core.Interfaces
{
    public interface IAppUserService
    {
        Task<AccountDto> GetAccountDto(string appUserId);
        Task<Account> GetAccount(string appUserId);
        Task<int> GetAccountId(string appUserId);
        Task CreatePersonal2FAuthSecret(AppUser appUser);
        Task FlagUserAssLoggedInVia2FAuth(AppUser appUser);
        Task<AppUserAccount> CreateAppUserAccountRelationAsync(int accountId, string appUserId);
    }
}
