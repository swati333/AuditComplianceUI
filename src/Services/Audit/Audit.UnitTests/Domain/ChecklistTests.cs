using Ehs.SharedKernel.Exceptions;
using FluentAssertions;
using ChecklistEntity = Audit.Domain.Entities.Checklist;

namespace Audit.UnitTests.Domain;

public class ChecklistTests
{
    private const string Actor = "configurator@example.com";

    [Fact]
    public void Create_rejects_an_empty_name()
    {
        var act = () => ChecklistEntity.Create("", null, Actor);

        act.Should().Throw<BusinessValidationException>();
    }

    [Fact]
    public void AddQuestion_appends_to_Questions()
    {
        var checklist = ChecklistEntity.Create("Safety Checklist", null, Actor);

        checklist.AddQuestion("Are exits marked?", isMandatory: true, displayOrder: 1, Actor);
        checklist.AddQuestion("Is the kit stocked?", isMandatory: false, displayOrder: 2, Actor);

        checklist.Questions.Should().HaveCount(2);
    }

    [Fact]
    public void MandatoryQuestionIds_returns_only_mandatory_questions()
    {
        var checklist = ChecklistEntity.Create("Safety Checklist", null, Actor);
        var mandatory = checklist.AddQuestion("Are exits marked?", isMandatory: true, displayOrder: 1, Actor);
        checklist.AddQuestion("Is the kit stocked?", isMandatory: false, displayOrder: 2, Actor);

        checklist.MandatoryQuestionIds().Should().ContainSingle().Which.Should().Be(mandatory.Id);
    }
}
