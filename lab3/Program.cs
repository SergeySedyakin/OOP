using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;

public interface ILogFilter
{
    bool Match(string text);
}

public interface ILogHandler
{
    void Handle(string text);
}

public class SimpleLogFilter : ILogFilter
{
    private readonly string _pattern;

    public SimpleLogFilter(string pattern)
    {
        _pattern = pattern;
    }

    public bool Match(string text)
    {
        return text.Contains(_pattern);
    }
}

public class ReLogFilter : ILogFilter
{
    private readonly Regex _regex;

    public ReLogFilter(string pattern)
    {
        _regex = new Regex(pattern);
    }

    public ReLogFilter(string pattern, RegexOptions options)
    {
        _regex = new Regex(pattern, options);
    }

    public bool Match(string text)
    {
        return _regex.IsMatch(text);
    }
}

public class FileHandler : ILogHandler
{
    private readonly string _filePath;
    public FileHandler(string filePath)
    {
        _filePath = filePath;
    }
    public void Handle(string text)
    {
        File.AppendAllText(_filePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {text}{Environment.NewLine}");
    }
}

public class SocketHandler : ILogHandler
{
    private readonly string _host;
    private readonly int _port;
    public SocketHandler(string host, int port)
    {
        _host = host;
        _port = port;
    }


    public void Handle(string text)
    {
        try
        {
            using (var client = new TcpClient(_host, _port))
            using (var stream = client.GetStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {text}");
                writer.Flush();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SocketHandler error: {ex.Message}");
        }
    }
}

public class ConsoleHandler : ILogHandler
{
    public void Handle(string text)
    {
        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {text}");
    }
}

public class SyslogHandler : ILogHandler
{
    public void Handle(string text)
    {
        Console.WriteLine($"EventLog: {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {text}"); //имитация записи в системные логги
    }
}

public class Logger
{
    private readonly List<ILogFilter> _filters;
    private readonly List<ILogHandler> _handlers;

    public Logger(List<ILogFilter> filters, List<ILogHandler> handlers)
    {
        _filters = filters ?? new List<ILogFilter>();
        _handlers = handlers ?? new List<ILogHandler>();
    }

    public void Log(string text)
    {
        foreach (var filter in _filters)
        {
            if (!filter.Match(text))
            {
                return; //cообщение не прошло фильтр
            }
        }

        foreach (var handler in _handlers)
        {
            handler.Handle(text);
        }
    }
}
class Program
{
    static void Main(string[] args)
    {
        List<ILogFilter> filters = new List<ILogFilter>
        {
            new SimpleLogFilter("цвет"),
            new ReLogFilter(@"красный|синий", RegexOptions.IgnoreCase)
        };

        List<ILogHandler> handlers = new List<ILogHandler>
        {
            new ConsoleHandler(),
            new FileHandler("log.txt"),
            new SyslogHandler()
        };

        var logger = new Logger(filters, handlers);

        logger.Log("У меня есть яблоко, цвет которого красный");
        logger.Log("Мне не важного, какой у тебя цвет обуви");
        logger.Log("Я катаюсь на велосипеде");
        logger.Log("ярко-красный");
        logger.Log("цвет настроения синий");
    }
}