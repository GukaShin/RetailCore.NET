namespace RetailCore.Tests.Integration;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<RetailCoreWebApplicationFactory>;
