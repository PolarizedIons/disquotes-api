using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using QuotesApi.Models.Paging;
using QuotesLib.Services;

namespace QuotesBot.Commands
{
    public class Quotes : ModuleBase
    {
        private const int ExtractLength = 1000;
        private const int ListPageSize = 9;
        private readonly IConfiguration _config;
        private readonly NatsQuotesService _natsQuotesService;

        public Quotes(IConfiguration config, NatsQuotesService natsQuotesService)
        {
            _config = config;
            _natsQuotesService = natsQuotesService;
        }

        [Command("quotes")]
        [Summary("Links to the quotes website")]
        public async Task QuotesWebsite()
        {
            await ReplyAsync($"Quotes can be found at ⟶ {_config["Discord:FrontEndBaseUrl"]}");
        }

        [Command("quote")]
        [Summary("View a single quote")]
        public async Task SingleQuote(int quoteNumber)
        {
            if (quoteNumber <= 0)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle("What did you think would happen?")
                    .WithDescription("Quote not found.")
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp()
                    .Build()
                );
                return;
            }

            var quote = await _natsQuotesService.FindByQuoteNumber(Context.Guild.Id, quoteNumber, enrichWithUser: true);
            if (quote == null)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle($"Quote #{quoteNumber}")
                    .WithDescription("Quote not found.")
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp()
                    .Build()
                );
                return;
            }

            var isExtract = quote.Text.Length > ExtractLength;
            var quoteText = quote.Text;
            if (isExtract)
            {
                quoteText = quoteText.Substring(0, ExtractLength) + "...";
            }

            var url = _config["Discord:FrontEndBaseUrl"] + _config["Discord:FrontEndViewQuoteUrl"]
                .Replace("{GuildId}", quote.GuildId)
                .Replace("{QuoteId}", quote.Id.ToString());

            await ReplyAsync(embed: new EmbedBuilder()
                .WithTitle($"Quote #{quoteNumber}: {quote.Title}")
                .WithThumbnailUrl(quote.User.ProfileUrl)
                .AddField("Author", $"{quote.User.Username}#{quote.User.Discriminator}")
                .AddField(isExtract ? "Extract" : "Text", quoteText)
                .AddField(isExtract ? "View Full Quote" : "View Quote", url)
                .WithColor(Color.Blue)
                .WithTimestamp(quote.CreatedAt)
                .Build()
            );
        }

        [Priority(100)]
        [Command("quote list")]
        [Alias("quotes list")]
        [Summary("View a quotes list")]
        public async Task QuotesList(int pageNumber = 1)
        {
            if (pageNumber <= 0)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle("What did you think would happen?")
                    .WithDescription("Page not found.")
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp()
                    .Build()
                );
                return;
            }

            var quotesList = await _natsQuotesService.FindApproved(
                new[] {Context.Guild.Id.ToString()},
                new PagingFilter
                {
                    PageNumber = pageNumber,
                    PageSize = ListPageSize,
                }
            );

            var totalPages = quotesList.TotalRows / ListPageSize;
            if (pageNumber > totalPages)
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle("Page doesn't exist")
                    .WithDescription("Page not found.")
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp()
                    .Build()
                );
                return;
            }

            var reply = new EmbedBuilder()
                .WithTitle($"Quotes Page #{pageNumber}");

            foreach (var quote in quotesList.Items)
            {
                reply.AddField($"#{quote.QuoteNumber}: {quote.Title}", $"{quote.User.Username}#{quote.User.Discriminator}", true);
            }

            await ReplyAsync(embed: reply
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .Build()
            );
        }
    }
}
