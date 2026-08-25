using FluentAssertions;
using Notification.Domain.Enums;
using PreferenceEntity = Notification.Domain.Entities.NotificationPreference;

namespace Notification.UnitTests.Domain;

public class NotificationPreferenceTests
{
    private const string Actor = "user-1";

    [Fact]
    public void Create_sets_the_requested_channel_and_state()
    {
        var preference = PreferenceEntity.Create("user-1", NotificationChannel.Email, isEnabled: false, Actor);

        preference.Channel.Should().Be(NotificationChannel.Email);
        preference.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void SetEnabled_toggles_the_flag()
    {
        var preference = PreferenceEntity.Create("user-1", NotificationChannel.Email, isEnabled: true, Actor);

        preference.SetEnabled(false, Actor);

        preference.IsEnabled.Should().BeFalse();
    }
}
