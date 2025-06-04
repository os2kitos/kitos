﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.Internal.References;
using Xunit;

namespace Tests.Integration.Presentation.Web.References
{
    public class ExternalReferenceInternalApiV2Test : BaseTest
    {
        [Fact]
        public async Task Can_Get_System_References_With_Last_Changed_Data()
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var org = await CreateOrganizationAsync();
            var references = Many<ExternalReferenceDataWriteRequestDTO>()
                .Transform(ExternalReferencesV2Helper.WithRandomMaster).ToList();
            var systemRequest = new CreateItSystemRequestDTO { OrganizationUuid = org.Uuid, Name = A<string>(), ExternalReferences = references };
            var system = await ItSystemV2Helper.CreateSystemAsync(token.Token, systemRequest);

            var referencesResponse = await ExternalReferencesInternalV2Helper.GetItSystemReferences(system.Uuid);

            Assert.NotNull(referencesResponse);
            var firstReference = referencesResponse.First();
            Assert.NotEmpty(firstReference.LastChangedByUsername);
            Assert.NotEqual(firstReference.LastChangedDate, DateTime.MinValue);

        }
    }
}
