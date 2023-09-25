using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiger.Clock;

namespace PicoController.Core.Devices.Inputs
{
    internal class Encoder : Input
    {
        
        public Encoder(
            string deviceId,
            byte inputId,
            IHandlerProvider handlerProvider,
            Func<string, Func<int, string, Task>?> getAction,
            bool split,
            ILogger logger)
            : base(deviceId, inputId, handlerProvider, getAction, logger, split)
        {
        }

        public override InputType InputType => InputType.Encoder;
        public override ImmutableArray<string> GetActions()
        {
            var actions = Split switch
            {
                true => new string[] { ActionNames.RotateSplitC, ActionNames.RotateSplitCC },
                false => new string[] { ActionNames.Rotate }
            };

            return actions.ToImmutableArray();
        }

        protected override async Task ExecuteInternal(InputMessage message)
        {
            const int counterClockwise = 1, clockwise = 1 << 1;
            if (!Split)
            {
                var value = message.Value switch { counterClockwise => -1, clockwise => 1, _ => 0 };
                await InvokeAction(value, ActionNames.Rotate);
            }
            else
            {
                var action = message.Value switch
                {
                    counterClockwise => ActionNames.RotateSplitC,
                    clockwise => ActionNames.RotateSplitCC,
                    _ => ""
                };

                await InvokeAction(1, action);
            }
        }
    }
}
