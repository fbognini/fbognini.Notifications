using fbognini.Notifications.Sinks.Email;
using Xunit;

namespace fbognini.Notifications.Tests;

public class EmailMessageTests
{
    private static EmailIdentity Identity() => new()
    {
        SmtpHost = "localhost",
        FromEmail = "support@example.com",
        ReplyToEmail = "help@example.com",
    };

    private static EmailMessage Message(string? to = "a@b.c", string? cc = null, string? bcc = null) => new()
    {
        ConfigurationId = "SUPPORT",
        To = to,
        Cc = cc,
        Bcc = bcc,
        Subject = "subject",
        Body = "body",
    };

    /// <summary>2.x dropped ReplyToEmail in the appsettings mapping, silently.</summary>
    [Fact]
    public void Reply_to_survives_all_the_way_to_the_message()
    {
        var mime = EmailSender.BuildMessage(Message(), Identity());

        Assert.NotNull(mime);
        Assert.Equal("help@example.com", mime!.ReplyTo.Mailboxes.Single().Address);
    }

    [Fact]
    public void A_from_name_is_used_when_present()
    {
        var identity = Identity();
        identity.FromName = "Example Support";

        var mime = EmailSender.BuildMessage(Message(), identity);

        Assert.Equal("Example Support", mime!.From.Mailboxes.Single().Name);
    }

    [Fact]
    public void A_from_name_on_the_message_wins_over_the_profile_and_keeps_its_address()
    {
        var identity = Identity();
        identity.FromName = "Example Support";

        var mime = EmailSender.BuildMessage(new EmailMessage
        {
            ConfigurationId = "SUPPORT",
            FromName = "Acme Store",
            To = "a@b.c",
            Subject = "s",
            Body = "b",
        }, identity);

        var from = mime!.From.Mailboxes.Single();
        Assert.Equal("Acme Store", from.Name);
        Assert.Equal("support@example.com", from.Address);
    }

    [Fact]
    public void A_from_name_on_the_message_replaces_the_name_written_inside_from_email()
    {
        var identity = Identity();
        identity.FromEmail = "Example <support@example.com>";

        var withName = EmailSender.BuildMessage(new EmailMessage
        {
            ConfigurationId = "SUPPORT",
            FromName = "Acme Store",
            To = "a@b.c",
            Subject = "s",
            Body = "b",
        }, identity);
        var withoutName = EmailSender.BuildMessage(Message(), identity);

        Assert.Equal("Acme Store", withName!.From.Mailboxes.Single().Name);
        Assert.Equal("support@example.com", withName.From.Mailboxes.Single().Address);
        Assert.Equal("Example", withoutName!.From.Mailboxes.Single().Name);
    }

    [Fact]
    public void A_reply_to_on_the_message_wins_over_the_profile()
    {
        var mime = EmailSender.BuildMessage(new EmailMessage
        {
            ConfigurationId = "SUPPORT",
            ReplyToEmail = "shop@acme.example",
            To = "a@b.c",
            Subject = "s",
            Body = "b",
        }, Identity());

        Assert.Equal("shop@acme.example", mime!.ReplyTo.Mailboxes.Single().Address);
    }

    [Fact]
    public void Blank_overrides_fall_back_to_the_profile()
    {
        var identity = Identity();
        identity.FromName = "Example Support";

        var mime = EmailSender.BuildMessage(new EmailMessage
        {
            ConfigurationId = "SUPPORT",
            FromName = " ",
            ReplyToEmail = "",
            To = "a@b.c",
            Subject = "s",
            Body = "b",
        }, identity);

        Assert.Equal("Example Support", mime!.From.Mailboxes.Single().Name);
        Assert.Equal("help@example.com", mime.ReplyTo.Mailboxes.Single().Address);
    }

    [Theory]
    [InlineData("a@b.c;d@e.f", 2)]
    [InlineData("a@b.c,d@e.f", 2)]
    [InlineData(" a@b.c ; d@e.f ;", 2)]
    [InlineData("a@b.c", 1)]
    public void Recipient_lists_accept_both_separators_and_stray_whitespace(string raw, int expected)
    {
        var mime = EmailSender.BuildMessage(Message(to: raw), Identity());

        Assert.Equal(expected, mime!.To.Mailboxes.Count());
    }

    [Fact]
    public void A_message_with_no_recipient_at_all_is_not_built()
    {
        Assert.Null(EmailSender.BuildMessage(Message(to: null), Identity()));
    }

    [Fact]
    public void Bcc_only_is_still_a_valid_message()
    {
        var mime = EmailSender.BuildMessage(Message(to: null, bcc: "hidden@example.com"), Identity());

        Assert.NotNull(mime);
        Assert.Single(mime!.Bcc.Mailboxes);
    }

    [Fact]
    public void Html_and_plain_bodies_land_in_different_parts()
    {
        var html = new EmailMessage
        {
            ConfigurationId = "SUPPORT",
            To = "a@b.c",
            Subject = "s",
            Body = "<p>hi</p>",
            IsHtml = true,
        };

        Assert.Equal("<p>hi</p>", EmailSender.BuildMessage(html, Identity())!.HtmlBody);
        Assert.Null(EmailSender.BuildMessage(html, Identity())!.TextBody);
        Assert.Equal("body", EmailSender.BuildMessage(Message(), Identity())!.TextBody);
    }
}
