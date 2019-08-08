using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Numerics;

namespace DDGFinder
{
    class ExpressionInterpreter
    {
        string expression;
        Symbol actualSymbol;
        int pos;
        static Regex regexNumber = new Regex(@"^\d+([,\.]\d+)?");
        Stack<decimal> numbers = new Stack<decimal>();
        Result result = Result.OK;

        public bool Compute(string expression, out Result result)
        {
            numbers.Clear();
            this.expression = expression.Clone().ToString();
            pos = 0;
            actualSymbol = getNextSymbol();
            if (actualSymbol != Symbol.None)
                Expression();
            if (numbers.Count != 1)
                result = Result.NumberStackWrongElements;
            else
                result = this.result;
            if (numbers.Count <= 0)
                return false;
            return decimal.Round(numbers.Pop()) > 0M ? true : false;
        }

        private Symbol getNextSymbol()
        {
            char c;
            if (pos >= expression.Length)
                return Symbol.ExpressionEnd;
            c = expression[pos++];
            switch (c)
            {
                case '(': return Symbol.OpenParenthesis;
                case ')': return Symbol.CloseParenthesis;
                case '+': return Symbol.Addition;
                case '-': return Symbol.Substraction;
                case '*': return Symbol.Multiplication;
                case '/': return Symbol.Division;
                case '%': return Symbol.Modulus;
                case '^': return Symbol.Exponential;
                case 'L': return Symbol.Logarithm;
            }
            string exp = expression.Substring(pos - 1);
            if (regexNumber.IsMatch(exp)) {
                Match m = regexNumber.Match(exp);
                string s = m.Value;
                pos += m.Length - 1;
                numbers.Push(decimal.Parse(s));
                return Symbol.Number;
            }
            result = Result.GetNextSymbolWrongSymbol;
            return Symbol.None;
        }

        private decimal toDecimal(BigInteger input)
        {
            if ((BigInteger)decimal.MaxValue < input)
                return decimal.MaxValue;
            else if ((BigInteger)decimal.MinValue > input)
                return decimal.MinValue;
            try
            {
                return (decimal)input;
            } catch (Exception e)
            {
                int a = 0;
            }
            return 0;
        }

        private void Expression()
        {
            Term();
            if (result != Result.OK)
                return;
            while (true)
            {
                decimal d1;
                decimal d2;
                switch (actualSymbol)
                {
                    case Symbol.Addition:
                        actualSymbol = getNextSymbol();
                        if (result != Result.OK)
                            return;
                        Term();
                        if (result != Result.OK)
                            return;
                        d1 = numbers.Pop();
                        d2 = numbers.Pop();
                        try
                        {
                            numbers.Push(toDecimal((BigInteger)d1 + (BigInteger)d2));
                        } catch (Exception e)
                        {
                            int a = 0;
                        }
                        break;
                    case Symbol.Substraction:
                        actualSymbol = getNextSymbol();
                        if (result != Result.OK)
                            return;
                        Term();
                        if (result != Result.OK)
                            return;
                        d1 = numbers.Pop();
                        d2 = numbers.Pop();
                        try
                        {
                            numbers.Push(toDecimal((BigInteger)d1 - (BigInteger)d2));
                        } catch (Exception e)
                        {
                            int a = 0;
                        }
                        break;
                    default: return;
                }
            }
        }

        private decimal toDecimal(double input)
        {
            BigInteger a = 0;
            BigInteger b = 0;
            BigInteger bigInput = (BigInteger)input;
            try
            {
                a = (BigInteger)decimal.MaxValue;
            }
            catch (Exception e)
            {
                int aa = 0;
            }
            try
            {
                b = (BigInteger)decimal.MinValue;
            } catch (Exception e)
            {
                int aa = 0;
            }
            try {
                if (bigInput > a)
                    return decimal.MaxValue;
                else if (bigInput < b)
                    return decimal.MinValue;
                else
                    return (decimal)bigInput;
            } catch (Exception e)
            {
                int aa = 0;
            }
            return 1;
        }

        private void Term()
        {
            Factor();
            if (result != Result.OK)
                return;
            while (true)
            {
                decimal d1;
                decimal d2;
                BigInteger d2Integer;
                switch (actualSymbol)
                {
                    case Symbol.Multiplication:
                        actualSymbol = getNextSymbol();
                        Factor();
                        if (result != Result.OK)
                            return;
                        d1 = numbers.Pop();
                        d2 = numbers.Pop();
                        try
                        {
                            numbers.Push(toDecimal((BigInteger)d1 * (BigInteger)d2));
                        } catch (Exception e)
                        {
                            int a = 0;
                        }
                        break;
                    case Symbol.Division:
                        actualSymbol = getNextSymbol();
                        Factor();
                        if (result != Result.OK)
                            return;
                        d1 = numbers.Pop();
                        d2 = numbers.Pop();
                        d2Integer = (BigInteger)d2;
                        if (Math.Abs(d2 - (decimal)d2Integer) < 0.01M)
                        {
                            if (d2Integer != 0)
                            {
                                try
                                {
                                    numbers.Push(toDecimal((BigInteger)d1 / (BigInteger)d2));
                                } catch (Exception e)
                                {
                                    int a = 0;
                                }
                            }
                            else
                            {
                                result = Result.DivisionByZero;
                                return;
                            }
                        }
                        else
                            numbers.Push(d1 / d2);
                        break;
                    case Symbol.Modulus:
                        actualSymbol = getNextSymbol();
                        Factor();
                        if (result != Result.OK)
                            return;
                        d1 = numbers.Pop();
                        d2 = numbers.Pop();
                        d2Integer = (BigInteger)d2;
                        if (Math.Abs(d2 - (decimal)d2Integer) < 0.01M)
                        {
                            if (d2Integer != 0)
                                numbers.Push(d1 % d2);
                            else
                            {
                                result = Result.ModulusOnZero;
                                return;
                            }
                        } else
                            numbers.Push(d1 % d2);
                        break;
                    case Symbol.Exponential:
                        actualSymbol = getNextSymbol();
                        Factor();
                        if (result != Result.OK)
                            return;
                        d1 = numbers.Pop();
                        d2 = numbers.Pop();
                        if (d1 < 0 || d2 < 0)
                        {
                            result = Result.ExponentWrongInputs;
                            return;
                        }
                        try
                        {
                            if (d2 > 100M)
                                numbers.Push(decimal.MaxValue);
                            else
                                numbers.Push(toDecimal(BigInteger.Pow((BigInteger)d1, toInt((BigInteger)d2))));
                        } catch (Exception e)
                        {
                            int a = 0;
                        }
                        break;
                    case Symbol.Logarithm:
                        actualSymbol = getNextSymbol();
                        Factor();
                        if (result != Result.OK)
                            return;
                        d1 = numbers.Pop();
                        d2 = numbers.Pop();
                        if (d1 < 0 || d2 < 0 || d2 == 1 || (d1 != 1 && d2 == 0) || (d1 == 0 && (d2 > 1 || (d2 > 0 && d2 < 1))))
                        {
                            result = Result.LogarithmWrongInputs;
                            return;
                        }
                        try
                        {
                            numbers.Push(toDecimal(BigInteger.Log(toBigInteger((double)d1), infinityToMaxValue((double)d2))));
                        } catch (Exception e)
                        {
                            int a = 0;
                        }
                        break;
                    default: return;
                }
            }
        }

        private int toInt(BigInteger input)
        {
            if (input > (BigInteger)int.MaxValue)
                return int.MaxValue;
            return (int)input;
        }

        private double infinityToMaxValue(double input)
        {
            if (Double.IsInfinity(input))
                return double.MaxValue;
            return input;
        }

        private BigInteger toBigInteger(double input)
        {
            if (Double.IsInfinity(input))
                return (BigInteger)double.MaxValue;
            return (BigInteger)input;
        }

        private void Factor()
        {
            if (actualSymbol == Symbol.OpenParenthesis)
            {
                actualSymbol = getNextSymbol();
                Expression();
                if (result != Result.OK)
                    return;
                if (actualSymbol != Symbol.CloseParenthesis)
                {
                    result = Result.CloseParenthesisMissing;
                    return;
                }
                actualSymbol = getNextSymbol();
            } else if (actualSymbol == Symbol.Number)
            {
                actualSymbol = getNextSymbol();
            } else
            {
                result = Result.FactorWrongSymbol;
            }
        }

        private enum Symbol
        {
            None, Addition, Substraction, Multiplication, Division, Modulus, Exponential, Logarithm, OpenParenthesis, CloseParenthesis, Number, ExpressionEnd
        }

        public enum Result
        {
            OK, DivisionByZero, ModulusOnZero, ExponentWrongInputs, LogarithmWrongInputs, FactorWrongSymbol, CloseParenthesisMissing, GetNextSymbolWrongSymbol, NumberStackWrongElements
        }
    }
}
