using System.Diagnostics;
using System.Text;

namespace TaskManagerApp;

public static class Tracer
{
    public static readonly TraceSource TaskManagerTrace = new("TaskManagerTrace");
    private static TextWriterTraceListener? fileListener;

    public static void Configure()
    {
        TaskManagerTrace.Switch = new SourceSwitch("SourceSwitch", "All");
        TaskManagerTrace.Listeners.Clear();
        TaskManagerTrace.Listeners.Add(new ConsoleTraceListener());

        try
        {
            Directory.CreateDirectory("logs");
            var writer = new StreamWriter(Path.Combine("logs", "taskmanagerTrace.log"),
                true, new UTF8Encoding(false));
            writer.AutoFlush = true;
            fileListener = new TextWriterTraceListener(writer);
            TaskManagerTrace.Listeners.Add(fileListener);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ExceptionHandler.Report(exception, "ConfigureTrace");
            Console.WriteLine("Трассировка продолжается в консоли");
        }
    }

    public static void Write(TraceEventType eventType, int id, string message)
    {
        try
        {
            TaskManagerTrace.TraceEvent(eventType, id, message);
            TaskManagerTrace.Flush();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ExceptionHandler.Report(exception, "WriteTrace");
            if (fileListener != null)
            {
                TaskManagerTrace.Listeners.Remove(fileListener);
            }
        }
    }

    public static void Close()
    {
        try
        {
            TaskManagerTrace.Flush();
            TaskManagerTrace.Close();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ExceptionHandler.Report(exception, "CloseTrace");
        }

        if (fileListener != null && !TaskManagerTrace.Listeners.Contains(fileListener))
        {
            try
            {
                fileListener.Close();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                ExceptionHandler.Report(exception, "CloseTraceFile");
            }
        }
    }
}
