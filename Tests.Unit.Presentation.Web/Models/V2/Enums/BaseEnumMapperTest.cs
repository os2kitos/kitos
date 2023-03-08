using System;
using System.Linq;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2.Enums
{
    public abstract class BaseEnumMapperTest<TChoice,TDomainEnum> where TChoice: Enum where TDomainEnum: Enum 
    {
        [Fact]
        public void Can_Map_All_Choices_To_Types()
        {
            //Arrange
            var enumValues = Enum.GetValues(typeof(TChoice)).Cast<TChoice>().ToList();
            var targetEnumValues = Enum.GetValues(typeof(TDomainEnum)).Cast<TDomainEnum>().ToList();

            //Act
            foreach (var enumValue in enumValues)
            {
                var mappedValue = ToDomainEnum(enumValue);
                
                //Assert
                Assert.Single(targetEnumValues, x =>  x.Equals(mappedValue));
            }
        }

        public abstract TDomainEnum ToDomainEnum(TChoice value);
        public abstract TChoice ToChoice(TDomainEnum value);


        [Fact]
        public void Can_Map_All_Types_To_Choices()
        {
            //Arrange
            var enumValues = Enum.GetValues(typeof(TDomainEnum)).Cast<TDomainEnum>().ToList(); 
            var targetEnumValues = Enum.GetValues(typeof(TChoice)).Cast<TChoice>().ToList();

            //Act
            foreach (var enumValue in enumValues)
            {
                var mappedValue = ToChoice(enumValue);

                //Assert
                Assert.Single(targetEnumValues, x => x.Equals(mappedValue));
            }
        }
    }
}
