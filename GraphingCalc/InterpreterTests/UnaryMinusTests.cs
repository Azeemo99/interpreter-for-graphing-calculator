using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterTests
{
    public class UnaryMinusTests
    {
        [Fact]
        public void Negative_Number()
        {
            var input = "-5";
            var result = InterpreterCore.eval(InterpreterCore.symbolTable, input);

            Assert.Equal("-5", result.Item2.Trim());
        }

        [Fact]
        public void Power_Before_UnaryMinus()
        {
            var input = "-2^2";
            var result = InterpreterCore.eval(InterpreterCore.symbolTable, input);

            // -(2^2) = -4
            Assert.Equal("-4", result.Item2.Trim());
        }
    }
}
