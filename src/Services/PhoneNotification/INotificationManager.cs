using PhoneNotification.Messages;

namespace PhoneNotification;

public interface INotificationManager
{
    Task<bool> Notify(INotificationMessage message);
}
