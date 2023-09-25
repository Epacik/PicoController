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
        public EncoderWithButton(
            string deviceId,
            byte inputId,
            IHandlerProvider handlerProvider,
            Func<string, Func<int, string, Task>?> getAction,
            int maxDelayBetweenClicks,
            bool split,
            ILogger logger)
            : base(deviceId, inputId, handlerProvider, getAction, logger, split)
        {
            _maxDelayBetweenClicks = maxDelayBetweenClicks;
        }

        private bool _isPressed;
        private bool _rotatedWhilePressed;
        private int _presses;
        private CancellationTokenSource? _tokenSource;
        private readonly int _maxDelayBetweenClicks;

        public override InputType InputType => InputType.EncoderWithButton;

        public override ImmutableArray<string> GetActions()
        {
            var actions = Split switch
            {
                true =>
                    new string[] {
                        ActionNames.RotateSplitC,
                        ActionNames.RotateSplitCC,
                        ActionNames.RotatePressedSplitC,
                        ActionNames.RotatePressedSplitCC,
                        ActionNames.Press,
                        ActionNames.DoublePress,
                        ActionNames.TriplePress
                    },
                false => 
                    new string[] {
                        ActionNames.Rotate,
                        ActionNames.RotatePressed,
                        ActionNames.Press,
                        ActionNames.DoublePress,
                        ActionNames.TriplePress
                    },
            };

            return actions.ToImmutableArray();
        }

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
    }
}
