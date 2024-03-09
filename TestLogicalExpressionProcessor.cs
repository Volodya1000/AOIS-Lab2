using Lab2;
    
namespace TestProject1
{
    public class LogicalExpressionProcessorTests
    {
        [Fact]
        public void RpnResult_CorrectExpression_GeneratedSuccessfully()
        {
            var processor = new LogicalExpressionProcessor("a&b|c");

            var result = processor.RpnResult;

            Assert.Equal("ab&c|", result);
        }

        [Fact]
        public void GenerateTruthTable_CorrectExpression_GeneratedSuccessfully()
        {
            // Arrange
            var processor = new LogicalExpressionProcessor("a&b|c");

            // Act
            var table = processor.TruthTableResult;

            // Assert
            Assert.Equal(8, table.GetRowCount());
            Assert.Contains("a", table.GetColumnNames());
            Assert.Contains("b", table.GetColumnNames());
            Assert.Contains("c", table.GetColumnNames());
        }

        [Fact]
        public void GenerateSCNF_CorrectExpression_GeneratedSuccessfully()
        {

            var processor = new LogicalExpressionProcessor("a&b|c");

            var result = processor.GenerateSCNF();

            Assert.Equal("(a|b|c)&(a|!b|c)&(!a|b|c)&(!a|!b|c)", result);
        }

        [Fact]
        public void GenerateSCNF_CorrectExpression_GeneratedSuccessfully1()
        {
            var processor = new LogicalExpressionProcessor("a~b>c");

            var result = processor.GenerateSCNF();

            Assert.Equal("(a|b|c)&(a|!b|c)&(!a|b|c)&(!a|!b|c)", result);
        }

        [Fact]
        public void GenerateSDNF_CorrectExpression_GeneratedSuccessfully()
        {
            var processor = new LogicalExpressionProcessor("a&b|c");
            var result = processor.GenerateSDNF();
            Assert.Equal("(!a&!b&c)|(!a&b&c)|(a&!b&c)|(a&b&c)", result);
        }

        [Fact]
        public void GetFunctionVector_CorrectExpression_GeneratedSuccessfully()
        {
            var processor = new LogicalExpressionProcessor("a&b|c");
            processor.CalculateExpression();
            var vector = processor.GetFunctionVector();
            Assert.Collection(vector,
                item1 => Assert.False(item1),
                item2 => Assert.True(item2),
                item3 => Assert.False(item3),
                item4 => Assert.True(item4),
                item5 => Assert.False(item5),
                item6 => Assert.True(item6),
                item7 => Assert.True(item7),
                item8 => Assert.True(item8)
            );
        }

    }




    public class TableTests
    {
        [Fact]
        public void AddColumn_WhenColumnDoesNotExist_ShouldAddColumn()
        {
            Table table = new Table(3);

            table.AddColumn("Column1");

            Assert.Contains("Column1", table.GetColumnNames());
        }

        [Fact]
        public void SetValue_WhenRowIndexOutOfRange_ShouldExtendTableAndSetValue()
        {
            Table table = new Table(2);
            table.AddColumn("Column1");

            table.SetValue("Column1", 1, true);

            Assert.True(table.GetCellValue("Column1", 1));
        }

        [Fact]
        public void AddRow_WhenValuesLengthDoesNotMatchColumnsCount_ShouldThrowArgumentException()
        {
            Table table = new Table(2);
            table.AddColumn("Column1");

            Assert.Throws<ArgumentException>(() => table.AddRow(true, false));
        }



        [Fact]
        public void GetRowCount_WhenTableHasColumns_ShouldReturnRowCount()
        {
            Table table = new Table(3);

            int rowCount = table.GetRowCount();

            Assert.Equal(3, rowCount);
        }

    }
}