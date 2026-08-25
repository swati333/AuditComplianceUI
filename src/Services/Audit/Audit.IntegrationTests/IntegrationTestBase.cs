namespace Audit.IntegrationTests;

[Collection(AuditApiCollection.Name)]
public abstract class IntegrationTestBase
{
    protected readonly HttpClient Client;
    protected readonly AuditApiFactory Factory;

    protected IntegrationTestBase(AuditApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }
}
