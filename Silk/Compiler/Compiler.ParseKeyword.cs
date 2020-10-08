using System;
using System.Collections.Generic;
using System.Diagnostics;
using Silk.Compiler.ByteCode;
using Silk.DataModel;
using ValueType = Silk.DataModel.ValueType;

namespace Silk.Compiler
{
    public partial class Compiler
    {
        /// <summary>
        /// Keyword to keyword handler lookup table.
        /// </summary>
        /// <remarks>
        /// Contains only keywords that can start a statement.
        /// </remarks>
        private static readonly Dictionary<Keyword, Action<global::Silk.Compiler.Compiler, Token>> KeywordParserLookup = new Dictionary<Keyword, Action<global::Silk.Compiler.Compiler, Token>>
        {
            [Keyword.Var] = (x, t) => x.ParseVar(t),
            [Keyword.GoTo] = (x, t) => x.ParseGoTo(t),
            [Keyword.If] = (x, t) => x.ParseIf(t),
            [Keyword.While] = (x, t) => x.ParseWhile(t),
            [Keyword.For] = (x, t) => x.ParseFor(t),
            [Keyword.Return] = (x, t) => x.ParseReturn(t),
        };

        private void ParseKeyword(Token token)
        {
            // Test for code illegally outside of functions
            if (!InFunction)
            {
                if (token.Keyword == Keyword.Var)
                {
                    if (!InHeader)
                    {
                        Error(ErrorCode.IllegalVar);
                        NextLine();
                        return;
                    }
                }
                else
                {
                    Error(ErrorCode.CodeOutsideFunction, token);
                    NextLine();
                    return;
                }
            }

            // Dispatch to keyword parser
            if (KeywordParserLookup.TryGetValue(token.Keyword, out Action<global::Silk.Compiler.Compiler, Token> action))
                action(this, token);
            else
                Error(ErrorCode.UnexpectedKeyword, token);
        }

        private void ParseVar(Token token)
        {
            // Get variable name
            token = Lexer.GetNext();
            if (token.Type != TokenType.Symbol)
            {
                Error(ErrorCode.ExpectedSymbol, token);
                return;
            }

            // Check if variable already exists
            int varId = GetVariableId(token.Value, false);
            if (varId != -1)
            {
                Error(ErrorCode.VariableAlreadyDefined, token);
                NextLine();
                return;
            }

            if (InHeader)
            {
                // Create global variable
                varId = GetVariableId(token.Value);
                Debug.Assert(((ByteCodeVariableFlag)varId).HasFlag(ByteCodeVariableFlag.Global));
                // Process any assignment
                if (Lexer.PeekNext().Type == TokenType.Equal)
                {
                    // Consume equal sign
                    Lexer.GetNext();
                    // Parse value (only literal value allowed here)
                    if (!ParseLiteral(out Variable variable))
                        return;
                    // Assign value to variable
                    Variables[ByteCodeUtils.GetVariableIndex(varId)] = variable;
                }
            }
            else
            {
                // Create local variable
                varId = GetVariableId(token.Value);
                // Process any assignment
                if (Lexer.PeekNext().Type == TokenType.Equal)
                {
                    // Consume equal sign
                    Lexer.GetNext();
                    Writer.Write(ByteCode.OpCode.Assign, varId);
                    if (!ParseExpression())
                        return;
                }
            }
        }

        private void ParseGoTo(Token token)
        {
            token = Lexer.GetNext();
            if (token.Type != TokenType.Symbol)
            {
                Error(ErrorCode.ExpectedSymbol, token);
                return;
            }
            // Do not combine next two write; need IP for ReferenceLabel
            Writer.Write(ByteCode.OpCode.Jump);
            Writer.Write(ReferenceLabel(token.Value));
            VerifyEndOfLine();
        }

        private void ParseIf(Token token)
        {
            bool foundElseWithoutIf = false;
            List<int> breakAddressIPs = new List<int>();

            // Write bytecode and false address placeholder
            Writer.Write(ByteCode.OpCode.JumpIfFalse);
            int falseAddressIP = Writer.Write(0);
            // Parse condition
            if (!ParseExpression())
                return;

            while (true)
            {
                // Parse conditional statements
                ParseCodeBlock();

                // Look for ELSE
                token = Lexer.GetNextSkipNewLines();
                if (token.Keyword != Keyword.Else)
                {
                    Lexer.UngetToken(token);
                    break;
                }
                // Test for extra ELSE
                if (foundElseWithoutIf)
                {
                    Error(ErrorCode.UnexpectedKeyword, token);
                    break;
                }

                // Write bytecodes to exit IF..ELSE block
                Writer.Write(ByteCode.OpCode.Jump);
                breakAddressIPs.Add(Writer.Write(0));

                // Fixup JumpIfFalse target address
                Writer.WriteAt(falseAddressIP, Writer.IP);

                // Is ELSE followed by IF?
                token = Lexer.GetNext();
                if (token.Keyword == Keyword.If)
                {
                    // Write bytecode and break address placeholder
                    Writer.Write(ByteCode.OpCode.JumpIfFalse);
                    falseAddressIP = Writer.Write(0);
                    // Parse condition
                    if (!ParseExpression())
                        return;
                }
                else
                {
                    Lexer.UngetToken(token);
                    foundElseWithoutIf = true;
                }
            }
            // Fixup last JumpIfFalse target address
            if (!foundElseWithoutIf)
                Writer.WriteAt(falseAddressIP, Writer.IP);
            // Fixup break jump addresses
            foreach (int ip in breakAddressIPs)
                Writer.WriteAt(ip, Writer.IP);
        }

        private void ParseWhile(Token token)
        {
            int loopStartIP = Writer.Write(ByteCode.OpCode.JumpIfFalse);
            int breakAddressIP = Writer.Write(0);
            if (!ParseExpression())
                return;
            VerifyEndOfLine();

            // Parse loop statements
            ParseCodeBlock();

            // Write loop bytecodes and final break address
            Writer.Write(ByteCode.OpCode.Jump, loopStartIP);
            Writer.WriteAt(breakAddressIP, Writer.IP);
        }

        private void ParseFor(Token token)
        {
            // Parse loop variable
            token = Lexer.GetNext();
            if (token.Type != TokenType.Symbol)
            {
                Error(ErrorCode.ExpectedSymbol, token);
                return;
            }
            int varId = GetVariableId(token.Value);

            // Parse assignment operator
            token = Lexer.GetNext();
            if (token.Type != TokenType.Equal)
            {
                Error(ErrorCode.ExpectedEquals, token);
                return;
            }
            // Assign initial value
            Writer.Write(ByteCode.OpCode.Assign, varId);
            if (!ParseExpression())
                return;
            // Parse TO keyword
            token = Lexer.GetNext();
            if (token.Keyword != Keyword.To)
            {
                Error(ErrorCode.ExpectedTo, token);
                return;
            }

            // Write bytecode and break address placeholder
            int loopStartIP = Writer.Write(ByteCode.OpCode.JumpIfFalse);
            int breakAddressIP = Writer.Write(0);

            // Parse TO expression and add logic to compare result to loop variable
            int compareIP = ByteCodeUtils.InvalidIP;
            if (!ParseExpression(true, () =>
            {
                Writer.Write(ByteCode.OpCode.EvalVariable, varId);
                compareIP = Writer.Write(ByteCode.OpCode.EvalIsGreaterThanOrEqual);
            }))
                return;

            int stepId;
            token = Lexer.PeekNext();
            if (token.Keyword == Keyword.Step)
            {
                // Consume step
                Lexer.GetNext();
                // Custom step value
                // Step must be numeric literal because if it's an expression, we wouldn't know until runtime how to
                // compare to the TO value (less than or equal vs greater than or equal)
                if (!ParseLiteral(out Variable stepLiteral))
                {
                    Error(ErrorCode.ExpectedLiteral);
                    return;
                }
                if (stepLiteral == 0 || (stepLiteral.Type != ValueType.Integer && stepLiteral.Type != ValueType.Float))
                {
                    Error(ErrorCode.InvalidStepValue, token);
                    return;
                }
                // Change compare logic if step is negative
                if (stepLiteral < 0)
                {
                    Debug.Assert(compareIP != ByteCodeUtils.InvalidIP);
                    Writer.WriteAt(compareIP, ByteCode.OpCode.EvalIsLessThanOrEqual);
                }
                stepId = GetLiteralId(stepLiteral);
            }
            else stepId = GetLiteralId(new Variable(1));
            VerifyEndOfLine();

            // Parse loop statements
            ParseCodeBlock();
            // Write step (increment) bytecodes
            Writer.Write(ByteCode.OpCode.Assign, varId);
            Writer.Write(3);   // 3 tokens
            Writer.Write(ByteCode.OpCode.EvalVariable, varId);
            Writer.Write(ByteCode.OpCode.EvalLiteral, stepId);
            Writer.Write(ByteCode.OpCode.EvalAdd);
            // Write loop codebytes and fixup break address
            Writer.Write(ByteCode.OpCode.Jump, loopStartIP);
            Writer.WriteAt(breakAddressIP, Writer.IP);
        }

        private void ParseReturn(Token token)
        {
            // Return can optionally be followed by expression
            Writer.Write(ByteCode.OpCode.Return);
            if (!ParseExpression(false))
                Writer.Write(0); // No return tokens
            VerifyEndOfLine();
        }

        private void ParseCodeBlock()
        {
            Token token = Lexer.GetNextSkipNewLines();
            if (token.Type == TokenType.LeftBrace)
            {
                while (ParseStatement())
                    ;
                token = Lexer.GetNext();
                if (token.Type != TokenType.RightBrace)
                    Error(ErrorCode.ExpectedRightBrace);
            }
            else
            {
                Lexer.UngetToken(token);
                ParseStatement();
            }
        }
    }
}
