﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Silk.Compiler;
using Silk.Compiler.ByteCode;
using Silk.DataModel;
using Silk.Utility;
using Boolean = Silk.Utility.Boolean;

namespace Silk.Runtime
{
    public class Runtime
    {
        private ByteCodeReader Reader;
        private Function[] Functions;
        private Variable[] Variables;
        private Variable[] Literals;
        private Stack<RuntimeFunction> FunctionStack;
        private Stack<Variable> VarStack;

        private object UserData;

        /// <summary>
        /// Executes the given program.
        /// </summary>
        /// <param name="program">Program to execute.</param>
        /// <returns>Returns a variable that contains the result of the program.</returns>
        public Variable Execute(CompiledProgram program)
        {
            if (program == null)
                throw new ArgumentNullException(nameof(program));
            if (program.IsEmpty)
                throw new Exception("Cannot execute empty program");

            // Populate data
            Reader = new ByteCodeReader(program.GetByteCodes());
            Functions = program.GetFunctions();
            Variables = program.GetVariables();
            Literals = program.GetLiterals();
            FunctionStack = new Stack<RuntimeFunction>();
            VarStack = new Stack<Variable>();
            UserData = null;

            OnBegin();

            // Initial bytecode is call to main() function
            OpCode bytecode = Reader.GetNext();
            Debug.Assert(bytecode == OpCode.ExecFunction);
            int mainId = Reader.GetNextValue();
            Debug.Assert(Functions[mainId] is UserFunction);
            RuntimeFunction function = new RuntimeFunction(Functions[mainId] as UserFunction);

            try
            {
                // Execute this function
                ExecuteFunction(function);
            }
            catch (Exception ex)
            {
                // Include line-number information if possible
                if (program.LineNumbers != null)
                {
                    Debug.Assert(program.LineNumbers.Length == program.ByteCodes.Length);
                    int ip = (Reader.IP - 1);
                    if (ip >= 0 && ip < program.LineNumbers.Length)
                    {
                        string s = $"\"{ex.Message}\" exception on line {program.LineNumbers[ip]}. See inner exception for details.";
                        throw new Exception(s, ex);
                    }
                }
                throw;
            }
            finally
            {
                OnEnd();
            }

            // Return result
            return function.ReturnValue;
        }

        private void ExecuteFunction(RuntimeFunction function)
        {
            Debug.Assert(function != null);
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            FunctionStack.Push(function);
            Reader.Push();
            Debug.Assert(function.IP != ByteCodeUtils.InvalidIP);
            Reader.GoTo(function.IP);

            // Dispatch each bytecode to its handler
            OpCode bytecode;
            do
            {
                bytecode = Reader.GetNext();
                Debug.Assert(ByteCodeHandlerLookup.ContainsKey(bytecode));
                ByteCodeHandlerLookup[bytecode](this);
            } while (bytecode != OpCode.Return);

            FunctionStack.Pop();
            Reader.Pop();
        }

        #region ByteCode dispatcher and handlers

        /// <summary>
        /// ByteCode dispatch table
        /// </summary>
        private static readonly Dictionary<OpCode, Action<Runtime>> ByteCodeHandlerLookup = new Dictionary<OpCode, Action<Runtime>>
        {
            [OpCode.Nop] = r => r.Nop(),
            [OpCode.ExecFunction] = r => r.ExecFunction(),
            [OpCode.Return] = r => r.Return(),
            [OpCode.Jump] = r => r.Jump(),
            [OpCode.JumpIfFalse] = r => r.JumpIfFalse(),
            [OpCode.Assign] = r => r.Assign(),
            [OpCode.AssignListVariable] = r => r.AssignListVariable(),
        };

        private void Nop()
        {
            // Should never be used
            Debug.Assert(false);
        }

        private void ExecFunction()
        {
            // Evaluate function and discard result
            EvalFunction();
            VarStack.Pop();
        }

        private void Return()
        {
            // Set return value if an expression is found
            var function = GetFunctionContext();
            if (EvalExpression())
                function.ReturnValue.SetValue(VarStack.Pop());
        }

        private void Jump()
        {
            int ip = Reader.GetNextValue();
            Debug.Assert(ip != ByteCodeUtils.InvalidIP);
            Reader.GoTo(ip);
        }

        private void JumpIfFalse()
        {
            int ip = Reader.GetNextValue();
            bool result = EvalExpression();
            Debug.Assert(result);
            Variable value = VarStack.Pop();
            if (value.IsFalse())
                Reader.GoTo(ip);
        }

        private void Assign()
        {
            Variable var = GetVariable(Reader.GetNextValue());
            bool result = EvalExpression();
            Debug.Assert(result);
            var.SetValue(VarStack.Pop());
        }

        private void AssignListVariable()
        {
            Variable array = GetVariable(Reader.GetNextValue());
            // Evaluate index
            bool result = EvalExpression();
            Debug.Assert(result);
            Variable index = VarStack.Pop();
            Variable var = array.GetAt(index.ToInteger() - 1);
            // Evaluate expression
            result = EvalExpression();
            Debug.Assert(result);
            var.SetValue(VarStack.Pop());
        }

        #endregion

        #region Expression evaluator

        /// <summary>
        /// ByteCode evaluators dispatch table.
        /// </summary>
        private static readonly Dictionary<OpCode, Action<Runtime>> EvalHandlerLookup = new Dictionary<OpCode, Action<Runtime>>
        {
            [OpCode.EvalLiteral] = r => r.EvalLiteral(),
            [OpCode.EvalVariable] = r => r.EvalVariable(),
            [OpCode.EvalCreateList] = r => r.EvalCreateList(),
            [OpCode.EvalInitializeList] = r => r.EvalInitializeList(),
            [OpCode.EvalListVariable] = r => r.EvalListVariable(),
            [OpCode.EvalFunction] = r => r.EvalFunction(),
            [OpCode.EvalAdd] = r => r.EvalAdd(),
            [OpCode.EvalSubtract] = r => r.EvalSubtract(),
            [OpCode.EvalMultiply] = r => r.EvalMultiply(),
            [OpCode.EvalDivide] = r => r.EvalDivide(),
            [OpCode.EvalPower] = r => r.EvalPower(),
            [OpCode.EvalModulus] = r => r.EvalModulus(),
            [OpCode.EvalConcat] = r => r.EvalConcat(),
            [OpCode.EvalNegate] = r => r.EvalNegate(),
            [OpCode.EvalAnd] = r => r.EvalAnd(),
            [OpCode.EvalOr] = r => r.EvalOr(),
            [OpCode.EvalXor] = r => r.EvalXor(),
            [OpCode.EvalNot] = r => r.EvalNot(),
            [OpCode.EvalIsEqual] = r => r.EvalIsEqual(),
            [OpCode.EvalIsNotEqual] = r => r.EvalIsNotEqual(),
            [OpCode.EvalIsGreaterThan] = r => r.EvalIsGreaterThan(),
            [OpCode.EvalIsGreaterThanOrEqual] = r => r.EvalIsGreaterThanOrEqual(),
            [OpCode.EvalIsLessThan] = r => r.EvalIsLessThan(),
            [OpCode.EvalIsLessThanOrEqual] = r => r.EvalIsLessThanOrEqual(),
        };

        /// <summary>
        /// Evaluates the next expression, pushes the result on the stack and
        /// returns true. If no expression is found, this method does not
        /// push anything on the stack and returns false.
        /// </summary>
        private bool EvalExpression()
        {
            // Get number of expression elements
            int count = Reader.GetNextValue();
            if (count == 0)
                return false;
            // Call handlers for each expression bytecode
            for (int i = 0; i < count; i++)
            {
                OpCode bytecode = Reader.GetNext();
                Debug.Assert(EvalHandlerLookup.ContainsKey(bytecode));
                EvalHandlerLookup[bytecode](this);
            }
            return true;
        }

        private void EvalLiteral()
        {
            // Must be copy to avoid modifying original
            VarStack.Push(new Variable(Literals[Reader.GetNextValue()]));
        }

        private void EvalVariable()
        {
            // Must be copy to avoid modifying original
            VarStack.Push(new Variable(GetVariable(Reader.GetNextValue())));
        }

        private void EvalCreateList()
        {
            // Create new array variable
            bool result = EvalExpression();
            Debug.Assert(result);
            Variable size = VarStack.Pop();
            VarStack.Push(Variable.CreateList(size.ToInteger()));
        }

        private void EvalInitializeList()
        {
            int count = Reader.GetNextValue();
            List<Variable> variables = new List<Variable>(count);
            for (int i = 0; i < count; i++)
            {
                bool result = EvalExpression();
                Debug.Assert(result);
                variables.Add(VarStack.Pop());
            }
            VarStack.Push(new Variable(variables));
        }

        private void EvalListVariable()
        {
            Variable array = GetVariable(Reader.GetNextValue());
            // Evaluate index
            bool result = EvalExpression();
            Debug.Assert(result);
            Variable index = VarStack.Pop();
            VarStack.Push(array.GetAt(index.ToInteger() - 1));
        }

        private void EvalFunction()
        {
            // Get function
            int functionId = Reader.GetNextValue();
            Function function = Functions[functionId];

            // Build arguments
            int argCount = Reader.GetNextValue();
            Variable[] arguments = new Variable[argCount];
            for (int i = 0; i < argCount; i++)
            {
                bool result = EvalExpression();
                Debug.Assert(result);
                arguments[i] = VarStack.Pop();
            }

            if (function.IsIntrinsic)
            {
                // Intrinsic functions are those defined from C# code
                Variable returnValue = new Variable();
                // Run function
                if (function is InternalFunction internalFunction)
                    internalFunction.Action(arguments, returnValue);
                else
                    OnFunction(function.Name, arguments, returnValue);
                // Push result onto stack
                VarStack.Push(returnValue);
            }
            else if (function is UserFunction userFunction)
            {
                // User function are those defined in the script we are running.
                // We don't require the number of arguments passed to match the number of
                // function parameters in user functions
                var runtimeFunction = new RuntimeFunction(userFunction);
                // Set parameter values
                Debug.Assert(runtimeFunction.Parameters != null);
                Array.Copy(arguments, runtimeFunction.Parameters, Math.Min(argCount, runtimeFunction.Parameters.Length));
                // Run function
                ExecuteFunction(runtimeFunction);
                // Push result onto stack
                VarStack.Push(new Variable(runtimeFunction.ReturnValue));
            }
        }

        private void EvalAdd()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(var1.Add(var2));
        }

        private void EvalSubtract()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(var1.Subtract(var2));
        }

        private void EvalMultiply()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(var1.Multiply(var2));
        }

        private void EvalDivide()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(var1.Divide(var2));
        }

        private void EvalPower()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(var1.Power(var2));
        }

        private void EvalModulus()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(var1.Modulus(var2));
        }

        private void EvalConcat()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(var1.Concat(var2));
        }

        private void EvalNegate()
        {
            Variable var = VarStack.Pop();
            VarStack.Push(var.Negate());
        }

        private void EvalAnd()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(new Variable(var1.ToInteger() & var2.ToInteger()));
        }

        private void EvalOr()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(new Variable(var1.ToInteger() | var2.ToInteger()));
        }

        private void EvalXor()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(new Variable(var1.ToInteger() ^ var2.ToInteger()));
        }

        private void EvalNot()
        {
            Variable var = VarStack.Pop();
            VarStack.Push(new Variable(~var.ToInteger()));
        }

        private void EvalIsEqual()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(new Variable(var1.IsEqual(var2) ? Boolean.True : Boolean.False));
        }

        private void EvalIsNotEqual()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(new Variable(var1.IsNotEqual(var2) ? Boolean.True : Boolean.False));
        }

        private void EvalIsGreaterThan()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(new Variable(var1.IsGreaterThan(var2) ? Boolean.True : Boolean.False));
        }

        private void EvalIsGreaterThanOrEqual()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(new Variable(var1.IsGreaterThanOrEqual(var2) ? Boolean.True : Boolean.False));
        }

        private void EvalIsLessThan()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(new Variable(var1.IsLessThan(var2) ? Boolean.True : Boolean.False));
        }

        private void EvalIsLessThanOrEqual()
        {
            Variable var2 = VarStack.Pop();
            Variable var1 = VarStack.Pop();
            VarStack.Push(new Variable(var1.IsLessThanOrEqual(var2) ? Boolean.True : Boolean.False));
        }

        #endregion

        #region Events

        public event EventHandler<BeginEventArgs> Begin;
        public event EventHandler<EndEventArgs> End;
        public event EventHandler<FunctionEventArgs> Function;

        internal void OnBegin()
        {
            BeginEventArgs e = new BeginEventArgs
            {
                UserData = UserData,
            };
            Begin?.Invoke(this, e);
            UserData = e.UserData;
        }

        internal void OnEnd()
        {
            EndEventArgs e = new EndEventArgs
            {
                UserData = UserData,
            };
            End?.Invoke(this, e);
            UserData = e.UserData;
        }

        internal void OnFunction(string name, Variable[] parameters, Variable returnValue)
        {
            FunctionEventArgs e = new FunctionEventArgs
            {
                Name = name,
                ReturnValue = returnValue,
                Parameters = parameters,
                UserData = UserData,
            };
            Function?.Invoke(this, e);
            UserData = e.UserData;
        }

        #endregion

        #region Helpers

        private Variable GetVariable(int varId)
        {
            if (ByteCodeUtils.IsGlobalVariable(varId))
                return Variables[ByteCodeUtils.GetVariableIndex(varId)];
            var context = GetFunctionContext();
            if (ByteCodeUtils.IsLocalVariable(varId))
                return context.Variables[ByteCodeUtils.GetVariableIndex(varId)];
            // Function parameter
            return context.Parameters[ByteCodeUtils.GetVariableIndex(varId)];
        }

        private RuntimeFunction GetFunctionContext()
        {
            Debug.Assert(FunctionStack.Count > 0);
            return FunctionStack.Peek();
        }

        #endregion

    }
}
