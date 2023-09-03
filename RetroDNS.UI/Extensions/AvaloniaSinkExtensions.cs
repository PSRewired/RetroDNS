using System;
using RetroDNS.UI.Logging;
using Serilog;
using Serilog.Configuration;

namespace RetroDNS.UI.Extensions;

public static class AvaloniaSinkExtensions
{
    public static LoggerConfiguration Avalonia(this LoggerSinkConfiguration loggerConfiguration, IFormatProvider? formatProvider = null) =>
        loggerConfiguration.Sink<AvaloniaSink>();
}
