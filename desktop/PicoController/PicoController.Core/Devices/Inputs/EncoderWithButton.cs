using PicoController.Core.Config;
using PicoController.Core.Extensions;
using Serilog;
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
            bool split,
            ILogger logger) 
            : base(deviceId, inputId, InputType.EncoderWithButton, availableActions, actions, logger, split)
        {
            _maxDelayBetweenClicks = maxDelayBetweenClicks;
        }

        private bool _isPressed;
        private bool _rotatedWhilePressed;
        private int _presses;
        private CancellationTokenSource? _tokenSource;
        private readonly int _maxDelayBetweenClicks;

        protected override async Task ExecuteInternal(InputMessage message)
        {
            const int counterClockwise = 1, clockwise = 1 << 1, pressed = 1 << 2, released = 1 << 3;

            if (!_isPressed && message.ValueHasBits(pressed))
                _isPressed = true;

            if (message.ValueHasBits(clockwise) || message.ValueHasBits(counterClockwise))
            {
                _rotatedWhilePressed = true;
                if (Split)
                {
                    var action = (message.ValueHasBits(clockwise), _isPressed) switch
                    {
                        (true, true) => ActionNames.RotatePressedSplitC,
                        (false, true) => ActionNames.RotatePressedSplitCC,
                        (true, false) => ActionNames.RotateSplitC,
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
            }

            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;

            if (_isPressed && message.ValueHasBits(released) && !_rotatedWhilePressed)
            {
                Interlocked.Increment(ref _presses);

                try
                {
                    _tokenSource = new CancellationTokenSource();
                    await Task.Delay(_maxDelayBetweenClicks, _tokenSource.Token);
                    await InvokeClicks(Interlocked.Exchange(ref _presses, 0));
                }
                catch (TaskCanceledException)
                {
                    // swallow
                }
            } 
            else if (_isPressed && message.ValueHasBits(released))
            {
                _isPressed = false;
                _rotatedWhilePressed = false;
            }
        }

        async Task InvokeClicks(int presses)
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
                    await InvokeClicks(presses - 3);
                    break;
            }
        }

        public static EncoderWithButton Create(
            int deviceId,
            byte inputId,
            Dictionary<string, Func<int, Task>?> actions,
            int maxDelayBetweenClicks,
            bool split,
            ILogger logger)
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
                    split,
                    logger);
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
                    split, 
                    logger);
            }
        }
    }
}
