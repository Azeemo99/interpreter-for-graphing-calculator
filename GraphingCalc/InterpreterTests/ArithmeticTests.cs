using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterTests
{
    public class ArithmeticTests
    {
        [Fact]
        public void Addition_Works()
        {
            var input = "1+2";
            var result = InterpreterCore.eval(InterpreterCore.symbolTable, input);

            Assert.Equal("3", result.Item2.Trim());
        }

        [Fact]
        public void Multiplication_Works()
        {
            var input = "3*5";
            var result = InterpreterCore.eval(InterpreterCore.symbolTable, input);

            Assert.Equal("15", result.Item2.Trim());
        }

        [Fact]
        public void Power_Works()
        {
            var input = "2^3";
            var result = InterpreterCore.eval(InterpreterCore.symbolTable, input);

            Assert.Equal("8", result.Item2.Trim());
        }
    }
}
