using Avalonia.Collections;
using FluentAvalonia.UI.Data;
using PicoController.Core.Config;
using PicoController.Core.Devices.Inputs;
using System.Collections.Specialized;
using System.Linq;

namespace PicoController.Gui.Models;

public class DeviceInputConfigModel : ReactiveObject
{
    public DeviceInputConfigModel()
    {

    }
    private byte _id;
    public byte Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private InputType _type;
    public InputType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    private bool _split;
    public bool Split
    {
        get => _split;
        set
        {
            this.RaiseAndSetIfChanged(ref _split, value);
            this.RaisePropertyChanged(nameof(Actions));
        } 
    }

    private class ActionsComparer : IComparer<ReactiveKeyValuePair<string, DeviceInputActionConfigModel>>
    {
        public int Compare(ReactiveKeyValuePair<string, DeviceInputActionConfigModel>? x, ReactiveKeyValuePair<string, DeviceInputActionConfigModel>? y)
        {
            int getIndex(string? key) => key switch
            {
                ActionNames.Press => 1,
                ActionNames.DoublePress => 2,
                ActionNames.TriplePress => 3,
                ActionNames.Rotate => 4,
                ActionNames.RotatePressed => 5,
                ActionNames.RotateSplitC => 6,
                ActionNames.RotateSplitCC => 7,
                ActionNames.RotatePressedSplitC => 8,
                ActionNames.RotatePressedSplitCC => 9,
                _ => 0,
            };

            var xIndex = getIndex(x?.Key);
            var yIndex = getIndex(y?.Key);

            return xIndex.CompareTo(yIndex);
        }

        public static ActionsComparer Instance { get; } = new(); 
    }

    private AvaloniaList<ReactiveKeyValuePair<string, DeviceInputActionConfigModel>>? _actions;
    public IEnumerable<ReactiveKeyValuePair<string, DeviceInputActionConfigModel>>? Actions
    {
        get => _actions!.Where(x => GetPossibleActions().Contains(x.Key));
        init
        {
            if(value is not null)
            {
                var all = GetAllPossibleActions()
                    .Select(x => new ReactiveKeyValuePair<string, DeviceInputActionConfigModel>(x, new ()));

                var v = value.UnionBy(all, x => x.Key).Order(ActionsComparer.Instance);

                this.RaiseAndSetIfChanged(ref _actions, new(v));
            }
        }
    }

    public AvaloniaList<ReactiveKeyValuePair<string, DeviceInputActionConfigModel>>? AllActions
    {
        get => _actions;
        set
        {
            if(_actions is not null)
            {
                _actions.CollectionChanged -= AllActions_CollectionChanged;
            }
            this.RaiseAndSetIfChanged(ref _actions, value);
            this.RaisePropertyChanged(nameof(Actions));

            if (_actions is not null)
            {
                _actions.CollectionChanged += AllActions_CollectionChanged;
            }
        }
    }

    private void AllActions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(Actions));
    }

    internal IEnumerable<string> GetPossibleActions() => Core.Config.Input.GetPossibleActions(Type, Split);
    internal IEnumerable<string> GetAllPossibleActions() => Core.Config.Input.GetAllPossibleActions(Type);
}
