using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Infrastructure.Services.Http;

public interface IKitosHttpClient
{
    Task<HttpResponseMessage> PostAsync(object content, Uri uri, string token);
}