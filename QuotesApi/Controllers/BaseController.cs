using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using QuotesApi.Services;

namespace QuotesApi.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected Guid UserId => Guid.Parse(User.Claims.First(claim => claim.Type == JwtService.AccountIdField).Value);
    }
}
