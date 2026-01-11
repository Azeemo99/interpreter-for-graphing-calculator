using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterTests
{
    public class PrecedenceTests
    {
        [Fact]
        public void Multiplication_Before_Addition()
        {
            var input = "2+3*4";
            var result = InterpreterCore.eval(InterpreterCore.symbolTable, input);

            Assert.Equal("14", result.Item2.Trim());
        }

        [Fact]
        public void Brackets_Override_Precedence()
        {
            var input = "(2+3)*4";
            var result = InterpreterCore.eval(InterpreterCore.symbolTable, input);

            Assert.Equal("20", result.Item2.Trim());
        }
    }
}
