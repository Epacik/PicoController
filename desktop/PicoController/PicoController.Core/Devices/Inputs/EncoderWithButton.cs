using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PicoController.Core.Devices.Inputs
{
    internal class EncoderWithButton : InputBase
    {
        const string actionPress                         = "press";
        const string actionDoublePress                   = "doublePress";
        const string actionTriplePress                   = "triplePress";
        const string actionRotateClockwise               = "rotationClockwise";
        const string actionRotateCounterClockwise        = "rotationCounterClockwise";
        const string actionRotateClockwisePressed        = "pressedRotationClockwise";
        const string actionRotateCounterClockwisePressed = "pressedRotationCounterClockwise";
        private static readonly string[] availableActions = 
            { actionPress, actionDoublePress, actionTriplePress, actionRotateClockwise, actionRotateCounterClockwise, actionRotateClockwisePressed, actionRotateCounterClockwisePressed };
        public EncoderWithButton(int deviceId, byte inputId, InputType type,  Dictionary<string, Func<Task>?> actions, int maxDelayBetweenClicks) : base(deviceId, inputId, type, availableActions, actions)
        {
            _maxDelayBetweenClicks = maxDelayBetweenClicks;
            _timer = new System.Timers.Timer(_maxDelayBetweenClicks);
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            switch (_presses)
            {
                case 1:
                    InvokeAction(actionPress); break;
                case 2:
                    InvokeAction(actionDoublePress); break;
                case > 2:
                    InvokeAction(actionTriplePress); break;
            }
            _presses = 0;
            _timer.Stop();
        }

        private bool _isPressed;
        private bool _rotatedWhilePressed;
        private int _presses;
        private readonly int _maxDelayBetweenClicks;
        private readonly System.Timers.Timer _timer;

        protected override void ExecuteInternal(InputMessage message)
        {
            const int counterClockwise = 1, clockwise = 1 << 1, pressed = 1 << 2, released = 1 << 3;

            if(!_isPressed && message.Value == pressed)
                _isPressed = true;

            if (_isPressed && message.Value == released && !_rotatedWhilePressed)
            {
                _presses++;
                _timer.Stop();
                _timer.Start();
            }
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
