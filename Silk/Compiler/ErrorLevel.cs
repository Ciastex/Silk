using System.ComponentModel;

namespace Silk.Compiler
{
    public enum ErrorLevel
    {
        [Description("ERROR")]
        Error,
        [Description("FATAL ERROR")]
        FatalError,
    }
}