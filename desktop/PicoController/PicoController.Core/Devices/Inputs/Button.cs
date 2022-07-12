using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core.Devices.Inputs
{
    internal class Button : InputBase
    {
        const string actionPress       = "press";
        const string actionDoublePress = "doublePress";
        const string actionTriplePress = "triplePress";
        private static readonly string[] availableActions = { actionPress, actionDoublePress, actionTriplePress };
        private readonly int _maxDelayBetweenClicks;
        private readonly System.Timers.Timer _timer;

        public Button(int deviceId, byte inputId, InputType type, Dictionary<string, Func<Task>?> actions, int maxDelayBetweenClicks) : base(deviceId, inputId, type, availableActions, actions)
        {
            _maxDelayBetweenClicks = maxDelayBetweenClicks;
            _timer = new System.Timers.Timer(_maxDelayBetweenClicks);
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
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

        private bool IsPressed;
        private int _presses;

        protected override void ExecuteInternal(InputMessage message)
        {
            const int Pressed = 1, Released = 1 << 1;
            if (IsPressed && message.Value == Released)
            {
                IsPressed = false;
                _presses++;
                //InvokeAction(actionPress);
                _timer.Stop();
                _timer.Start();
            }
            else if(!IsPressed && message.Value == Pressed)
            {
                IsPressed = true;
            }
        }
    }
}
