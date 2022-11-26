namespace PicoController.Core.Devices.Inputs;

public static class ActionNames
{
    public const string Press                = "press";
    public const string DoublePress          = "doublePress";
    public const string TriplePress          = "triplePress";

    public const string Rotate               = "rotation";
    public const string RotatePressed        = "pressedRotation";

    public const string RotateSplitC         = "rotationClockwise";
    public const string RotateSplitCC        = "rotationCounterClockwise";
    public const string RotatePressedSplitC  = "pressedRotationClockwise";
    public const string RotatePressedSplitCC = "pressedRotationCounterClockwise";
}
