using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class DataProcessingRegistrationResponseMapperTest : WithAutoFixture
    {
        private readonly DataProcessingRegistrationResponseMapper _sut;

        public DataProcessingRegistrationResponseMapperTest()
        {
            _sut = new DataProcessingRegistrationResponseMapper();
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_Root_Properties()
        {
            //Arrange
            var dpr = new DataProcessingRegistration();
            AssignBasicProperties(dpr);

            //Act
            var dto = _sut.MapDataProcessingRegistrationDTO(dpr);

            //Assert
            Assert.Equal(dpr.Uuid, dto.Uuid);
            Assert.Equal(dpr.LastChanged, dto.LastModified);
            AssertUser(dpr.ObjectOwner, dto.CreatedBy);
            AssertUser(dpr.LastChangedByUser, dto.LastModifiedBy);
            AssertOrganization(dpr.Organization, dto.OrganizationContext);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapSystemUsageDTO_Maps_General_Properties_Section(bool withCrossReferences)
        {
            //Arrange
            var dpr = new DataProcessingRegistration();
            AssignBasicProperties(dpr);
            AssignGeneralPropertiesSection(dpr, withCrossReferences);

            //Act
            var dto = _sut.MapDataProcessingRegistrationDTO(dpr);

            //Assert
            var general = dto.General;
            AssertOptionalIdentity(dpr.DataResponsible, general.DataResponsible);
            Assert.Equal(dpr.DataResponsibleRemark, general.DataResponsibleRemark);
            AssertYesNoIrrelevant(dpr.IsAgreementConcluded, general.IsAgreementConcluded);
            Assert.Equal(dpr.AgreementConcludedRemark, general.IsAgreementConcludedRemark);
            Assert.Equal(dpr.AgreementConcludedAt, general.AgreementConcludedAt);
            AssertYesNoUndecided(dpr.TransferToInsecureThirdCountries, general.TransferToInsecureThirdCountries);
            AssertOptionalIdentity(dpr.BasisForTransfer, general.BasisForTransfer);
            AssertOptionalIdentity(dpr.InsecureCountriesSubjectToDataTransfer, general.InsecureCountriesSubjectToDataTransfer);
            AssertOrganizations(dpr.DataProcessors, general.DataProcessors);
            AssertYesNoUndecided(dpr.HasSubDataProcessors, general.HasSubDataProcessors);
            AssertOrganizations(dpr.SubDataProcessors, general.SubDataProcessors);
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_Role_Assignment_Section()
        {
            //Arrange
            var dpr = new DataProcessingRegistration();
            AssignBasicProperties(dpr);
            AssignRoles(dpr);

            //Act
            var dto = _sut.MapDataProcessingRegistrationDTO(dpr);

            //Assert
            var expected = dpr.Rights.Select(right => new
            {
                roleId = right.Role.Uuid,
                roleName = right.Role.Name,
                userId = right.User.Uuid,
                userName = right.User.GetFullName()
            }).ToList();
            var actual = dto.Roles.Select(roleAssignment => new
            {
                roleId = roleAssignment.Role.Uuid,
                roleName = roleAssignment.Role.Name,
                userId = roleAssignment.User.Uuid,
                userName = roleAssignment.User.Name
            }).ToList();
            Assert.Equal(expected.Count, actual.Count);
            foreach (var comparison in expected.Zip(actual, (expectedEntry, actualEntry) => new { expectedEntry, actualEntry }).ToList())
            {
                Assert.Equal(comparison.expectedEntry.roleId, comparison.actualEntry.roleId);
                Assert.Equal(comparison.expectedEntry.roleName, comparison.actualEntry.roleName);
                Assert.Equal(comparison.expectedEntry.userId, comparison.actualEntry.userId);
                Assert.Equal(comparison.expectedEntry.userName, comparison.actualEntry.userName);
            }
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_ExternalReferences_Properties_Section()
        {
            //Arrange
            var dpr = new DataProcessingRegistration();
            AssignBasicProperties(dpr);
            AssignExternalReferences(dpr);

            //Act
            var dto = _sut.MapDataProcessingRegistrationDTO(dpr);

            //Assert
            AssertExternalReferences(dpr, dto.ExternalReferences.ToList());
        }

        #region Creates

        private User CreateUser()
        {
            return new User
            {
                Name = A<string>(),
                LastName = A<string>(),
                Uuid = A<Guid>()
            };
        }

        private Organization CreateOrganization()
        {
            return new Organization { Name = A<string>(), Cvr = A<string>(), Uuid = A<Guid>() };
        }

        #endregion

        #region Assigns

        private void AssignBasicProperties(DataProcessingRegistration dpr)
        {
            dpr.Id = A<int>();
            dpr.LastChanged = A<DateTime>();
            dpr.Uuid = A<Guid>();
            dpr.ObjectOwner = CreateUser();
            dpr.LastChangedByUser = CreateUser();
            dpr.Organization = CreateOrganization();
        }

        private void AssignGeneralPropertiesSection(DataProcessingRegistration dpr, bool withOptionalCrossReferences)
        {
            dpr.DataResponsible = withOptionalCrossReferences
                ? new DataProcessingDataResponsibleOption() { Uuid = A<Guid>(), Name = A<string>() }
                : null;
            dpr.DataResponsibleRemark = A<string>();
            dpr.IsAgreementConcluded = A<YesNoIrrelevantOption>();
            dpr.AgreementConcludedRemark = A<string>();
            dpr.AgreementConcludedAt = A<DateTime>();
            dpr.TransferToInsecureThirdCountries = A<YesNoUndecidedOption>();
            dpr.BasisForTransfer = withOptionalCrossReferences
                ? new DataProcessingBasisForTransferOption() { Uuid = A<Guid>(), Name = A<string>() }
                : null;
            dpr.InsecureCountriesSubjectToDataTransfer = withOptionalCrossReferences
                ? new List<DataProcessingCountryOption> { new() { Uuid = A<Guid>(), Name = A<string>() } }
                : null;
            dpr.DataProcessors = new List<Organization>{ CreateOrganization(), CreateOrganization() };
            dpr.HasSubDataProcessors = A<YesNoUndecidedOption>();
            dpr.SubDataProcessors = new List<Organization> { CreateOrganization(), CreateOrganization() };
        }

        private void AssignRoles(DataProcessingRegistration dpr)
        {
            var rights = Many<Guid>().Select(id => new DataProcessingRegistrationRight()
            {
                User = CreateUser(),
                Role = new DataProcessingRegistrationRole() { Name = A<string>(), Uuid = id }
            }).ToList();
            dpr.Rights = rights;
        }

        private void AssignExternalReferences(DataProcessingRegistration dpr)
        {
            dpr.ExternalReferences = Many<string>().Select((title, i) => new ExternalReference
            {
                Title = title,
                URL = A<string>(),
                ExternalReferenceId = A<string>(),
                Id = i
            }).ToList();
            dpr.Reference = dpr.ExternalReferences.OrderBy(x => A<int>()).First();
        }

        #endregion

        #region Asserts

        private static void AssertYesNoUndecided(YesNoUndecidedOption? expectedFromSource, YesNoUndecidedChoice? actual)
        {
            YesNoUndecidedChoice? expected = expectedFromSource switch
            {
                YesNoUndecidedOption.No => YesNoUndecidedChoice.No,
                YesNoUndecidedOption.Yes => YesNoUndecidedChoice.Yes,
                YesNoUndecidedOption.Undecided => YesNoUndecidedChoice.Undecided,
                null => null,
                _ => throw new ArgumentOutOfRangeException(nameof(expectedFromSource), expectedFromSource, null)
            };
            Assert.Equal(expected, actual);
        }

        private static void AssertYesNoIrrelevant(YesNoIrrelevantOption? expectedFromSource, YesNoIrrelevantChoice? actual)
        {
            YesNoIrrelevantChoice? expected = expectedFromSource switch
            {
                YesNoIrrelevantOption.NO => YesNoIrrelevantChoice.No,
                YesNoIrrelevantOption.YES => YesNoIrrelevantChoice.Yes,
                YesNoIrrelevantOption.IRRELEVANT => YesNoIrrelevantChoice.Irrelevant,
                YesNoIrrelevantOption.UNDECIDED => YesNoIrrelevantChoice.Undecided,
                null => null,
                _ => throw new ArgumentOutOfRangeException(nameof(expectedFromSource), expectedFromSource, null)
            };
            Assert.Equal(expected, actual);
        }

        private static void AssertOptionalIdentity<T>(IEnumerable<T> optionalExpectedIdentities, IEnumerable<IdentityNamePairResponseDTO> actualIdentities) where T : IHasUuid, IHasName
        {
            if (optionalExpectedIdentities == null)
            {
                Assert.Null(actualIdentities);
            }
            else
            {
                var orderedOptionalExpectedIdentities = optionalExpectedIdentities.OrderBy(x => x.Uuid).ToList();
                var orderedActualIdentities = actualIdentities.OrderBy(x => x.Uuid).ToList();

                Assert.Equal(orderedOptionalExpectedIdentities.Count(), orderedActualIdentities.Count());
                for (var i = 0; i < orderedOptionalExpectedIdentities.Count(); i++)
                {
                    AssertOptionalIdentity(orderedOptionalExpectedIdentities[i], orderedActualIdentities[i]);
                }
            }
        }

        private static void AssertOptionalIdentity<T>(T optionalExpectedIdentity, IdentityNamePairResponseDTO actualIdentity) where T : IHasUuid, IHasName
        {
            if (optionalExpectedIdentity == null)
                Assert.Null(actualIdentity);
            else
                AssertIdentity(optionalExpectedIdentity, actualIdentity);
        }

        private static void AssertExternalReferences(DataProcessingRegistration dpr, List<ExternalReferenceDataDTO> dtoExternalReferences)
        {
            var actualMaster = Assert.Single(dtoExternalReferences, reference => reference.MasterReference);
            AssertExternalReference(dpr.Reference, actualMaster);
            Assert.Equal(dpr.ExternalReferences.Count, dtoExternalReferences.Count);

            foreach (var comparison in dpr.ExternalReferences.OrderBy(x => x.Title)
                .Zip(dtoExternalReferences.OrderBy(x => x.Title), (expected, actual) => new { expected, actual })
                .ToList())
            {
                AssertExternalReference(comparison.expected, comparison.actual);
            }
        }

        private static void AssertExternalReference(ExternalReference reference, ExternalReferenceDataDTO actualMaster)
        {
            Assert.Equal(reference.Title, actualMaster.Title);
            Assert.Equal(reference.URL, actualMaster.Url);
            Assert.Equal(reference.ExternalReferenceId, actualMaster.DocumentId);
        }

        private static void AssertUser(User user, IdentityNamePairResponseDTO dtoValue)
        {
            Assert.Equal((user.GetFullName(), user.Uuid), (dtoValue.Name, dtoValue.Uuid));
        }
        private static void AssertOrganizations(IEnumerable<Organization> organizations, IEnumerable<ShallowOrganizationResponseDTO> shallowOrganizationDTOs)
        {
            var orderedOrganizations = organizations.OrderBy(x => x.Uuid).ToList();
            var orderedShallowOrganizationDTOs = shallowOrganizationDTOs.OrderBy(x => x.Uuid).ToList();

            Assert.Equal(orderedOrganizations.Count(), orderedShallowOrganizationDTOs.Count());
            for (var i = 0; i < orderedOrganizations.Count(); i++)
            {
                AssertOrganization(orderedOrganizations[i], orderedShallowOrganizationDTOs[i]);
            }
        }

        private static void AssertOrganization(Organization organization, ShallowOrganizationResponseDTO shallowOrganizationDTO)
        {
            AssertIdentity(organization, shallowOrganizationDTO);
            Assert.Equal(organization.Cvr, shallowOrganizationDTO.Cvr);
        }

        private static void AssertIdentity<T>(T sourceIdentity, IdentityNamePairResponseDTO dto) where T : IHasUuid, IHasName
        {
            Assert.Equal(sourceIdentity.Name, dto.Name);
            Assert.Equal(sourceIdentity.Uuid, dto.Uuid);
        }

        #endregion
    }
}
