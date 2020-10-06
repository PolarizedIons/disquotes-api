using System.Threading.Tasks;
using QuotesApi.Models.Users;
using QuotesCore.Services;
using QuotesLib.Models;
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
        
        public Task<User> OnFindUser(FindUserRequest findUserRequest)
        {
            return _userService.FindUser(findUserRequest.UserId);
        }
    }
}
