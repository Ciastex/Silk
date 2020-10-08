using System.ComponentModel;

namespace Silk.Compiler
{
    public enum ErrorCode
    {
        [Description("Operation completed successfully")]
        NoError,
        [Description("Too many errors encountered")]
        TooManyErrors,
        [Description("Internal error")]
        InternalError,

        [Description("Code is not allowed outside of functions")]
        CodeOutsideFunction,
        [Description("More than one function defined with the same name")]
        DuplicateFunctionName,
        [Description("Label defined more than once")]
        DuplicateLabel,
        [Description("Function was not defined")]
        FunctionNotDefined,
        [Description("The VAR keyword can only appear within a function, or before the first function")]
        IllegalVar,
        [Description("STEP value must be a non-zero numeric literal")]
        InvalidStepValue,
        [Description("Label was referenced but never defined")]
        LabelNotDefined,
        [Description("Function main() was not defined : You must define main() as the program's starting point")]
        MainNotDefined,
        [Description("New line in string literal")]
        NewLineInString,
        [Description("Variable has already been defined")]
        VariableAlreadyDefined,
        [Description("Use of undefined variable")]
        VariableNotDefined,
        [Description("Wrong number of arguments")]
        WrongNumberOfArguments,

        [Description("Expected equal sign \"=\"")]
        ExpectedEquals,
        [Description("An expression was expected")]
        ExpectedExpression,
        [Description("Expected opening curly brace \"{\"")]
        ExpectedLeftBrace,
        [Description("Opening parenthesis expected \"(\"")]
        ExpectedLeftParen,
        [Description("Expected literal value")]
        ExpectedLiteral,
        [Description("Operand expected")]
        ExpectedOperand,
        [Description("Closing curly brace \"}\" expected")]
        ExpectedRightBrace,
        [Description("Closing parenthesis \")\" expected")]
        ExpectedRightParen,
        [Description("Closing square bracket \"]\" expected")]
        ExpectedRightBracket,
        [Description("Identifier name expected")]
        ExpectedSymbol,
        [Description("Expected TO keyword")]
        ExpectedTo,

        [Description("Unexpected character encountered")]
        UnexpectedCharacter,
        [Description("Keyword is unexpected here")]
        UnexpectedKeyword,
        [Description("Unexpected token encountered")]
        UnexpectedToken,
    }
}