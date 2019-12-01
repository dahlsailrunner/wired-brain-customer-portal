using Serilog;
using Serilog.Context;

namespace WiredBrain.Logging
{
    public static class SecurityLog
    {
        public static void Warning<T>(string messageTemplate, T propertyValue)
        {
            using (LogContext.PushProperty("Security", true))
            {
                Log.Warning(messageTemplate, propertyValue);
            }
        }
    }
}
