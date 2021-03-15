using AutoMapper;
using Project.Core.Api.V1;
using Project.Core.Api.V1.RequestModels.Queries;
using Project.Core.Api.V1.ResponseModels;
using Project.Core.Domain;
using Project.Core.Helpers;
using Project.Core.Interfaces;
using Project.Portal.Api.V1.Filters;
using Project.Portal.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Portal.Controllers.Api.V1
{
    [Authorize(Policy = "PractitionerApiUserPolicy", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ServiceFilter(typeof(ValidatePractitionerAccessFilter))]
    public class ClientsController : ApiBaseController
    {
        private readonly ILogger<ClientsController> _logger;
        private IClientService _clientService;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;

        public ClientsController(IClientService clientService, 
            IMapper mapper, 
            ILogger<ClientsController> logger,
            IUriService uriService)
        {
            _clientService = clientService;
            _mapper = mapper;
            _logger = logger;
            _uriService = uriService;
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PagedResponseModel<ClientResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status401Unauthorized)]
        [HttpGet(ApiRoutes.Clients.GetAll)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllClientsQuery query, [FromQuery]PaginationQuery paginationQuery)
        {
            var accountId = (int)HttpContext.Items["accountId"];

            var paginationFilter = _mapper.Map<PaginationFilter>(paginationQuery);
            var filter = _mapper.Map<GetAllClientsFilter>(query);
            var clientSearchResult = await _clientService.GetClientListFromAccountIdAsync(accountId, filter, paginationFilter);
            var clientResponse = _mapper.Map<List<ClientResponseModel>>(clientSearchResult.results);

            if (paginationFilter == null || paginationFilter.PageNumber < 1 || paginationFilter.PageSize < 1)
            {
                return Ok(new PagedResponseModel<ClientResponseModel>(clientResponse));
            }

            var paginatedesponse = PaginationHelper.CreatePaginatedRespone(_uriService, paginationFilter, clientResponse, clientSearchResult.total);

            return Ok(paginatedesponse);
        }
    } 
}
