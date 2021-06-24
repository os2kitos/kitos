using System;
using System.Threading.Tasks;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class DisposableExtensions
    {
        /// <summary>
        /// Disposes the async disposable response and returns an empty task
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="candidate"></param>
        /// <returns></returns>
        public static async Task DisposeAsync<T>(this Task<T> candidate) where T : IDisposable
        {
            using var disposable = await candidate;
        }
    }
}
