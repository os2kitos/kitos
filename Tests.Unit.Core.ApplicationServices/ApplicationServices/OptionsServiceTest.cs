using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Core.DomainServices.Options;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices
{
    public class OptionsServiceTest : WithAutoFixture
    {
        private readonly OptionsService<SystemRelation, RelationFrequencyType, LocalRelationFrequencyType> _sut;
        private readonly Mock<IGenericRepository<LocalRelationFrequencyType>> _localOptionsRepository;
        private readonly Mock<IGenericRepository<RelationFrequencyType>> _globalOptionsRepository;

        public OptionsServiceTest()
        {
            //NOTE: If used types are removed, replace them by someone else to satisfy the generic constraint
            _localOptionsRepository = new Mock<IGenericRepository<LocalRelationFrequencyType>>();
            _globalOptionsRepository = new Mock<IGenericRepository<RelationFrequencyType>>();
            _localOptionsRepository.Setup(x => x.AsQueryable()).Returns(new List<LocalRelationFrequencyType>().AsQueryable());
            _globalOptionsRepository.Setup(x => x.AsQueryable()).Returns(new List<RelationFrequencyType>().AsQueryable());
            _sut = new OptionsService<SystemRelation, RelationFrequencyType, LocalRelationFrequencyType>(
                _localOptionsRepository.Object,
                _globalOptionsRepository.Object);
        }

        [Fact]
        public void GetAvailableOptions_Returns_All_Locally_Enabled_Options_From_Same_Organization()
        {
            //Arrange
            var organizationId = A<int>();
            var enabledOption1 = MakeOptionPair(organizationId, true, true);
            var enabledOption2 = MakeOptionPair(organizationId, true, true);
            var discardedOption1 = MakeOptionPair(organizationId, true, false);
            var discardedOption2 = MakeOptionPair(organizationId, false, true);
            var discardedOption3 = MakeOptionPair(organizationId + 1, true, true);
            SetupRepositories(enabledOption1, enabledOption2, discardedOption1, discardedOption2, discardedOption3);

            //Act
            var frequencyTypes = _sut.GetAvailableOptions(organizationId).ToList();

            //Assert
            Assert.Equal(2, frequencyTypes.Count);
            Assert.True(frequencyTypes.Select(x => x.Id).OrderBy(x=>x).SequenceEqual(new[] { enabledOption1.global.Id, enabledOption2.global.Id }.OrderBy(x=>x)));
        }

        [Fact]
        public void GetAvailableOptions_Returns_All_Obligatory_Enabled_Options()
        {
            //Arrange
            var organizationId = A<int>();
            var enabledOption1 = MakeOptionPair(organizationId, true, true, true);
            var enabledOption2 = MakeOptionPair(organizationId, false, true, true);
            var discardedOption = MakeOptionPair(organizationId, false, true);
            SetupRepositories(enabledOption1, enabledOption2, discardedOption);

            //Act
            var frequencyTypes = _sut.GetAvailableOptions(organizationId).ToList();

            //Assert
            Assert.Equal(2, frequencyTypes.Count);
            Assert.True(frequencyTypes.Select(x => x.Id).OrderBy(x => x).SequenceEqual(new[] { enabledOption1.global.Id, enabledOption2.global.Id }.OrderBy(x => x)));
        }

        private void SetupRepositories(params (LocalRelationFrequencyType local, RelationFrequencyType global)[] options)
        {
            _localOptionsRepository.Setup(x => x.AsQueryable()).Returns(options.Select(x => x.local).AsQueryable());
            _globalOptionsRepository.Setup(x => x.AsQueryable()).Returns(options.Select(x => x.global).AsQueryable());
        }

        private (LocalRelationFrequencyType local, RelationFrequencyType global) MakeOptionPair(
            int orgId,
            bool locallyEnabled = false,
            bool globallyEnabled = false,
            bool globallyObligatory = false)
        {
            var localRelationFrequencyType = new LocalRelationFrequencyType
            {
                Id = A<int>(),
                IsActive = locallyEnabled,
                OrganizationId = orgId
            };
            var relationFrequencyType = new RelationFrequencyType
            {
                Id = A<int>(),
                IsEnabled = globallyEnabled,
                IsObligatory = globallyObligatory
            };

            var option = new Mock<OptionEntity<RelationFrequencyType>>();

            localRelationFrequencyType.Option = option.Object;
            localRelationFrequencyType.OptionId = relationFrequencyType.Id;
            return (localRelationFrequencyType, relationFrequencyType);
        }
    }
}
