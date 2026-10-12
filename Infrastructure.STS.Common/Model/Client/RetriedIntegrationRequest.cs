using System;
using System.Collections.Generic;
using System.Linq;
using Polly;

namespace Infrastructure.STS.Common.Model.Client
{
    /// <summary>
    /// Wraps an integration request in a retry loop with standard retry-exponential backoff with the option to provide custom retries
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RetriedIntegrationRequest<T>(
        Func<T> executeRequest,
        IEnumerable<TimeSpan>? customSleepDurations = null)
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly IReadOnlyList<TimeSpan> StandardSleepDurations = new double[] { 1, 3, 5 }
            .Select(TimeSpan.FromSeconds)
            .ToList()
            .AsReadOnly();

        private readonly IEnumerable<TimeSpan> _sleepDurations = customSleepDurations ?? StandardSleepDurations;
        private readonly Func<T> _executeRequest = executeRequest ?? throw new ArgumentNullException(nameof(executeRequest));

        public T Execute()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetry(_sleepDurations)
                .Execute(() => _executeRequest());
        }
    }
}
