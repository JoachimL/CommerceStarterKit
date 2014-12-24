using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Moq;
using NUnit.Framework;
using OxxCommerceStarterKit.Web.Models.ViewModels.Email;
using OxxCommerceStarterKit.Web.Services.Email;
using OxxCommerceStarterKit.Web.Services.Email.Models;
using Ploeh.AutoFixture;
using Should;
using StructureMap;

namespace CommerceStarterKit.Web.UnitTests.Services.Email
{
    public class EmailServiceTests
    {
        private static readonly Fixture Fixture = new Fixture();
        private Mock<INotificationSettingsRepository> _notificationSettingsRepositoryMock;
        private Mock<IEmailDispatcher> _emailDispatcher;
        private Mock<ICurrentMarket> _currentMarketMock;
        private EmailService _sut;
        private string _header;
        private string _footer;
        private string _fromEmail;
        private Mock<IContentLoader> _contentLoaderMock;

        [SetUp]
        public virtual void SetUp()
        {
            SetUpNotificationSettings();
            _emailDispatcher = new Mock<IEmailDispatcher>();
            _currentMarketMock = new Mock<ICurrentMarket>();
            _contentLoaderMock = new Mock<IContentLoader>();
            _sut = new EmailService(_notificationSettingsRepositoryMock.Object, _emailDispatcher.Object, _currentMarketMock.Object, _contentLoaderMock.Object);
        }

        private void SetUpNotificationSettings()
        {
            _header = Fixture.Create<string>();
            _footer = Fixture.Create<string>();
            _fromEmail = string.Concat(Fixture.Create<string>(), "@", Fixture.Create<string>(), ".com");
            CreateNotificationSettingsMock();
            CreateNotificationSettingsRepositoryMock();
        }

        private void CreateNotificationSettingsRepositoryMock()
        {
            _notificationSettingsRepositoryMock = new Mock<INotificationSettingsRepository>();
            _notificationSettingsRepositoryMock.Setup(r => r.GetNotificationSettings(It.IsAny<string>()))
                                               .Returns(Fixture.Create<NotificationSettings>());
        }

        private void CreateNotificationSettingsMock()
        {
            var notificationSettingsMock = new Mock<NotificationSettings>();
            notificationSettingsMock.Setup(f => f.MailHeader.ToString()).Returns(_header);
            notificationSettingsMock.Setup(f => f.MailFooter.ToString()).Returns(_footer);
            notificationSettingsMock.Setup(f => f.From).Returns(_fromEmail);
            Fixture.Register(() => notificationSettingsMock.Object);
        }

        private void SetUpValidEmail<T>(System.Linq.Expressions.Expression<Func<T, bool>> validEmail) where T : Postal.Email
        {
            _emailDispatcher.Setup(
                d => d.SendEmail(It.Is<T>(validEmail), It.IsAny<ILogger>()))
                .Returns(new SendEmailResponse() { Success = true });
        }

        public class When_sending_a_password_reset_email : EmailServiceTests
        {
            public class _and_the_system_lacks_notification_settings : When_sending_a_password_reset_email
            {
                public override void SetUp()
                {
                    base.SetUp();

                    _notificationSettingsRepositoryMock.Setup(r => r.GetNotificationSettings(It.IsAny<string>())).Returns((NotificationSettings)null);
                }

                [Test]
                public void _then_the_send_operation_fails()
                {
                    var result = _sut.SendResetPasswordEmail(Fixture.Create<string>(), Fixture.Create<string>(), Fixture.Create<string>(),
                                                             Fixture.Create<string>(), Fixture.Create<string>());
                    result.ShouldEqual(false);
                }
            }

            public class _and_the_system_is_set_up_correctly : When_sending_a_password_reset_email
            {
                [Test]
                public void _then_the_email_is_sent_to_the_correct_recipient()
                {
                    var email = Fixture.Create<string>();
                    SetUpValidEmail<ResetPassword>(m => m.To == email);

                    var result = _sut.SendResetPasswordEmail(email, Fixture.Create<string>(), Fixture.Create<string>(),
                                                             Fixture.Create<string>(), Fixture.Create<string>());
                    result.ShouldEqual(true);
                }

                [Test]
                public void _then_the_email_contains_the_supplied_hash_as_token()
                {
                    var passwordHash = Fixture.Create<string>();
                    SetUpValidEmail<ResetPassword>(m => m.Token == passwordHash);
                    var result = _sut.SendResetPasswordEmail(Fixture.Create<string>(), Fixture.Create<string>(),
                                                             Fixture.Create<string>(), passwordHash,
                                                             Fixture.Create<string>());
                    result.ShouldEqual(true);
                }

                [Test]
                public void _then_the_email_refers_to_the_supplied_reset_url()
                {
                    var resetUrl = Fixture.Create<string>();
                    SetUpValidEmail<ResetPassword>(m => m.ResetUrl == resetUrl);
                    var result = _sut.SendResetPasswordEmail(Fixture.Create<string>(), Fixture.Create<string>(),
                                                             Fixture.Create<string>(), Fixture.Create<string>(),
                                                             resetUrl);
                    result.ShouldEqual(true);
                }

                [Test]
                public void _then_the_email_is_sent_with_the_provided_subject()
                {
                    var subject = Fixture.Create<string>();
                    SetUpValidEmail<ResetPassword>(m => m.Subject == subject);
                    var result = _sut.SendResetPasswordEmail(Fixture.Create<string>(), subject, Fixture.Create<string>(),
                                                             Fixture.Create<string>(), Fixture.Create<string>());
                    result.ShouldEqual(true);
                }

                [Test]
                public void _then_the_NotificationSettings_header_is_used_as_the_emails_header()
                {
                    SetUpValidEmail<ResetPassword>(m => m.Header == _header);
                    var result = _sut.SendResetPasswordEmail(Fixture.Create<string>(), Fixture.Create<string>(),
                                                             Fixture.Create<string>(),
                                                             Fixture.Create<string>(), Fixture.Create<string>());
                    result.ShouldEqual(true);
                }

                [Test]
                public void _then_the_NotificationSettings_footer_is_used_as_the_emails_footer()
                {
                    SetUpValidEmail<ResetPassword>(m => FooterEquals(m.Footer, _footer));
                    var result = _sut.SendResetPasswordEmail(Fixture.Create<string>(), Fixture.Create<string>(),
                                                             Fixture.Create<string>(),
                                                             Fixture.Create<string>(), Fixture.Create<string>());
                    result.ShouldEqual(true);
                }

                private bool FooterEquals(string p, string _footer)
                {
                    return p == _footer;
                }
            }

            public class _and_the_email_dispatchers_throws_an_exception : When_sending_a_password_reset_email
            {
                public override void SetUp()
                {
                    base.SetUp();

                    _emailDispatcher.Setup(d => d.SendEmail(It.IsAny<Postal.Email>(), It.IsAny<ILogger>()))
                                    .Throws(new Exception(Fixture.Create<string>()));
                }
                [Test]
                public void _then_the_send_operation_fails()
                {
                    var result = _sut.SendResetPasswordEmail(Fixture.Create<string>(), Fixture.Create<string>(), Fixture.Create<string>(),
                                                             Fixture.Create<string>(), Fixture.Create<string>());
                    result.ShouldEqual(false);
                }
            }
        }

        public class When_sending_a_welcome_email : EmailServiceTests
        {
            public class _and_the_system_is_set_up_correctly : When_sending_a_welcome_email
            {
                [Test]
                public void _then_the_email_is_sent_to_the_new_customer()
                {
                    var email = Fixture.Create<string>();
                    SetUpValidEmail<Welcome>(m => m.To == email);
                    _sut.SendWelcomeEmail(email, It.IsAny<string>(), It.IsAny<string>());


                }
            }
        }
    }
}
