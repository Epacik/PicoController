using PicoController.Core.Config;
using PicoController.Gui.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Design
{
    internal class DesignRepositoryHelper : ReactiveObject, IRepositoryHelper
    {
        public Config? SavedConfigCopy { get => default; set => _ = value; }
        static readonly IConfigRepository _repo = new DesignConfigRepository();
        public IConfigRepository Repository => _repo;

        public event EventHandler? ReloadRequested;

        public bool IsDirty
        {
            get => false;
            set => _ = value;
        }

        public void AddChanges(string deviceId, int inputId, bool split)
        {
            
        }

        public void AddChanges(string deviceId, int inputId, string actionName, InputAction action)
        {
        }

        public bool HasUnsavedChanges(string deviceId, int inputId, string actionName)
        {
            return false;
        }

        public void RaisePropertyChanged(PropertyChangedEventArgs args)
        {
        }

        public void RaisePropertyChanging(PropertyChangingEventArgs args)
        {
        }

        public void RequestReload()
        {
        }

        public Task SaveChangesAsync()
         => Task.Run(() => { });
    }
}
