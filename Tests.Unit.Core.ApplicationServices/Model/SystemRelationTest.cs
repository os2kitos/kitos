using Core.DomainModel.ItSystemUsage;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model
{
    public class SystemRelationTest : WithAutoFixture
    {
        private readonly ItSystemUsage _fromSystemUsage;
        private readonly SystemRelation _sut;

        public SystemRelationTest()
        {
            _fromSystemUsage = new ItSystemUsage { Id = A<int>(), OrganizationId = A<int>() };
            _sut = new SystemRelation(_fromSystemUsage);
        }

        //[Fact]
        //public void //TODO:
    }
}
