using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2
{
    public class LogicalExpressionProcessor
    {
        private List<char> uniqueOperands;
        private string rpnResult;
        private Table truthTableResult;

        public LogicalExpressionProcessor(string expression)
        {
            this.rpnResult = ToReversePolishNotation(expression);
            if (this.rpnResult != "error")
            {
                this.uniqueOperands = GetUniqueOperands();
                this.truthTableResult = GenerateTruthTable();
            }

        }

        public string RpnResult => rpnResult;
        public Table TruthTableResult => truthTableResult;

        private Table GenerateTruthTable()
        {
            int n = uniqueOperands.Count;
            int rows = (int)Math.Pow(2, n);
            Table table = new Table(rows);
            foreach (char operand in uniqueOperands)
            {
                table.AddColumn(operand.ToString());
            }

            for (int i = 0; i < rows; i++)
            {
                string binary = Convert.ToString(i, 2).PadLeft(n, '0');
                bool[] rowValues = binary.Select(c => c == '1').ToArray();
                for (int j = 0; j < rowValues.Length; j++)
                {
                    table.SetValue(uniqueOperands[j].ToString(), i, rowValues[j]);
                }
            }

            return table;
        }

        private List<char> GetUniqueOperands()
        {
            List<char> operands = this.rpnResult.Distinct().Where(c => Char.IsLetter(c)).OrderBy(c => c).ToList();
            return operands;
        }

        private string ToReversePolishNotation(string expression)
        {
            Stack<char> stack = new Stack<char>();
            string output = "";
            int openParenthesesCount = 0;
            int closeParenthesesCount = 0;

            foreach (char c in expression)
            {
                if (Char.IsLetterOrDigit(c))
                {
                    output += c;
                }
                else if (c == '(')
                {
                    stack.Push(c);
                    openParenthesesCount++;
                }
                else if (c == ')')
                {
                    if (openParenthesesCount <= closeParenthesesCount)
                    {
                        return "error";
                    }
                    while (stack.Count > 0 && stack.Peek() != '(')
                    {
                        output += stack.Pop();
                    }
                    stack.Pop(); // Отбрасываем '('
                    closeParenthesesCount++;
                }
                else
                {
                    while (stack.Count > 0 && Precedence(stack.Peek()) >= Precedence(c))
                    {
                        output += stack.Pop();
                    }
                    stack.Push(c);
                }
            }

            while (stack.Count > 0)
            {
                output += stack.Pop();
            }

            if (openParenthesesCount != closeParenthesesCount)
            {
                return "error";
            }

            return output;
        }


        private int Precedence(char c)
        {
            switch (c)
            {
                case '!':
                    return 5;
                case '&':
                    return 4;
                case '|':
                    return 3;
                case '>':
                    return 2;
                case '~':
                    return 1;
                default:
                    return 0;
            }
        }

        private static bool IsOperand(string token)
        {
            return !token.Equals("!") && !token.Equals("&") && !token.Equals("|") && !token.Equals(">") && !token.Equals("~");
        }

        private static bool PerformOperation(bool operand1, bool operand2, string op)
        {
            switch (op)
            {
                case "&":
                    return operand1 && operand2;
                case "|":
                    return operand1 || operand2;
                case ">":
                    return !operand1 || operand2;
                case "~":
                    return operand1 == operand2;
                default:
                    throw new ArgumentException("Недопустимый оператор");
            }
        }
        public void CalculateExpression()
        {
            Stack<string> stack = new Stack<string>();
            char[] tokens = rpnResult.ToCharArray();

            foreach (var token in tokens)
            {
                if (IsOperand(token.ToString()))
                {
                    stack.Push(token.ToString());
                }
                else
                {
                    string operand1, operand2;
                    if (token == '!')
                    {
                        operand1 = stack.Pop();
                        string resultColumn = $"!{operand1}";
                        truthTableResult.AddColumn(resultColumn);
                        for (int i = 0; i < truthTableResult.GetRowCount(); i++)
                        {
                            bool val = !truthTableResult.GetCellValue(operand1, i);
                            truthTableResult.SetValue(resultColumn, i, val);
                        }
                        stack.Push(resultColumn);
                    }
                    else
                    {
                        operand2 = stack.Pop();
                        operand1 = stack.Pop();
                        string resultColumn = $"({operand1}{token}{operand2})";
                        truthTableResult.AddColumn(resultColumn);
                        for (int i = 0; i < truthTableResult.GetRowCount(); i++)
                        {
                            bool val1 = truthTableResult.GetCellValue(operand1, i);
                            bool val2 = truthTableResult.GetCellValue(operand2, i);
                            bool result = PerformOperation(val1, val2, token.ToString());
                            truthTableResult.SetValue(resultColumn, i, result);
                        }
                        stack.Push(resultColumn);
                    }
                }
            }
        }

        public string GenerateSKNF10()
        {
            List<int> cnfClauses = new List<int>();

            int lastIndex = truthTableResult.GetColumnNames().Count - 1;

            for (int i = 0; i < truthTableResult.GetRowCount(); i++)
            {
                if (!truthTableResult.GetCellValue(truthTableResult.GetColumnNames()[lastIndex], i))
                {
                    string disjunction = "";
                    foreach (char operand in uniqueOperands)
                    {
                        bool operandValue = truthTableResult.GetCellValue(operand.ToString(), i);
                        if (operandValue)
                            disjunction += "1";
                        else
                            disjunction += "0";
                    }
                    cnfClauses.Add(Convert.ToInt32(disjunction, 2));
                }
            }
            string cnf = string.Join(",", cnfClauses);
            cnf = "(" + cnf + ")& ";
            return cnf;
        }
        public string GenerateSCNF()
        {
            List<string> cnfClauses = new List<string>();

            int lastIndex = truthTableResult.GetColumnNames().Count - 1;

            for (int i = 0; i < truthTableResult.GetRowCount(); i++)
            {
                if (!truthTableResult.GetCellValue(truthTableResult.GetColumnNames()[lastIndex], i))
                {
                    List<string> disjunction = new List<string>();
                    foreach (char operand in uniqueOperands)
                    {
                        bool operandValue = truthTableResult.GetCellValue(operand.ToString(), i);
                        if (operandValue)
                            disjunction.Add($"!{operand}");
                        else
                            disjunction.Add($"{operand}");
                    }
                    cnfClauses.Add($"({string.Join("|", disjunction)})");
                }
            }
            string cnf = string.Join("&", cnfClauses);
            return cnf;
        }

        public string GenerateSDNF10()
        {
            List<int> cnfClauses = new List<int>();

            int lastIndex = truthTableResult.GetColumnNames().Count - 1;

            for (int i = 0; i < truthTableResult.GetRowCount(); i++)
            {
                if (truthTableResult.GetCellValue(truthTableResult.GetColumnNames()[lastIndex], i))
                {
                    string disjunction = "";
                    foreach (char operand in uniqueOperands)
                    {
                        bool operandValue = truthTableResult.GetCellValue(operand.ToString(), i);
                        if (operandValue)
                            disjunction += "1";
                        else
                            disjunction += "0";
                    }
                    cnfClauses.Add(Convert.ToInt32(disjunction, 2));
                }
            }
            string cnf = string.Join(",", cnfClauses);
            cnf = "(" + cnf + ")| ";
            return cnf;
        }

        public string GenerateSDNF()
        {
            List<string> dnfClauses = new List<string>();
            int lastIndex = truthTableResult.GetColumnNames().Count - 1;
            for (int i = 0; i < truthTableResult.GetRowCount(); i++)
            {
                if (truthTableResult.GetCellValue(truthTableResult.GetColumnNames()[lastIndex], i))
                {
                    List<string> conjunction = new List<string>();
                    foreach (char operand in uniqueOperands)
                    {
                        bool operandValue = truthTableResult.GetCellValue(operand.ToString(), i);
                        if (!operandValue)
                            conjunction.Add($"!{operand}");
                        else
                            conjunction.Add($"{operand}");
                    }
                    dnfClauses.Add($"({string.Join("&", conjunction)})");
                }
            }
            string dnf = string.Join("|", dnfClauses);
            return dnf;
        }

        public List<bool> GetFunctionVector()
        {
            List<bool> functionVector = new List<bool>();
            int lastIndex = truthTableResult.GetColumnNames().Count - 1;
            for (int i = 0; i < truthTableResult.GetRowCount(); i++)
            {
                bool value = truthTableResult.GetCellValue(truthTableResult.GetColumnNames()[lastIndex], i);
                functionVector.Add(value);
            }

            return functionVector;
        }

        public int GetFunctionVectorDecimalValue()
        {
            List<bool> functionVector = GetFunctionVector();
            string binaryString = "";
            foreach (bool value in functionVector)
            {
                binaryString += value ? "1" : "0";
            }
            int decimalValue = Convert.ToInt32(binaryString, 2);
            return decimalValue;
        }
    }
}
