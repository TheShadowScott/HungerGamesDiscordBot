using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
namespace HungerGames.Helpers;
public class Logger
{
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Critical = 5
    }
    private ILogger<Logger> _logger;
    public Logger(ILogger<Logger> logger)
    {
        _logger = logger;
    }
    private static Logger CreateSelfInstance()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger<Logger>();
        return new Logger(logger);
    }
    public void Log(LogLevel level, string message, params object[] args)
    {
        if (!IsEnabled(level))
            return;
        switch (level)
        {
            case LogLevel.Trace:
                _logger.LogTrace(message, args);
                break;
            case LogLevel.Debug:
                _logger.LogDebug(message, args);
                break;
            case LogLevel.Info:
                _logger.LogInformation(message, args);
                break;
            case LogLevel.Warn:
                _logger.LogWarning(message, args);
                break;
            case LogLevel.Error:
                _logger.LogError(message, args);
                break;
            case LogLevel.Critical:
                _logger.LogCritical(message, args);
                break;
        }
    }
    public static Logger GlobalLogger { get; } = CreateSelfInstance();
    public bool IsEnabled(LogLevel level)
    {
        return _logger.IsEnabled((Microsoft.Extensions.Logging.LogLevel)(int)level);
    }
}
