using System;

namespace Infrastructure.Services.Configuration
{
    public class KitosUrl
    {
        public Uri Url { get; }

        public KitosUrl(Uri url)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }
    }
}
