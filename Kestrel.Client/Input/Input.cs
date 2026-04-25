using Silk.NET.Input;

namespace Kestrel.Client.Input;

public class Input
{
    readonly IKeyboard _keyboard;
    readonly HashSet<Key> _down = [];
    readonly HashSet<Key> _pressed = [];

    public Input(IKeyboard keyboard)
    {
        _keyboard = keyboard;
        _keyboard.KeyDown += OnKeyDown;
        _keyboard.KeyUp += OnKeyUp;
    }

    public bool IsKeyDown(Key key) => _down.Contains(key) || _keyboard.IsKeyPressed(key);
    public bool IsKeyPressed(Key key) => _pressed.Contains(key);

    public void NewFrame() => _pressed.Clear();

    public void Dispose()
    {
        _keyboard.KeyDown -= OnKeyDown;
        _keyboard.KeyUp -= OnKeyUp;
    }

    void OnKeyDown(IKeyboard _, Key key, int __)
    {
        if (_down.Add(key)) _pressed.Add(key);
    }

    void OnKeyUp(IKeyboard _, Key key, int __) => _down.Remove(key);
}
