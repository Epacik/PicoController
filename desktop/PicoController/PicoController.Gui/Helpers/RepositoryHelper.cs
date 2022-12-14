using OneOf;
using PicoController.Core.Config;
using PicoController.Core.Devices.Inputs;
using PicoController.Core.Extensions;
using ReactiveUI;

namespace PicoController.Gui.Helpers;

internal class RepositoryHelper : ReactiveObject, IRepositoryHelper
{
    public RepositoryHelper()
    {
        Repository = Locator.Current.GetRequiredService<IConfigRepository>();
        Repository.Changed += Repository_Changed;
        SavedConfigCopy = Repository.Read();
        WorkingConfigCopy = SavedConfigCopy?.Clone();
    }

    private void Repository_Changed(object? sender, ConfigChangedEventArgs e)
    {
        SavedConfigCopy = e.NewConfig;
        WorkingConfigCopy = SavedConfigCopy?.Clone();
    }

    public event EventHandler? ReloadRequested;

    public void RequestReload()
    {
        SavedConfigCopy = Repository.Read();
        WorkingConfigCopy = SavedConfigCopy?.Clone();

        ReloadRequested?.Invoke(this, EventArgs.Empty);
    }

    public IConfigRepository Repository { get; }
    private Dictionary<string, List<Change>> _unsavedChanges = new();
    private readonly object _changesLock = new();

    public void AddChanges(string deviceId, int inputId, bool split)
    {
        Device workingDevice = GetDeviceHelper(deviceId);
        this.RaisePropertyChanging(nameof(WorkingConfigCopy));

        workingDevice.Inputs[inputId].Split = split;

        lock (this._changesLock)
        {
            if (!_unsavedChanges.ContainsKey(deviceId))
            {
                _unsavedChanges[deviceId] = new();
            }

            _unsavedChanges[deviceId].Add(new(inputId, new SplitChange(split)));
        }
        this.RaisePropertyChanged(nameof(WorkingConfigCopy));
        IsDirty = true;
    }

    public void AddChanges(string deviceId, int inputId, string actionName, InputAction action)
    {
        Device workingDevice = GetDeviceHelper(deviceId);

        //if (!workingDevice.Inputs[inputId].Actions.ContainsKey(actionName))
            //throw new InvalidOperationException($"Unknown action: {actionName}");

        this.RaisePropertyChanging(nameof(WorkingConfigCopy));

        workingDevice.Inputs[inputId].Actions[actionName] = action;

        lock (this._changesLock)
        {
            if (!_unsavedChanges.ContainsKey(deviceId))
            {
                _unsavedChanges[deviceId] = new();
            }

            _unsavedChanges[deviceId].Add(new(inputId, new InputActionChange(actionName, action)));
        }
        this.RaisePropertyChanged(nameof(WorkingConfigCopy));
        IsDirty = true;
    }

    private Device GetDeviceHelper(string deviceId)
    {
        var workingDevice = WorkingConfigCopy?.Devices.Find(x => x.Id == deviceId);
        if (workingDevice is null)
            throw new InvalidOperationException($"Unknown device: {deviceId}");
        return workingDevice;
    }

    public async Task SaveChangesAsync()
    {
        var cnf = await Repository.ReadAsync();
        if (cnf is null)
            return;

        Dictionary<string, List<Change>>? changes = null;
        lock (_changesLock)
        {
            changes = _unsavedChanges;
            _unsavedChanges = new();
        }

        if (changes is null)
            return;

        for (int i = 0; i < cnf.Devices.Count; i++)
        {
            Device? device = cnf.Devices[i];
            foreach(Change change in changes?.Where(x => x.Key == device.Id)?.SelectMany(x => x.Value) ?? Array.Empty<Change>())
            {
                if (change.Value.IsT0)
                    device.Inputs[change.InputId].Actions[change.Value.AsT0.ActionName] = change.Value.AsT0.Action;
                else
                    device.Inputs[change.InputId].Split = change.Value.AsT1.Split;
            }
        }

        await Repository.SaveAsync(cnf);
        SavedConfigCopy = cnf;
        WorkingConfigCopy = cnf.Clone();
        IsDirty = false;
    }

    public bool HasUnsavedChanges(string deviceId, int inputId, string actionName)
    {
        if(_unsavedChanges.TryGetValue(deviceId, out var changes))
        {
            return changes.Any(
                x => x.InputId == inputId 
                  && x.Value.IsT0
                  && x.Value.AsT0.ActionName == actionName);
        }
        return false;
    }

    private bool _isDirty;
    public bool IsDirty
    {
        get => _isDirty;
        set => this.RaiseAndSetIfChanged(ref _isDirty, value);
    }

    private Config? _workingConfigCopy;
    public Config? WorkingConfigCopy
    {
        get => _workingConfigCopy;
        set => this.RaiseAndSetIfChanged(ref _workingConfigCopy, value);
    }

    private Config? _savedConfigCopy;
    public Config? SavedConfigCopy
    {
        get => _savedConfigCopy;
        set => this.RaiseAndSetIfChanged(ref _savedConfigCopy, value);
    }

}

internal record struct Change(int InputId, OneOf<InputActionChange, SplitChange> Value);
internal record struct InputActionChange(string ActionName, InputAction Action);
internal record struct SplitChange(bool Split);