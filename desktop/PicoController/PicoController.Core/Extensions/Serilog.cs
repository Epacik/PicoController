using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core.Extensions;

public static class SerilogExtensions
{
    public static bool ExistsAndIsEnabled(this ILogger? logger, LogEventLevel level)
        => logger?.IsEnabled(level) == true;
}
