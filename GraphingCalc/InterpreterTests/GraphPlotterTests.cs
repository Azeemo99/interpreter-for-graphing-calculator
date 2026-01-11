using Microsoft.FSharp.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static InterpreterCore;

namespace InterpreterTests
{
    public class GraphPlotterTests
    {
        [Fact]
        public void QuadraticFunction_ReturnsCorrectValues()
        {
            var funcExpr = "x^2 + 2*x + 1";
            var xVals = new List<double> { -1, 0, 1, 2 };
            var fsharpXVals = ListModule.OfSeq(xVals);

            var results = InterpreterCore.plotEval(InterpreterCore.symbolTable, funcExpr, fsharpXVals);

            var yVals = results.Select(p => p.Item2).ToList();

            var expected = new List<double> { 0, 1, 4, 9 };
            Assert.Equal(expected, yVals);
        }

        [Fact]
        public void CubicFunction_ReturnsCorrectValues()
        {
            var funcExpr = "x^3 - x";
            var xVals = new List<double> { -2, -1, 0, 1, 2 };
            var fsharpXVals = ListModule.OfSeq(xVals);

            var results = InterpreterCore.plotEval(InterpreterCore.symbolTable, funcExpr, fsharpXVals);
            var yVals = results.Select(p => p.Item2).ToList();

            var expected = new List<double> { -6, 0, 0, 0, 6 };
            Assert.Equal(expected, yVals);
        }

        [Fact]
        public void TrigFunction_ReturnsCorrectValues()
        {
            var funcExpr = "sin(x)";
            var xVals = new List<double> { 0, System.Math.PI / 2, System.Math.PI };
            var fsharpXVals = ListModule.OfSeq(xVals);

            var results = InterpreterCore.plotEval(InterpreterCore.symbolTable, funcExpr, fsharpXVals);
            var yVals = results.Select(p => p.Item2).ToList();

            var expected = new List<double> { 0, 1, 0 };
            Assert.Equal(expected, yVals.Select(y => System.Math.Round(y, 6)).ToList());
        }
    }
}
