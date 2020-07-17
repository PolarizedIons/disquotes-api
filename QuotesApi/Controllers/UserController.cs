using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuotesApi.Models;
using QuotesApi.Models.Users;
using QuotesApi.Services;

namespace QuotesApi.Controllers
{
    [Route("user")]
    public class UserController : BaseController
    {
        private UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Gets a user from their user id
        /// </summary>
        /// <param name="userId">The platform user id</param>
        /// <returns></returns>
        [
            HttpGet("{userId:guid}"),
            ProducesResponseType(typeof(ApiResult<User>), 200),
            Authorize,
        ]
        public async Task<ApiResult<User>> GetUserById([FromRoute] Guid userId)
        {
            var user = await _userService.FindUser(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }
    }
}
