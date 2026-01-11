using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterTests
{
    public class DifferentiationTests
    {
        [Fact]
        public void Diff_Constant_ReturnsZero()
        {
            string expr = "5";
            string derivative = AST.differentiate(expr);

            var symTable = InterpreterCore.symbolTable;
            double result = InterpreterCore.evalAtX(derivative, symTable, 10);

            Assert.Equal(0.0, result, 5);
        }

        [Fact]
        public void Diff_Variable_ReturnsOne()
        {
            string expr = "x";
            string derivative = AST.differentiate(expr);

            var symTable = InterpreterCore.symbolTable;
            double result = InterpreterCore.evalAtX(derivative, symTable, 123.0);

            Assert.Equal(1.0, result, 5);
        }

        [Fact]
        public void Diff_Polynomial_SimplePower()
        {
            string expr = "x^3"; // derivative should be 3*x^2
            string derivative = AST.differentiate(expr);

            var symTable = InterpreterCore.symbolTable;
            double resultAt2 = InterpreterCore.evalAtX(derivative, symTable, 2); // 3*2^2 = 12
            double resultAt3 = InterpreterCore.evalAtX(derivative, symTable, 3); // 3*3^2 = 27

            Assert.Equal(12.0, resultAt2, 5);
            Assert.Equal(27.0, resultAt3, 5);
        }
    }
}
