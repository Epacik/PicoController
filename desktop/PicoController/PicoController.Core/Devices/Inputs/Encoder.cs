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
        
        private Encoder(
            int deviceId,
            byte inputId,
            IEnumerable<string> availableActions,
            Dictionary<string, Func<int, Task>?> actions,
            bool split,
            ILogger logger)
            : base(deviceId, inputId, InputType.Encoder, availableActions, actions, logger, split)
        {
        }

        protected override async Task ExecuteInternal(InputMessage message)
        {
            const int counterClockwise = 1, clockwise = 1 << 1;
            if (Split)
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

        public static Encoder Create(int deviceId, byte inputId,  Dictionary<string, Func<int, Task>?> actions, bool split, ILogger logger)
        {
            if (split)
            {
                return new Encoder(
                    deviceId,
                    inputId,
                    new string[] { ActionNames.Rotate },
                    actions,
                    split,
                    logger);
            }
            else
            {
                return new Encoder(
                    deviceId,
                    inputId,
                    new string[] { ActionNames.RotateSplitC, ActionNames.RotateSplitCC },
                    actions,
                    split,
                    logger);
            } 
        }
    }
}
