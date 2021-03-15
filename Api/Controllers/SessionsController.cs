using AutoMapper;
using Project.Core.Api.V1;
using Project.Core.Api.V1.RequestModels;
using Project.Core.Api.V1.ResponseModels;
using Project.Core.Data.Model;
using Project.Core.Domain;
using Project.Core.DTO.Session;
using Project.Core.Interfaces;
using Project.Portal.Api.V1.Filters;
using Project.Portal.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Portal.Controllers.Api.V1
{
    [Authorize(Policy = "PractitionerApiUserPolicy", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ServiceFilter(typeof(ValidatePractitionerAccessFilter))]
    public class SessionsController : ApiBaseController
    {
        private readonly IMapper _mapper;
        private readonly ILogger<SessionsController> _logger;
        private readonly ISessionService _sessionService;
        private readonly IBodyImageBusinessService _bodyImageBusinessService;
        private readonly IClientService _clientService;
        private readonly IBodyImageSetService _bodyImageSetService;

        public SessionsController(
            IMapper mapper,
            ILogger<SessionsController> logger,
            IUriService uriService,
            ISessionService sessionService,
            IBodyImageBusinessService bodyImageBusinessService, 
            IClientService clientService,
            IBodyImageSetService bodyImageSetService)
        {
            _mapper = mapper;
            _logger = logger;
            _sessionService = sessionService;
            _bodyImageBusinessService = bodyImageBusinessService;
            _clientService = clientService;
            _bodyImageSetService = bodyImageSetService;
        }

        ///<summary>Returns the latest session with state "in progress" that has less body image sets than three.</summary>
        /// <remarks>
        /// Cases:<br />
        /// 1. There aren't any in progress sessions - the user cannot add photos and has to create a new session on the website or in the API.<br />
        /// 2. There are more than one in progress sessions - user cannot add photos and has to finish all older session leaving only the latest one as in progress in order to add more photos or finish all sessions and create a new one.<br />
        /// 3. There is one in progress session but it already has three body image sets - the user has to finish it and create a new one.<br />
        /// 4. There is one in progress session but it's not the latest one - the user has to finish it and create a new one.<br />
        /// 5. There is one in progress session with one body image set - user can add two more image sets.
        /// </remarks>
        [HttpPost(ApiRoutes.Sessions.GetLatestInProgress)]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SessionResponseModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SessionResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLatestInProgress([FromBody] SessionListRequestModel requestModel)
        {
            var accountId = (int)HttpContext.Items["accountId"];

            var msg = "";

            if (ModelState.IsValid)
            {
                try
                {
                    bool ownsClient = await _clientService.DoesPractitionerOwnsClient(accountId, requestModel.ClientId);
                    if (!ownsClient) 
                    {
                        msg = "You cannot access client with id " + requestModel.ClientId;

                        return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponseModel(msg));
                    }

                    var inProgressSessions = await _sessionService.GetSessionsInProgressAsync(requestModel.ClientId, accountId);

                    var sessionsCount = inProgressSessions.Count;
                    if (sessionsCount < 1)
                    {
                        msg = "Please create a new body image session.";

                        return NotFound(new ErrorResponseModel(msg));
                    }
                    else if (sessionsCount > 1)
                    {
                        msg = $"There are {sessionsCount} in progress sessions. " +
                            "Please finish all older sessions and leave in progress only the newest one in order to add more body images.";

                        return NotFound(new ErrorResponseModel(msg));
                    }
                    else if (sessionsCount == 1 && inProgressSessions.FirstOrDefault().BodyImageSets.Count() < 3)
                    {
                        var allSessions = await _sessionService.GetSessionListFromClientIdAsync(requestModel.ClientId, accountId);
                        if (inProgressSessions.FirstOrDefault().Id != allSessions.FirstOrDefault().Id)
                        {
                            msg = $"It seems that the latest session is finished while one of the preceding sessions is not. " +
                                                            "Please finish all in progress sessions and create a new one on the website to add more body images.";

                            return NotFound(new ErrorResponseModel(msg));
                        }
                    }
                    else if (sessionsCount == 1 && inProgressSessions.FirstOrDefault().BodyImageSets.Count() >= 3)
                    {
                        msg = "The last session has already three body image sets and you cannot add more. " +
                                "Please finish that session and create a new one on the website to add more body images.";

                        return NotFound(new ErrorResponseModel(msg));
                    }

                    var sessionResponse = _mapper.Map<SessionResponseModel>(inProgressSessions.FirstOrDefault());
                    return Ok(sessionResponse);
                }
                catch (Exception e)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                                      new ErrorResponseModel("Application error", e.Message, ModelState));
                }
            }

            return BadRequest(new ErrorResponseModel(ModelState));
        }

        /// <summary>Creates session</summary>
        /// <remarks>
        /// To add image sets to the existing session from mobile device please use Update API endpoint.
        /// Session can contain 3 body image sets.
        /// Each image set contains 4 body images.
        /// </remarks>
        /// <param name="requestModel">Max upload size of the request is 50MB. For image fields: imagedataFront, imagedataBack, imagedataLeft and imagedataRight send png encoded with Base64. Remember to remove 'data:image/png;base64,' in the begining of the encoded string.</param>
        [HttpPost(ApiRoutes.Sessions.Create)]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [RequestSizeLimit(100 * 1024 * 1024)]//50MB
        [Produces("application/json")]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SessionResponseModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateSessionRequestModel requestModel)
        {
            var accountId = (int)HttpContext.Items["accountId"];
            
            var msg = "";

            if (accountId < 1)
            {
                return Unauthorized(new ErrorResponseModel("You're not authorized"));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bool ownsClient = await _clientService.DoesPractitionerOwnsClient(accountId, requestModel.ClientId);
                    if (!ownsClient) 
                    {
                        msg = "You cannot access client with id " + requestModel.ClientId;

                        return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponseModel(msg));
                    }

                    var inProgressSessions = await _sessionService.GetSessionsInProgressAsync(requestModel.ClientId, accountId);

                    var sessionsCount = inProgressSessions.Count;
                    if (sessionsCount > 0)
                    {
                        msg = $"There are {sessionsCount} in progress sessions. " +
                            "Please finish all older sessions in order to create a new one.";

                        return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponseModel(msg));
                    }

                    var createSession = _mapper.Map<CreateSession>(requestModel);

                    var session = await _bodyImageBusinessService.SaveBodyImageSetAsync(accountId, createSession);
                    var sessionResponse = _mapper.Map<SessionResponseModel>(session);
                    return Created("", sessionResponse);
                }
                catch (Exception e)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                                      new ErrorResponseModel("Application error", e.Message, ModelState));
                }
            }

            return BadRequest(new ErrorResponseModel(ModelState));
        }

        /// <summary>
        /// Adds a Body Image Set to an existing session.
        /// </summary>
        /// <remarks>
        /// Session has to be in progress, can contain 3 body image sets.
        /// Each image set contains 4 body images.
        /// </remarks>
        /// <param name="requestModel">Max upload size of the request is 50MB. For image fields: imagedataFront, imagedataBack, imagedataLeft and imagedataRight send png encoded with Base64. Remember to remove 'data:image/png;base64,' in the begining of the encoded string.</param>
        [HttpPost(ApiRoutes.Sessions.Update)]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [RequestSizeLimit(100 * 1024 * 1024)]//50MB
        [Produces("application/json")]
        [ProducesResponseType(typeof(SessionResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromBody] UpdateSessionRequestModel requestModel)
        {
            var accountId = (int)HttpContext.Items["accountId"];
            
            string msg = "";

            if (ModelState.IsValid)
            {
                try
                {
                    SessionDto sessionDto = await _sessionService.GetSessionDtoById(requestModel.SessionId);

                    if (sessionDto == null)
                    {
                        msg = "Session with id " + requestModel.SessionId + " not found.";

                        return StatusCode(StatusCodes.Status404NotFound, new ErrorResponseModel(msg));
                    }

                    bool ownsClient = await _clientService.DoesPractitionerOwnsClient(accountId, sessionDto.ClientId);
                    if (!ownsClient)
                    {
                        msg = "You cannot access client's data";

                        return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponseModel(msg));
                    }

                    if (sessionDto.IsStatusFinished())
                    {
                        msg = "Session is finished, you cannot add more body images.";

                        return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponseModel(msg));
                    }

                    //if the code goes here after if (sessionDto.IsStatusFinished()) it means the session has in progress status

                    var inProgressSessions = await _sessionService.GetSessionsInProgressAsync(sessionDto.ClientId, accountId);

                    var sessionsCount = inProgressSessions.Count;
                    if (sessionsCount > 1)
                    {
                        msg = $"There are {sessionsCount} in progress sessions. " +
                            "Please finish all older sessions and leave in progress only the latest one in order to add more body images.";

                        return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponseModel(msg));
                    }
                    else if (sessionsCount == 1) 
                    {
                        var allSessions = await _sessionService.GetSessionListFromClientIdAsync(sessionDto.ClientId, accountId);
                        
                        if (inProgressSessions.FirstOrDefault().Id == sessionDto.Id && allSessions.FirstOrDefault().Id != inProgressSessions.FirstOrDefault().Id)  
                        {
                            msg = $"It seems that the chosen session is is not the latest in progress one. " +
                                                       "Please finish all in progress sessions except the latest one or create a new one on the website to add more body images.";

                            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponseModel(msg));
                        }

                        var bodyImageSets = await _bodyImageSetService.GetAllForSession(sessionDto.Id);

                        if (bodyImageSets.Count >= Session.MaxBodyImageSetsNo)
                        {
                            msg = "Adding 4th body image set to session is not allowed.";

                            return StatusCode(StatusCodes.Status422UnprocessableEntity, new ErrorResponseModel(msg));
                        }

                        var createSession = _mapper.Map<CreateSession>(requestModel);
                        createSession.HasAvatars = false;

                        var session = await _bodyImageBusinessService.SaveBodyImageSetAsync(accountId, createSession);
                        var sessionResponse = _mapper.Map<SessionResponseModel>(session);
                        return Ok(sessionResponse);
                    }

                    msg = "An error occured.";

                    return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponseModel(msg));
                }
                catch (Exception e)
                {
                    msg = "Application error";

                    return StatusCode(StatusCodes.Status500InternalServerError,
                                      new ErrorResponseModel(msg, e.Message, ModelState));
                }
            }

            return BadRequest(new ErrorResponseModel(ModelState));
        }
    }
}
