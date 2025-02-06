using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.HelpTexts;
using Core.ApplicationServices.Model.HelpTexts;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainServices;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.HelpTexts
{
    public class HelpTextApplicationServiceTest: WithAutoFixture
    {
        private readonly HelpTextApplicationService _sut;
        private readonly Mock<IGenericRepository<HelpText>> _helpTextsRepository;
        private readonly Mock<IOrganizationalUserContext> _userContext;
        private readonly Mock<IDomainEvents> _domainEvents;
        private readonly Mock<IHelpTextService> _helpTextService;

        public HelpTextApplicationServiceTest()
        {
            _domainEvents = new Mock<IDomainEvents>();
            _helpTextsRepository = new Mock<IGenericRepository<HelpText>>();
            _userContext = new Mock<IOrganizationalUserContext>();
            _helpTextService = new Mock<IHelpTextService>();
            _sut = new HelpTextApplicationService(_helpTextsRepository.Object, _userContext.Object, _domainEvents.Object, _helpTextService.Object);
        }

        [Fact]
        public void Can_Get_Help_Text()
        {
            var expected = SetupRepositoryReturnsOne();
            var result = _sut.GetHelpText(expected.Key);

            Assert.True(result.Ok);
            Assert.Equivalent(expected, result.Value);
        }

        [Fact]
        public void Get_Help_Text_Returns_Not_Found_If_Not_Found()
        {
            _helpTextsRepository.Setup(_ => _.AsQueryable()).Returns(new List<HelpText>().AsQueryable());
            var result = _sut.GetHelpText(A<string>());

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Can_Get_Help_Texts()
        {
            var expected = SetupRepositoryReturnsOne();
            var result = _sut.GetHelpTexts();

            var helpText = result.FirstOrDefault();
            Assert.NotNull(helpText);
            Assert.Equivalent(expected, helpText);
        }

        [Fact]
        public void Create_Help_Text_Returns_Forbidden_If_Not_Global_Admin()
        {
            SetupIsNotGlobalAdmin();

            var parameters = A<HelpTextCreateParameters>();

            var result = _sut.CreateHelpText(parameters);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Create_Help_Text_Returns_Conflict_If_Existing_Key()
        {
            SetupIsGlobalAdmin();

            var parameters = A<HelpTextCreateParameters>();
            _helpTextService.Setup(_ => _.IsAvailableKey(parameters.Key)).Returns(false);

            var result = _sut.CreateHelpText(parameters);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Conflict, result.Error.FailureType);
        }

        [Fact]
        public void Can_Create_Help_Text()
        {
            SetupIsGlobalAdmin();
            var parameters = A<HelpTextCreateParameters>();
            _helpTextService.Setup(_ => _.IsAvailableKey(parameters.Key)).Returns(true);

            var result = _sut.CreateHelpText(parameters);

            Assert.True(result.Ok);
            var helpText = result.Value;
            Assert.Equal(parameters.Description, helpText.Description);
            Assert.Equal(parameters.Title, helpText.Title);
            Assert.Equal(parameters.Key, helpText.Key);
        }

        [Fact]
        public void Can_Delete_Help_Text()
        {
            var existing = SetupRepositoryReturnsOne();
            SetupIsGlobalAdmin();

            var errorMaybe = _sut.DeleteHelpText(existing.Key);

            Assert.False(errorMaybe.HasValue);
            _helpTextsRepository.Verify(_ => _.Delete(existing));

        }

        [Fact]
        public void Can_Patch_Help_Text_Except_Key()
        {
            var existing = SetupRepositoryReturnsOne();
            SetupIsGlobalAdmin();
            var parameters = new HelpTextUpdateParameters()
            {
                Title = A<Maybe<string>>().AsChangedValue(),
                Description = A<Maybe<string>>().AsChangedValue()
            };

            var result = _sut.PatchHelpText(existing.Key, parameters);

            Assert.True(result.Ok);
            var helpText = result.Value;
            Assert.Equal(parameters.Description.NewValue.Value, helpText.Description);
            Assert.Equal(parameters.Title.NewValue.Value, helpText.Title);
            Assert.Equal(existing.Key, helpText.Key);
        }

        [Fact]
        public void Patch_Help_Text_Returns_Forbidden_If_Not_Global_Admin()
        {
            SetupIsNotGlobalAdmin();

            var parameters = A<HelpTextUpdateParameters>();

            var result = _sut.PatchHelpText(A<string>(), parameters);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Delete_Help_Text_Returns_Forbidden_If_Not_Global_Admin()
        {
            SetupIsNotGlobalAdmin();

            var result = _sut.DeleteHelpText(A<string>());

            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.Forbidden, result.Value.FailureType);
        }


        private HelpText SetupRepositoryReturnsOne()
        {
            var existing = new HelpText()
            {
                Description = A<string>(),
                Key = A<string>(),
                Title = A<string>()
            };
            _helpTextsRepository.Setup(_ => _.AsQueryable()).Returns(new List<HelpText>() { existing }.AsQueryable());
            return existing;
        }

        private void SetupIsGlobalAdmin()
        {
            _userContext.Setup(_ => _.IsGlobalAdmin()).Returns(true);
        }

        private void SetupIsNotGlobalAdmin()
        {
            _userContext.Setup(_ => _.IsGlobalAdmin()).Returns(false);
        }


    }
}
