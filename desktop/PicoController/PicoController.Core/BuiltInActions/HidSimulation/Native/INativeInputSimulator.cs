using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core.BuiltInActions.HidSimulation.Native;

internal interface INativeInputSimulator
{
    void MouseButtonPressed(MouseButton button);
    void MouseButtonReleased(MouseButton button);
    void MouseScrollWheelScrolled(Axis axis, short amount, Point? pointerPosition = null);
    void MouseMove(double x, double y, MoveMode moveMode);
}

internal enum MouseButton
{
    Left, Right, Middle, X
}
internal enum Axis
{
    Vertical, Horizontal,
    None
}
internal enum MoveMode
{
    Relative, Absolute
}
internal struct Point
{
    public Point()
    {
        X = 0;
        Y = 0;
    }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Point WithX(int x) => new(x, Y);
    public Point WithY(int y) => new(X, y);

    public readonly int X;
    public readonly int Y;
}
