using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterTests
{
    public class IntegrationTests
    {
        [Fact]
        public void IndefiniteIntegral_SimplePolynomial()
        {
            string expr = "x^2";

            string integral = AST.intergrate(expr);

            double result = InterpreterCore.evalAtX(integral, InterpreterCore.symbolTable, 2);

            Assert.Equal(8.0 / 3.0, result, 5);
        }

        [Fact]
        public void DefiniteIntegral_SimplePolynomial()
        {
            string expr = "x^2";

            double start = 0;
            double end = 2;

            string integral = AST.intergrate(expr);
            double definiteResult = InterpreterCore.evalAtX(integral, InterpreterCore.symbolTable, end)
                                  - InterpreterCore.evalAtX(integral, InterpreterCore.symbolTable, start);

            Assert.Equal(8.0 / 3.0, definiteResult, 5);
        }
    }
}
