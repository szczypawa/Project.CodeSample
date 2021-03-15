using Project.Core.Api.V1;
using Project.Core.Api.V1.RequestModels;
using Project.Core.Api.V1.ResponseModels;
using Project.Core.Interfaces;
using Project.Core.Auth;
using Project.Shared.CustomExceptions.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Linq;

namespace Project.Portal.Controllers.Api.V1
{
    public class AuthController : ApiBaseController
    {
        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly ILogger<AuthController> _logger;
        private readonly IAccountService _accountService;
        private readonly ITokenManager _tokenManager;
        private readonly IIdentityService _identityService;

        public AuthController(IAccountService accountService, 
            IJwtFactory jwtFactory, 
            IOptions<JwtIssuerOptions> jwtOptions, 
            ILogger<AuthController> logger,
            ITokenManager tokenManager,
            IIdentityService identityService)
        {
            _accountService = accountService;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
            _tokenManager = tokenManager;
            _identityService = identityService;
        }

        /// <summary>
        /// Do not send authentication header to this endpoint.
        /// </summary>
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Produces("application/json")]
        [ProducesResponseType(typeof(AuthSuccessResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status401Unauthorized)]
        [HttpPost(ApiRoutes.Auth.Login)]
        public async Task<IActionResult> Post([FromBody] UserLoginRequestModel request)
        {
            var authResponse = await _identityService.LoginAsync(request.Email, request.Password);

            if (!authResponse.Success)
            {
                return Unauthorized(new ErrorResponseModel(authResponse.Errors));
            }

            return Ok(new AuthSuccessResponseModel
            {
                authToken = authResponse.AuthToken,
                refreshToken = authResponse.RefreshToken
            });
        }

        /// <summary>
        /// Do not send authentication header to this endpoint.
        /// </summary>
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [HttpPost(ApiRoutes.Auth.Refresh)]
        [ProducesResponseType(typeof(AuthSuccessResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestModel request)
        {
            var authResponse = await _identityService.RefreshTokenAsync(request.authToken, request.RefreshToken);

            if (!authResponse.Success)
            {
                if (authResponse.Errors.FirstOrDefault(w => w.Contains("This token hasn't expired yet")) != null)
                {
                    return NoContent();
                }
                return Unauthorized(new ErrorResponseModel(authResponse.Errors));
            }

            await DeactivateCurrentTokenAsync();

            return Ok(new AuthSuccessResponseModel
            {
                authToken = authResponse.AuthToken,
                refreshToken = authResponse.RefreshToken
            });
        }

        [Authorize(Policy = "PractitionerApiUserPolicy", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost(ApiRoutes.Auth.Logout)]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelAuthToken()
        {
            await DeactivateCurrentTokenAsync();

            return Ok();
        }

        private async Task DeactivateCurrentTokenAsync()
        {
            await _tokenManager.DeactivateCurrentAsync();
        }
    }
}
