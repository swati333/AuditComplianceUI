namespace Audit.IntegrationTests;

/// <summary>Shares one SQL Server container/host across every test class in this collection (one container start, not one per class).</summary>
[CollectionDefinition(Name)]
public sealed class AuditApiCollection : ICollectionFixture<AuditApiFactory>
{
    public const string Name = "AuditApi";
}
