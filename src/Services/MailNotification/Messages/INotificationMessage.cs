namespace MailNotification.Messages;

public interface INotificationMessage
{
    string OrderId { get; }

    string Message { get; }
}
