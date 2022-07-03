using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core.Devices.Inputs
{
    internal class EncoderWithButton : InputBase
    {
        const string actionPress                         = "press";
        const string actionRotateClockwise               = "rotationClockwise";
        const string actionRotateCounterClockwise        = "rotationCounterClockwise";
        const string actionRotateClockwisePressed        = "pressedRotationClockwise";
        const string actionRotateCounterClockwisePressed = "pressedRotationCounterClockwise";
        private static readonly string[] availableActions = 
            { actionPress, actionRotateClockwise, actionRotateCounterClockwise, actionRotateClockwisePressed, actionRotateCounterClockwisePressed };
        public EncoderWithButton(int deviceId, byte inputId, InputType type,  Dictionary<string, Func<Task>?> actions) : base(deviceId, inputId, type, availableActions, actions)
        {
        }

        private bool _isPressed;
        private bool _rotatedWhilePressed;
        protected override void ExecuteInternal(InputMessage message)
        {
            const int counterClockwise = 1, clockwise = 1 << 1, pressed = 1 << 2, released = 1 << 3;

            if(!_isPressed && message.Value == pressed)
                _isPressed = true;

            if (_isPressed && message.Value == released && !_rotatedWhilePressed)
                InvokeAction(actionPress);
            if (_isPressed && message.Value == released)
            {
                _isPressed = false;
                _rotatedWhilePressed = false;
            }

            if (message.Value == clockwise)
            {
                _rotatedWhilePressed = true;
                InvokeAction(_isPressed ? actionRotateClockwisePressed : actionRotateClockwise);
            }
            else if (message.Value == counterClockwise)
            {
                _rotatedWhilePressed = true;
                InvokeAction(_isPressed ? actionRotateCounterClockwisePressed : actionRotateCounterClockwise);
            }
        }
    }
}
