
using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.ApplicationServices.GlobalOptions;
using Core.DomainModel;
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
        public GenericGlobalOptionsServiceTest()
        {
            _globalOptionsRepository = new Mock<IGenericRepository<TestOptionEntity>>();
            _sut = new GenericGlobalOptionsService<TestOptionEntity, TestReferenceType>(_globalOptionsRepository.Object);
        }

        [Fact]
        public void Can_Get_Options()
        {
            var expected = new List<TestOptionEntity>
            {
                new()
                {
                    Description = A<string>(),
                    Id = A<int>(),
                    IsEnabled = A<bool>(),
                    IsObligatory = A<bool>(),
                    Name = A<string>()
                }
            };
            _globalOptionsRepository.Setup(_ => _.AsQueryable()).Returns(expected.AsQueryable());

            var result = _sut.GetGlobalOptions();

            Assert.True(result.Ok);
            var options = result.Value;
            Assert.Equivalent(expected, options);
        }
    }
}
