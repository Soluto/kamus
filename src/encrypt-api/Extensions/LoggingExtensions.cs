using Serilog;

namespace Kamus.Extensions
{
    public static class LoggingExtensions
    {
        public static ILogger AsAudit(this ILogger logger)
        {
            return logger.ForContext("log_type", "audit");
        }
    }
}