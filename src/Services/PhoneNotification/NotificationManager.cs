using PhoneNotification.Messages;

namespace PhoneNotification;

public class NotificationManager : INotificationManager
{
    private const int SIMULATION_PROCESSING_TIME_MS = 200;

    public async Task<bool> Notify(INotificationMessage message)
    {
        await Task.Delay(SIMULATION_PROCESSING_TIME_MS);
        return true;
    }
}

