namespace PicoController.Core.Input;

public enum InputType : UInt16
{
    Button = 1,
    Encoder = 2,
    EncoderWithButton = 3,

    MAX = EncoderWithButton,
}
