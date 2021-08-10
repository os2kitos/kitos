using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DataTypes;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.Organization;
using Moq;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItSystemUsageResponseMapperTest : WithAutoFixture
    {
        private readonly ItSystemUsageResponseMapper _sut;
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly Mock<IAttachedOptionRepository> _attachedOptionsRepositoryMock;
        private readonly Mock<ISensitivePersonalDataTypeRepository> _sensitivePersonalDataTypeRepositoryMock;
        private readonly Mock<IGenericRepository<RegisterType>> _registerTypeRepositoryMock;

        public ItSystemUsageResponseMapperTest()
        {
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _attachedOptionsRepositoryMock = new Mock<IAttachedOptionRepository>();
            _sensitivePersonalDataTypeRepositoryMock = new Mock<ISensitivePersonalDataTypeRepository>();
            _registerTypeRepositoryMock = new Mock<IGenericRepository<RegisterType>>();
            _sut = new ItSystemUsageResponseMapper(
                _organizationRepositoryMock.Object,
                _attachedOptionsRepositoryMock.Object,
                _sensitivePersonalDataTypeRepositoryMock.Object,
                _registerTypeRepositoryMock.Object
                );
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_Root_Properties()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            Assert.Equal(itSystemUsage.Uuid, dto.Uuid);
            Assert.Equal(itSystemUsage.LastChanged, dto.LastModified);
            AssertUser(itSystemUsage.ObjectOwner, dto.CreatedBy);
            AssertUser(itSystemUsage.LastChangedByUser, dto.LastModifiedBy);
            AssertIdentity(itSystemUsage.ItSystem, dto.SystemContext);
            AssertOrganization(itSystemUsage.Organization, dto.OrganizationContext);
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_General_Properties_Section()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignGeneralPropertiesSection(itSystemUsage);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            Assert.Equal(itSystemUsage.LocalSystemId, dto.General.LocalSystemId);
            Assert.Equal(itSystemUsage.LocalCallName, dto.General.LocalCallName);
            Assert.Equal(itSystemUsage.Note, dto.General.Notes);
            AssertUserCount(itSystemUsage, dto.General.NumberOfExpectedUsers);
            Assert.Equal(itSystemUsage.Version, dto.General.SystemVersion);
            AssertIdentity(itSystemUsage.MainContract.ItContract, dto.General.MainContract);
            Assert.Equal(itSystemUsage.Concluded, dto.General.Validity.ValidFrom);
            Assert.Equal(itSystemUsage.ExpirationDate, dto.General.Validity.ValidTo);
            Assert.Equal(itSystemUsage.Active, dto.General.Validity.EnforcedValid);
            Assert.Equal(itSystemUsage.IsActive, dto.General.Validity.Valid);
            AssertIdentities(itSystemUsage.ItProjects, dto.General.AssociatedProjects);
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_Role_Assignment_Section()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignRoles(itSystemUsage);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            var expected = itSystemUsage.Rights.Select(right => new
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
        public void MapSystemUsageDTO_Maps_LocalKLEDeviations_Properties_Section()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignKle(itSystemUsage);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            var expectedAdditions = itSystemUsage.TaskRefs.Select(tr => (tr.TaskKey, tr.Uuid)).ToList();
            var actualAdditions = dto.LocalKLEDeviations.AddedKLE.Select(kle => (kle.Name, kle.Uuid)).ToList();
            Assert.Equal(expectedAdditions, actualAdditions);

            var expectedRemovals = itSystemUsage.TaskRefsOptOut.Select(tr => (tr.TaskKey, tr.Uuid)).ToList();
            var actualRemovals = dto.LocalKLEDeviations.RemovedKLE.Select(kle => (kle.Name, kle.Uuid)).ToList();
            Assert.Equal(expectedRemovals, actualRemovals);
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_OrganizationalUsage_Properties_Section()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignOrganizationalUsage(itSystemUsage);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            AssertIdentity(itSystemUsage.ResponsibleUsage.OrganizationUnit, dto.OrganizationUsage.ResponsibleOrganizationUnit);
            var expectedUnits = itSystemUsage.UsedBy.Select(x => x.OrganizationUnit).OrderBy(x => x.Name).ToList();
            var actualUnits = dto.OrganizationUsage.UsingOrganizationUnits.OrderBy(x => x.Name).ToList();
            Assert.Equal(expectedUnits.Count, actualUnits.Count);
            foreach (var comparison in expectedUnits.Zip(actualUnits, (expected, actual) => new { expected, actual }).ToList())
            {
                AssertIdentity(comparison.expected, comparison.actual);
            }
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_ExternalReferences_Properties_Section()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignExternalReferences(itSystemUsage);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            AssertExternalReferences(itSystemUsage, dto.ExternalReferences.ToList());
        }

        [Fact]
        public void MapSystemUsageDTO_Maps_OutgoingSystemRelations_Properties_Section()
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignSystemRelations(itSystemUsage);

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            var expectedRelations = itSystemUsage.UsageRelations.ToList();
            var actualRelations = dto.OutgoingSystemRelations.ToList();
            Assert.Equal(expectedRelations.Count, actualRelations.Count);
            foreach (var comparison in expectedRelations.OrderBy(x => x.Uuid).Zip(actualRelations.OrderBy(x => x.Uuid), (expected, actual) => new { expected, actual }).ToList())
            {
                AssertRelation(comparison.expected, comparison.actual);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapSystemUsageDTO_Maps_Archiving_Properties_Section(bool withCrossReferences)
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignArchiving(itSystemUsage, withCrossReferences);
            Organization supplierOrganization = default;
            if (withCrossReferences)
            {
                supplierOrganization = CreateOrganization();
                _organizationRepositoryMock.Setup(x => x.GetById(itSystemUsage.SupplierId.Value)).Returns(supplierOrganization);
            }

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            Assert.Equal(itSystemUsage.ArchiveFromSystem, dto.Archiving.Active);
            Assert.Equal(itSystemUsage.ArchiveNotes, dto.Archiving.Notes);
            Assert.Equal(itSystemUsage.Registertype, dto.Archiving.DocumentBearing);
            Assert.Equal(itSystemUsage.ArchiveFreq, dto.Archiving.FrequencyInMonths);
            AssertOptionalIdentity(itSystemUsage.ArchiveLocation, dto.Archiving.Location);
            AssertOptionalIdentity(itSystemUsage.ArchiveTestLocation, dto.Archiving.TestLocation);
            AssertOptionalIdentity(itSystemUsage.ArchiveType, dto.Archiving.Type);
            AssertOptionalIdentity(supplierOrganization, dto.Archiving.Supplier);
            var expectedArchivePeriods = itSystemUsage.ArchivePeriods.OrderBy(x => x.UniqueArchiveId).ToList();
            var actualJournalPeriods = dto.Archiving.JournalPeriods.OrderBy(x => x.ArchiveId).ToList();
            Assert.Equal(expectedArchivePeriods.Count, actualJournalPeriods.Count);
            foreach (var comparison in expectedArchivePeriods.Zip(actualJournalPeriods, (expected, actual) => new { expected, actual }).ToList())
            {
                Assert.Equal(comparison.expected.Approved, comparison.actual.Approved);
                Assert.Equal(comparison.expected.StartDate, comparison.actual.StartDate);
                Assert.Equal(comparison.expected.EndDate, comparison.actual.EndDate);
                Assert.Equal(comparison.expected.UniqueArchiveId, comparison.actual.ArchiveId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapSystemUsageDTO_Maps_GDPR_Properties_Section(bool withCrossReferences)
        {
            //Arrange
            var itSystemUsage = new ItSystemUsage();
            AssignBasicProperties(itSystemUsage);
            AssignGDPR(itSystemUsage, withCrossReferences); //TODO

            //Act
            var dto = _sut.MapSystemUsageDTO(itSystemUsage);

            //Assert
            //TODO
        }

        private (IReadOnlyList<AttachedOption> sensitivePersonData, IReadOnlyList<AttachedOption> registerTypeData) AssignGDPR(ItSystemUsage itSystemUsage, bool withCrossReferences)
        {
            itSystemUsage.GeneralPurpose = A<string>();
            itSystemUsage.isBusinessCritical = A<DataOptions>();
            itSystemUsage.DPIA = A<DataOptions>();
            itSystemUsage.DPIADateFor = A<DateTime>();
            itSystemUsage.DPIASupervisionDocumentationUrlName = A<string>();
            itSystemUsage.DPIASupervisionDocumentationUrl = A<string>();
            itSystemUsage.HostedAt = A<HostedAt>();
            itSystemUsage.LinkToDirectoryUrlName = A<string>();
            itSystemUsage.LinkToDirectoryUrl = A<string>();
            itSystemUsage.SensitiveDataLevels = Many<SensitiveDataLevel>().Select(sensitiveDataLevel => new ItSystemUsageSensitiveDataLevel() { SensitivityDataLevel = sensitiveDataLevel }).ToList();
            //TODO: Attached options - cross reference - change from void to (sensitivepersondata and registertypedata)
            itSystemUsage.precautions = A<DataOptions>();
            itSystemUsage.precautionsOptionsAccessControl = A<bool>();
            itSystemUsage.precautionsOptionsEncryption = A<bool>();
            itSystemUsage.precautionsOptionsLogning = A<bool>();
            itSystemUsage.precautionsOptionsPseudonomisering = A<bool>();
            itSystemUsage.TechnicalSupervisionDocumentationUrlName = A<string>();
            itSystemUsage.TechnicalSupervisionDocumentationUrl = A<string>();
            itSystemUsage.answeringDataDPIA = A<DataOptions>();
            itSystemUsage.DPIAdeleteDate = A<DateTime>();
            itSystemUsage.numberDPIA = A<int>();
            itSystemUsage.riskAssessment = A<DataOptions>();
            itSystemUsage.riskAssesmentDate = A<DateTime>();
            itSystemUsage.RiskSupervisionDocumentationUrlName = A<string>();
            itSystemUsage.RiskSupervisionDocumentationUrl = A<string>();
            itSystemUsage.noteRisks = A<string>();
            itSystemUsage.preriskAssessment = A<RiskLevel>();
            itSystemUsage.UserSupervision = A<DataOptions>();
            itSystemUsage.UserSupervisionDate = A<DateTime>();
            itSystemUsage.UserSupervisionDocumentationUrlName = A<string>();
            itSystemUsage.UserSupervisionDocumentationUrl = A<string>();

            var sensitivePersonalDataTypes = Many<Guid>().Select(uuid => new SensitivePersonalDataType() { Id = A<int>(), Uuid = uuid, Name = A<string>() });
            var registerTypes = Many<Guid>().Select(uuid => new RegisterType() { Id = A<int>(), Uuid = uuid, Name = A<string>() });

        }

        /*
         *   private GDPRRegistrationsResponseDTO MapGDPR(ItSystemUsage systemUsage)
        {
            var personDataTypesMap = new Lazy<IDictionary<int, SensitivePersonalDataType>>(() => _sensitivePersonalDataTypeRepository.GetSensitivePersonalDataTypes().ToDictionary(type => type.Id));
            var registerTypesMap = new Lazy<IDictionary<int, RegisterType>>(() => _registerTypesRepository.Get().ToDictionary(type => type.Id));
            var attachedOptions = _attachedOptionRepository.GetBySystemUsageId(systemUsage.Id).ToList();

            return new GDPRRegistrationsResponseDTO
            {
                SensitivePersonData = attachedOptions
                    .Where(option => option.OptionType == OptionType.SENSITIVEPERSONALDATA)
                    .Where(option => personDataTypesMap.Value.ContainsKey(option.OptionId))
                    .Select(option => personDataTypesMap.Value[option.OptionId].MapIdentityNamePairDTO()),
                RegisteredDataCategories = attachedOptions
                    .Where(option => option.OptionType == OptionType.REGISTERTYPEDATA)
                    .Where(option => registerTypesMap.Value.ContainsKey(option.OptionId))
                    .Select(option => registerTypesMap.Value[option.OptionId].MapIdentityNamePairDTO()),
            };
        }
         */

        private void AssignArchiving(ItSystemUsage itSystemUsage, bool withOptionalCrossReferences)
        {
            itSystemUsage.ArchiveFromSystem = A<bool?>();
            itSystemUsage.ArchiveNotes = A<string>();
            itSystemUsage.Registertype = A<bool?>();
            itSystemUsage.ArchiveFreq = A<int?>();
            itSystemUsage.ArchiveLocation = withOptionalCrossReferences
                ? new ArchiveLocation { Uuid = A<Guid>(), Name = A<string>() }
                : null;
            itSystemUsage.ArchiveTestLocation = withOptionalCrossReferences ? new ArchiveTestLocation { Uuid = A<Guid>(), Name = A<string>() } : null;
            itSystemUsage.ArchiveType = withOptionalCrossReferences ? new ArchiveType { Uuid = A<Guid>(), Name = A<string>() } : null;
            itSystemUsage.SupplierId = withOptionalCrossReferences ? A<int>() : null;
            itSystemUsage.ArchivePeriods = Many<string>().Select(id => new ArchivePeriod
            {
                Approved = A<bool>(),
                StartDate = A<DateTime>(),
                EndDate = A<DateTime>(),
                UniqueArchiveId = id
            }).ToList();
        }

        private static void AssertRelation(SystemRelation expected, SystemRelationResponseDTO actual)
        {
            Assert.Equal(expected.Uuid, actual.Uuid);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.Reference, actual.UrlReference);
            AssertIdentity(expected.ToSystemUsage, actual.ToSystemUsage);
            AssertOptionalIdentity(expected.AssociatedContract, actual.AssociatedContract);
            AssertOptionalIdentity(expected.UsageFrequency, actual.RelationFrequency);
            AssertOptionalIdentity(expected.RelationInterface, actual.UsingInterface);
        }

        private static void AssertOptionalIdentity<T>(T optionalExpectedIdentity, IdentityNamePairResponseDTO actualIdentity) where T : IHasUuid, IHasName
        {
            if (optionalExpectedIdentity == null)
                Assert.Null(actualIdentity);
            else
                AssertIdentity(optionalExpectedIdentity, actualIdentity);
        }

        private void AssignSystemRelations(ItSystemUsage itSystemUsage)
        {
            itSystemUsage.UsageRelations = new[]
            {
                CreateSystemRelation(itSystemUsage,false,false,false),
                CreateSystemRelation(itSystemUsage,false,false,true),
                CreateSystemRelation(itSystemUsage,false,true,true),
                CreateSystemRelation(itSystemUsage,true,true,true)
            }.ToList();
        }

        private SystemRelation CreateSystemRelation(ItSystemUsage itSystemUsage, bool withContract = false, bool withFrequency = false, bool withInterface = false)
        {
            return new SystemRelation(itSystemUsage)
            {
                Uuid = A<Guid>(),
                Reference = A<string>(),
                AssociatedContract = withContract ? new ItContract() { Uuid = A<Guid>(), Name = A<string>() } : null,
                RelationInterface = withInterface ? new ItInterface { Uuid = A<Guid>(), Name = A<string>() } : null,
                UsageFrequency = withFrequency ? new RelationFrequencyType() { Uuid = A<Guid>(), Name = A<string>() } : null,
                ToSystemUsage = new ItSystemUsage() { Uuid = A<Guid>(), ItSystem = new ItSystem() { Name = A<string>() } },
                Description = A<string>()
            };
        }

        private static void AssertExternalReferences(ItSystemUsage itSystemUsage, List<ExternalReferenceDataDTO> dtoExternalReferences)
        {
            var actualMaster = Assert.Single(dtoExternalReferences, reference => reference.MasterReference);
            AssertExternalReference(itSystemUsage.Reference, actualMaster);
            Assert.Equal(itSystemUsage.ExternalReferences.Count, dtoExternalReferences.Count);

            foreach (var comparison in itSystemUsage.ExternalReferences.OrderBy(x => x.Title)
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

        private void AssignExternalReferences(ItSystemUsage itSystemUsage)
        {
            itSystemUsage.ExternalReferences = Many<string>().Select((title, i) => new ExternalReference
            {
                Title = title,
                URL = A<string>(),
                ExternalReferenceId = A<string>(),
                Id = i
            }).ToList();
            itSystemUsage.Reference = itSystemUsage.ExternalReferences.OrderBy(x => A<int>()).First();
        }

        private void AssignOrganizationalUsage(ItSystemUsage itSystemUsage)
        {
            var responsibleUsage = CreateOrganizationUnit();
            itSystemUsage.ResponsibleUsage = new ItSystemUsageOrgUnitUsage { OrganizationUnit = responsibleUsage };
            itSystemUsage.UsedBy = new[] { CreateOrganizationUnit(), CreateOrganizationUnit(), responsibleUsage }
                .Select(unit => new ItSystemUsageOrgUnitUsage { OrganizationUnit = unit }).ToList();
        }

        private OrganizationUnit CreateOrganizationUnit()
        {
            return new OrganizationUnit { Name = A<string>(), Uuid = A<Guid>() };
        }

        private void AssignKle(ItSystemUsage itSystemUsage)
        {
            itSystemUsage.TaskRefs = Many<Guid>().Select(x => new TaskRef() { TaskKey = A<string>(), Uuid = A<Guid>() }).ToList();
            itSystemUsage.TaskRefsOptOut = Many<Guid>().Select(x => new TaskRef() { TaskKey = A<string>(), Uuid = A<Guid>() }).ToList();
        }

        private void AssignRoles(ItSystemUsage itSystemUsage)
        {
            var rights = Many<Guid>().Select(id => new ItSystemRight()
            {
                User = CreateUser(),
                Role = new ItSystemRole() { Name = A<string>(), Uuid = id }
            }).ToList();
            itSystemUsage.Rights = rights;
        }
        private static void AssertUserCount(ItSystemUsage itSystemUsage, ExpectedUsersIntervalDTO generalNumberOfExpectedUsers)
        {
            (int? from, int? to) expected = itSystemUsage.UserCount switch
            {
                UserCount.BELOWTEN => (0, 9),
                UserCount.TENTOFIFTY => (10, 50),
                UserCount.FIFTYTOHUNDRED => (50, 100),
                UserCount.HUNDREDPLUS => (100, null),
                _ => throw new ArgumentOutOfRangeException()
            };
            Assert.Equal(expected, (generalNumberOfExpectedUsers.LowerBound, generalNumberOfExpectedUsers.UpperBound));
        }

        private static void AssertIdentities<T>(ICollection<T> sourceCollection, IEnumerable<IdentityNamePairResponseDTO> dtoCollection) where T : IHasUuid, IHasName
        {
            var expectedValues = sourceCollection.OrderBy(x => x.Name).ToList();
            var actualValues = dtoCollection.OrderBy(x => x.Name).ToList();

            Assert.Equal(expectedValues.Count, actualValues.Count);

            foreach (var comparison in expectedValues.Zip(actualValues, (expected, actual) => new { expected, actual }).ToList())
            {
                AssertIdentity(comparison.expected, comparison.actual);
            }
        }

        private void AssignGeneralPropertiesSection(ItSystemUsage itSystemUsage)
        {
            itSystemUsage.LocalSystemId = A<string>();
            itSystemUsage.LocalCallName = A<string>();
            itSystemUsage.Note = A<string>();
            itSystemUsage.UserCount = A<UserCount>();
            itSystemUsage.Version = A<string>();
            itSystemUsage.ItSystemCategories = new ItSystemCategories { Name = A<string>(), Uuid = A<Guid>() };
            itSystemUsage.MainContract = new ItContractItSystemUsage { ItContract = new ItContract() { Name = A<string>(), Uuid = A<Guid>() } };
            itSystemUsage.ItProjects = Many<string>().Select(name => new ItProject() { Name = name, Uuid = new Guid() }).ToList();
            itSystemUsage.Active = A<bool>();
            itSystemUsage.Concluded = A<DateTime>();
            itSystemUsage.ExpirationDate = A<DateTime>();
        }

        private void AssignBasicProperties(ItSystemUsage itSystemUsage)
        {
            itSystemUsage.LastChanged = A<DateTime>();
            itSystemUsage.Uuid = A<Guid>();
            itSystemUsage.ObjectOwner = CreateUser();
            itSystemUsage.LastChangedByUser = CreateUser();
            itSystemUsage.ItSystem = CreateSystem();
            itSystemUsage.Organization = CreateOrganization();
        }

        private static void AssertOrganization(Organization organization, ShallowOrganizationResponseDTO dtoOrganizationContext)
        {
            AssertIdentity(organization, dtoOrganizationContext);
            Assert.Equal(organization.Cvr, dtoOrganizationContext.Cvr);
        }

        private static void AssertIdentity(ItSystemUsage sourceIdentity, IdentityNamePairResponseDTO dto)
        {
            Assert.Equal(sourceIdentity.ItSystem.Name, dto.Name);
            Assert.Equal(sourceIdentity.Uuid, dto.Uuid);
        }

        private static void AssertIdentity<T>(T sourceIdentity, IdentityNamePairResponseDTO dto) where T : IHasUuid, IHasName
        {
            Assert.Equal(sourceIdentity.Name, dto.Name);
            Assert.Equal(sourceIdentity.Uuid, dto.Uuid);
        }

        private Organization CreateOrganization()
        {
            return new Organization { Name = A<string>(), Cvr = A<string>(), Uuid = A<Guid>() };
        }

        private ItSystem CreateSystem()
        {
            return new ItSystem
            {
                Uuid = A<Guid>(),
                Name = A<string>()
            };
        }

        private static void AssertUser(User user, IdentityNamePairResponseDTO dtoValue)
        {
            Assert.Equal((user.GetFullName(), user.Uuid), (dtoValue.Name, dtoValue.Uuid));
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
    }
}
