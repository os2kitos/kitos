using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Newtonsoft.Json;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Headers
{
    public class V2EnumSerializationTest : WithAutoFixture
    {
        [Fact, Description("Verifies https://os2web.atlassian.net/browse/KITOSUDV-4347 which ensures that enums are actually returned as strings, which is also how they are described in swagger.json")]
        public async Task ByDefault_V2_Serializes_Enums_As_Strings()
        {
            await TestEnumSerialization(false, (choice, json) =>
            {
                var definition = new
                {
                    General = new
                    {
                        IsAgreementConcluded = ""
                    }
                };
                var result = JsonConvert.DeserializeAnonymousType(json, definition);
                var expectedOutput = choice.ToString("G");
                Assert.Equal(expectedOutput, result?.General?.IsAgreementConcluded);
            });
        }

        [Fact, Description("Verifies that client relying on integer responses in enums prior to fix https://os2web.atlassian.net/browse/KITOSUDV-4347 can still get the same response as before")]
        public async Task Can_Revert_To_Enum_As_Number_Using_Header()
        {
            await TestEnumSerialization(true, (choice, json) =>
            {
                var definition = new
                {
                    General = new
                    {
                        IsAgreementConcluded = 2
                    }
                };
                var result = JsonConvert.DeserializeAnonymousType(json, definition);
                var expectedOutput = Convert.ToInt32(choice);
                Assert.Equal(expectedOutput, result?.General?.IsAgreementConcluded);
            });

        }

        private async Task TestEnumSerialization(bool withNumberEnums, Action<YesNoIrrelevantChoice, string> assertJsonAgainstExpectedSelection)
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var organizationId = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);
            var choice = EnumRange.AllExcept(YesNoIrrelevantChoice.No).RandomItem();
            var createdDto = await DataProcessingRegistrationV2Helper.PostAsync(token.Token,
                new CreateDataProcessingRegistrationRequestDTO
                {
                    Name = A<string>(),
                    OrganizationUuid = organizationId,
                    General = new DataProcessingRegistrationGeneralDataWriteRequestDTO
                    {
                        IsAgreementConcluded = choice
                    }
                });

            //Act
            using var getResponse = await DataProcessingRegistrationV2Helper.SendGetDPRAsync(token.Token, createdDto.Uuid, withNumberEnums);

            //Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var json = await getResponse.Content.ReadAsStringAsync();
            assertJsonAgainstExpectedSelection(choice, json);
        }
    }
}
