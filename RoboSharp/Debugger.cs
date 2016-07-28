using System;

namespace RoboSharp
{
    public sealed class Debugger
    {
        private static readonly Lazy<Debugger> instance = new Lazy<Debugger>(() => new Debugger());

        private Debugger()
        {

        }

        public static Debugger Instance
        {
            get { return instance.Value; }
        }

        public EventHandler<DebugMessageArgs> DebugMessageEvent;

        public class DebugMessageArgs : EventArgs
        {
            public object Message { get; set; }
        }

        private void RaiseDebugMessageEvent(object message)
        {
            DebugMessageEvent?.Invoke(this, new DebugMessageArgs
            {
                Message = message
            });
        }

        internal void DebugMessage(object data)
        {
            RaiseDebugMessageEvent(data);
        }
    }
}
