using System.Text;

public class Table
{
    private Dictionary<string, List<bool>> columns;
    private Dictionary<string, int> columnIndexByName;
    private int rowCount;

    public Table(int rowCount)
    {
        this.rowCount = rowCount;
        columns = new Dictionary<string, List<bool>>();
        columnIndexByName = new Dictionary<string, int>();
    }

    public void AddColumn(string columnName)
    {
        if (!columns.ContainsKey(columnName))
        {
            columns.Add(columnName, Enumerable.Repeat(false, rowCount).ToList()); 
            columnIndexByName.Add(columnName, columnIndexByName.Count);
        }
    }

    public void SetValue(string columnName, int rowIndex, bool value)
    {
        if (rowIndex >= 0 && rowIndex < rowCount)
        {
            if (columns.ContainsKey(columnName))
            {
                columns[columnName][rowIndex] = value;
            }
            else
            {
                throw new KeyNotFoundException($"Столбец с именем '{columnName}' не существует.");
            }
        }
        else // "Расширение таблицы, если указанная строка отсутствует"
        {
            foreach (var column in columns)
            {
                if (column.Value.Count < rowCount)
                {
                    int deficit = rowCount - column.Value.Count;
                    column.Value.AddRange(Enumerable.Repeat(false, deficit));
                }
            }

            if (columns.ContainsKey(columnName))
            {
                columns[columnName][rowIndex] = value;
            }
            else
            {
                throw new KeyNotFoundException($"Столбец с именем '{columnName}' не существует.");
            }
        }
    }

    public void AddRow(params bool[] values)
    {
        foreach (var column in columns)
        {
            if (values.Length != columns.Count)
            {
                throw new ArgumentException("Число значений не соответствует числу столбцов.");
            }

            column.Value.Add(values[GetColumnIndex(column.Key)]);
        }
    }

    public string GetTableString()
    {
        StringBuilder tableString = new StringBuilder();

        Dictionary<string, int> columnWidths = new Dictionary<string, int>();
        foreach (var column in columns)
        {
            int maxColumnWidth = column.Key.Length;
            foreach (var value in column.Value)
            {
                int valueLength = (value ? "1" : "0").Length;
                if (valueLength > maxColumnWidth)
                    maxColumnWidth = valueLength;
            }
            columnWidths.Add(column.Key, maxColumnWidth);
        }

        List<string> headerValues = new List<string>();
        foreach (var column in columns)
        {
            headerValues.Add(column.Key.PadRight(columnWidths[column.Key]));
        }
        tableString.AppendLine(string.Join("\t", headerValues));

        for (int i = 0; i < columns.Values.First().Count; i++)
        {
            List<string> rowValues = new List<string>();
            foreach (var column in columns)
            {
                string valueString = column.Value[i] ? "1" : "0";
                rowValues.Add(valueString.PadRight(columnWidths[column.Key]));
            }
            tableString.AppendLine(string.Join("\t", rowValues));
        }

        return tableString.ToString();
    }


    public bool GetCellValue(string columnName, int rowIndex)
    {
        if (columns.ContainsKey(columnName))
        {
            if (rowIndex >= 0 && rowIndex < columns[columnName].Count)
            {
                return columns[columnName][rowIndex];
            }
            else
            {
                throw new IndexOutOfRangeException($"Индекс строки '{rowIndex}' выходит за пределы.");
            }
        }
        else
        {
            throw new KeyNotFoundException($"Столбец с именем '{columnName}' не существует.");
        }
    }

    private int GetColumnIndex(string columnName)
    {
        if (columnIndexByName.ContainsKey(columnName))
        {
            return columnIndexByName[columnName];
        }
        throw new KeyNotFoundException($"Столбец с именем '{columnName}' не существует.");
    }

    public int GetRowCount()
    {
        return rowCount;
    }


    public List<string> GetColumnNames()
    {
        return columns.Keys.ToList();
    }
}

public class LogicalExpressionProcessor
{
    private List<char> uniqueOperands;
    private string rpnResult;
    private Table truthTableResult;

    public LogicalExpressionProcessor(string expression)
    {
        this.rpnResult = ToReversePolishNotation(expression);
        if(this.rpnResult!="error")
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
}

class Program
{

    static void Main()
    {
        while (true)
        {
            Console.WriteLine("Введите логическое выражение:");
            string input = Console.ReadLine();
            LogicalExpressionProcessor processor = new LogicalExpressionProcessor(input);
            if (processor.RpnResult != "error")
            {
                Console.WriteLine();
                Console.WriteLine("Таблица истинности:");
                processor.CalculateExpression();
                Console.WriteLine("Результат выражения:");
                Console.WriteLine(processor.TruthTableResult.GetTableString());
                string cnf = processor.GenerateSCNF();
                Console.WriteLine("CКНФ: " + cnf);
                string dnf = processor.GenerateSDNF();
                Console.WriteLine("CДНФ: " + dnf);
                List<bool> functionVector = processor.GetFunctionVector();
                Console.WriteLine("Вектор функции:");
                Console.WriteLine(string.Concat(functionVector.Select(b => b ? "1" : "0")));
            }
            else 
             Console.WriteLine("Неверное логическое выражение");
        }
    }
}
