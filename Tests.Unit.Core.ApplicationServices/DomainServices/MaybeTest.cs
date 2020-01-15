using System;
using Core.DomainServices.Model.Result;
using Xunit;

namespace Tests.Unit.Core.DomainServices
{
    public class MaybeTest
    {
        [Fact]
        public static void None_Does_Not_Have_Value()
        {
            //Arrange
            var maybe = Maybe<string>.None;

            //Act
            var maybeHasItem = maybe.HasValue;

            //Assert
            Assert.False(maybeHasItem);
        }

        [Fact]
        public static void None_Throws_On_Get_Value()
        {
            //Arrange
            var maybe = Maybe<string>.None;

            //Act + Assert
            Assert.Throws<InvalidOperationException>(() => maybe.Value);
        }

        [Fact]
        public void Some_Does_Have_Value()
        {
            //Arrange
            var expectedValue = Guid.NewGuid();

            //Act
            var maybe = Maybe<Guid>.Some(expectedValue);

            //Assert
            Assert.True(maybe.HasValue);
            Assert.Equal(expectedValue, maybe.Value);
        }

        [Fact]
        public void GetValueOrFallback_Returns_Value_If_HasValue()
        {
            //Arrange
            var expectedValue = Guid.NewGuid();
            var fallback = Guid.NewGuid();

            //Act
            var maybe = Maybe<Guid>.Some(expectedValue);

            //Assert
            Assert.True(maybe.HasValue);
            Assert.Equal(expectedValue, maybe.GetValueOrFallback(fallback));
        }

        [Fact]
        public void GetValueOrFallback_Returns_Value_If_Not_HasValue()
        {
            //Arrange
            var fallback = Guid.NewGuid();

            //Act
            var maybe = Maybe<Guid>.None;

            //Assert
            Assert.False(maybe.HasValue);
            Assert.Equal(fallback, maybe.GetValueOrFallback(fallback));
        }

        [Fact]
        public void Select_Maps_Returns_Mapped_Value()
        {
            //Arrange
            var originalValue = Guid.NewGuid();
            var anotherValue = Guid.NewGuid();
            var expectedResult = $"{originalValue}{anotherValue}";
            var maybe = Maybe<Guid>.Some(originalValue);

            //Act
            var mapped = maybe.Select(org => $"{org}{anotherValue}");

            //Assert
            Assert.True(mapped.HasValue);
            Assert.Equal(expectedResult, mapped.Value);
        }

        [Fact]
        public void Select_Maps_Returns_None_If_Original_Value_Is_None()
        {
            //Arrange
            var maybe = Maybe<Guid>.None;

            //Act
            var mapped = maybe.Select(org => $"{org}{Guid.NewGuid()}");

            //Assert
            Assert.False(mapped.HasValue);
        }

        [Fact]
        public void Select_Enables_Linq()
        {
            //Arrange
            var initial = Maybe<int[]>.Some(new[] { 1, 2 });

            //Act
            var mapped = from numbers in initial
                         select string.Join(".", numbers);

            //Assert
            Assert.Equal("1.2", mapped.Value);
        }
    }
}
