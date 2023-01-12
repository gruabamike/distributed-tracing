using MailNotification.Messages;

namespace MailNotification;

public interface INotificationManager
{
    Task<bool> Notify(INotificationMessage message);
}
