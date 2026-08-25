namespace Finding.IntegrationTests;

/// <summary>Shares one SQL Server container/host across every test class in this collection.</summary>
[CollectionDefinition(Name)]
public sealed class FindingApiCollection : ICollectionFixture<FindingApiFactory>
{
    public const string Name = "FindingApi";
}
