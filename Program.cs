using System.Text;

namespace Lab2
{


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
                    Console.WriteLine("CKНФ в числовой форме: " + processor.GenerateSKNF10());
                    string dnf = processor.GenerateSDNF();
                    Console.WriteLine("CДНФ: " + dnf);
                    Console.WriteLine("CДНФ в числовой форме: " + processor.GenerateSDNF10());
                    List<bool> functionVector = processor.GetFunctionVector();
                    Console.WriteLine("Вектор функции:");
                    Console.WriteLine(string.Concat(functionVector.Select(b => b ? "1" : "0")));
                    Console.WriteLine("Вектор функции в десятичной системе :" + processor.GetFunctionVectorDecimalValue());
                }
                else
                    Console.WriteLine("Неверное логическое выражение");
            }
        }
    }

}