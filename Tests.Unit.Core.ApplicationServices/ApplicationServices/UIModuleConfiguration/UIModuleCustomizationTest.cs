using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.DomainModel.UIConfiguration;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.UIModuleConfiguration
{
    public class UIModuleCustomizationTest
    {
        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void UpdateConfigurationNodes_Preserves_Existing_Node_Identity(bool enabled, bool recommended)
        {
            var existingNode = new CustomizedUINode
            {
                Id = 42, ModuleId = 7, Key = "Existing.Key", Enabled = true, Recommended = false
            };
            var removedNode = new CustomizedUINode { Key = "Removed.Key" };
            var addedNode = new CustomizedUINode { Key = "Added.Key" };
            var sut = new UIModuleCustomization
            {
                Nodes = new List<CustomizedUINode> { existingNode, removedNode }
            };

            var result = sut.UpdateConfigurationNodes(new[]
            {
                new CustomizedUINode { Key = existingNode.Key, Enabled = enabled, Recommended = recommended },
                addedNode
            });

            Assert.False(result.HasValue);
            Assert.Equal(2, sut.Nodes.Count);
            Assert.Contains(existingNode, sut.Nodes);
            Assert.Contains(addedNode, sut.Nodes);
            Assert.DoesNotContain(removedNode, sut.Nodes);
            Assert.Equal(42, existingNode.Id);
            Assert.Equal(7, existingNode.ModuleId);
            Assert.Equal(enabled, existingNode.Enabled);
            Assert.Equal(recommended, existingNode.Recommended);
        }

        [Fact]
        public void UpdateConfigurationNodes_Returns_BadInput_When_Nodes_Are_Null()
        {
            var sut = new UIModuleCustomization();

            var result = sut.UpdateConfigurationNodes(null);

            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
        }

        [Fact]
        public void UpdateConfigurationNodes_Returns_BadInput_When_Key_Is_Invalid()
        {
            var sut = new UIModuleCustomization();
            var nodes = new List<CustomizedUINode> { new() { Key = "invalid key", Enabled = true } };

            var result = sut.UpdateConfigurationNodes(nodes);

            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
        }

        [Fact]
        public void UpdateConfigurationNodes_Returns_BadInput_When_Keys_Are_Duplicate()
        {
            var sut = new UIModuleCustomization();
            var nodes = new List<CustomizedUINode>
            {
                new() { Key = "Valid.Key", Enabled = true },
                new() { Key = "Valid.Key", Enabled = false }
            };

            var result = sut.UpdateConfigurationNodes(nodes);

            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.BadInput, result.Value.FailureType);
        }

        [Fact]
        public void UpdateConfigurationNodes_Updates_Nodes_When_Input_Is_Valid()
        {
            var sut = new UIModuleCustomization
            {
                Nodes = new List<CustomizedUINode> { new() { Key = "Old.Key", Enabled = false } }
            };
            var nodes = new List<CustomizedUINode>
            {
                new() { Key = "New.Key", Enabled = true }
            };

            var result = sut.UpdateConfigurationNodes(nodes);

            Assert.False(result.HasValue);
            Assert.Single(sut.Nodes);
            Assert.Contains(sut.Nodes, x => x.Key == "New.Key" && x.Enabled);
        }

        [Fact]
        public void UpdateConfigurationNodes_Updates_Recommended_When_Key_And_Enabled_Are_Unchanged()
        {
            var sut = new UIModuleCustomization
            {
                Nodes = new List<CustomizedUINode>
                {
                    new() { Key = "Existing.Key", Enabled = true, Recommended = false }
                }
            };
            var nodes = new List<CustomizedUINode>
            {
                new() { Key = "Existing.Key", Enabled = true, Recommended = true }
            };

            var result = sut.UpdateConfigurationNodes(nodes);

            Assert.False(result.HasValue);
            var updatedNode = Assert.Single(sut.Nodes);
            Assert.Equal("Existing.Key", updatedNode.Key);
            Assert.True(updatedNode.Enabled);
            Assert.True(updatedNode.Recommended);
        }
    }
}
