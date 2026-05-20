using Xunit;

namespace IntegrationTests;

/// <summary>
/// Single SQL Server container and one factory shared by all integration tests,
/// avoiding multiple parallel MSSQL pulls/starts on CI runners.
/// </summary>
[CollectionDefinition("IntegrationTests")]
public sealed class IntegrationTestsCollection : ICollectionFixture<IntegrationTestWebApplicationFactory>
{
}
