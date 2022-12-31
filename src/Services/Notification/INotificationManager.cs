using Notification.Messages;

namespace Notification;

public interface INotificationManager
{
    Task<bool> Notify(INotificationMessage message);
}
