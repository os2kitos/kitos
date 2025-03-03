using AutoFixture;
using PubSub.Application.DTOs;
using PubSub.Application.Mapping;
using PubSub.Test.Base.Tests.Toolkit.Patterns;

namespace PubSub.Test.Unit.Application
{
    public class PublishRequestMapperTest: WithAutoFixture
    {
        private PublishRequestMapper _sut;

        public PublishRequestMapperTest()
        {
            _sut = new PublishRequestMapper();
        }

        [Fact]
        public void Can_Map_From_Dto()
        {
            var dto = A<PublishRequestDto>();

            var publication = _sut.FromDto(dto);

            Assert.Equal(dto.Topic, publication.Topic.Name);
            Assert.Equal(dto.Message, publication.Message);
        }
    }
}
