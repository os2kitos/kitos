using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.DomainServices.SSO;
using Serilog;

namespace Presentation.Web.Infrastructure.SSO
{
    /// <summary>
    /// Local-development substitute for <see cref="StsBrugerInfoService"/> that avoids calling the
    /// real KOMBIT STS BrugerService.  Useful when the KOMBIT test environment does not have a
    /// service agreement for the test CVR (e.g. Korsbaek, CVR 11111111).
    ///
    /// Activated by setting the "LocalSsoTestUserEmail" app setting.
    /// Returns a <see cref="StsBrugerInfo"/> whose <c>Emails</c> list contains the configured
    /// email address, which must belong to an existing KITOS user in the local database.
    ///
    /// NOTE: Never register this service in production environments.
    /// </summary>
    internal sealed class LocalTestStsBrugerInfoService : IStsBrugerInfoService
    {
        private static readonly Guid FakeOrganizationUuid = new Guid("00000000-dead-beef-0000-000000000000");

        private readonly string _testUserEmail;
        private readonly ILogger _logger;

        public LocalTestStsBrugerInfoService(string testUserEmail, ILogger logger)
        {
            _testUserEmail = testUserEmail;
            _logger = logger;
        }

        public Result<StsBrugerInfo, string> GetStsBrugerInfo(Guid uuid, string cvrNumber)
        {
            _logger.Warning(
                "[LocalTestSso] Returning fake StsBrugerInfo for UUID={Uuid} CVR={Cvr} — KOMBIT STS call bypassed for local testing",
                uuid, cvrNumber);

            return new StsBrugerInfo(
                uuid,
                new List<string> { _testUserEmail },
                FakeOrganizationUuid,
                cvrNumber,
                "LocalTest",
                "User");
        }
    }
}
