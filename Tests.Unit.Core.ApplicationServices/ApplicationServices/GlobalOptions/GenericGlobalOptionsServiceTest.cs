﻿
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.GlobalOptions;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainServices;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.GlobalOptions
{
    public class TestOptionEntity : OptionEntity<TestReferenceType> { }
    public class TestReferenceType { }
    public class GenericGlobalOptionsServiceTest: WithAutoFixture
    {
        private readonly GenericGlobalOptionsService<TestOptionEntity, TestReferenceType> _sut;
        private readonly Mock<IGenericRepository<TestOptionEntity>> _globalOptionsRepository;
        private readonly Mock<IOrganizationalUserContext> _activeUserContext;
        private readonly Mock<IDomainEvents> _domainEvents;
        public GenericGlobalOptionsServiceTest()
        {
            _globalOptionsRepository = new Mock<IGenericRepository<TestOptionEntity>>();
            _activeUserContext = new Mock<IOrganizationalUserContext>();
            _domainEvents = new Mock<IDomainEvents>();
            _sut = new GenericGlobalOptionsService<TestOptionEntity, TestReferenceType>(_globalOptionsRepository.Object, _activeUserContext.Object, _domainEvents.Object);
        }

        [Fact]
        public void Can_Get_Options()
        {
            var expected = SetupRepositoryReturnsOneOption(A<Guid>());
            SetupIsGlobalAdmin();
            var result = _sut.GetGlobalOptions();

            Assert.True(result.Ok);
            var options = result.Value;
            Assert.Equivalent(expected, options);
        }

        [Fact]
        public void Get_Returns_Forbidden_If_Cannot_Read_All()
        {
            SetupIsNotGlobalAdmin();
            var result = _sut.GetGlobalOptions();

            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_Create_New_Option()
        {
            SetupIsGlobalAdmin();
            var existingOptions = SetupRepositoryReturnsOneOption(A<Guid>());
            Assert.NotNull(existingOptions);
            Assert.Equal(1, existingOptions.Count);
            var parameters = new GlobalRegularOptionCreateParameters()
            {
                Description = A<string>(),
                Name = A<string>(),
                IsObligatory = A<bool>()
            };

            var result = _sut.CreateGlobalOption(parameters);

            Assert.True(result.Ok);
            var option = result.Value;

            Assert.Equal(parameters.Description, option.Description);
            Assert.Equal(parameters.Name, option.Name);
            Assert.Equal(parameters.IsObligatory, option.IsObligatory);
            Assert.NotNull(option.Uuid);
            Assert.Equal(existingOptions.First().Priority + 1, option.Priority);
            Assert.False(option.IsEnabled);
            _globalOptionsRepository.Verify(_ => _.Insert(option));
            _globalOptionsRepository.Verify(_ => _.Save());
        }

        [Fact]
        public void Create_Returns_Forbidden_If_Not_Allowed()
        {
            SetupIsNotGlobalAdmin();
            var parameters = new GlobalRegularOptionCreateParameters()
            {
                Description = A<string>(),
                Name = A<string>(),
                IsObligatory = A<bool>()
            };

            var result = _sut.CreateGlobalOption(parameters);

            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Can_Patch_Option()
        {
            SetupIsGlobalAdmin();
            var optionUuid = A<Guid>();
            SetupRepositoryReturnsOneOption(optionUuid);
            var parameters = new GlobalRegularOptionUpdateParameters()
            {
                IsEnabled = Maybe<bool>.Some(A<bool>()).AsChangedValue(),
                IsObligatory = Maybe<bool>.Some(A<bool>()).AsChangedValue(),
                Name = Maybe<string>.Some(A<string>()).AsChangedValue(),
                Description = Maybe<string>.Some(A<string>()).AsChangedValue(),
            };

            var result = _sut.PatchGlobalOption(optionUuid, parameters);

            Assert.True(result.Ok);
            var option = result.Value;
            Assert.Equal(parameters.Description.NewValue.Value, option.Description);
            Assert.Equal(parameters.Name.NewValue.Value, option.Name);
            Assert.Equal(parameters.IsObligatory.NewValue.Value, option.IsObligatory);
            Assert.Equal(parameters.IsEnabled.NewValue.Value, option.IsEnabled);
        }

        [Fact]
        public void Patch_Option_Does_Nothing_If_No_Value_Changes()
        {
            SetupIsGlobalAdmin();
            var optionUuid = A<Guid>();
            var existing = SetupRepositoryReturnsOneOption(optionUuid).FirstOrDefault();
            Assert.NotNull(existing);
            var parameters = new GlobalRegularOptionUpdateParameters()
            {
                IsEnabled = OptionalValueChange<Maybe<bool>>.None,
                IsObligatory = OptionalValueChange<Maybe<bool>>.None,
                Name = OptionalValueChange<Maybe<string>>.None,
                Description = OptionalValueChange<Maybe<string>>.None,
            };

            var result = _sut.PatchGlobalOption(optionUuid, parameters);

            Assert.True(result.Ok);
            var option = result.Value;
            Assert.Equal(existing.Description, option.Description);
            Assert.Equal(existing.Name, option.Name);
            Assert.Equal(existing.IsObligatory, option.IsObligatory);
            Assert.Equal(existing.IsEnabled, option.IsEnabled);
        }

        [Fact]
        public void Patch_Returns_Forbidden_If_Not_Allowed()
        {
            SetupIsNotGlobalAdmin();
            var optionUuid = A<Guid>();
            SetupRepositoryReturnsOneOption(optionUuid);
            var parameters = new GlobalRegularOptionUpdateParameters()
            {
                IsEnabled = Maybe<bool>.Some(A<bool>()).AsChangedValue(),
                IsObligatory = Maybe<bool>.Some(A<bool>()).AsChangedValue(),
                Name = Maybe<string>.Some(A<string>()).AsChangedValue(),
                Description = Maybe<string>.Some(A<string>()).AsChangedValue(),
            };

            var result = _sut.PatchGlobalOption(optionUuid, parameters);

            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Patch_Returns_Not_Found_If_No_Existing_Option()
        {
            SetupIsGlobalAdmin();
            var optionUuid = A<Guid>();
            _globalOptionsRepository.Setup(_ => _.AsQueryable()).Returns(new List<TestOptionEntity>().AsQueryable());
            var parameters = new GlobalRegularOptionUpdateParameters()
            {
                IsEnabled = Maybe<bool>.Some(A<bool>()).AsChangedValue(),
                IsObligatory = Maybe<bool>.Some(A<bool>()).AsChangedValue(),
                Name = Maybe<string>.Some(A<string>()).AsChangedValue(),
                Description = Maybe<string>.Some(A<string>()).AsChangedValue(),
            };

            var result = _sut.PatchGlobalOption(optionUuid, parameters);

            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        private List<TestOptionEntity> SetupRepositoryReturnsOneOption(Guid uuid)
        {
            var expected = new List<TestOptionEntity>
            {
                new()
                {
                    Uuid = uuid,
                    Description = A<string>(),
                    Id = A<int>(),
                    IsEnabled = A<bool>(),
                    IsObligatory = A<bool>(),
                    Name = A<string>(),
                    Priority = A<int>()
                }
            };
            _globalOptionsRepository.Setup(_ => _.AsQueryable()).Returns(expected.AsQueryable());
            return expected;
        }

        private void SetupIsGlobalAdmin()
        {
            _activeUserContext.Setup(_ => _.IsGlobalAdmin()).Returns(true);
        }

        private void SetupIsNotGlobalAdmin()
        {
            _activeUserContext.Setup(_ => _.IsGlobalAdmin()).Returns(false);
        }
    }
}
