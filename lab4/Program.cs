using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab4
{
    public interface IPropertyChangedListener<T>
    {
        void OnPropertyChanged(T obj, string propertyName);
    }

    public interface INotifyDataChanged<T>
    {
        void AddPropertyChangedListener(IPropertyChangedListener<T> listener);
        void RemovePropertyChangedListener(IPropertyChangedListener<T> listener);
    }

    public interface IPropertyChangingListener<T>
    {
        bool OnPropertyChanging(T obj, string propertyName, object oldValue, object newValue);
    }

    public interface INotifyDataChanging<T>
    {
        void AddPropertyChangingListener(IPropertyChangingListener<T> listener);
        void RemovePropertyChangingListener(IPropertyChangingListener<T> listener);
    }

    public abstract class ObservableObject : INotifyDataChanged<ObservableObject>, INotifyDataChanging<ObservableObject>
    {
        private readonly List<IPropertyChangedListener<ObservableObject>> _changedListeners = new List<IPropertyChangedListener<ObservableObject>>();
        private readonly List<IPropertyChangingListener<ObservableObject>> _changingListeners = new List<IPropertyChangingListener<ObservableObject>>();

        public void AddPropertyChangedListener(IPropertyChangedListener<ObservableObject> listener)
        {
            if (!_changedListeners.Contains(listener))
                _changedListeners.Add(listener);
        }

        public void RemovePropertyChangedListener(IPropertyChangedListener<ObservableObject> listener)
        {
            _changedListeners.Remove(listener);
        }

        public void AddPropertyChangingListener(IPropertyChangingListener<ObservableObject> listener)
        {
            if (!_changingListeners.Contains(listener))
                _changingListeners.Add(listener);
        }

        public void RemovePropertyChangingListener(IPropertyChangingListener<ObservableObject> listener)
        {
            _changingListeners.Remove(listener);
        }

        protected bool SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            //валидация изменения
            foreach (var listener in _changingListeners)
            {
                if (!listener.OnPropertyChanging(this, propertyName, field, value))
                {
                    Console.WriteLine($"Изменение свойства {propertyName} отклонено валидатором");
                    return false;
                }
            }

            var oldValue = field;
            field = value;

            //уведомление об изменении
            foreach (var listener in _changedListeners)
            {
                listener.OnPropertyChanged(this, propertyName);
            }

            Console.WriteLine($"Свойство {propertyName} изменено с {oldValue} на {value}");
            return true;
        }
    }

    public class UserProfile : ObservableObject
    {
        private string _name;
        private int _age;
        private string _email;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value, "Имя");
        }

        public int Age
        {
            get => _age;
            set => SetProperty(ref _age, value, "Возраст");
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value, "Email");
        }
    }

    //слушатели и валидаторы
    public class ConsoleLogger : IPropertyChangedListener<ObservableObject>
    {
        public void OnPropertyChanged(ObservableObject obj, string propertyName)
        {
            Console.WriteLine($"[Логгер] Свойство {propertyName} было изменено");
        }
    }

    public class EmailValidator : IPropertyChangingListener<ObservableObject>
    {
        public bool OnPropertyChanging(ObservableObject obj, string propertyName, object oldValue, object newValue)
        {
            if (propertyName == "Email" && newValue is string email)
            {
                if (!email.Contains("@"))
                {
                    Console.WriteLine("[Валидатор] Некорректные данные! Email должен содержать символ @");
                    return false;
                }
            }
            return true;
        }
    }

    public class AgeValidator : IPropertyChangingListener<ObservableObject>
    {
        public bool OnPropertyChanging(ObservableObject obj, string propertyName, object oldValue, object newValue)
        {
            if (propertyName == "Возраст" && newValue is int age)
            {
                if (age < 0 || age > 120)
                {
                    Console.WriteLine("[Валидатор] Некорректные данные! Возраст должен быть между 0 и 120");
                    return false;
                }
            }
            return true;
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            UserProfile user = new UserProfile();

            ConsoleLogger logger = new ConsoleLogger();
            EmailValidator emailValidator = new EmailValidator();
            AgeValidator ageValidator = new AgeValidator();

            //подписываемся на события
            user.AddPropertyChangedListener(logger);
            user.AddPropertyChangingListener(emailValidator);
            user.AddPropertyChangingListener(ageValidator);

            Console.WriteLine("Успешные изменения:");
            user.Name = "Сергей";
            user.Age = 21;
            user.Email = "sergei@mail.com";

            Console.WriteLine("Некорректные изменения:");
            user.Age = -5; 
            user.Email = "fdsfdsfdsfsddsf";

            Console.WriteLine("Текущие значения");
            Console.WriteLine($"Имя: {user.Name}, Возраст: {user.Age}, Email: {user.Email}");
        }
    }
}
