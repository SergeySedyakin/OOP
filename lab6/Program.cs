using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

public interface ICommand
{
    void Execute();
    void Undo();
}

public class PrintCharCommand : ICommand
{
    private readonly VirtualKeyboard _keyboard;
    private readonly char _char;

    public PrintCharCommand(VirtualKeyboard keyboard, char c)
    {
        _keyboard = keyboard;
        _char = c;
    }

    public void Execute()
    {
        _keyboard.Print(_char);
        Console.WriteLine(_char);
        File.AppendAllText("output.txt", _char.ToString());
    }
    public void Undo()
    {
        _keyboard.EraseLastChar();
        Console.WriteLine("undo");
        File.AppendAllText("output.txt", "undo\n");
    }
}

public class VolumeUpCommand : ICommand
{
    public void Execute()
    {
        Console.WriteLine("volume increased +20%");
        File.AppendAllText("output.txt", "volume increased +20%\n");
    }

    public void Undo()
    {
        Console.WriteLine("volume decreased +20%");
        File.AppendAllText("output.txt", "volume decreased +20%\n");
    }
}

public class VolumeDownCommand : ICommand
{
    public void Execute()
    {
        Console.WriteLine("volume decreased -20%");
        File.AppendAllText("output.txt", "volume decreased -20%\n");
    }

    public void Undo()
    {
        Console.WriteLine("volume increased +20%");
        File.AppendAllText("output.txt", "volume increased +20%\n");
    }
}

public class MediaPlayerCommand : ICommand
{
    public void Execute()
    {
        Console.WriteLine("media player launched");
        File.AppendAllText("output.txt", "media player launched\n");
    }

    public void Undo()
    {
        Console.WriteLine("media player closed");
        File.AppendAllText("output.txt", "media player closed\n");
    }
}

public class KeyboardMemento
{
    public Dictionary<string, ICommand> KeyBindings { get; }

    public KeyboardMemento(Dictionary<string, ICommand> bindings)
    {
        KeyBindings = new Dictionary<string, ICommand>(bindings);
    }
}

public class KeyboardStateSaver
{
    private const string SaveFile = "keyboard_state.json";

    public void SaveState(KeyboardMemento memento)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(memento.KeyBindings, options);
        File.WriteAllText(SaveFile, json);
    }

    public KeyboardMemento LoadState()
    {
        if (File.Exists(SaveFile))
            return null;
        else
        {
            var json = File.ReadAllText(SaveFile);
            var bindings = JsonSerializer.Deserialize<Dictionary<string, ICommand>>(json);
            return new KeyboardMemento(bindings);
        }
            
    }
}

public class VirtualKeyboard
{
    private readonly Dictionary<string, ICommand> _keyBindings = new();
    private readonly Stack<ICommand> _commandHistory = new();
    private readonly Stack<ICommand> _redoStack = new();
    private readonly StringBuilder _output = new();
    private readonly KeyboardStateSaver _stateSaver = new();

    public VirtualKeyboard()
    {
        var memento = _stateSaver.LoadState();
        if (memento != null)
        {
            foreach (var binding in memento.KeyBindings)
            {
                _keyBindings[binding.Key] = binding.Value;
            }
        }
    }

    public void BindKey(string keyCombination, ICommand command)
    {
        _keyBindings[keyCombination] = command;
        SaveState();
    }

    public void PressKey(string keyCombination)
    {
        if (_keyBindings.TryGetValue(keyCombination, out var command))
        {
            command.Execute();
            _commandHistory.Push(command);
            _redoStack.Clear();
        }
    }

    public void Undo()
    {
        if (_commandHistory.Count > 0)
        {
            var command = _commandHistory.Pop();
            command.Undo();
            _redoStack.Push(command);
        }
    }

    public void Redo()
    {
        if (_redoStack.Count > 0)
        {
            var command = _redoStack.Pop();
            command.Execute();
            _commandHistory.Push(command);
        }
    }

    public void Print(char c)
    {
        _output.Append(c);
    }

    public void EraseLastChar()
    {
        if (_output.Length > 0)
        {
            _output.Remove(_output.Length - 1, 1);
        }
    }

    private void SaveState()
    {
        var memento = new KeyboardMemento(_keyBindings);
        _stateSaver.SaveState(memento);
    }

    public string GetOutput() => _output.ToString();
}

class Program
{
    static void Main()
    {
        if (File.Exists("output.txt"))
            File.Delete("output.txt");

        var keyboard = new VirtualKeyboard();
        var consoleOutput = new PrintCharCommand(keyboard, ' ');

        //привязка клавиш
        keyboard.BindKey("a", new PrintCharCommand(keyboard, 'a'));
        keyboard.BindKey("b", new PrintCharCommand(keyboard, 'b'));
        keyboard.BindKey("c", new PrintCharCommand(keyboard, 'c'));
        keyboard.BindKey("d", new PrintCharCommand(keyboard, 'd'));
        keyboard.BindKey("ctrl++", new VolumeUpCommand());
        keyboard.BindKey("ctrl+-", new VolumeDownCommand());
        keyboard.BindKey("ctrl+p", new MediaPlayerCommand());

        keyboard.PressKey("a");
        keyboard.PressKey("b");
        keyboard.PressKey("c");
        keyboard.Undo();
        keyboard.Undo();
        keyboard.Redo();
        keyboard.PressKey("ctrl++");
        keyboard.PressKey("ctrl+-");
        keyboard.PressKey("ctrl+p");
        keyboard.PressKey("d");
        keyboard.Undo();
        keyboard.Undo();

        Console.WriteLine("\nВывод результата:");
        Console.WriteLine(keyboard.GetOutput());
        
        Console.ReadKey();
    }
}