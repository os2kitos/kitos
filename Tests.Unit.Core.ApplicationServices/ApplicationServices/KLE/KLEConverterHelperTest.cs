using System;
using System.Xml.Linq;
using Core.DomainServices.Repositories.KLE;
using Core.DomainServices.Time;
using Infrastructure.Services.KLEDataBridge;
using Moq;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.KLE
{
    public class KLEConverterHelperTest
    {
        [Fact]
        private void ConvertToTaskRefs_Given_Sample_Creates_Valid_TaskRefs()
        {
            //Arrange
            var mockKLEDataBridge = new Mock<IKLEDataBridge>();
            var document = XDocument.Load("./ApplicationServices/KLE/20200106-kle-single-item.xml");
            mockKLEDataBridge.Setup(b => b.GetAllActiveKleNumbers()).Returns(document);
            var sut = new KLEConverterHelper(Mock.Of<IOperationClock>(x=>x.Now == DateTime.MinValue));

            //Act
            var result = sut.ConvertToTaskRefs(document);

            //Assert
            Assert.True(result.TryGet("00", out var mainGroup));
            Assert.Equal(Guid.Parse("4eba8818-da19-4e37-b3e1-b9b6be2f2f13"), mainGroup.Uuid);
            Assert.True(result.TryGet("00.05.00", out var item));
            Assert.Equal(Guid.Parse("0cc253a9-163f-4563-acdf-b34a477f3ad1"), item.Uuid);
        }
    }
}
