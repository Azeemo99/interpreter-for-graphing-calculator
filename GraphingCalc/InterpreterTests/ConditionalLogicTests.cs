namespace InterpreterTests
{
    public class ConditionalLogicTests
    {
        [Fact]
        public void Test_ConditionalLogic()
        {
            string input = "if (1) then 100 else 200";

            var result = InterpreterCore.eval(InterpreterCore.symbolTable, input);

            Assert.Contains("100", result.Item2);
        }
    }
}