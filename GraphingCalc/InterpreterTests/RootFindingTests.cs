using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterTests
{
    public class RootFindingTests
    {
        [Fact]
        public void Bisection_SingleRoot()
        {
            // f(x) = x - 2 -> root at x = 2
            string expr = "x - 2";
            var symTable = InterpreterCore.symbolTable;
            double root = AST.bisection(expr, 0, 5, symTable);

            Assert.Equal(2.0, root, 5);
        }

        [Fact]
        public void FindAllRoots_MultipleRoots()
        {
            // f(x) = (x - 1)*(x - 3) -> roots at x = 1 and x = 3
            string expr = "(x - 1)*(x - 3)";
            var symTable = InterpreterCore.symbolTable;
            var roots = AST.findAllRoots(expr, 0, 5, symTable);

            Assert.Contains(1.0, roots, new DoubleComparer(1e-5));
            Assert.Contains(3.0, roots, new DoubleComparer(1e-5));
        }

        [Fact]
        public void Bisection_NoRoot_ThrowsException()
        {
            // f(x) = x^2 + 1 has no real roots
            string expr = "x^2 + 1";
            var symTable = InterpreterCore.symbolTable;

            Assert.Throws<AST.RootFindingException>(() => AST.bisection(expr, -5, 5, symTable));
        }
    }

    public class DoubleComparer : IEqualityComparer<double>
    {
        private readonly double _tolerance;
        public DoubleComparer(double tolerance) => _tolerance = tolerance;
        public bool Equals(double a, double b) => Math.Abs(a - b) < _tolerance;
        public int GetHashCode(double obj) => obj.GetHashCode();
    }
}
