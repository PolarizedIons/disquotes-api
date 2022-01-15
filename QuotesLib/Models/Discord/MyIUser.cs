using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Discord;

namespace QuotesLib.Models.Discord
{
    public class MyIUser : IUser
    {
        public ulong Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Mention { get; set; }
        public IActivity Activity { get; }
        public UserStatus Status { get; }
        public IImmutableSet<ClientType> ActiveClients { get; }
        public IImmutableList<IActivity> Activities { get; }
        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            var extension = FormatToExtension(format, AvatarId);
            return $"https://cdn.discordapp.com/avatars/{Id}/{AvatarId}.{extension}?size={size}";
        }

        private string FormatToExtension(ImageFormat format, string imageId)
        {
            if (format == ImageFormat.Auto)
            {
                format = imageId.StartsWith("a_") ? ImageFormat.Gif : ImageFormat.Png;
            }

            return format switch
            {
                ImageFormat.Png => "png",
                ImageFormat.WebP => "webp",
                ImageFormat.Jpeg => "jpeg",
                ImageFormat.Gif => "gif",
                _ => throw new ArgumentException($"Invalid image format {format}")
            };
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
        public UserProperties? PublicFlags { get; }
    }
}
