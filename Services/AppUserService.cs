using AutoMapper;
using Project.Core.Auth;
using Project.Core.Data.Model;
using Project.Core.DTO.Account;
using Project.Core.Helpers;
using Project.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;
using Account = Project.Core.Data.Model.Account;

namespace Project.Core.Services
{
    public class AppUserService : IAppUserService
    {
        private readonly ProjectDbContext _dbContext;
        private readonly IExceptionLogService _exceptionLogService;
        private readonly IMapper _mapper;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly JwtSettings _jwtSettings;
        private readonly IConfiguration _configuration;

        public AppUserService(ProjectDbContext dbContext, 
            IMapper mapper, 
            IExceptionLogService exceptionLogService, 
            TokenValidationParameters tokenValidationParameters,
              IJwtFactory jwtFactory, 
              IOptions<JwtIssuerOptions> jwtOptions,
              JwtSettings jwtSettings,
              IConfiguration configuration)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _exceptionLogService = exceptionLogService;
            _tokenValidationParameters = tokenValidationParameters;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
            _jwtSettings = jwtSettings;
            _configuration = configuration;
        }

        public async Task<AccountDto> GetAccountDto(string appUserId)
        {
            var account = await GetAccount(appUserId);
            return _mapper.Map<AccountDto>(account);
        }

        public async Task<Account> GetAccount(string appUserId) 
        {
            var appUserAccount = await _dbContext.AppUserAccount.Include(x => x.Account).FirstOrDefaultAsync(f => f.AppUserId.Equals(appUserId));
            if (appUserAccount == null) 
            {
                return null;
            }
            return appUserAccount.Account;
        }
        public async Task<int> GetAccountId(string appUserId)
        {
            var appUserAccount = await _dbContext.AppUserAccount.FirstOrDefaultAsync(f => f.AppUserId.Equals(appUserId));
            if (appUserAccount == null)
            {
                return -1;
            }
            return appUserAccount.AccountId;
        }

        public async Task CreatePersonal2FAuthSecret(AppUser appUser)
        {
            appUser.Personal2FAuthSecret = _GeneratePersonal2FAuthSecret();
            await _dbContext.SaveChangesAsync();
        }

        public async Task FlagUserAssLoggedInVia2FAuth(AppUser appUser)
        {
            appUser.LoggedInUsing2FAuth = true;
            await _dbContext.SaveChangesAsync();
        }

        private string _GeneratePersonal2FAuthSecret()
        {
            return StringHelper.CreatePBKDF2Hash(Guid.NewGuid().ToString("N"));
        }

        public async Task<AppUserAccount> CreateAppUserAccountRelationAsync(int accountId, string appUserId)
        {
            var appUserAccount = new AppUserAccount
            {
                AppUserId = appUserId,
                AccountId = accountId
            };

            var appUserAccountResult = await _dbContext.AppUserAccount.AddAsync(appUserAccount);
            await _dbContext.SaveChangesAsync();

            return appUserAccountResult.Entity;
        }
    }
}
