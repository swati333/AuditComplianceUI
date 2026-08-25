namespace Finding.IntegrationTests;

[Collection(FindingApiCollection.Name)]
public abstract class IntegrationTestBase
{
    protected readonly HttpClient Client;
    protected readonly FindingApiFactory Factory;

    protected IntegrationTestBase(FindingApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }
}
