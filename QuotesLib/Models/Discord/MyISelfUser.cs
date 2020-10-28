using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Discord;

namespace QuotesLib.Models.Discord
{
    public class MyISelfUser : ISelfUser
    {
        public ulong Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Mention { get; set; }
        public IActivity Activity { get; set; }
        public UserStatus Status { get; set; }
        public IImmutableSet<ClientType> ActiveClients { get; set; }
        public IImmutableList<IActivity> Activities { get; set; }
        public Task ModifyAsync(Action<SelfUserProperties> func, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string Email { get; set; }
        public bool IsVerified { get; set; }
        public bool IsMfaEnabled { get; set; }
        public UserProperties Flags { get; }
        public PremiumType PremiumType { get; set; }
        public string Locale { get; set; }
        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            throw new NotImplementedException();
        }

        public string GetDefaultAvatarUrl()
        {
            throw new NotImplementedException();
        }

        public Task<IDMChannel> GetOrCreateDMChannelAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string AvatarId { get; set; }
        public string Discriminator { get; set; }
        public ushort DiscriminatorValue { get; set; }
        public bool IsBot { get; set; }
        public bool IsWebhook { get; set; }
        public string Username { get; set; }
    }
}