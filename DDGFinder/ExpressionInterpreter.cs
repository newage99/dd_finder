using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

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
            return ((int)decimal.Round(numbers.Pop())) > 0 ? true : false;
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

        private void Expression()
        {
            Term();
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
                        numbers.Push(numbers.Pop() + numbers.Pop());
                        break;
                    case Symbol.Substraction:
                        actualSymbol = getNextSymbol();
                        if (result != Result.OK)
                            return;
                        Term();
                        if (result != Result.OK)
                            return;
                        numbers.Push(numbers.Pop() - numbers.Pop());
                        break;
                    default: return;
                }
            }
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
                int d2Integer;
                switch (actualSymbol)
                {
                    case Symbol.Multiplication:
                        actualSymbol = getNextSymbol();
                        Factor();
                        if (result != Result.OK)
                            return;
                        numbers.Push(numbers.Pop() * numbers.Pop());
                        break;
                    case Symbol.Division:
                        actualSymbol = getNextSymbol();
                        Factor();
                        if (result != Result.OK)
                            return;
                        d1 = numbers.Pop();
                        d2 = numbers.Pop();
                        d2Integer = (int)d2;
                        if (Math.Abs(d2 - d2Integer) < 0.01M)
                        {
                            if (d2Integer != 0)
                                numbers.Push(d1 / d2Integer);
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
                        d2Integer = (int)d2;
                        if (Math.Abs(d2 - d2Integer) < 0.01M)
                        {
                            if (d2Integer != 0)
                                numbers.Push(d1 % d2Integer);
                            else
                            {
                                result = Result.ModulusOnZero;
                                return;
                            }
                        }
                        else
                            numbers.Push(d1 % d2);
                        break;
                    default: return;
                }
            }
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
            None, Addition, Substraction, Multiplication, Division, Modulus, OpenParenthesis, CloseParenthesis, Number, ExpressionEnd
        }

        public enum Result
        {
            OK, DivisionByZero, ModulusOnZero, FactorWrongSymbol, CloseParenthesisMissing, GetNextSymbolWrongSymbol, NumberStackWrongElements
        }
    }
}
