using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core.Devices.Inputs
{
    internal class Encoder : InputBase
    {
        const string actionRotateClockwise         = "rotationClockwise";
        const string actionRotateCounterClockwise  = "rotationCounterClockwise";
        private static readonly string[] availableActions = { actionRotateClockwise, actionRotateCounterClockwise };

        public Encoder(int deviceId, byte inputId, InputType type, Dictionary<string, Func<Task>?> actions) : base(deviceId, inputId, type, availableActions, actions)
        {
        }

        protected override void ExecuteInternal(InputMessage message)
        {
            const int CounterClockwise = 1, Clockwise = 1 << 1;
            if (message.Value == Clockwise)
                InvokeAction(actionRotateClockwise);
            else if (message.Value == CounterClockwise)
                InvokeAction(actionRotateCounterClockwise);
        }
    }
}
