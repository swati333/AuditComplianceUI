namespace Notification.IntegrationTests;

[Collection(NotificationApiCollection.Name)]
public abstract class IntegrationTestBase
{
    protected readonly HttpClient Client;
    protected readonly NotificationApiFactory Factory;

    protected IntegrationTestBase(NotificationApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }
}
