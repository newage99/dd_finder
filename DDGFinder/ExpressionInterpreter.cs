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
        //Stack<double> numbers = new Stack<double>();
        Stack<Stack<double>> numbers = new Stack<Stack<double>>();
        Stack<Symbol> operations = new Stack<Symbol>();
        Result result = Result.OK;

        private double Addition(double a, double b)
        {
            limitDoubles(ref a, ref b);
            return a + b;
        }

        private double Substraction(double a, double b)
        {
            limitDoubles(ref a, ref b);
            return a - b;
        }

        private double Multiplication(double a, double b)
        {
            limitDoubles(ref a, ref b);
            return a * b;
        }

        private double Division(double a, double b)
        {
            limitDoubles(ref a, ref b);
            return a / b;
        }

        private double Modulus(double a, double b)
        {
            limitDoubles(ref a, ref b);
            return a % b;
        }

        private double Exponential(double a, double b)
        {
            limitDouble(ref a);
            limitDouble(ref b, 100D, 0.01D);
            return Math.Pow(a, b);
        }

        private double Logarithm(double a, double b)
        {
            limitDouble(ref a);
            limitDouble(ref b, 100D, 2D, 100D, 2D);
            return Math.Log(a, b);
        }

        private double Pop()
        {
            return numbers.Peek().Pop();
        }

        private void Push(double input)
        {
            numbers.Peek().Push(input);
        }

        private int Count()
        {
            return numbers.Peek().Count;
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
                    if (actualSymbol == Symbol.OpenParenthesis)
                    {
                        numbers.Push(new Stack<double>());
                    }
                    else if (actualSymbol != Symbol.Number && actualSymbol != Symbol.CloseParenthesis)
                        operations.Push(actualSymbol);
                    else if (Count() > 1)
                    {
                        if (actualSymbol == Symbol.CloseParenthesis)
                        {
                            Stack<double> aa = numbers.Pop();
                            if (Count() != 1)
                            {
                                int a = 0;
                            }
                            Push(aa.Pop());
                        }
                        Symbol operation = operations.Pop();
                        if (operation == Symbol.Addition)
                            Push(Addition(Pop(), Pop()));
                        else if (operation == Symbol.Substraction)
                            Push(Substraction(Pop(), Pop()));
                        else if (operation == Symbol.Multiplication)
                            Push(Multiplication(Pop(), Pop()));
                        else if (operation == Symbol.Division)
                            Push(Division(Pop(), Pop()));
                        else if (operation == Symbol.Modulus)
                            Push(Modulus(Pop(), Pop()));
                        else if (operation == Symbol.Exponential)
                            Push(Exponential(Pop(), Pop()));
                        else if (operation == Symbol.Logarithm)
                            Push(Logarithm(Pop(), Pop()));
                    }
                    else
                    {
                        int a = 0;
                    }
                    actualSymbol = getNextSymbol();
                }
            } catch (Exception e)
            {
                int a = 0;
            }
            if (numbers.Count != 1 || numbers.Peek().Count != 1)
                result = Result.NumberStackWrongElements;
            else
                result = this.result;
            if (numbers.Count <= 0)
                return false;
            return Math.Round(Pop()) > 0D ? true : false;
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
                case '(': return Symbol.OpenParenthesis;
                case ')': return Symbol.CloseParenthesis;
            }
            string exp = expression.Substring(pos - 1);
            if (regexNumber.IsMatch(exp))
            {
                Match m = regexNumber.Match(exp);
                string s = m.Value;
                pos += m.Length - 1;
                Push(double.Parse(s));
                return Symbol.Number;
            }
            result = Result.GetNextSymbolWrongSymbol;
            throw new Exception("getNextSymbol: Wrong input.");
        }

        private enum Symbol
        {
            Number, Addition, Substraction, Multiplication, Division, Modulus, Exponential, Logarithm, OpenParenthesis, CloseParenthesis, ExpressionEnd
        }

        public enum Result
        {
            OK, DivisionByZero, ModulusOnZero, ExponentWrongInputs, LogarithmWrongInputs, FactorWrongSymbol, CloseParenthesisMissing, GetNextSymbolWrongSymbol, NumberStackWrongElements
        }

        private void limitDouble(
            ref double input,
            double topThreshold = 100000000000000D,
            double bottomThreshold = 0.00000000001D,
            double topResult = 100000000000000D,
            double bottomResult = 0.00000000001D
            ) {
            if (input > topThreshold)
                input = topResult;
            else if (input < -topThreshold)
                input = -topResult;
            else if ((input < bottomThreshold && input >= 0D) || input == 0D)
                input = bottomResult;
            else if (input > -bottomThreshold && input <= 0D)
                input = -bottomResult;
        }

        private void limitDoubles(ref double a, ref double b)
        {
            limitDouble(ref a);
            limitDouble(ref b);
        }
    }
}
