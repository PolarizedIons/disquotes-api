using System.Collections.Generic;
using System.Threading.Tasks;
using QuotesApi.Models.Users;
using QuotesCore.Services;
using QuotesLib.Models;
using QuotesLib.Models.Security;
using QuotesLib.Nats;
using QuotesLib.Nats.Users;
using QuotesLib.Services;

namespace QuotesCore.NatsResponders
{
    public class UserResponder : NatsResponder, ISingletonDiService
    {
        private readonly UserService _userService;

        public UserResponder(UserService userService, NatsService natsService) : base(natsService)
        {
            _userService = userService;
        }

        public Task<IEnumerable<User>> OnFindAllUsers(FindAllUsersRequest req)
        {
            return _userService.FindAllUsers();
        }
        
        public Task<User> OnFindUser(FindUserRequest req)
        {
            return _userService.FindUser(req.UserId);
        }

        public Task<User> OnFindDiscordUser(FindDiscordUserRequest req)
        {
            return _userService.FindDiscordUser(req.UserId);
        }

        public Task<User> OnLoginDiscordUser(LoginDiscordUserRequest req)
        {
            return _userService.LoginDiscordUser(req.User);
        }

        public Task<RefreshTokenStatus> OnValidateRefreshToken(ValidateRefreshTokenRequest req)
        {
            return _userService.ValidateRefreshToken(req.AccountId, req.RefreshToken);
        }

        public Task<User> OnUpdateRefreshToken(UpdateRefreshTokenRequest req)
        {
            return _userService.UpdateRefreshToken(req.UserId);
        }

        public Task OnUpdateUser(UpdateUserRequest req)
        {
            return _userService.UpdateUser(req.PlatformUserId, req.DiscordUser);
        }
    }
}
