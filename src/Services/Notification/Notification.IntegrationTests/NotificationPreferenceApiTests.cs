using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Notification.Contracts.Dtos;
using Notification.Contracts.Requests;

namespace Notification.IntegrationTests;

public sealed class NotificationPreferenceApiTests : IntegrationTestBase
{
    public NotificationPreferenceApiTests(NotificationApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetForUser_defaults_every_channel_to_enabled_when_no_preference_row_exists()
    {
        var userId = $"user-{Guid.NewGuid()}";

        var response = await Client.GetAsync($"/api/v1/notification-preferences/{userId}");

        var preferences = await response.Content.ReadFromJsonAsync<List<NotificationPreferenceDto>>();
        preferences!.Should().OnlyContain(p => p.IsEnabled);
        preferences.Select(p => p.Channel).Should().BeEquivalentTo(["Email", "InApp"]);
    }

    [Fact]
    public async Task Upsert_disables_a_channel_and_GetForUser_reflects_it()
    {
        var userId = $"user-{Guid.NewGuid()}";

        var putResponse = await Client.PutAsJsonAsync($"/api/v1/notification-preferences/{userId}/Email", new UpsertPreferenceRequest(false));
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await Client.GetAsync($"/api/v1/notification-preferences/{userId}");
        var preferences = await getResponse.Content.ReadFromJsonAsync<List<NotificationPreferenceDto>>();

        preferences!.Single(p => p.Channel == "Email").IsEnabled.Should().BeFalse();
        preferences.Single(p => p.Channel == "InApp").IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task Upsert_with_an_unknown_channel_returns_400()
    {
        var response = await Client.PutAsJsonAsync($"/api/v1/notification-preferences/user-1/Sms", new UpsertPreferenceRequest(false));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
