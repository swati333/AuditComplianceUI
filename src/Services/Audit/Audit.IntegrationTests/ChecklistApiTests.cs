using System.Net;
using System.Net.Http.Json;
using Audit.Contracts.Dtos;
using Audit.Contracts.Requests;
using FluentAssertions;

namespace Audit.IntegrationTests;

public sealed class ChecklistApiTests : IntegrationTestBase
{
    public ChecklistApiTests(AuditApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_persists_questions_in_display_order()
    {
        var request = new CreateChecklistRequest(
            $"Checklist {Guid.NewGuid()}",
            "desc",
            [
                new CreateChecklistQuestionRequest("Second", IsMandatory: false, DisplayOrder: 2),
                new CreateChecklistQuestionRequest("First", IsMandatory: true, DisplayOrder: 1),
            ]);

        var response = await Client.PostAsJsonAsync("/api/v1/checklists", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var checklist = await response.Content.ReadFromJsonAsync<ChecklistDto>();
        checklist!.Questions.Select(q => q.Text).Should().ContainInOrder("First", "Second");
    }

    [Fact]
    public async Task GetById_returns_404_for_an_unknown_checklist()
    {
        var response = await Client.GetAsync($"/api/v1/checklists/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_includes_a_newly_created_checklist()
    {
        var createResponse = await Client.PostAsJsonAsync(
            "/api/v1/checklists",
            new CreateChecklistRequest($"Checklist {Guid.NewGuid()}", null, []));
        var created = await createResponse.Content.ReadFromJsonAsync<ChecklistDto>();

        var listResponse = await Client.GetAsync("/api/v1/checklists");

        var list = await listResponse.Content.ReadFromJsonAsync<List<ChecklistDto>>();
        list!.Should().Contain(c => c.Id == created!.Id);
    }
}
