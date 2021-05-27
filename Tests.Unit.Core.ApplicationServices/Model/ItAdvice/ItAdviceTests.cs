using Core.DomainModel.Advice;
using System;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model.ItAdvice
{
    public class ItAdviceTest : WithAutoFixture
    {
        private readonly Advice _sut;

        public ItAdviceTest()
        {
            _sut = new Advice
            {
                Id = A<int>()
            };
        }

        [Fact]
        public void CanBeDeleted_Returns_False_When_Advice_Has_Been_Sent()
        {
            //Arrange
            _sut.AdviceSent.Add(createAdviceSent(_sut));

            //Act + Assert
            Assert.False(_sut.CanBeDeleted);
        }

        [Fact]
        public void CanBeDeleted_Returns_False_When_Advice_Is_Active()
        {
            //Arrange
            _sut.IsActive = true;

            //Act + Assert
            Assert.False(_sut.CanBeDeleted);
        }

        [Fact]
        public void CanBeDeleted_Returns_True_When_Advice_Is_Deactivated_And_Has_Not_Been_Sent()
        {
            //Arrange
            _sut.IsActive = false;
            _sut.AdviceSent.Clear();

            //Act + Assert
            Assert.True(_sut.CanBeDeleted);
        }


        private AdviceSent createAdviceSent(Advice advice)
        {
            return new AdviceSent
            {
                Advice = advice,
                AdviceId = advice.Id,
                Id = A<int>(),
                AdviceSentDate = A<DateTime>()
            };
        }
    }
}
