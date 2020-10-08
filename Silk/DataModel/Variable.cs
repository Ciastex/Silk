using System;
using System.Collections.Generic;
using System.Diagnostics;
using Silk.Compiler;

namespace Silk.DataModel
{
    /// <remarks>
    /// To handle variables of different types, the actual value is stored
    /// in a Value-derived object.
    /// </remarks>
    public class Variable : IEquatable<Variable>
    {
        private Value _value;

        public Variable() => SetValue(0);
        public Variable(string value) => SetValue(value);
        public Variable(int value) => SetValue(value);
        public Variable(double value) => SetValue(value);
        public Variable(Variable var) => SetValue(var);
        public Variable(IEnumerable<Variable> variables) => SetValue(variables);

        internal Variable(Token token) => SetValue(token);
        internal Variable(Value value) => SetValue(value);

        public static Variable CreateList(int size) => new Variable(new ListValue(size));

        public void SetValue(string value)
        {
            if (_value is StringValue stringValue)
                stringValue.Value = value;
            else
                _value = new StringValue(value);
        }

        public void SetValue(int value)
        {
            if (_value is IntegerValue integerValue)
                integerValue.Value = value;
            else
                _value = new IntegerValue(value);
        }

        public void SetValue(double value)
        {
            if (_value is FloatValue floatValue)
                floatValue.Value = value;
            else
                _value = new FloatValue(value);
        }

        public void SetValue(IEnumerable<Variable> values)
        {
            if (_value is ListValue listValue)
            {
                listValue.Value.Clear();
                listValue.Value.AddRange(values);
            }
            else _value = new ListValue(values);
        }

        public void SetValue(Variable var)
        {
            switch (var.Type)
            {
                case ValueType.String:
                    SetValue(var.ToString());
                    break;
                case ValueType.Integer:
                    SetValue(var.ToInteger());
                    break;
                case ValueType.Float:
                    SetValue(var.ToFloat());
                    break;
                case ValueType.List:
                    SetValue(var.GetList());
                    break;
                default:
                    throw new Exception($"Attempting to convert Variable with unknown data type ({var.Type})");
            }
        }

        internal void SetValue(Token token)
        {
            switch (token.Type)
            {
                case TokenType.String:
                    _value = new StringValue(token.Value);
                    break;
                case TokenType.Integer:
                    _value = new IntegerValue(token.Value);
                    break;
                case TokenType.Float:
                    _value = new FloatValue(token.Value);
                    break;
                default:
                    throw new Exception($"Attempted to convert {token.Type} token ({token.Value}) to variable.");
            }
        }

        internal void SetValue(Value value)
        {
            Debug.Assert(value != null);
            _value = value;
        }

        public ValueType Type => _value.Type;

        public bool IsList => _value.Type == ValueType.List;
        public int ListCount => _value.ListCount;

        public Variable GetAt(int index)
            => (_value is ListValue listValue) ? listValue.GetAt(index) : (index == 0) ? this : new Variable();

        public IEnumerable<Variable> GetList()
            => IsList ? _value.GetList() : new[] {this};

        public override string ToString() 
            => _value.ToString();

        public int ToInteger() 
            => _value.ToInteger();

        public double ToFloat() 
            => _value.ToFloat();

        public bool IsFloat() => _value.IsFloat();

        public Variable Add(Variable value) => _value.Add(value);
        public Variable Add(string value) => _value.Add(value);
        public Variable Add(int value) => _value.Add(value);
        public Variable Add(double value) => _value.Add(value);

        public Variable Subtract(Variable value) => _value.Subtract(value);
        public Variable Subtract(string value) => _value.Subtract(value);
        public Variable Subtract(int value) => _value.Subtract(value);
        public Variable Subtract(double value) => _value.Subtract(value);

        public Variable Multiply(Variable value) => _value.Multiply(value);
        public Variable Multiply(string value) => _value.Multiply(value);
        public Variable Multiply(int value) => _value.Multiply(value);
        public Variable Multiply(double value) => _value.Multiply(value);

        public Variable Divide(Variable value) => _value.Divide(value);
        public Variable Divide(string value) => _value.Divide(value);
        public Variable Divide(int value) => _value.Divide(value);
        public Variable Divide(double value) => _value.Divide(value);

        public Variable Power(Variable value) => _value.Power(value);
        public Variable Power(string value) => _value.Power(value);
        public Variable Power(int value) => _value.Power(value);
        public Variable Power(double value) => _value.Power(value);

        public Variable Modulus(Variable value) => _value.Modulus(value);
        public Variable Modulus(string value) => _value.Modulus(value);
        public Variable Modulus(int value) => _value.Modulus(value);
        public Variable Modulus(double value) => _value.Modulus(value);

        public Variable Concat(Variable value) => _value.Concat(value);
        public Variable Concat(string value) => _value.Concat(value);
        public Variable Concat(int value) => _value.Concat(value);
        public Variable Concat(double value) => _value.Concat(value);

        public Variable Negate() => _value.Negate();


        public bool IsEqual(Variable var) => _value.IsEqual(var);
        public bool IsEqual(string var) => _value.IsEqual(var);
        public bool IsEqual(int var) => _value.IsEqual(var);
        public bool IsEqual(double var) => _value.IsEqual(var);

        public bool IsNotEqual(Variable var) => _value.IsNotEqual(var);
        public bool IsNotEqual(string var) => _value.IsNotEqual(var);
        public bool IsNotEqual(int var) => _value.IsNotEqual(var);
        public bool IsNotEqual(double var) => _value.IsNotEqual(var);

        public bool IsGreaterThan(Variable var) => _value.IsGreaterThan(var);
        public bool IsGreaterThan(string var) => _value.IsGreaterThan(var);
        public bool IsGreaterThan(int var) => _value.IsGreaterThan(var);
        public bool IsGreaterThan(double var) => _value.IsGreaterThan(var);

        public bool IsGreaterThanOrEqual(Variable var) => _value.IsGreaterThanOrEqual(var);
        public bool IsGreaterThanOrEqual(string var) => _value.IsGreaterThanOrEqual(var);
        public bool IsGreaterThanOrEqual(int var) => _value.IsGreaterThanOrEqual(var);
        public bool IsGreaterThanOrEqual(double var) => _value.IsGreaterThanOrEqual(var);

        public bool IsLessThan(Variable var) => _value.IsLessThan(var);
        public bool IsLessThan(string var) => _value.IsLessThan(var);
        public bool IsLessThan(int var) => _value.IsLessThan(var);
        public bool IsLessThan(double var) => _value.IsLessThan(var);

        public bool IsLessThanOrEqual(Variable var) => _value.IsLessThanOrEqual(var);
        public bool IsLessThanOrEqual(string var) => _value.IsLessThanOrEqual(var);
        public bool IsLessThanOrEqual(int var) => _value.IsLessThanOrEqual(var);
        public bool IsLessThanOrEqual(double var) => _value.IsLessThanOrEqual(var);

        public bool IsTrue() => _value.IsTrue();
        public bool IsFalse() => _value.IsFalse();

        public static bool operator ==(Variable var1, Variable var2) => var1.IsEqual(var2);
        public static bool operator ==(Variable var1, string var2) => var1.IsEqual(var2);
        public static bool operator ==(Variable var1, int var2) => var1.IsEqual(var2);
        public static bool operator ==(Variable var1, double var2) => var1.IsEqual(var2);

        public static bool operator !=(Variable var1, Variable var2) => var1.IsNotEqual(var2);
        public static bool operator !=(Variable var1, string var2) => var1.IsNotEqual(var2);
        public static bool operator !=(Variable var1, int var2) => var1.IsNotEqual(var2);
        public static bool operator !=(Variable var1, double var2) => var1.IsNotEqual(var2);

        public static bool operator >(Variable var1, Variable var2) => var1.IsGreaterThan(var2);
        public static bool operator >(Variable var1, string var2) => var1.IsGreaterThan(var2);
        public static bool operator >(Variable var1, int var2) => var1.IsGreaterThan(var2);
        public static bool operator >(Variable var1, double var2) => var1.IsGreaterThan(var2);

        public static bool operator >=(Variable var1, Variable var2) => var1.IsGreaterThanOrEqual(var2);
        public static bool operator >=(Variable var1, string var2) => var1.IsGreaterThanOrEqual(var2);
        public static bool operator >=(Variable var1, int var2) => var1.IsGreaterThanOrEqual(var2);
        public static bool operator >=(Variable var1, double var2) => var1.IsGreaterThanOrEqual(var2);

        public static bool operator <(Variable var1, Variable var2) => var1.IsLessThan(var2);
        public static bool operator <(Variable var1, string var2) => var1.IsLessThan(var2);
        public static bool operator <(Variable var1, int var2) => var1.IsLessThan(var2);
        public static bool operator <(Variable var1, double var2) => var1.IsLessThan(var2);

        public static bool operator <=(Variable var1, Variable var2) => var1.IsLessThanOrEqual(var2);
        public static bool operator <=(Variable var1, string var2) => var1.IsLessThanOrEqual(var2);
        public static bool operator <=(Variable var1, int var2) => var1.IsLessThanOrEqual(var2);
        public static bool operator <=(Variable var1, double var2) => var1.IsLessThanOrEqual(var2);

        public static Variable operator +(Variable var1, Variable var2) => var1.Add(var2);
        public static Variable operator +(Variable var1, string value) => var1.Add(value);
        public static Variable operator +(Variable var1, int value) => var1.Add(value);
        public static Variable operator +(Variable var1, double value) => var1.Add(value);

        public static Variable operator -(Variable var1, Variable var2) => var1.Subtract(var2);
        public static Variable operator -(Variable var1, string value) => var1.Subtract(value);
        public static Variable operator -(Variable var1, int value) => var1.Subtract(value);
        public static Variable operator -(Variable var1, double value) => var1.Subtract(value);

        public static Variable operator *(Variable var1, Variable var2) => var1.Multiply(var2);
        public static Variable operator *(Variable var1, string value) => var1.Multiply(value);
        public static Variable operator *(Variable var1, int value) => var1.Multiply(value);
        public static Variable operator *(Variable var1, double value) => var1.Multiply(value);

        public static Variable operator /(Variable var1, Variable var2) => var1.Divide(var2);
        public static Variable operator /(Variable var1, string value) => var1.Divide(value);
        public static Variable operator /(Variable var1, int value) => var1.Divide(value);
        public static Variable operator /(Variable var1, double value) => var1.Divide(value);

        public static Variable operator %(Variable var1, Variable var2) => var1.Modulus(var2);
        public static Variable operator %(Variable var1, string value) => var1.Modulus(value);
        public static Variable operator %(Variable var1, int value) => var1.Modulus(value);
        public static Variable operator %(Variable var1, double value) => var1.Modulus(value);

        public static Variable operator -(Variable var1) => var1.Negate();

        public Variable this[int index] => GetAt(index);

        public override bool Equals(object value)
        {
            return Equals(value as Variable);
        }

        public bool Equals(Variable value)
        {
            if (ReferenceEquals(value, null))
                return false;
            return _value.Equals(value._value);
        }

        public override int GetHashCode() => _value.GetHashCode();
    }
}