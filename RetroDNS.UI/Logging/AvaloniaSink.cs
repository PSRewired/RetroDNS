using System;
using Serilog.Core;
using Serilog.Events;

namespace RetroDNS.UI.Logging;

public class AvaloniaSink : ILogEventSink
{
    public Action<string>? Subscriber { get; set; }

    public void Emit(LogEvent logEvent)
    {
        Subscriber?.Invoke($"[{logEvent.Level}] {logEvent.RenderMessage()}");

        if (logEvent.Exception != null)
        {
            Subscriber?.Invoke(logEvent.Exception.ToString());
        }
    }
}
