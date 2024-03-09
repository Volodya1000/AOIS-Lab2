using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2
{
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
}
