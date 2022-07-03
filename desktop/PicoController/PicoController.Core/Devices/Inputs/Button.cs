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
        const string actionPress = "press";
        private static readonly string[] availableActions = { actionPress };
        public Button(byte id, InputType type, Dictionary<string, Func<Task>?> actions) : base(id, type, availableActions, actions)
        {}

        private bool IsPressed;
        protected override void ExecuteInternal(InputMessage message)
        {
            const int Pressed = 1, Released = 1 << 1;
            if (IsPressed && message.Value == Released)
            {
                IsPressed = false;
                InvokeAction(actionPress);
            }
            else if(!IsPressed && message.Value == Pressed)
            {
                IsPressed = true;
            }
        }
    }
}
