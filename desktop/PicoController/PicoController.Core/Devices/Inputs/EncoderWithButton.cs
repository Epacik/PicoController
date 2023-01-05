using PicoController.Core.Config;
using PicoController.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Tiger.Clock;

namespace PicoController.Core.Devices.Inputs
{
    internal class EncoderWithButton : Input
    {
        private EncoderWithButton(
            int deviceId,
            byte inputId,
            IEnumerable<string> availableActions,
            Dictionary<string, Func<int, Task>?> actions,
            int maxDelayBetweenClicks,
            bool split) 
            : base(deviceId, inputId, InputType.EncoderWithButton, availableActions, actions, split)
        {
            _maxDelayBetweenClicks = maxDelayBetweenClicks;
            _timer = new System.Timers.Timer(_maxDelayBetweenClicks);
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = false;
        }

        private async void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            async Task invoke(int presses)
            {
                switch (presses)
                {
                    case 1:
                        await InvokeAction(0, ActionNames.Press); break;
                    case 2:
                        await InvokeAction(0, ActionNames.DoublePress); break;
                    case 3:
                        await InvokeAction(0, ActionNames.TriplePress); break;
                    case > 3:
                        await InvokeAction(0, ActionNames.TriplePress);
                        await invoke(presses - 3);
                        break;
                }
            }

            await invoke(Interlocked.Exchange(ref _presses, 0));
        }

        private bool _isPressed;
        private bool _rotatedWhilePressed;
        private int _presses;
        private readonly int _maxDelayBetweenClicks;
        private readonly System.Timers.Timer _timer;

        protected override async Task ExecuteInternal(InputMessage message)
        {
            const int counterClockwise = 1, clockwise = 1 << 1, pressed = 1 << 2, released = 1 << 3;

            if (!_isPressed && message.ValueHasBits(pressed))
                _isPressed = true;

            if (_isPressed && message.ValueHasBits(released) && !_rotatedWhilePressed)
            {
                Interlocked.Increment(ref _presses);
                _timer.Stop();
                _timer.Start();
            }

            if (_isPressed && message.ValueHasBits(released))
            {
                _isPressed = false;
                _rotatedWhilePressed = false;
            }
            
            if(message.ValueHasBits(clockwise) || message.ValueHasBits(counterClockwise))
            {
                _rotatedWhilePressed = true;
                if (Split)
                {
                    var action = (message.ValueHasBits(clockwise), _isPressed) switch
                    {
                        (true, true)   => ActionNames.RotatePressedSplitC,
                        (false, true)  => ActionNames.RotatePressedSplitCC,
                        (true, false)  => ActionNames.RotateSplitC,
                        (false, false) => ActionNames.RotateSplitCC,
                    };
                    await InvokeAction(1, action);
                }
                else
                {
                    var value = message.ValueHasBits(clockwise) ? 1 : -1;
                    var action = _isPressed ? ActionNames.RotatePressed : ActionNames.Rotate;
                    await InvokeAction(value, action);
                }

                //if (message.ValueHasBits(clockwise))
                //{
                //    InvokeAction(1, _isPressed ? ActionNames.RotatePressed : ActionNames.Rotate);
                //}
                //else if (message.ValueHasBits(counterClockwise))
                //{
                //    InvokeAction(-1, _isPressed ? ActionNames.RotatePressed : ActionNames.Rotate);
                //}
            }
        }

        public static EncoderWithButton Create(
            int deviceId,
            byte inputId,
            Dictionary<string, Func<int, Task>?> actions,
            int maxDelayBetweenClicks,
            bool split)
        {
            if (split)
            {
                return new EncoderWithButton(
                    deviceId,
                    inputId,
                    new string[] {
                        ActionNames.Rotate,
                        ActionNames.RotatePressed,
                        ActionNames.Press,
                        ActionNames.DoublePress,
                        ActionNames.TriplePress
                    },
                    actions,
                    maxDelayBetweenClicks,
                    split);
            }
            else
            {
                return new EncoderWithButton(
                    deviceId,
                    inputId,
                    new string[] {
                        ActionNames.RotateSplitC,
                        ActionNames.RotateSplitCC,
                        ActionNames.RotatePressedSplitC,
                        ActionNames.RotatePressedSplitCC,
                        ActionNames.Press,
                        ActionNames.DoublePress,
                        ActionNames.TriplePress
                    },
                    actions,
                    maxDelayBetweenClicks,
                    split);
            }
        }
    }
}
