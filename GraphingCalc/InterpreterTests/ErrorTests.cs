using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterTests
{
    public class ErrorTests
    {
        [Fact]
        public void Division_By_Zero_Should_Report_Error()
        {
            var result = InterpreterCore.eval(InterpreterCore.symbolTable, "10/0");

            Assert.Contains("error", result.Item2.ToLower());
        }
    }
}
