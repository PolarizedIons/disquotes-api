using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using QuotesApi.Models;
using QuotesApi.Models.Users;
using QuotesApi.Services;

namespace QuotesApi.Controllers
{
    [Route("security")]
    public class SecurityController : BaseController
    {
        private readonly UserService _userService;
        private readonly DiscordOAuthService _discordOAuthService;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _config;

        public SecurityController(UserService userService, DiscordOAuthService discordOAuthService, JwtService jwtService, IConfiguration config)
        {
            _userService = userService;
            _discordOAuthService = discordOAuthService;
            _jwtService = jwtService;
            _config = config;
        }
        
        /// <summary>
        /// Discord url for authentication
        /// </summary>
        /// <returns>The oauth url</returns>
        [
            HttpGet("oauth"),
            AllowAnonymous,
            ProducesResponseType(typeof(ApiResult<string>), 200),
        ]
        public async Task<ApiResult<string>> GetAuthUrl()
        {
            return Ok(_discordOAuthService.GetOAuth2Url());
        }

        /// <summary>
        /// Auth callback url for Discord login
        /// </summary>
        /// <param name="code">auth code provided by discord</param>
        /// <returns></returns>
        [
            HttpGet("callback"),
            AllowAnonymous,
            ProducesResponseType(typeof(ApiResult<object>), 401),
        ]
        public async Task<ActionResult> AuthCallback([FromQuery] string code)
        {
            var discordAuthToken = await _discordOAuthService.ExchangeCodeForAccessToken(code);
            if (discordAuthToken == null)
            {
                return (ApiResult<string>) Unauthorized("Unable to get access token!");
            }

            var discordUser = await _discordOAuthService.GetUserFromAuthToken(discordAuthToken);
            var user = await _userService.CreateOrUpdateUser(discordUser);
            var jwtToken = _jwtService.CreateTokenFor(user);
            return Redirect(_config["Discord:FrontendUrl"].Replace("{token}", jwtToken));
        }

        /// <summary>
        /// Get the current logged in user
        /// </summary>
        /// <returns></returns>
        [
            HttpGet("me"),
            Authorize,
            ProducesResponseType(typeof(ApiResult<User>), 200)
        ]
        public async Task<ApiResult<User>> GetMe()
        {
            return Ok(await _userService.FindUser(UserId));
        }
    }
}
