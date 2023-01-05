using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace MessageBroker.ActiveMQ.AutoInstrumentation;

internal sealed class ActiveMQDiagnosticListener : DiagnosticListener
{
    public ActiveMQDiagnosticListener(string name) : base(name)
    {
        AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly())!.Unloading += ActiveMQDiagnosticListener_Unloading;
    }

    private void ActiveMQDiagnosticListener_Unloading(AssemblyLoadContext obj)
    {
        Dispose();
    }
}
