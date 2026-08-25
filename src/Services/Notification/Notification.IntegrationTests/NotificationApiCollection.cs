namespace Notification.IntegrationTests;

/// <summary>Shares one SQL Server container/host across every test class in this collection.</summary>
[CollectionDefinition(Name)]
public sealed class NotificationApiCollection : ICollectionFixture<NotificationApiFactory>
{
    public const string Name = "NotificationApi";
}
