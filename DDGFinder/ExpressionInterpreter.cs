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
        Stack<double> numbers = new Stack<double>();
        Result result = Result.OK;
        Symbol operation;

        private double Addition(double a, double b)
        {

        }

        private double Substraction(double a, double b)
        {

        }

        private double Multiplication(double a, double b)
        {

        }

        private double Division(double a, double b)
        {

        }

        private double Modulus(double a, double b)
        {

        }

        private double Exponential(double a, double b)
        {

        }

        private double Logarithm(double a, double b)
        {

        }

        public bool Compute(string expression, out Result result)
        {
            numbers.Clear();
            this.expression = expression.Clone().ToString();
            pos = 0;
            try
            {
                actualSymbol = getNextSymbol();
                while (actualSymbol != Symbol.ExpressionEnd)
                {
                    if (actualSymbol != Symbol.Number)
                        operation = actualSymbol;
                    else if (numbers.Count > 1)
                    {
                        if (operation == Symbol.Addition)
                            numbers.Push(Addition(numbers.Pop(), numbers.Pop()));
                        else if (operation == Symbol.Substraction)
                            numbers.Push(Substraction(numbers.Pop(), numbers.Pop()));
                        else if (operation == Symbol.Multiplication)
                            numbers.Push(Multiplication(numbers.Pop(), numbers.Pop()));
                        else if (operation == Symbol.Division)
                            numbers.Push(Division(numbers.Pop(), numbers.Pop()));
                        else if (operation == Symbol.Modulus)
                            numbers.Push(Modulus(numbers.Pop(), numbers.Pop()));
                        else if (operation == Symbol.Exponential)
                            numbers.Push(Exponential(numbers.Pop(), numbers.Pop()));
                        else if (operation == Symbol.Logarithm)
                            numbers.Push(Logarithm(numbers.Pop(), numbers.Pop()));
                    }
                    actualSymbol = getNextSymbol();
                }
            } catch (Exception e)
            {
                int a = 0;
            }
            if (numbers.Count != 1)
                result = Result.NumberStackWrongElements;
            else
                result = this.result;
            if (numbers.Count <= 0)
                return false;
            return Math.Round(numbers.Pop()) > 0D ? true : false;
        }

        private Symbol getNextSymbol()
        {
            char c;
            if (pos >= expression.Length)
                return Symbol.ExpressionEnd;
            c = expression[pos++];
            switch (c)
            {
                case '+': return Symbol.Addition;
                case '-': return Symbol.Substraction;
                case '*': return Symbol.Multiplication;
                case '/': return Symbol.Division;
                case '%': return Symbol.Modulus;
                case '^': return Symbol.Exponential;
                case 'L': return Symbol.Logarithm;
            }
            string exp = expression.Substring(pos - 1);
            if (regexNumber.IsMatch(exp))
            {
                Match m = regexNumber.Match(exp);
                string s = m.Value;
                pos += m.Length - 1;
                numbers.Push(double.Parse(s));
                return Symbol.Number;
            }
            result = Result.GetNextSymbolWrongSymbol;
            throw new Exception("getNextSymbol: Wrong input.");
        }

        private enum Symbol
        {
            Number, Addition, Substraction, Multiplication, Division, Modulus, Exponential, Logarithm, ExpressionEnd
        }

        public enum Result
        {
            OK, DivisionByZero, ModulusOnZero, ExponentWrongInputs, LogarithmWrongInputs, FactorWrongSymbol, CloseParenthesisMissing, GetNextSymbolWrongSymbol, NumberStackWrongElements
        }

        /*private void Expression()
        {
            if (result != Result.OK)
                return;
            while (true)
            {
                
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
                            numbers.Push(toDecimal((BigInteger) d1 + (BigInteger) d2));
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
                            numbers.Push(toDecimal((BigInteger) d1 - (BigInteger) d2));
                        } catch (Exception e)
                        {
                            int a = 0;
                        }
                        break;
                    case Symbol.Multiplication:
                        actualSymbol = getNextSymbol();
                        //Factor();
                        if (result != Result.OK)
                            return;
                        d1 = numbers.Pop();
                        d2 = numbers.Pop();
                        try
                        {
                            numbers.Push(toDecimal((BigInteger) d1 * (BigInteger) d2));
                        }
                        catch (Exception e)
                        {
                            int a = 0;
                        }
                        break;
                    case Symbol.Division:
                        actualSymbol = getNextSymbol();
                        //Factor();
                        if (result != Result.OK)
                            return;
                        d1 = numbers.Pop();
                        d2 = numbers.Pop();
                        d2Integer = (BigInteger) d2;
                        if (Math.Abs(d2 - (decimal) d2Integer) < 0.01M)
                        {
                            if (d2Integer != 0)
                            {
                                try
                                {
                                    numbers.Push(toDecimal((BigInteger) d1 / (BigInteger) d2));
                                }
                                catch (Exception e)
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
                        //Factor();
                        if (result != Result.OK)
                            return;
                        d1 = numbers.Pop();
                        d2 = numbers.Pop();
                        d2Integer = (BigInteger) d2;
                        if (Math.Abs(d2 - (decimal) d2Integer) < 0.01M)
                        {
                            if (d2Integer != 0)
                                numbers.Push(d1 % d2);
                            else
                            {
                                result = Result.ModulusOnZero;
                                return;
                            }
                        }
                        else
                            numbers.Push(d1 % d2);
                        break;
                    case Symbol.Exponential:
                        actualSymbol = getNextSymbol();
                        //Factor();
                        if (result != Result.OK)
                            return;
                        d1 = numbers.Pop();
                        d2 = numbers.Pop();
                        if (d1< 0 || d2< 0)
                        {
                            result = Result.ExponentWrongInputs;
                            return;
                        }
                        try
                        {
                            if (d2 > 100M)
                                numbers.Push(decimal.MaxValue);
                            else
                                numbers.Push(toDecimal(BigInteger.Pow((BigInteger) d1, toInt((BigInteger) d2))));
                        }
                        catch (Exception e)
                        {
                            int a = 0;
                        }
                        break;
                    case Symbol.Logarithm:
                        actualSymbol = getNextSymbol();
                        //Factor();
                        if (result != Result.OK)
                            return;
                        d1 = numbers.Pop();
                        d2 = numbers.Pop();
                        if (d1< 0 || d2< 0 || d2 == 1 || (d1 != 1 && d2 == 0) || (d1 == 0 && (d2 > 1 || (d2 > 0 && d2< 1))))
                        {
                            result = Result.LogarithmWrongInputs;
                            return;
                        }
                        try
                        {
                            numbers.Push(toDecimal(BigInteger.Log(toBigInteger((double) d1), infinityToMaxValue((double) d2))));
                        }
                        catch (Exception e)
                        {
                            int a = 0;
                        }
                        break;
                    default: return;
                }
            }
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
            //Factor();
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
                        //Factor();
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
                        //Factor();
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
                        //Factor();
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
                        //Factor();
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
                        //Factor();
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
        }*/
    }
}
