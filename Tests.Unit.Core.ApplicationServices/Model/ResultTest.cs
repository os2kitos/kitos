﻿using System;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Xunit;

namespace Tests.Unit.Core.Model
{
    public class ResultTest
    {
        [Fact]
        public void Success_Returns_OkResult_With_Value()
        {
            var value = Guid.NewGuid();

            var result = Result<Guid, bool>.Success(value);

            Assert.True(result.Ok);
            Assert.Equal(value, result.Value);
            Assert.Throws<InvalidOperationException>(() => result.Error);
        }

        [Fact]
        public void Success_Returns_ErrorResult_With_ErrorValue()
        {
            var value = Guid.NewGuid();

            var result = Result<bool, Guid>.Failure(value);

            Assert.False(result.Ok);
            Assert.Equal(value, result.Error);
            Assert.Throws<InvalidOperationException>(() => result.Value);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MatchFailure_Returns_Maybe_Error(bool hasError)
        {
            //Arrange
            var value = Guid.NewGuid();

            //Act
            var result = (hasError ? Result<Guid, Guid>.Failure(value) : Result<Guid, Guid>.Success(value)).MatchFailure();

            //Assert
            if (hasError)
            {
                Assert.True(result.HasValue);
                Assert.Equal(value, result.Value);
            }
            else
            {
                Assert.True(result.IsNone);
            }
        }
    }
}
