using System;
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
        public ApiResult<User> GetUserById([FromRoute] Guid userId)
        {
            var user = _userService.FindUser(userId);
            return Ok(user);
        }
    }
}
