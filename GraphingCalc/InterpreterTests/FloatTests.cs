using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterTests
{
    public class FloatTests
    {
        [Fact]
        public void Decimal_Addition()
        {
            var input = "0.5 + 0.25";
            var result = InterpreterCore.eval(InterpreterCore.symbolTable, input);

            Assert.Equal("Result = 0.750000", result.Item2.Trim());
        }
    }

    //MAYBE ADD MORE (mult? div?)
}
