using System;
using System.Linq;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using Xunit;

namespace Tests.Unit.Presentation.Web.Mapping
{
    public class LifeCycleStatusMappingTest
    {
        [Fact]
        public void Can_Map_All_Choices_To_Types()
        {
            //Arrange
            var enumValues = Enum.GetValues(typeof(LifeCycleStatusChoice)).Cast<LifeCycleStatusChoice>();
            var targetEnumValues = Enum.GetValues(typeof(LifeCycleStatusType)).Cast<LifeCycleStatusType>().ToList();

            //Act
            foreach (var enumValue in enumValues)
            {
                var mappedValue = enumValue.ToLifeCycleStatusType();
                
                //Assert
                Assert.Single(targetEnumValues, x => x == mappedValue);
            }
        }
        [Fact]
        public void Can_Map_All_Types_To_Choices()
        {
            //Arrange
            var enumValues = Enum.GetValues(typeof(LifeCycleStatusType)).Cast<LifeCycleStatusType>();
            var targetEnumValues = Enum.GetValues(typeof(LifeCycleStatusChoice)).Cast<LifeCycleStatusChoice>().ToList();

            //Act
            foreach (var enumValue in enumValues)
            {
                var mappedValue = enumValue.ToLifeCycleStatusChoice();
                
                //Assert
                Assert.Single(targetEnumValues, x => x == mappedValue);
            }
        }
    }
}
