using Tests.Container.Helpers;

namespace Tests.Container.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class MigrationTestsCollection : ICollectionFixture<ContainerRuntimeFixture>
{
    public const string Name = "Migration tests";
}
