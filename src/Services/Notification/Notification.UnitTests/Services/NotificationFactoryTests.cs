using FluentAssertions;
using NSubstitute;
using Notification.Application.Common;
using Notification.Application.Services;
using Notification.Domain.Enums;
using NotificationEntity = Notification.Domain.Entities.Notification;
using PreferenceEntity = Notification.Domain.Entities.NotificationPreference;
using TemplateEntity = Notification.Domain.Entities.NotificationTemplate;

namespace Notification.UnitTests.Services;

public class NotificationFactoryTests
{
    private const string Actor = "system";
    private const string EventCode = "AuditPlanned";

    private readonly INotificationTemplateRepository _templateRepository = Substitute.For<INotificationTemplateRepository>();
    private readonly INotificationPreferenceRepository _preferenceRepository = Substitute.For<INotificationPreferenceRepository>();
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly NotificationFactory _factory;

    public NotificationFactoryTests()
    {
        _factory = new NotificationFactory(_templateRepository, _preferenceRepository, _notificationRepository);
    }

    private static IReadOnlyList<TemplateEntity> BothChannelTemplates() =>
    [
        TemplateEntity.Create(EventCode, NotificationChannel.Email, "Subject", "Body", Actor),
        TemplateEntity.Create(EventCode, NotificationChannel.InApp, null, "Body", Actor),
    ];

    [Fact]
    public async Task Creates_one_notification_per_active_template_when_recipient_is_known_and_has_no_preferences()
    {
        _templateRepository.GetActiveByCodeAsync(EventCode, Arg.Any<CancellationToken>()).Returns(BothChannelTemplates());
        _preferenceRepository.GetAsync("user-1", Arg.Any<NotificationChannel>(), Arg.Any<CancellationToken>()).Returns((PreferenceEntity?)null);

        await _factory.CreateFromEventAsync(EventCode, "user-1", new Dictionary<string, string>(), Guid.NewGuid());

        _notificationRepository.Received(2).Add(Arg.Any<NotificationEntity>());
    }

    [Fact]
    public async Task Skips_the_Email_template_when_there_is_no_resolvable_recipient()
    {
        _templateRepository.GetActiveByCodeAsync(EventCode, Arg.Any<CancellationToken>()).Returns(BothChannelTemplates());

        await _factory.CreateFromEventAsync(EventCode, recipientUserId: null, new Dictionary<string, string>(), Guid.NewGuid());

        _notificationRepository.Received(1).Add(Arg.Is<NotificationEntity>(n => n.Channel == NotificationChannel.InApp));
        _notificationRepository.DidNotReceive().Add(Arg.Is<NotificationEntity>(n => n.Channel == NotificationChannel.Email));
    }

    [Fact]
    public async Task Skips_a_channel_the_user_has_disabled()
    {
        _templateRepository.GetActiveByCodeAsync(EventCode, Arg.Any<CancellationToken>()).Returns(BothChannelTemplates());
        _preferenceRepository.GetAsync("user-1", NotificationChannel.Email, Arg.Any<CancellationToken>())
            .Returns(PreferenceEntity.Create("user-1", NotificationChannel.Email, isEnabled: false, Actor));
        _preferenceRepository.GetAsync("user-1", NotificationChannel.InApp, Arg.Any<CancellationToken>()).Returns((PreferenceEntity?)null);

        await _factory.CreateFromEventAsync(EventCode, "user-1", new Dictionary<string, string>(), Guid.NewGuid());

        _notificationRepository.Received(1).Add(Arg.Is<NotificationEntity>(n => n.Channel == NotificationChannel.InApp));
        _notificationRepository.DidNotReceive().Add(Arg.Is<NotificationEntity>(n => n.Channel == NotificationChannel.Email));
    }

    [Fact]
    public async Task Renders_placeholders_from_the_supplied_dictionary_into_the_notification_body()
    {
        var template = TemplateEntity.Create(EventCode, NotificationChannel.InApp, null, "Audit {{AuditId}} is planned.", Actor);
        _templateRepository.GetActiveByCodeAsync(EventCode, Arg.Any<CancellationToken>()).Returns([template]);

        NotificationEntity? captured = null;
        _notificationRepository.When(r => r.Add(Arg.Any<NotificationEntity>())).Do(call => captured = call.Arg<NotificationEntity>());

        await _factory.CreateFromEventAsync(EventCode, "user-1", new Dictionary<string, string> { ["AuditId"] = "abc-123" }, Guid.NewGuid());

        captured!.Body.Should().Be("Audit abc-123 is planned.");
    }

    [Fact]
    public async Task Creates_nothing_when_no_active_template_exists_for_the_event_code()
    {
        _templateRepository.GetActiveByCodeAsync("UnknownEvent", Arg.Any<CancellationToken>()).Returns([]);

        await _factory.CreateFromEventAsync("UnknownEvent", "user-1", new Dictionary<string, string>(), Guid.NewGuid());

        _notificationRepository.DidNotReceiveWithAnyArgs().Add(default!);
    }
}
