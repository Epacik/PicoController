using PicoController.Core.Config;
using ReactiveUI;
using Splat;
using System.ComponentModel;
using System.Threading.Tasks;

namespace PicoController.Gui.Helpers;

internal interface IRepositoryHelper : IReactiveNotifyPropertyChanged<IReactiveObject>, IHandleObservableErrors, IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, IEnableLogger
{
    void RequestReload();
    event EventHandler? ReloadRequested;
    bool IsDirty { get; protected set; }
    void AddChanges(string deviceId, int inputId, string actionName, InputAction action);
    Task SaveChangesAsync();
    bool HasUnsavedChanges(string deviceId, int inputId, string actionName);
    Config? WorkingConfigCopy { get; set; }
    Config? SavedConfigCopy { get; set; }
    IConfigRepository Repository { get; }

}
