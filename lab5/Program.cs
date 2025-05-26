using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public class User : IComparable<User>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Login { get; set; }

    [JsonIgnore]
    public string Password { get; set; }

    public string Email { get; set; } 
    public string Address { get; set; }
    public int CompareTo(User other) => Name.CompareTo(other?.Name);
    public override string ToString() =>
        $"User(Id={Id}, Name='{Name}', Login='{Login}', Email='{Email}', Address='{Address}')";
}

public interface IDataRepository<T> where T : class
{
    IEnumerable<T> GetAll();
    T GetById(int id);
    void Add(T item);
    void Update(T item);
    void Delete(T item);
}

public interface IUserRepository : IDataRepository<User>
{
    User GetByLogin(string login);
}

public class FileDataRepository<T> : IDataRepository<T> where T : class
{
    private readonly string _filePath;
    private List<T> _items;

    public FileDataRepository(string filePath)
    {
        _filePath = filePath;
        _items = LoadData();
    }

    private List<T> LoadData()
    {
        if (!File.Exists(_filePath))
            return new List<T>();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }

    private void SaveData()
    {
        var json = JsonSerializer.Serialize(_items);
        File.WriteAllText(_filePath, json);
    }

    public IEnumerable<T> GetAll() => _items;

    public T GetById(int id)
    {
        var property = typeof(T).GetProperty("Id");
        return _items.FirstOrDefault(item => (int)property!.GetValue(item)! == id);
    }

    public void Add(T item)
    {
        _items.Add(item);
        SaveData();
    }

    public void Update(T item)
    {
        var property = typeof(T).GetProperty("Id");
        int id = (int)property!.GetValue(item)!;

        int index = _items.FindIndex(i => (int)property.GetValue(i)! == id);
        if (index != -1)
        {
            _items[index] = item;
            SaveData();
        }
    }

    public void Delete(T item)
    {
        _items.Remove(item);
        SaveData();
    }
}

public class UserRepository : FileDataRepository<User>, IUserRepository
{
    public UserRepository(string filePath) : base(filePath) { }

    public User GetByLogin(string login) =>
        GetAll().FirstOrDefault(u => u.Login == login);
}

public interface IAuthService
{
    void SignIn(User user);
    void SignOut();
    bool IsAuthorized { get; }
    User CurrentUser { get; }
}

public class AuthService : IAuthService
{
    private const string AuthFile = "auth_session.json";
    private readonly IUserRepository _userRepository;
    private User _currentUser;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        TryAutoSignIn();
    }

    private void TryAutoSignIn()
    {
        if (File.Exists(AuthFile))
        {
            var json = File.ReadAllText(AuthFile);
            var userId = JsonSerializer.Deserialize<int>(json);
            _currentUser = _userRepository.GetById(userId);
        }
    }

    public void SignIn(User user)
    {
        _currentUser = user;
        var json = JsonSerializer.Serialize(user.Id);
        File.WriteAllText(AuthFile, json);
    }

    public void SignOut()
    {
        _currentUser = null;
        File.Delete(AuthFile);
    }

    public bool IsAuthorized => _currentUser != null;
    public User CurrentUser => _currentUser;
}

class Program
{
    static void Main()
    {
        UserRepository userRepository = new UserRepository("users.json");
        AuthService authService = new AuthService(userRepository);

        Console.WriteLine("Система авторизации запущена...");
        if (authService.IsAuthorized)
        {
            Console.WriteLine($"\nАвтоматически авторизован пользователь: {authService.CurrentUser}");
        }

        User testUser = new User
        {
            Id = 1,
            Name = "Иван",
            Login = "ivan",
            Password = "qdfdffgfdergy",
            Email = "ivan@mail.com",
            Address = "ул. Невского, д.10"
        };

        userRepository.Add(testUser);
        Console.WriteLine($"\nДобавлен пользователь: {testUser}");

        authService.SignIn(testUser);
        Console.WriteLine($"\nПользователь авторизован: {authService.CurrentUser}");

        testUser.Email = "new_email@mail.com";
        userRepository.Update(testUser);
        Console.WriteLine($"\nДанные пользователя обновлены: {testUser}");

        var foundUser = userRepository.GetByLogin("ivan");
        Console.WriteLine($"\nНайден пользователь по логину ivan: {foundUser}");

        //authService.SignOut();
        //Console.WriteLine($"\nПользователь вышел из системы...");

        List<User> users = new List<User>
        {
            new User { Id = 2, Name = "Алексей Петров", Login = "alex" },
            new User { Id = 3, Name = "Сергей Сидоров", Login = "sergey" },
            testUser
        };

        users.Sort();
        Console.WriteLine("\nОтсортированные пользователи по имени:");
        foreach (var user in users)
        {
            Console.WriteLine(user);
        }

        Console.ReadKey();
    }
}