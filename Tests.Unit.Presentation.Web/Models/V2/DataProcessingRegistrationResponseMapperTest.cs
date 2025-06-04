using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Moq;
using Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.DataProcessing;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Types.DataProcessing;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class DataProcessingRegistrationResponseMapperTest : WithAutoFixture
    {
        private readonly DataProcessingRegistrationResponseMapper _sut;
        private readonly Mock<IExternalReferenceResponseMapper> _externalReferenceResponseMapperMock;

        public DataProcessingRegistrationResponseMapperTest()
        {
            _externalReferenceResponseMapperMock = new Mock<IExternalReferenceResponseMapper>();
            _sut = new DataProcessingRegistrationResponseMapper(_externalReferenceResponseMapperMock.Object);
        }

        [Fact]
        public void MapDataProcessingRegistrationDTO_Maps_Root_Properties()
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

        [Fact]
        public void MapDataProcessingRegistrationDTO_Maps_No_Properties()
        {
            //Arrange
            var dpr = new DataProcessingRegistration();

            //Act
            var dto = _sut.MapDataProcessingRegistrationDTO(dpr);

            //Assert
            Assert.Equal(dpr.Uuid, dto.Uuid);
            Assert.Equal(dpr.LastChanged, dto.LastModified);

            Assert.Null(dto.CreatedBy);
            Assert.Null(dto.LastModifiedBy);
            Assert.Null(dto.Name);
            Assert.Null(dto.OrganizationContext);
            Assert.Empty(dto.ExternalReferences);
            Assert.Empty(dto.SystemUsages);
            Assert.Empty(dto.Roles);

            var general = dto.General;
            Assert.Null(general.DataResponsible);
            Assert.Null(general.DataResponsibleRemark);
            Assert.Null(general.IsAgreementConcluded);
            Assert.Null(general.IsAgreementConcludedRemark);
            Assert.Null(general.AgreementConcludedAt);
            Assert.Null(general.TransferToInsecureThirdCountries);
            Assert.Null(general.BasisForTransfer);
            Assert.Empty(general.InsecureCountriesSubjectToDataTransfer);
            Assert.Empty(general.DataProcessors);
            Assert.Null(general.HasSubDataProcessors);
            Assert.Empty(general.SubDataProcessors);
            Assert.Null(general.MainContract);
            Assert.Empty(general.AssociatedContracts);
            Assert.True(general.Valid);
            Assert.Null(general.ResponsibleOrganizationUnit);

            var oversight = dto.Oversight;
            Assert.Empty(oversight.OversightOptions);
            Assert.Null(oversight.OversightOptionsRemark);
            Assert.Null(oversight.OversightInterval);
            Assert.Null(oversight.OversightIntervalRemark);
            Assert.Null(oversight.IsOversightCompleted);
            Assert.Null(oversight.OversightCompletedRemark);
            Assert.Null(oversight.OversightScheduledInspectionDate);
            Assert.Empty(oversight.OversightDates);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapDataProcessingRegistrationDTO_Maps_General_Properties_Section(bool withCrossReferences)
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
            AssertOptionalIdentities(dpr.InsecureCountriesSubjectToDataTransfer, general.InsecureCountriesSubjectToDataTransfer);
            AssertOrganizations(dpr.DataProcessors, general.DataProcessors);
            AssertYesNoUndecided(dpr.HasSubDataProcessors, general.HasSubDataProcessors);
            AssertSubDataProcessors(dpr.AssignedSubDataProcessors, general.SubDataProcessors);
            AssertIdentity(dpr.MainContract, general.MainContract);
            AssertOptionalIdentities(dpr.AssociatedContracts, general.AssociatedContracts);
            Assert.Equal(dpr.IsActiveAccordingToMainContract, general.Valid);
            Assert.Equal(dpr.ResponsibleOrganizationUnit.Uuid, general.ResponsibleOrganizationUnit.Uuid);
            Assert.Equal(dpr.ResponsibleOrganizationUnit.Name, general.ResponsibleOrganizationUnit.Name);
        }

        private void AssertSubDataProcessors(IEnumerable<SubDataProcessor> assignedSubDataProcessors, IEnumerable<DataProcessorRegistrationSubDataProcessorResponseDTO> mappedSubDataProcessors)
        {
            var expectedSubDataProcessors = assignedSubDataProcessors.ToDictionary(x => x.Organization.Uuid);
            var actualSubDataProcessors = mappedSubDataProcessors.ToDictionary(x => x.DataProcessorOrganization.Uuid);

            Assert.Equal(expectedSubDataProcessors.Count, actualSubDataProcessors.Count);
            foreach (var expectedSubDataProcessor in expectedSubDataProcessors)
            {
                Assert.True(actualSubDataProcessors.TryGetValue(expectedSubDataProcessor.Key, out _));
                var actualSubDataProcessor = actualSubDataProcessors[expectedSubDataProcessor.Key];
                AssertOrganization(expectedSubDataProcessor.Value.Organization, actualSubDataProcessor.DataProcessorOrganization);
                AssertOptionalIdentity(expectedSubDataProcessor.Value.SubDataProcessorBasisForTransfer, actualSubDataProcessor.BasisForTransfer);
                AssertYesNoUndecided(expectedSubDataProcessor.Value.TransferToInsecureCountry, actualSubDataProcessor.TransferToInsecureThirdCountry);
                AssertOptionalIdentity(expectedSubDataProcessor.Value.InsecureCountry, actualSubDataProcessor.InsecureThirdCountrySubjectToDataProcessing);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapDataProcessingRegistrationDTO_Maps_SystemUsageUuids_Section(bool withSystemUsages)
        {
            //Arrange
            var dpr = new DataProcessingRegistration();
            AssignBasicProperties(dpr);
            if (withSystemUsages)
            {
                var usage1 = CreateSystemUsage();
                var usage2 = CreateSystemUsage();
                AssignSystemUsages(dpr, new[] { usage1, usage2 });
            }
            else
            {
                AssignSystemUsages(dpr, Array.Empty<ItSystemUsage>());
            }

            //Act
            var dto = _sut.MapDataProcessingRegistrationDTO(dpr);

            //Assert
            if (withSystemUsages)
            {
                AssertSystemUsages(dpr, dto.SystemUsages.ToList());
            }
            else
            {
                Assert.Empty(dto.SystemUsages);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapDataProcessingRegistrationDTO_Maps_Oversight_Properties_Section(bool withCrossReferences)
        {
            //Arrange
            var dpr = new DataProcessingRegistration();
            AssignBasicProperties(dpr);
            AssignOversightProperties(dpr, withCrossReferences);

            //Act
            var dto = _sut.MapDataProcessingRegistrationDTO(dpr);

            //Assert
            var oversight = dto.Oversight;
            AssertOptionalIdentities(dpr.OversightOptions, oversight.OversightOptions);
            Assert.Equal(dpr.OversightOptionRemark, oversight.OversightOptionsRemark);
            AssertOversightInterval(dpr.OversightInterval, oversight.OversightInterval);
            Assert.Equal(dpr.OversightIntervalRemark, oversight.OversightIntervalRemark);
            AssertYesNoUndecided(dpr.IsOversightCompleted, oversight.IsOversightCompleted);
            Assert.Equal(dpr.OversightCompletedRemark, oversight.OversightCompletedRemark);
            Assert.Equal(dpr.OversightScheduledInspectionDate, oversight.OversightScheduledInspectionDate);
            AssertOversightDates(dpr.OversightDates, oversight.OversightDates);
        }

        [Fact]
        public void MapDataProcessingRegistrationDTO_Maps_Role_Assignment_Section()
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
        public void MapDataProcessingRegistrationDTO_Maps_ExternalReferences_Properties_Section()
        {
            //Arrange
            var dpr = new DataProcessingRegistration();
            AssignBasicProperties(dpr);
            AssignExternalReferences(dpr);
            var mappedReferences = Many<ExternalReferenceDataResponseDTO>();
            _externalReferenceResponseMapperMock
                .Setup(x => x.MapExternalReferences(dpr.ExternalReferences))
                .Returns(mappedReferences);

            //Act
            var dto = _sut.MapDataProcessingRegistrationDTO(dpr);

            //Assert
            _externalReferenceResponseMapperMock.Verify(x => x.MapExternalReferences(dpr.ExternalReferences), Times.Once);
            Assert.Equivalent(mappedReferences, dto.ExternalReferences);
        }

        #region Creates

        private ItSystemUsage CreateSystemUsage()
        {
            var systemId = A<int>();
            return new ItSystemUsage
            {
                Id = A<int>(),
                Uuid = A<Guid>(),
                ItSystemId = systemId,
                ItSystem = new ItSystem()
                {
                    Id = systemId,
                    Uuid = A<Guid>(),
                    Name = A<string>()
                }
            };
        }

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

        private SubDataProcessor CreateSubDataProcessor(DataProcessingRegistration dataProcessingRegistration, bool withBasisForTransfer, bool withTransferToInsecureThirdCountry, bool withCountry)
        {
            return new SubDataProcessor
            {
                Organization = CreateOrganization(),
                DataProcessingRegistration = dataProcessingRegistration,
                SubDataProcessorBasisForTransfer = withBasisForTransfer ? new DataProcessingBasisForTransferOption { Uuid = A<Guid>(), Name = A<string>() } : null,
                TransferToInsecureCountry = withTransferToInsecureThirdCountry ? YesNoUndecidedOption.Yes : EnumRange.AllExcept(YesNoUndecidedOption.Yes).RandomItem(),
                InsecureCountry = withCountry ? new DataProcessingCountryOption { Name = A<string>(), Uuid = A<Guid>() } : null
            };
        }

        #endregion

        #region Assigns

        private void AssignOversightProperties(DataProcessingRegistration dpr, bool withOptionalCrossReferences)
        {
            dpr.OversightOptions = withOptionalCrossReferences
                ? new List<DataProcessingOversightOption>
                    {
                        new(){ Uuid = A<Guid>(), Name = A<string>() },
                        new(){ Uuid = A<Guid>(), Name = A<string>() }
                    }
                : null;
            dpr.OversightOptionRemark = A<string>();
            dpr.OversightInterval = A<YearMonthIntervalOption>();
            dpr.OversightIntervalRemark = A<string>();
            dpr.IsOversightCompleted = A<YesNoUndecidedOption>();
            dpr.OversightCompletedRemark = A<string>();
            dpr.OversightScheduledInspectionDate = A<DateTime>();
            dpr.OversightDates = new List<DataProcessingRegistrationOversightDate>()
            {
                new()
                {
                    Parent = dpr,
                    ParentId = dpr.Id,
                    Id = A<int>(),
                    OversightDate = A<DateTime>(),
                    OversightRemark = A<string>()
                },
                new()
                {
                    Parent = dpr,
                    ParentId = dpr.Id,
                    Id = A<int>(),
                    OversightDate = A<DateTime>(),
                    OversightRemark = A<string>()
                }
            };
        }

        private static void AssignSystemUsages(DataProcessingRegistration dpr, ItSystemUsage[] usages)
        {
            dpr.SystemUsages = usages;
        }

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
            dpr.DataProcessors = new List<Organization> { CreateOrganization(), CreateOrganization() };
            dpr.HasSubDataProcessors = A<YesNoUndecidedOption>();
            dpr.AssignedSubDataProcessors = new List<SubDataProcessor>
            {
                CreateSubDataProcessor(dpr,false,false,false),
                CreateSubDataProcessor(dpr,true,false,false),
                CreateSubDataProcessor(dpr,false,true,false),
                CreateSubDataProcessor(dpr,false,true,true),
                CreateSubDataProcessor(dpr,true,true,true),
                CreateSubDataProcessor(dpr,true,true,false)
            };
            dpr.MainContract = new ItContract() { Uuid = A<Guid>(), Name = A<string>() };
            dpr.ResponsibleOrganizationUnit = new OrganizationUnit { Uuid = A<Guid>(), Name = A<string>() };
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
                Uuid = A<Guid>(),
                Title = title,
                URL = A<string>(),
                ExternalReferenceId = A<string>(),
                Id = i
            }).ToList();
            dpr.Reference = dpr.ExternalReferences.OrderBy(x => A<int>()).First();
        }

        #endregion

        #region Asserts

        private static void AssertOversightDates(ICollection<DataProcessingRegistrationOversightDate> expectedOversightDates, IEnumerable<OversightDateDTO> actualOversightDates)
        {
            var orderedExpected = expectedOversightDates.OrderBy(x => x.OversightDate).ToList();
            var orderedActual = actualOversightDates.OrderBy(x => x.CompletedAt).ToList();
            Assert.Equal(orderedExpected.Count, orderedActual.Count);

            foreach (var comparison in orderedExpected
                .Zip(orderedActual, (expected, actual) => new { expected, actual })
                .ToList())
            {
                Assert.Equal(comparison.expected.Uuid, comparison.actual.Uuid);
                Assert.Equal(comparison.expected.OversightDate, comparison.actual.CompletedAt);
                Assert.Equal(comparison.expected.OversightRemark, comparison.actual.Remark);
            }
        }

        private void AssertSystemUsages(DataProcessingRegistration dpr, List<IdentityNamePairResponseDTO> actual)
        {
            Assert.Equal(dpr.SystemUsages.Count, actual.Count);

            foreach (var comparison in dpr.SystemUsages.OrderBy(x => x.Uuid)
                .Zip(actual.OrderBy(x => x.Uuid), (expected, actual) => new { expected, actual })
                .ToList())
            {
                AssertSystemUsage(comparison.expected, comparison.actual);
            }
        }

        private static void AssertSystemUsage(ItSystemUsage expected, IdentityNamePairResponseDTO actual)
        {
            Assert.Equal(expected.Uuid, actual.Uuid);
            Assert.Equal(expected.ItSystem.Name, actual.Name);
        }

        private static void AssertOversightInterval(YearMonthIntervalOption? expectedFromSource, OversightIntervalChoice? actual)
        {
            OversightIntervalChoice? expected = expectedFromSource switch
            {
                YearMonthIntervalOption.Half_yearly => OversightIntervalChoice.BiYearly,
                YearMonthIntervalOption.Yearly => OversightIntervalChoice.Yearly,
                YearMonthIntervalOption.Every_second_year => OversightIntervalChoice.EveryOtherYear,
                YearMonthIntervalOption.Other => OversightIntervalChoice.Other,
                YearMonthIntervalOption.Undecided => OversightIntervalChoice.Undecided,
                null => null,
                _ => throw new ArgumentOutOfRangeException(nameof(expectedFromSource), expectedFromSource, null)
            };
            Assert.Equal(expected, actual);
        }

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

        private static void AssertOptionalIdentities<T>(IEnumerable<T> optionalExpectedIdentities, IEnumerable<IdentityNamePairResponseDTO> actualIdentities) where T : IHasUuid, IHasName
        {
            if (optionalExpectedIdentities == null)
            {
                Assert.Null(actualIdentities);
            }
            else
            {
                var orderedOptionalExpectedIdentities = optionalExpectedIdentities.OrderBy(x => x.Uuid).ToList();
                var orderedActualIdentities = actualIdentities.OrderBy(x => x.Uuid).ToList();

                Assert.Equal(orderedOptionalExpectedIdentities.Count, orderedActualIdentities.Count);

                foreach (var comparison in orderedOptionalExpectedIdentities
                    .Zip(orderedActualIdentities, (expected, actual) => new { expected, actual })
                    .ToList())
                {
                    AssertOptionalIdentity(comparison.expected, comparison.actual);
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

        private static void AssertUser(User user, IdentityNamePairResponseDTO dtoValue)
        {
            Assert.Equal((user.GetFullName(), user.Uuid), (dtoValue.Name, dtoValue.Uuid));
        }
        private static void AssertOrganizations(IEnumerable<Organization> organizations, IEnumerable<ShallowOrganizationResponseDTO> shallowOrganizationDTOs)
        {
            var orderedOrganizations = organizations.OrderBy(x => x.Uuid).ToList();
            var orderedShallowOrganizationDTOs = shallowOrganizationDTOs.OrderBy(x => x.Uuid).ToList();

            Assert.Equal(orderedOrganizations.Count(), orderedShallowOrganizationDTOs.Count());

            foreach (var comparison in orderedOrganizations
                .Zip(orderedShallowOrganizationDTOs, (expected, actual) => new { expected, actual })
                .ToList())
            {
                AssertOrganization(comparison.expected, comparison.actual);
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
