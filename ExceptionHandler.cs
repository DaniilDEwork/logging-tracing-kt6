using Serilog;

namespace TaskManagerApp;

public static class ExceptionHandler
{
    public static void Execute(string operation, Action action, string? taskTitle = null)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            exception.Data["Operation"] = operation;
            if (taskTitle != null)
            {
                exception.Data["TaskTitle"] = taskTitle;
            }

            if (exception is ArgumentException or KeyNotFoundException)
            {
                if (exception.Data["TaskManagerErrorLogged"] is not true)
                {
                    Report(exception, operation);
                }
            }
            else
            {
                throw;
            }
        }
    }

    public static void Report(Exception exception, string operation, bool fatal = false, string? taskTitle = null)
    {
        exception.Data["Operation"] = operation;
        if (taskTitle != null)
        {
            exception.Data["TaskTitle"] = taskTitle;
        }

        Console.WriteLine("!!! Ошибка !!!");
        Console.WriteLine($"Операция: {operation}");
        Console.WriteLine($"Причина: {exception.Message}");
        Console.WriteLine(fatal ? "Приложение завершается" : "Произошла ошибка. Подробнее см. логи.");

        var logger = Log.ForContext("Operation", operation)
            .ForContext("ExceptionType", exception.GetType().FullName)
            .ForContext("ExceptionMessage", exception.Message)
            .ForContext("StackTrace", exception.StackTrace);

        if (exception.Data["TaskTitle"] is string title)
        {
            logger = logger.ForContext("TaskTitle", title);
        }

        if (fatal)
        {
            logger.Fatal(exception, "!!! Ошибка !!! Критическое исключение в операции {Operation}", operation);
        }
        else
        {
            logger.Error(exception, "!!! Ошибка !!! Перехвачено исключение в операции {Operation}", operation);
            exception.Data["TaskManagerErrorLogged"] = true;
        }
    }
}
