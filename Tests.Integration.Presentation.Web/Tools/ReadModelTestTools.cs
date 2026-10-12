using Core.DomainModel.BackgroundJobs;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class ReadModelTestTools
    {
        public static async Task WaitForReadModelQueueDepletion(DateTime? createdAfter = null)
        {
            await WaitForAsync(
                () =>
                {
                    return Task.FromResult(
                        DatabaseAccess.MapFromEntitySet<PendingReadModelUpdate, bool>(x =>
                        {
                            var query = x.AsQueryable();
                            if (createdAfter.HasValue)
                                query = query.Where(u => u.CreatedAt >= createdAfter.Value);
                            return !query.Any();
                        }));
                }, TimeSpan.FromSeconds(120));
        }

        private static async Task WaitForAsync(Func<Task<bool>> check, TimeSpan howLong)
        {
            bool conditionMet;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                conditionMet = await check();
            } while (conditionMet == false && stopwatch.Elapsed <= howLong);

            Assert.True(conditionMet, $"Failed to meet required condition within {howLong.TotalMilliseconds} milliseconds");
        }
    }
}
