using System.Net.Http.Json;
using Audit.Contracts.Dtos;
using Audit.Contracts.Requests;
using Audit.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.IntegrationTests;

/// <summary>
/// Verifies the Transactional Outbox actually receives a row per published
/// domain event (CLAUDE.md §8/§11: "outbox message creation on domain
/// events" is required test coverage for every publishing service) — see
/// Finding Service's identical <c>OutboxPublishingApiTests</c>.
/// </summary>
public sealed class OutboxPublishingApiTests : IntegrationTestBase
{
    public OutboxPublishingApiTests(AuditApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Creating_an_audit_writes_an_AuditCreated_outbox_row()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/audits", new CreateAuditRequest("Fire Safety Audit", "desc", "Full site", "Plant A"));
        var created = (await response.Content.ReadFromJsonAsync<AuditDetailDto>())!;

        var outboxTypes = await GetOutboxEventTypesForAsync(created.Id);

        outboxTypes.Should().Contain("AuditCreated");
    }

    [Fact]
    public async Task Planning_an_audit_writes_an_AuditPlanned_outbox_row_in_addition_to_AuditCreated()
    {
        var createResponse = await Client.PostAsJsonAsync("/api/v1/audits", new CreateAuditRequest("Fire Safety Audit", "desc", "Full site", "Plant A"));
        var created = (await createResponse.Content.ReadFromJsonAsync<AuditDetailDto>())!;
        await Client.PostAsJsonAsync($"/api/v1/audits/{created.Id}/team-members", new AssignTeamMemberRequest("auditor-1", "Auditor One", "Auditor"));

        var start = DateTime.UtcNow.AddDays(1);
        await Client.PostAsJsonAsync($"/api/v1/audits/{created.Id}/plan", new PlanAuditRequest(start, start.AddDays(2)));

        var outboxTypes = await GetOutboxEventTypesForAsync(created.Id);

        outboxTypes.Should().Contain("AuditCreated");
        outboxTypes.Should().Contain("AuditPlanned");
    }

    private async Task<List<string>> GetOutboxEventTypesForAsync(Guid auditId)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        return await dbContext.OutboxMessages
            .Where(m => m.Content.Contains(auditId.ToString()))
            .Select(m => m.Type)
            .ToListAsync();
    }
}
