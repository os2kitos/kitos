using System.Linq;
using Infrastructure.Services.Types;
using Xunit;

namespace Tests.Unit.Core.Infrastructure
{
    public class EnumerableExtensionsTest
    {
        [Fact]
        public void UpdateTo_Adds_New_Items_To_Target_Collection()
        {
            //Arrange
            var currentState = new[] { 1, 2, 3, 4 }.ToList();
            var nextState = currentState.Append(5).Append(6).ToList();

            //Act
            nextState.MirrorTo(currentState, val => val);

            //Assert that current state was updated with the additions made in next state
            Assert.Equal(nextState, currentState);
        }

        [Fact]
        public void UpdateTo_Removes_Items_Absent_In_NewState_From_Target_Collection()
        {
            //Arrange
            var currentState = new[] { 1, 2, 3, 4 }.ToList();
            var nextState = currentState.Skip(1).Take(2).ToList();

            //Act
            nextState.MirrorTo(currentState, val => val);

            //Assert that current state was updated to match the nextState
            Assert.Equal(nextState, currentState);
        }

        [Fact]
        public void UpdateTo_Empty()
        {
            //Arrange
            var currentState = new[] { 1, 2, 3, 4 }.ToList();
            var nextState = Enumerable.Empty<int>().ToList();

            //Act
            nextState.MirrorTo(currentState, val => val);

            //Assert that current state was updated to match the nextState
            Assert.Equal(nextState, currentState);
        }

        [Fact]
        public void UpdateTo_From_Empty()
        {
            //Arrange
            var currentState = Enumerable.Empty<int>().ToList();
            var nextState = new[] { 1, 2, 3 }.ToList();

            //Act
            nextState.MirrorTo(currentState, val => val);

            //Assert that current state was updated to match the nextState
            Assert.Equal(nextState, currentState);
        }
    }
}
