namespace MailNotification.Messages;

public class NotificationMessage : INotificationMessage
{
    public NotificationMessage(string orderId, string message)
    {
        OrderId = orderId;
        Message = message;
    }

    public string OrderId { get; private set; }
    public string Message { get; private set; }
}
