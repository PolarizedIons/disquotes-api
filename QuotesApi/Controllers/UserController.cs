using System;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuotesApi.Models.Users;
using QuotesLib.Models;
using QuotesLib.Services;

namespace QuotesApi.Controllers
{
    [Route("user")]
    public class UserController : BaseController
    {
        private NatsUserService _natsUserService;

        public UserController(NatsUserService natsUserService)
        {
            _natsUserService = natsUserService;
        }

        /// <summary>
        /// Gets a user from their user id
        /// </summary>
        /// <param name="userId">The platform user id</param>
        /// <returns></returns>
        [
            HttpGet("{userId:guid}"),
            ProducesResponseType(typeof(ApiResult<UserDto>), 200),
            Authorize,
        ]
        public async Task<ApiResult<UserDto>> GetUserById([FromRoute] Guid userId)
        {
            var user = await _natsUserService.FindUser(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user.Adapt<UserDto>());
        }
    }
}
