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
        Stack<Stack<Symbol>> operations = new Stack<Stack<Symbol>>();
        Result result = Result.OK;

        private double Addition(double a, double b)
        {
            limitDoubles(ref a, ref b, bottomResult: 0D);
            return b + a;
        }

        private double Substraction(double a, double b)
        {
            limitDoubles(ref a, ref b, bottomResult: 0D);
            return b - a;
        }

        private double Multiplication(double a, double b)
        {
            limitDoubles(ref a, ref b, bottomResult: 0D);
            return b * a;
        }

        private double Division(double a, double b, out bool incorrect_input)
        {
            incorrect_input = a == 0D;
            limitDouble(ref a);
            limitDouble(ref b, bottomResult: 0D);
            return b / a;
        }

        private double Modulus(double a, double b)
        {
            limitDoubles(ref a, ref b);
            return b % a;
        }

        private double Exponential(double a, double b)
        {
            limitDouble(ref a, 100D, 0.01D);
            limitDouble(ref b);
            return Math.Pow(b, a);
        }

        private double Logarithm(double a, double b, out bool incorrect_input)
        {
            incorrect_input = a == 0D;
            limitDouble(ref a, 100D, 2D, 100D, 2D);
            limitDouble(ref b);
            return Math.Log(b, a);
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

        private Symbol OperationPop()
        {
            return operations.Peek().Pop();
        }

        private void OperationPush(Symbol input)
        {
            operations.Peek().Push(input);
        }

        private int OperationCount()
        {
            return operations.Peek().Count;
        }

        private void applyOperation()
        {
            if (operations.Count <= 0)
                return;
            if (Count() > 1)
            {
                Symbol operation = OperationPop();
                if (operation == Symbol.Addition)
                    Push(Addition(Pop(), Pop()));
                else if (operation == Symbol.Substraction)
                    Push(Substraction(Pop(), Pop()));
                else if (operation == Symbol.Multiplication)
                    Push(Multiplication(Pop(), Pop()));
                else if (operation == Symbol.Division)
                {
                    Push(Division(Pop(), Pop(), out bool incorrect_input));
                    if (incorrect_input)
                        result = Result.DivisionByZero;
                }
                else if (operation == Symbol.Modulus)
                    Push(Modulus(Pop(), Pop()));
                else if (operation == Symbol.Exponential)
                    Push(Exponential(Pop(), Pop()));
                else if (operation == Symbol.Logarithm)
                {
                    Push(Logarithm(Pop(), Pop(), out bool incorrect_input));
                    if (incorrect_input)
                        result = Result.LogarithmWrongInputs;
                }
            } else
                result = Result.NumberStackWrongElements;
        }

        public bool Compute(string expression, out Result outResult)
        {
            numbers.Clear();
            operations.Clear();
            numbers.Push(new Stack<double>());
            operations.Push(new Stack<Symbol>());
            this.expression = expression.Clone().ToString();
            pos = 0;
            try
            {
                actualSymbol = getNextSymbol();
                while (actualSymbol != Symbol.ExpressionEnd && result == Result.OK)
                {
                    if (actualSymbol == Symbol.OpenParenthesis)
                    {
                        numbers.Push(new Stack<double>());
                        operations.Push(new Stack<Symbol>());
                    }
                    else if (actualSymbol == Symbol.CloseParenthesis)
                    {
                        Stack<double> aa;
                        try
                        {
                            aa = numbers.Pop();
                            operations.Pop();
                            if (Count() > 1 || aa.Count > 1)
                            {
                                int a = 0;
                            }
                            double aux = aa.Pop();
                            Push(aux);
                            if (OperationCount() > 0)
                                OperationPush(OperationPop());
                            if (Count() > 1)
                            {
                                applyOperation();
                            }
                        } catch (Exception e)
                        {
                            int a = 0;
                        }
                    }
                    else if (actualSymbol != Symbol.Number)
                        OperationPush(actualSymbol);
                    else if (Count() > 1)
                        applyOperation();
                    actualSymbol = getNextSymbol();
                }
            } catch (Exception e)
            {
                int a = 0;
            }
            if (result == Result.OK && (numbers.Count != 1 || Count() != 1))
                result = Result.NumberStackWrongElements;
            outResult = result;
            if (outResult != Result.OK)
                return false;
            return Pop() > 0D ? true : false;
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

        private void limitDoubles(ref double a, ref double b, double bottomResult = 0.00000000001D)
        {
            limitDouble(ref a, bottomResult: bottomResult);
            limitDouble(ref b, bottomResult: bottomResult);
        }
    }
}
