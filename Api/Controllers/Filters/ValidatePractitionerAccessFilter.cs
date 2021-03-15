using Project.Core.Data.Model;
using Project.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Project.Portal.Extensions;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Api.V1.ResponseModels;

namespace Project.Portal.Api.V1.Filters
{
    public class ValidatePractitionerAccessFilter : IAsyncActionFilter
    {
        private readonly IAppUserService _appUserService;
        private readonly UserManager<AppUser> _userManager;

        public ValidatePractitionerAccessFilter(
            IAppUserService appUserService,
            UserManager<AppUser> userManager)
        {
            _appUserService = appUserService;
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var appUserId = context.HttpContext.User.AppUserId();

            AppUser appUser = await _userManager.FindByIdAsync(appUserId);
            if (appUser == null)
            {
                context.Result = new UnauthorizedObjectResult(new ErrorResponseModel("You're not authorized"));
                return;
            }

            var accountId = await _appUserService.GetAccountId(appUserId);

            if (accountId < 1)
            {
                context.Result = new UnauthorizedObjectResult(new ErrorResponseModel("You're not authorized"));
                return;
            }

            context.HttpContext.Items.Add("accountId", accountId);

            var result = await next();
        }
    }
}
