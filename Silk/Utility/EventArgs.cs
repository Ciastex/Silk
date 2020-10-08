using System;
using Silk.Compiler;
using Silk.DataModel;

namespace Silk.Utility
{
    public class BeginEventArgs : EventArgs
    {
        public object UserData { get; set; }
    }

    public class EndEventArgs : EventArgs
    {
        public object UserData { get; set; }
    }

    public class FunctionEventArgs : EventArgs
    {
        public string Name { get; internal set; }
        public Variable[] Parameters { get; internal set; }
        public Variable ReturnValue { get; internal set; }
        public object UserData { get; set; }
    }

    internal class ErrorEventArgs : EventArgs
    {
        public ErrorCode ErrorCode { get; set; }
        public string Token { get; set; }
    }
}
