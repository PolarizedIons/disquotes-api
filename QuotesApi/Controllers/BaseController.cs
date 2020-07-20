using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using QuotesApi.Exceptions;
using QuotesApi.Models.Paging;
using QuotesApi.Services;

namespace QuotesApi.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected Guid UserId => Guid.Parse(User.Claims.First(claim => claim.Type == JwtService.AccountIdField).Value);

        protected ulong UserDiscordId => ulong.Parse(User.Claims.First(claim => claim.Type == JwtService.DiscordIdField).Value);

        protected void ValidatePagingFilter(PagingFilter pagingFilter)
        {
            if (pagingFilter.PageNumber < 1)
            {
                throw new BadRequestException("Page number cannot be less than 1");
            }

            if (pagingFilter.PageSize < 1)
            {
                throw new BadRequestException("Page size cannot be less than 1");
            }
            
            if (pagingFilter.PageSize > 1000)
            {
                throw new BadRequestException("Page size cannot be more than 1000");
            }
        }
    }
}
