﻿namespace Silk.Compiler.ByteCode
{
    internal enum OpCode
    {
        Nop,
        ExecFunction,
        Return,
        Jump,
        Assign,
        AssignListVariable,
        JumpIfFalse,
        EvalLiteral,
        EvalVariable,
        EvalCreateList,
        EvalInitializeList,
        EvalListVariable,
        EvalFunction,
        EvalAdd,
        EvalSubtract,
        EvalMultiply,
        EvalDivide,
        EvalPower,
        EvalModulus,
        EvalConcat,
        EvalNegate,
        EvalAnd,
        EvalOr,
        EvalXor,
        EvalNot,
        EvalIsEqual,
        EvalIsNotEqual,
        EvalIsGreaterThan,
        EvalIsGreaterThanOrEqual,
        EvalIsLessThan,
        EvalIsLessThanOrEqual,
    }
}
