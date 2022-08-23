using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core.BuiltInActions.HidSimulation.Native
{
    internal class EmptyInputSimulator : INativeInputSimulator
    {
        public void MouseButtonPressed(MouseButton button)
        {
        }

        public void MouseButtonReleased(MouseButton button)
        {
        }

        public void MouseMove(double x, double y, MoveMode moveMode)
        {
        }

        public void MouseScrollWheelScrolled(Axis axis, short amount, Point? pointerPosition = null)
        {
        }
    }
}
