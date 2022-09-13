using System;
using System.Linq;
using Core.Abstractions.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Extensions
{
    public class EnumerableMirrorExtensionsTest : WithAutoFixture
    {
        public class TSourceType
        {
            /// <summary>
            /// On type 1 the id is in the original form - Guid
            /// </summary>
            public Guid Id { get; set; }
        }

        public class TDestinationType
        {
            /// <summary>
            /// On Type 2 the id is on string form
            /// </summary>
            public string IdAsString { get; set; }
        }


        [Fact]
        public void ComputeMirrorActions_Detects_New_Items()
        {
            //Arrange
            var sourceTypes = Many<TSourceType>(10).ToList();
            var destinationTypes = sourceTypes.Select(x => new TDestinationType { IdAsString = x.Id.ToString("D") }).ToList();
            var newItems = Many<Guid>(2).Select(id => new TSourceType() { Id = id }).ToList();

            //Add the two new items to the source collection and randomize the order
            sourceTypes = sourceTypes
                .Concat(newItems)
                .ToList()
                .OrderBy(_ => A<int>())
                .ToList();

            //Act
            var contexts = sourceTypes
                .ComputeMirrorActions(destinationTypes, x => x.Id.ToString("D"), x => x.IdAsString)
                .GroupBy(x => x.Action)
                .ToDictionary(x => x.Key, x => x.ToList());

            //Assert
            var additions = contexts[EnumerableMirrorExtensions.MirrorAction.AddToDestination];
            Assert.Equal(2, additions.Count);

            foreach (var actionContext in additions)
            {
                Assert.True(actionContext.SourceValue.HasValue);
                Assert.False(actionContext.DestinationValue.HasValue);

                var sourceValue = actionContext.SourceValue.Value;
                Assert.Contains(sourceValue, newItems);
            }
        }

        [Fact]
        public void ComputeMirrorActions_Detects_Existing_Items()
        {
            //Arrange
            const int amountToMerge = 10;
            var sourceTypes = Many<TSourceType>(amountToMerge).ToList();
            var destinationTypes = sourceTypes.Select(x => new TDestinationType { IdAsString = x.Id.ToString("D") }).ToList();
            var newItems = Many<Guid>(2).Select(id => new TSourceType() { Id = id }).ToList();

            //Add the two new items to the source collection and randomize the order
            sourceTypes = sourceTypes
                .Concat(newItems)
                .ToList()
                .OrderBy(_ => A<int>())
                .ToList();

            //Act
            var contexts = sourceTypes
                .ComputeMirrorActions(destinationTypes, x => x.Id.ToString("D"), x => x.IdAsString)
                .GroupBy(x => x.Action)
                .ToDictionary(x => x.Key, x => x.ToList());

            //Assert
            var itemsToMerge = contexts[EnumerableMirrorExtensions.MirrorAction.MergeToDestination];
            Assert.Equal(amountToMerge, itemsToMerge.Count);

            foreach (var actionContext in itemsToMerge)
            {
                Assert.True(actionContext.SourceValue.HasValue);
                Assert.True(actionContext.DestinationValue.HasValue);

                var sourceValue = actionContext.SourceValue.Value;
                Assert.Contains(sourceValue, sourceTypes);

                var destinationValue = actionContext.DestinationValue.Value;
                Assert.Contains(destinationValue, destinationTypes);
            }
        }

        [Fact]
        public void ComputeMirrorActions_Detects_Removed_Items()
        {
            //Arrange
            var sourceTypes = Many<TSourceType>(10).ToList();
            var destinationTypes = sourceTypes.Select(x => new TDestinationType { IdAsString = x.Id.ToString("D") }).ToList();
            const int itemsToRemove = 2;

            var removed = sourceTypes.OrderBy(_ => A<int>()).Take(itemsToRemove).ToList();
            sourceTypes = sourceTypes.Except(removed).ToList();

            //Act
            var contexts = sourceTypes
                .ComputeMirrorActions(destinationTypes, x => x.Id.ToString("D"), x => x.IdAsString)
                .GroupBy(x => x.Action)
                .ToDictionary(x => x.Key, x => x.ToList());

            //Assert
            var itemsToMerge = contexts[EnumerableMirrorExtensions.MirrorAction.RemoveFromDestination];
            Assert.Equal(itemsToRemove, itemsToMerge.Count);

            foreach (var actionContext in itemsToMerge)
            {
                Assert.False(actionContext.SourceValue.HasValue);
                Assert.True(actionContext.DestinationValue.HasValue);

                var destinationValue = actionContext.DestinationValue.Value;
                Assert.Contains(destinationValue, destinationTypes);
            }
        }
    }
}
