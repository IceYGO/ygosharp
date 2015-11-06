using System;

namespace YGOSharp.Network
{
    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }

        public ExceptionEventArgs(Exception ex)
        {
            Exception = ex;
        }
    }
}