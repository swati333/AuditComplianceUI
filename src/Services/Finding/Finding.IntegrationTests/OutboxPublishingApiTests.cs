using System.Net.Http.Json;
using Finding.Contracts.Dtos;
using Finding.Contracts.Requests;
using Finding.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Finding.IntegrationTests;

/// <summary>Verifies the Transactional Outbox actually receives the right rows — CLAUDE.md's "Immediately publish an event for Critical findings" requirement made concrete.</summary>
public sealed class OutboxPublishingApiTests : IntegrationTestBase
{
    public OutboxPublishingApiTests(FindingApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Creating_a_Critical_finding_writes_both_FindingCreated_and_CriticalFindingCreated()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/findings", new CreateFindingRequest(Guid.NewGuid(), "Critical issue", null, "Critical"));
        var created = (await response.Content.ReadFromJsonAsync<FindingDetailDto>())!;

        var outboxTypes = await GetOutboxEventTypesForAsync(created.Id);

        outboxTypes.Should().Contain("FindingCreated");
        outboxTypes.Should().Contain("CriticalFindingCreated");
    }

    [Fact]
    public async Task Creating_a_Low_severity_finding_does_not_publish_CriticalFindingCreated()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/findings", new CreateFindingRequest(Guid.NewGuid(), "Minor issue", null, "Low"));
        var created = (await response.Content.ReadFromJsonAsync<FindingDetailDto>())!;

        var outboxTypes = await GetOutboxEventTypesForAsync(created.Id);

        outboxTypes.Should().Contain("FindingCreated");
        outboxTypes.Should().NotContain("CriticalFindingCreated");
    }

    private async Task<List<string>> GetOutboxEventTypesForAsync(Guid findingId)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FindingDbContext>();

        return await dbContext.OutboxMessages
            .Where(m => m.Content.Contains(findingId.ToString()))
            .Select(m => m.Type)
            .ToListAsync();
    }
}
