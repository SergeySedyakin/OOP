using System;
using System.Collections.Generic;

public enum LifeStyle
{
    PerRequest,
    Scoped,
    Singleton
}

//инжектор зависимостей
public class DependencyInjector : IDisposable
{
    private readonly Dictionary<Type, (Type, LifeStyle, object[])> _registrations = new();
    private readonly Dictionary<Type, object> _singletonInstances = new();
    private readonly Dictionary<Type, object> _scopedInstances = new();

    public void Register<TInterface, TImplementation>(LifeStyle lifeStyle, params object[] parameters)
        where TImplementation : TInterface
    {
        _registrations[typeof(TInterface)] = (typeof(TImplementation), lifeStyle, parameters);
    }

    public void Register<TInterface>(Func<DependencyInjector, TInterface> factoryMethod)
    {
        _registrations[typeof(TInterface)] = (null, LifeStyle.PerRequest, new object[] { factoryMethod });
    }

    public TInterface GetInstance<TInterface>()
    {
        return (TInterface)GetInstance(typeof(TInterface));
    }

    public object GetInstance(Type interfaceType)
    {
        if (!_registrations.TryGetValue(interfaceType, out var registration))
        {
            throw new InvalidOperationException($"No registration found for {interfaceType.Name}");
        }

        var (implType, lifeStyle, parameters) = registration;

        // Обработка фабричного метода
        if (implType == null && parameters[0] is Delegate factory)
        {
            return factory.DynamicInvoke(this);
        }

        switch (lifeStyle)
        {
            case LifeStyle.Singleton:
                if (!_singletonInstances.TryGetValue(interfaceType, out var singletonInstance))
                {
                    singletonInstance = CreateInstance(implType, parameters);
                    _singletonInstances[interfaceType] = singletonInstance;
                }
                return singletonInstance;

            case LifeStyle.Scoped:
                if (!_scopedInstances.TryGetValue(interfaceType, out var scopedInstance))
                {
                    scopedInstance = CreateInstance(implType, parameters);
                    _scopedInstances[interfaceType] = scopedInstance;
                }
                return scopedInstance;

            case LifeStyle.PerRequest:
                return CreateInstance(implType, parameters);

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private object CreateInstance(Type type, object[] parameters)
    {
        var constructors = type.GetConstructors();
        var constructor = constructors[0];

        var constructorParams = constructor.GetParameters();
        var resolvedParams = new object[constructorParams.Length];

        for (int i = 0; i < constructorParams.Length; i++)
        {
            if (i < parameters.Length && parameters[i] != null)
            {
                resolvedParams[i] = parameters[i];
            }
            else
            {
                resolvedParams[i] = GetInstance(constructorParams[i].ParameterType);
            }
        }

        return Activator.CreateInstance(type, resolvedParams);
    }

    public IDisposable BeginScope()
    {
        return new DependencyScope(this);
    }

    public void Dispose()
    {
        foreach (var instance in _singletonInstances.Values)
        {
            if (instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        _singletonInstances.Clear();
        _scopedInstances.Clear();
    }

    private class DependencyScope : IDisposable
    {
        private readonly DependencyInjector _injector;

        public DependencyScope(DependencyInjector injector)
        {
            _injector = injector;
        }

        public void Dispose()
        {
            foreach (var instance in _injector._scopedInstances.Values)
            {
                if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _injector._scopedInstances.Clear();
        }
    }
}

//Примеры
public interface ILogger
{
    void Log(string message);
}

public class DebugLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"[DEBUG] {DateTime.Now}: {message}");
    }
}

public class ReleaseLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"[RELEASE] {message}");
    }
}

public interface IDatabase
{
    void Connect();
}

public class SqlDatabase : IDatabase
{
    private readonly ILogger _logger;

    public SqlDatabase(ILogger logger)
    {
        _logger = logger;
    }

    public void Connect()
    {
        _logger.Log("Connecting to SQL database...");
        Console.WriteLine("Connected to SQL database");
    }
}

public class NoSqlDatabase : IDatabase
{
    private readonly ILogger _logger;

    public NoSqlDatabase(ILogger logger)
    {
        _logger = logger;
    }

    public void Connect()
    {
        _logger.Log("Connecting to NoSQL database...");
        Console.WriteLine("Connected to NoSQL database");
    }
}

public interface IEmailService
{
    void SendEmail(string to, string message);
}

public class SmtpEmailService : IEmailService
{
    private readonly ILogger _logger;

    public SmtpEmailService(ILogger logger)
    {
        _logger = logger;
    }

    public void SendEmail(string to, string message)
    {
        _logger.Log($"Sending email to {to}");
        Console.WriteLine($"Email sent to {to}: {message}");
    }
}

public class MockEmailService : IEmailService
{
    public void SendEmail(string to, string message)
    {
        Console.WriteLine($"Mock email to {to}: {message}");
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Debug Configuration ===");
        var debugInjector = new DependencyInjector();
        ConfigureDebugServices(debugInjector);
        RunApplication(debugInjector);

        Console.WriteLine("\n=== Release Configuration ===");
        var releaseInjector = new DependencyInjector();
        ConfigureReleaseServices(releaseInjector);
        RunApplication(releaseInjector);

        Console.WriteLine("\n=== LifeStyle Demonstration ===");
        var injector = new DependencyInjector();

        injector.Register<ILogger, DebugLogger>(LifeStyle.Singleton);

        injector.Register<IDatabase, SqlDatabase>(LifeStyle.Scoped);

        injector.Register<IEmailService, SmtpEmailService>(LifeStyle.PerRequest);


        var logger1 = injector.GetInstance<ILogger>();
        var logger2 = injector.GetInstance<ILogger>();
        Console.WriteLine($"Singleton instances equal: {ReferenceEquals(logger1, logger2)}");


        using (var scope = injector.BeginScope())
        {
            var db1 = injector.GetInstance<IDatabase>();
            var db2 = injector.GetInstance<IDatabase>();
            Console.WriteLine($"Scoped instances equal in same scope: {ReferenceEquals(db1, db2)}");
        }


        var email1 = injector.GetInstance<IEmailService>();
        var email2 = injector.GetInstance<IEmailService>();
        Console.WriteLine($"PerRequest instances equal: {ReferenceEquals(email1, email2)}");

        //фабричный метод
        injector.Register<IEmailService>(di => new MockEmailService());
        var mockEmail = injector.GetInstance<IEmailService>();
        mockEmail.SendEmail("test@mail.com", "Factory method works!");
    }

    private static void ConfigureDebugServices(DependencyInjector injector)
    {
        injector.Register<ILogger, DebugLogger>(LifeStyle.Singleton);
        injector.Register<IDatabase, SqlDatabase>(LifeStyle.Scoped);
        injector.Register<IEmailService, SmtpEmailService>(LifeStyle.PerRequest);
    }

    private static void ConfigureReleaseServices(DependencyInjector injector)
    {
        injector.Register<ILogger, ReleaseLogger>(LifeStyle.Singleton);
        injector.Register<IDatabase, NoSqlDatabase>(LifeStyle.Scoped);
        injector.Register<IEmailService, MockEmailService>(LifeStyle.PerRequest);
    }

    private static void RunApplication(DependencyInjector injector)
    {
        // Работа в scope
        using (injector.BeginScope())
        {
            var db = injector.GetInstance<IDatabase>();
            db.Connect();

            var emailService = injector.GetInstance<IEmailService>();
            emailService.SendEmail("user@example.com", "Hello from DI!");
        }

        // Новый scope
        using (injector.BeginScope())
        {
            var db = injector.GetInstance<IDatabase>();
            db.Connect();
        }
    }
}