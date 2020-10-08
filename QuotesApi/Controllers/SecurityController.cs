using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using QuotesApi.Exceptions;
using QuotesApi.Models.Security;
using QuotesApi.Models.Users;
using QuotesApi.Services;
using QuotesLib.Models;
using QuotesLib.Models.Discord;
using QuotesLib.Models.Security;
using QuotesLib.Services;

namespace QuotesApi.Controllers
{
    [Route("security")]
    public class SecurityController : BaseController
    {
        private readonly NatsUserService _natsUserService;
        private readonly DiscordOAuthService _discordOAuthService;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _config;
        private readonly NatsDiscordService _natsDiscordService;

        public SecurityController(NatsUserService natsUserService, DiscordOAuthService discordOAuthService, NatsDiscordService natsDiscordService, JwtService jwtService, IConfiguration config)
        {
            _natsUserService = natsUserService;
            _discordOAuthService = discordOAuthService;
            _natsDiscordService = natsDiscordService;
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

            var discordUser = await _natsDiscordService.GetUserFromAuthToken(discordAuthToken);
            var user = await _natsUserService.LoginDiscordUser(discordUser.Adapt<MyIUser>());
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
            ProducesResponseType(typeof(ApiResult<UserDto>), 200)
        ]
        public async Task<ApiResult<UserDto>> GetMe()
        {
            var user = await _natsUserService.FindUser(UserId);
            return Ok(user.Adapt<UserDto>());
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
            var status = await _natsUserService.ValidateRefreshToken(refreshTokenDto.AccountId, refreshTokenDto.RefreshToken);
            if (status != RefreshTokenStatus.VALID)
            {
                throw new UnauthorizedException("Refresh token: " + status);
            }

            var user = await _natsUserService.UpdateRefreshToken(refreshTokenDto.AccountId);
            return Ok(new RefreshTokenResponse
            {
                AccessToken = _jwtService.CreateAccessTokenFor(user),
                RefreshToken = user.RefreshToken!.Value.ToString(),
                RefreshTokenExpires = user.RefreshTokenExpires!.Value,
            });
        }
    }
}
