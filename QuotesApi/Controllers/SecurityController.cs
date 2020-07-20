using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using QuotesApi.Models;
using QuotesApi.Models.Security;
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
        public ApiResult<string> GetAuthUrl()
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
            ProducesResponseType(301),
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
            var user = await _userService.LoginDiscordUser(discordUser);
            var accessToken = _jwtService.CreateAccessTokenFor(user);
            return Redirect(_config["Discord:FrontendUrl"]
                .Replace("{access_token}", accessToken)
                .Replace("{refresh_token}", user.RefreshToken.ToString())
            );
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
            var user = await _userService.FindUser(UserId, true);
            return Ok(user);
        }

        /// <summary>
        /// Get a new Access and Refresh token from a valid Refresh Token.
        /// </summary>
        /// <param name="refreshTokenDto"></param>
        [
            HttpPost("refresh"),
            AllowAnonymous,
            ProducesResponseType(typeof(ApiResult<RefreshTokenResponse>), 200),
            ProducesResponseType(typeof(ApiResult<object>), 401)
        ]
        public async Task<ApiResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            _userService.ValidateRefreshToken(refreshTokenDto.AccountId, refreshTokenDto.RefreshToken);
            var user = await _userService.UpdateRefreshToken(refreshTokenDto.AccountId);
            return Ok(new RefreshTokenResponse
            {
                AccessToken = _jwtService.CreateAccessTokenFor(user),
                RefreshToken = user.RefreshToken.Value.ToString(),
                RefreshTokenExpires = user.RefreshTokenExpires.Value,
            });
        }
    }
}
