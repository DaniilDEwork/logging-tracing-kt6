using System.Diagnostics;
using Serilog;

namespace TaskManagerApp;

public class TaskManager
{
    private readonly List<Task> tasks = new();
    private int nextId = 1;

    public void AddTask(string title)
    {
        var logger = Log.ForContext("Operation", "AddTask");
        Stopwatch stopwatch = Stopwatch.StartNew();
        string result = "Error";
        string details = "задача не добавлена";
        Tracer.Write(TraceEventType.Start, 0, "Начало операции AddTask");

        try
        {
            Tracer.Write(TraceEventType.Verbose, 4, "AddTask: проверка названия задачи");
            title = ValidateTitle(title);

            Tracer.Write(TraceEventType.Verbose, 4, "AddTask: создание задачи и добавление в коллекцию");
            var task = new Task { Id = nextId++, Title = title };
            tasks.Add(task);
            result = "Success";
            details = $"добавлена задача '{title}'";
            logger.ForContext("Result", result)
                .ForContext("TaskItem", task, destructureObjects: true)
                .Information("Добавлена задача {TaskTitle}. ID: {TaskId}. Всего задач: {TaskCount}",
                    task.Title, task.Id, tasks.Count);
        }
        catch (Exception exception)
        {
            result = "Error";
            details = exception.Message;
            Tracer.Write(TraceEventType.Error, 3, $"AddTask: {exception.Message}");
            ExceptionHandler.Report(exception, "AddTask", taskTitle: title);
            throw;
        }
        finally
        {
            FinishOperation("AddTask", stopwatch, result, details, logger);
        }
    }

    public void RemoveTask(string title)
    {
        var logger = Log.ForContext("Operation", "RemoveTask");
        Stopwatch stopwatch = Stopwatch.StartNew();
        string result = "Error";
        string details = "задача не удалена";
        Tracer.Write(TraceEventType.Start, 0, "Начало операции RemoveTask");

        try
        {
            Tracer.Write(TraceEventType.Verbose, 4, "RemoveTask: проверка названия задачи");
            title = ValidateTitle(title);

            Tracer.Write(TraceEventType.Verbose, 4, $"RemoveTask: поиск задачи '{title}' в коллекции");
            int index = tasks.FindIndex(task =>
                string.Equals(task.Title, title, StringComparison.OrdinalIgnoreCase));

            if (index == -1)
            {
                details = $"задача '{title}' не найдена";
                throw new KeyNotFoundException($"Задача \"{title}\" не найдена для удаления");
            }

            Task removedTask = tasks[index];
            tasks.RemoveAt(index);
            result = "Success";
            details = $"удалена задача '{removedTask.Title}'";
            logger.ForContext("Result", result)
                .ForContext("TaskItem", removedTask, destructureObjects: true)
                .Information("Задача \"{TaskTitle:l}\" удалена. ID: {TaskId}. Всего задач: {TaskCount}",
                    removedTask.Title, removedTask.Id, tasks.Count);
        }
        catch (Exception exception)
        {
            result = "Error";
            details = exception.Message;
            Tracer.Write(TraceEventType.Error, 3, $"RemoveTask: {exception.Message}");
            ExceptionHandler.Report(exception, "RemoveTask", taskTitle: title);
            throw;
        }
        finally
        {
            FinishOperation("RemoveTask", stopwatch, result, details, logger);
        }
    }

    public void ListTasks()
    {
        var logger = Log.ForContext("Operation", "ListTasks");
        Stopwatch stopwatch = Stopwatch.StartNew();
        string result = "Error";
        Tracer.Write(TraceEventType.Start, 0, "Начало операции ListTasks");

        try
        {
            Tracer.Write(TraceEventType.Verbose, 4, $"ListTasks: проверка коллекции, задач: {tasks.Count}");

            if (tasks.Count == 0)
            {
                logger.Information("Список задач пуст");
            }
            else
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. ID: {tasks[i].Id} | {tasks[i].Title}");
                }
            }

            logger.ForContext("Result", "Success")
                .ForContext("Tasks", tasks, destructureObjects: true)
                .Information("Показан список из {Count} задач", tasks.Count);
            result = "Success";
        }
        catch (Exception exception)
        {
            result = "Error";
            Tracer.Write(TraceEventType.Error, 3, $"ListTasks: {exception.Message}");
            ExceptionHandler.Report(exception, "ListTasks");
            throw;
        }
        finally
        {
            string details = tasks.Count == 0 ? "список пуст" : "список выведен";
            FinishOperation("ListTasks", stopwatch, result, details, logger);
        }
    }

    private static string ValidateTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Название задачи не может быть пустым", nameof(title));
        }

        title = title.Trim();
        if (title.Length > 100 || title.Any(char.IsControl))
        {
            throw new ArgumentException("Допустимо до 100 символов без управляющих символов", nameof(title));
        }

        return title;
    }

    private void FinishOperation(string operation, Stopwatch stopwatch, string result, string details, ILogger logger)
    {
        stopwatch.Stop();
        double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
        string resultText = result switch
        {
            "Success" => "успех",
            "Warning" => "предупреждение",
            _ => "ошибка"
        };

        Tracer.Write(TraceEventType.Stop, 1,
            $"Завершение {operation}. Результат: {resultText}, {details}. Всего задач: {tasks.Count}. Время: {elapsedMs:F3} мс");
        logger.Debug("Завершение операции {Operation}. Результат: {Result}. Время: {ElapsedMs} мс. Всего задач: {TaskCount}",
            operation, result, elapsedMs, tasks.Count);
    }
}
