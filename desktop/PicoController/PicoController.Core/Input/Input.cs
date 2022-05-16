using System.Text;

namespace PicoController.Core.Input
{
    internal abstract class Input
    {
        public readonly byte Id;
        public readonly InputType Type;
        public readonly Dictionary<UInt32, Func<Task>?> Actions = new();
        private readonly string Typename;

        public Input(byte id, InputType type, Dictionary<UInt32, Func<Task>?> actions)
        {
            Id = id;
            Type = type;
            Actions = actions;
            Typename = GetType().Name;
        }

        protected enum ValidationErrors
        {
            None             = 0,
            IdDoNotMatch     = 1,
            TypeDoNotMatch   = 1 << 1,
            ActionDoNotExist = 1 << 2,
            ActionIsNull     = 1 << 3,
        }

        protected ValidationErrors ValidateMessage(InputMessage message)
        {
            var error = ValidationErrors.None;
            if (message.InputId != Id)
                error |= ValidationErrors.IdDoNotMatch;

            if (message.InputType != Type)
                error |= ValidationErrors.TypeDoNotMatch;

            else if(!Actions.ContainsKey(message.Value))
                error |= ValidationErrors.ActionDoNotExist;
            
            else if (Actions[message.Value] is null)
                error |= ValidationErrors.ActionIsNull;

            return error;
        }

        public virtual async Task<bool> TryRunActionAsync(InputMessage message)
        {
            var errors = ValidateMessage(message);
            if (errors != ValidationErrors.None)
                return false;

            await Actions[message.Value]!.Invoke();
            return true;
        }


        public virtual async Task RunActionAsync(InputMessage message)
        {
            var errors = ValidateMessage(message);
            
            if (errors.HasFlag(ValidationErrors.IdDoNotMatch) || errors.HasFlag(ValidationErrors.TypeDoNotMatch))
                throw NotMatchingIdOrType(errors, nameof(message));

            if (errors.HasFlag(ValidationErrors.ActionDoNotExist))
                throw new ArgumentException($"{Typename}.Actions do not specify action of key {message.Value}", nameof(message));

            if (errors.HasFlag(ValidationErrors.ActionIsNull))
                throw new InvalidOperationException($"{Typename}.Actions specify action of key {message.Value}, but the action was null");

            await Actions[message.Value]!.Invoke();
        }

        private ArgumentException NotMatchingIdOrType(ValidationErrors errors, string paramName)
        {
            var builder = new StringBuilder();

            var id = errors.HasFlag(ValidationErrors.IdDoNotMatch);
            var type = errors.HasFlag(ValidationErrors.TypeDoNotMatch);

            if (id)
                builder.AppendFormat("{0}.InputId do not match {1}.Id", paramName, Typename);

            if(id && type)
                builder.AppendLine("And");

            if (type)
                builder.AppendFormat("{0}.InputType do not match {1}.Type", paramName, Typename);

            return new ArgumentException(paramName, builder.ToString());
        }
    }
}
