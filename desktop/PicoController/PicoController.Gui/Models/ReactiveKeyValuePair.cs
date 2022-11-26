namespace PicoController.Gui.Models;
public class ReactiveKeyValuePair<TKey, TValue> : ReactiveObject
{
    public ReactiveKeyValuePair() { }
    public ReactiveKeyValuePair(TKey key, TValue value)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }


    private TKey? _key;
    public TKey? Key
    {
        get => _key;
        set => this.RaiseAndSetIfChanged(ref _key, value);
    }

    private TValue? _value;
    public TValue? Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }
}
