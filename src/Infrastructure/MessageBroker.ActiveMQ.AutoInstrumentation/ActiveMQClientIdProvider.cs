namespace MessageBroker.ActiveMQ.AutoInstrumentation;

internal static class ActiveMQClientIdProvider
{
    private static int uniqueCliendId = 0;

    public static string GetClientId(string clientName)
        => $"{clientName}_{Interlocked.Increment(ref uniqueCliendId)}";
}
