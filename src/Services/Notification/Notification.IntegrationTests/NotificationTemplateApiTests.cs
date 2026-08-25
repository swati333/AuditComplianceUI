using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Notification.Contracts.Dtos;

namespace Notification.IntegrationTests;

/// <summary>Templates are seeded (Notification.Infrastructure.Persistence.SeedData) — one Email + one InApp per consumed event code.</summary>
public sealed class NotificationTemplateApiTests : IntegrationTestBase
{
    public NotificationTemplateApiTests(NotificationApiFactory factory)
        : base(factory)
    {
    }

    [Theory]
    [InlineData("AuditPlanned")]
    [InlineData("CriticalFindingCreated")]
    [InlineData("ActionPlanAssigned")]
    [InlineData("ActionPlanSubmitted")]
    [InlineData("ActionPlanRejected")]
    [InlineData("ActionPlanOverdue")]
    [InlineData("ReportGenerated")]
    [InlineData("ReportGenerationFailed")]
    public async Task Seed_data_provides_both_channels_for_every_consumed_event_code(string code)
    {
        var response = await Client.GetAsync("/api/v1/notification-templates");
        var templates = await response.Content.ReadFromJsonAsync<List<NotificationTemplateDto>>();

        templates!.Where(t => t.Code == code).Select(t => t.Channel).Should().BeEquivalentTo(["Email", "InApp"]);
    }

    [Fact]
    public async Task GetById_returns_404_for_an_unknown_template()
    {
        var response = await Client.GetAsync($"/api/v1/notification-templates/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
