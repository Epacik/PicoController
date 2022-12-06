namespace PicoController.Gui.Converters;

public static class Converters
{
    public static BooleanToGridLengthConverter BooleanToGridLengthConverter { get; } = new();
    public static DeviceToDeviceViewModelConverter DeviceToDeviceViewModelConverter { get; } = new();
    public static InputTypeToStringConverter InputTypeToStringConverter { get; } = new();
    public static ObjectToIsNullConverter ObjectToIsNullConverter { get; } = new();
}
