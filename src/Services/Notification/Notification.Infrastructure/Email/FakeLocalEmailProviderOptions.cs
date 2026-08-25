namespace Notification.Infrastructure.Email;

public sealed class FakeLocalEmailProviderOptions
{
    public const string SectionName = "FakeEmail";

    /// <summary>Directory .eml-style files are written to. Relative paths resolve against the content root.</summary>
    public string OutputDirectory { get; set; } = "App_Data/FakeEmails";
}
