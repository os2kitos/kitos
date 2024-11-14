using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices
{
    public class HelpTextServiceTest: WithAutoFixture
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Validate_Key(bool expectAvailable)
        {
            var key = A<string>();
            var repository = new Mock<IGenericRepository<HelpText>>();
            if (expectAvailable)
            {
                repository.Setup(_ => _.AsQueryable()).Returns(new List<HelpText>().AsQueryable()); }
            else
            {
                repository.Setup(_ => _.AsQueryable()).Returns(new List<HelpText>()
                {
                    new HelpText() { Key = key, Description = A<string>(), Title = A<string>() }
                }.AsQueryable());
            }

            var sut = new HelpTextService(repository.Object);

            var validKey = sut.IsAvailableKey(key);

            Assert.Equal(expectAvailable, validKey);

        }
    }
}
