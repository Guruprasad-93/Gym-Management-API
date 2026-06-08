using Xunit;

namespace Gym.API.IntegrationTests.Infrastructure;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<GymWebApplicationFactory>
{
}
