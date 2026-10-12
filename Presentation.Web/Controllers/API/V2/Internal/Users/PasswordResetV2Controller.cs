using Core.ApplicationServices;
using Core.DomainModel;
using Presentation.Web.Infrastructure.Attributes;
using System;
using System.Net;
using Core.Abstractions.Types;
using Core.ApplicationServices.Users.Write;
using Presentation.Web.Models.API.V2.Internal.Request;
using Presentation.Web.Models.API.V2.Internal.Response;
using Core.Abstractions.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.ApplicationServices.ScheduledJobs;

namespace Presentation.Web.Controllers.API.V2.Internal.Users
{
    [AllowAnonymous]
    [AllowRightsHoldersAccess]
    [Route("api/v2/internal/users/password-reset")]
    public class PasswordResetInternalV2Controller : InternalApiV2Controller
    {
        private readonly IUserService _userService;
        private readonly IUserWriteService _userWriteService;
        private readonly IHangfireApi _hangfire;

        public PasswordResetInternalV2Controller(IUserService userService, IUserWriteService userWriteService, IHangfireApi hangfire)
        {
            _userService = userService;
            _userWriteService = userWriteService;
            _hangfire = hangfire;
        }

        [Route("create")]
        [HttpPost]
        [ApiResponse(HttpStatusCode.NoContent)]
        public IActionResult RequestPasswordReset([FromBody] RequestPasswordResetRequestDTO request)
        {
            _hangfire.Schedule(() => _userWriteService.RequestPasswordReset(request.Email));
            return NoContent();
        }

        [Route("{requestId}")]
        [HttpGet]
        [ApiResponse(typeof(PasswordResetResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetPasswordReset([FromRoute] string requestId)
        {
            try
            {
                var requestResult = _userService.GetPasswordReset(requestId).FromNullable();
                if (requestResult.IsNone) return NotFound();
                var response = MapPasswordResetToResponseDTO(requestResult.Value);
                return Ok(response);
            }
            catch
            {
                return UnknownError();
            }
        }

        [Route("{requestId}")]
        [HttpPost]
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        public IActionResult PostPasswordReset([FromRoute] string requestId, [FromBody] ResetPasswordRequestDTO request)
        {
            try
            {
                var resetRequest = _userService.GetPasswordReset(requestId);
                _userService.ResetPassword(resetRequest, request.Password);
                return NoContent();
            }
            catch
            {
                return UnknownError();
            }
        }

        private IActionResult UnknownError()
        {
            return FromOperationFailure(OperationFailure.UnknownError);
        }

        private PasswordResetResponseDTO MapPasswordResetToResponseDTO(PasswordResetRequest request)
        {
            return new PasswordResetResponseDTO
            {
                Email = request.User.Email
            };
        }
    }
}

