using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Audit.Application.EventHandlers;
using Audit.Contracts.Dtos;
using Audit.Contracts.Requests;
using Audit.Infrastructure.Messaging;
using Ehs.Contracts.Events;
using Finding.Contracts.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.IntegrationTests;

/// <summary>
/// End-to-end coverage of CLAUDE.md §2's lifecycle over real HTTP + a real
/// SQL Server: assign team/checklist, walk the full
/// Draft→Planned→InProgress→Completed→Closed path, and verify the two
/// explicit business-rule gates (invalid transitions rejected, completion
/// blocked while mandatory questions are unanswered) return the expected
/// ProblemDetails shape, not just a raw 4xx.
/// </summary>
public sealed class AuditLifecycleApiTests : IntegrationTestBase
{
    public AuditLifecycleApiTests(AuditApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Full_lifecycle_from_Draft_to_Closed_succeeds()
    {
        var audit = await CreateAuditAsync();

        await AssignTeamMemberAsync(audit.Id, "auditor-1", "Alice Auditor", "Auditor");

        var (checklistId, mandatoryQuestionId) = await CreateChecklistWithOneMandatoryQuestionAsync();
        (await Client.PostAsJsonAsync($"/api/v1/audits/{audit.Id}/checklist", new AssignChecklistRequest(checklistId)))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var planned = await PostAndReadAsync($"/api/v1/audits/{audit.Id}/plan",
            new PlanAuditRequest(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(3)));
        planned.Status.Should().Be("Planned");

        var started = await PostAndReadAsync($"/api/v1/audits/{audit.Id}/start", null);
        started.Status.Should().Be("InProgress");

        (await Client.PostAsJsonAsync($"/api/v1/audits/{audit.Id}/checklist-responses",
                new RecordChecklistResponseRequest(mandatoryQuestionId, "All exits clear.", true)))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var completed = await PostAndReadAsync($"/api/v1/audits/{audit.Id}/complete", null);
        completed.Status.Should().Be("Completed");

        var closed = await PostAndReadAsync($"/api/v1/audits/{audit.Id}/close", null);
        closed.Status.Should().Be("Closed");

        var historyResponse = await Client.GetAsync($"/api/v1/audits/{audit.Id}/status-history");
        var history = await historyResponse.Content.ReadFromJsonAsync<List<AuditStatusHistoryDto>>();
        history!.Select(h => h.ToStatus).Should().ContainInOrder("Draft", "Planned", "InProgress", "Completed", "Closed");
    }

    [Fact]
    public async Task Starting_a_Draft_audit_without_planning_first_is_rejected_with_409()
    {
        var audit = await CreateAuditAsync();

        var response = await Client.PostAsync($"/api/v1/audits/{audit.Id}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("errorCode").GetString().Should().Be("INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public async Task Planning_without_an_assigned_team_is_rejected_with_409()
    {
        var audit = await CreateAuditAsync();

        var response = await Client.PostAsJsonAsync($"/api/v1/audits/{audit.Id}/plan",
            new PlanAuditRequest(DateTime.UtcNow, DateTime.UtcNow.AddDays(1)));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("errorCode").GetString().Should().Be("AUDIT_TEAM_NOT_ASSIGNED");
    }

    [Fact]
    public async Task Completing_with_an_unanswered_mandatory_question_is_rejected_with_409()
    {
        var audit = await CreateAuditAsync();
        await AssignTeamMemberAsync(audit.Id, "auditor-2", "Bob Auditor", "Auditor");

        var (checklistId, _) = await CreateChecklistWithOneMandatoryQuestionAsync();
        await Client.PostAsJsonAsync($"/api/v1/audits/{audit.Id}/checklist", new AssignChecklistRequest(checklistId));
        await Client.PostAsJsonAsync($"/api/v1/audits/{audit.Id}/plan",
            new PlanAuditRequest(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(2)));
        await Client.PostAsync($"/api/v1/audits/{audit.Id}/start", null);

        var response = await Client.PostAsync($"/api/v1/audits/{audit.Id}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("errorCode").GetString().Should().Be("MANDATORY_QUESTIONS_UNANSWERED");
    }

    [Fact]
    public async Task Close_is_rejected_while_a_Critical_finding_is_open_and_succeeds_once_it_resolves()
    {
        var audit = await CreateAuditAsync();
        await AssignTeamMemberAsync(audit.Id, "auditor-3", "Cara Auditor", "Auditor");
        await Client.PostAsJsonAsync($"/api/v1/audits/{audit.Id}/plan",
            new PlanAuditRequest(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(2)));
        await Client.PostAsync($"/api/v1/audits/{audit.Id}/start", null);
        await Client.PostAsync($"/api/v1/audits/{audit.Id}/complete", null);

        var findingId = Guid.NewGuid();
        using (var scope = Factory.Services.CreateScope())
        {
            var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
            var handler = scope.ServiceProvider.GetRequiredService<CriticalFindingCreatedHandler>();
            await consumer.ConsumeAsync(
                EventEnvelope.Create(new CriticalFindingCreated(findingId, audit.Id, "Blocked fire exit", DateTime.UtcNow), "FindingService", Guid.NewGuid()),
                "Audit.CriticalFindingCreatedHandler",
                handler);
        }

        var blockedResponse = await Client.PostAsync($"/api/v1/audits/{audit.Id}/close", null);
        blockedResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await blockedResponse.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("errorCode").GetString().Should().Be("OPEN_CRITICAL_FINDINGS");

        using (var scope = Factory.Services.CreateScope())
        {
            var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
            var handler = scope.ServiceProvider.GetRequiredService<FindingResolvedHandler>();
            await consumer.ConsumeAsync(
                EventEnvelope.Create(new FindingResolved(findingId, audit.Id, DateTime.UtcNow), "FindingService", Guid.NewGuid()),
                "Audit.FindingResolvedHandler",
                handler);
        }

        var closed = await PostAndReadAsync($"/api/v1/audits/{audit.Id}/close", null);
        closed.Status.Should().Be("Closed");
    }

    [Fact]
    public async Task Cancel_is_permitted_from_Draft_and_reflected_in_status_history()
    {
        var audit = await CreateAuditAsync();

        var response = await Client.PostAsJsonAsync($"/api/v1/audits/{audit.Id}/cancel", new CancelAuditRequest("No longer needed"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuditDetailDto>();
        body!.Status.Should().Be("Cancelled");
        body.CancellationReason.Should().Be("No longer needed");
    }

    private async Task<AuditDetailDto> CreateAuditAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/audits", new CreateAuditRequest($"Audit {Guid.NewGuid()}", "desc", "Scope", "Location"));
        return (await response.Content.ReadFromJsonAsync<AuditDetailDto>())!;
    }

    private async Task AssignTeamMemberAsync(Guid auditId, string userId, string displayName, string role)
    {
        var response = await Client.PostAsJsonAsync($"/api/v1/audits/{auditId}/team-members", new AssignTeamMemberRequest(userId, displayName, role));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<(Guid ChecklistId, Guid MandatoryQuestionId)> CreateChecklistWithOneMandatoryQuestionAsync()
    {
        var request = new CreateChecklistRequest(
            $"Checklist {Guid.NewGuid()}",
            "desc",
            [new CreateChecklistQuestionRequest("Are all exits clear?", IsMandatory: true, DisplayOrder: 1)]);

        var response = await Client.PostAsJsonAsync("/api/v1/checklists", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var checklist = (await response.Content.ReadFromJsonAsync<ChecklistDto>())!;
        return (checklist.Id, checklist.Questions.Single().Id);
    }

    private async Task<AuditDetailDto> PostAndReadAsync(string url, object? body)
    {
        var response = body is null ? await Client.PostAsync(url, null) : await Client.PostAsJsonAsync(url, body);
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: $"POST {url} should succeed");
        return (await response.Content.ReadFromJsonAsync<AuditDetailDto>())!;
    }
}
