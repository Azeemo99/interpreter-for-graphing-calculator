using Microsoft.FSharp.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace InterpreterGUI
{
    public partial class PlotWindow : Window


    {
        private Point? lastMousePos = null;
        private class View
        {
            public double XLeft = -10;
            public double XRight = 10;
            public double YBottom = -10;
            public double YTop = 10;
        }

        private View view = new View();
        private FSharpMap<string, InterpreterCore.v> symTable;
        private const double leftMargin = 40;
        private const double rightMargin = 20;
        private const double topMargin = 20;
        private const double bottomMargin = 40;
        private List<double> currentRoots = new List<double>();

        public PlotWindow(FSharpMap<string, InterpreterCore.v> sharedTable)
        {
            
            InitializeComponent();
            PlotCanvas.MouseWheel += PlotCanvas_MouseWheel;
            PlotCanvas.MouseDown += PlotCanvas_MouseDown;
            PlotCanvas.MouseMove += PlotCanvas_MouseMove;
            PlotCanvas.MouseUp += PlotCanvas_MouseUp;
            symTable = sharedTable;
        }

        //Main  function using Catmull-Rom spline for smooth curve
        public void PlotFunction(string funcExpr)
        {
            double xStart = view.XLeft;
            double xEnd = view.XRight;
            double yMin = view.YBottom;
            double yMax = view.YTop;
            try
            {
                // --- 1. Build x samples ---
                double step = (view.XRight - view.XLeft) / 300;
                List<double> xVals = new List<double>();
                for (double x = xStart; x <= xEnd; x += step)
                    xVals.Add(x);

                var fsharpXVals = Microsoft.FSharp.Collections.ListModule.OfSeq(xVals);

                // --- 2. Call F# plotEval ---
                var result = InterpreterCore.plotEval(symTable, funcExpr, fsharpXVals);

                
                var rawPoints = result
                    .Where(p => !double.IsNaN(p.Item2) && !double.IsInfinity(p.Item2))
                    .Select(p => new Point(p.Item1, p.Item2))
                    .ToList();

                if (rawPoints.Count == 0)
                {
                    MessageBox.Show("No valid points to plot.");
                    return;
                }

                // --- 4. Check for horizontal line ---
                List<Point> smoothPoints;
                if (rawPoints.All(p => p.Y == rawPoints[0].Y))
                {
                    
                    smoothPoints = new List<Point>(rawPoints);
                }
                else if (rawPoints.Count < 4)
                {
                    
                    smoothPoints = new List<Point>(rawPoints);
                }
                else
                {
                    // Apply Catmull-Rom smoothing
                    smoothPoints = new List<Point>();
                    for (int i = 0; i < rawPoints.Count - 3; i++)
                        for (double t = 0; t <= 1.0; t += 0.05)
                            smoothPoints.Add(CatmullRom(rawPoints[i], rawPoints[i + 1],
                                                        rawPoints[i + 2], rawPoints[i + 3], t));
                }

                
                
                if (yMax == yMin)
                {
                    yMin -= 1.0;
                    yMax += 1.0;
                }
                

                // --- 6. Clear canvas ---
                PlotCanvas.Children.Clear();
                double canvasWidth = PlotCanvas.ActualWidth;
                double canvasHeight = PlotCanvas.ActualHeight;

                // Define plot area
                double plotWidth = canvasWidth - leftMargin - rightMargin;
                double plotHeight = canvasHeight - topMargin - bottomMargin;

                
                DrawGridLines(xStart, xEnd, yMin, yMax, canvasWidth, canvasHeight);

                // X-axis 
                double xAxisPos;
                if (view.YBottom > 0)
                    xAxisPos = topMargin + plotHeight;      
                else if (view.YTop < 0)
                    xAxisPos = topMargin;                 
                else
                    xAxisPos = topMargin + (yMax - 0) / (yMax - yMin) * plotHeight;

                Line xAxisLine = new Line
                {
                    X1 = leftMargin,
                    X2 = canvasWidth - rightMargin,
                    Y1 = xAxisPos,
                    Y2 = xAxisPos,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                PlotCanvas.Children.Add(xAxisLine);

                // Y-axis 
                double yAxisPos;
                if (view.XLeft > 0)
                    yAxisPos = leftMargin;                  
                else if (view.XRight < 0)
                    yAxisPos = canvasWidth - rightMargin; 
                else
                    yAxisPos = leftMargin + (0 - xStart) / (xEnd - xStart) * plotWidth;

                Line yAxisLine = new Line
                {
                    X1 = yAxisPos,
                    X2 = yAxisPos,
                    Y1 = topMargin,
                    Y2 = canvasHeight - bottomMargin,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                PlotCanvas.Children.Add(yAxisLine);


                
                DrawAxisTicks(xStart, xEnd, yMin, yMax, canvasWidth, canvasHeight, xAxisPos);

                // Draw curve 
                Polyline polyline = new Polyline
                {
                    Stroke = Brushes.Blue,
                    StrokeThickness = 2
                };

                foreach (var p in smoothPoints)
                {
                    // Only add points 
                    if (p.Y >= view.YBottom && p.Y <= view.YTop)
                    {
                        polyline.Points.Add(WorldToScreen(p));
                    }
                    else
                    {
                        
                        if (polyline.Points.Count > 0)
                        {
                            PlotCanvas.Children.Add(polyline);
                            polyline = new Polyline
                            {
                                Stroke = Brushes.Blue,
                                StrokeThickness = 2
                            };
                        }
                    }
                }

                
                if (polyline.Points.Count > 0)
                    PlotCanvas.Children.Add(polyline);



                DrawAxisTicks(xStart, xEnd, yMin, yMax, canvasWidth, canvasHeight, xAxisPos);
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error plotting function: " + ex.Message);
            }
        }
        private Point WorldToScreen(Point p)
        {
            double canvasWidth = PlotCanvas.ActualWidth;
            double canvasHeight = PlotCanvas.ActualHeight;
            double plotWidth = canvasWidth - leftMargin - rightMargin;
            double plotHeight = canvasHeight - topMargin - bottomMargin;

            // Convert world coordinates to screen coordinates
            double x = leftMargin + (p.X - view.XLeft) / (view.XRight - view.XLeft) * plotWidth;
            double y = topMargin + (view.YTop - p.Y) / (view.YTop - view.YBottom) * plotHeight;

            // Clamp to plot area
            x = Math.Max(leftMargin, Math.Min(canvasWidth - rightMargin, x));
            y = Math.Max(topMargin, Math.Min(canvasHeight - bottomMargin, y));

            return new Point(x, y);
        }





        public void UpdateSymbolTable(FSharpMap<string, InterpreterCore.v> newTable)
        {
            symTable = newTable;
        }



        //Catmull-Rom 
        private Point CatmullRom(Point p0, Point p1, Point p2, Point p3, double t)
        {
            double t2 = t * t;
            double t3 = t2 * t;

            double x = 0.5 * ((2 * p1.X) +
                              (-p0.X + p2.X) * t +
                              (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * t2 +
                              (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * t3);

            double y = 0.5 * ((2 * p1.Y) +
                              (-p0.Y + p2.Y) * t +
                              (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * t2 +
                              (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * t3);

            return new Point(x, y);
        }
        private void PlotCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoom = e.Delta > 0 ? 0.9 : 1.1;
            Point m = e.GetPosition(PlotCanvas);

            double wx = view.XLeft +
                m.X / PlotCanvas.ActualWidth * (view.XRight - view.XLeft);

            double wy = view.YTop -
                m.Y / PlotCanvas.ActualHeight * (view.YTop - view.YBottom);

            view.XLeft = wx + (view.XLeft - wx) * zoom;
            view.XRight = wx + (view.XRight - wx) * zoom;
            view.YBottom = wy + (view.YBottom - wy) * zoom;
            view.YTop = wy + (view.YTop - wy) * zoom;

            PlotFunction(FunctionBox.Text);
            foreach (var root in currentRoots)
                DrawRoot(root);

        }
        //hi
        private void PlotCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                lastMousePos = e.GetPosition(PlotCanvas);

        }

        private void PlotCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (lastMousePos == null || e.LeftButton != MouseButtonState.Pressed)
                return;

            Point cur = e.GetPosition(PlotCanvas);
            Vector d = cur - lastMousePos.Value;

            double dx = d.X / PlotCanvas.ActualWidth *
                        (view.XRight - view.XLeft);
            double dy = d.Y / PlotCanvas.ActualHeight *
                        (view.YTop - view.YBottom);

            view.XLeft -= dx;
            view.XRight -= dx;
            view.YBottom += dy;
            view.YTop += dy;

            lastMousePos = cur;
            PlotFunction(FunctionBox.Text);
            foreach (var root in currentRoots)
                DrawRoot(root);

        }

        private void PlotCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            lastMousePos = null;
        }


       
        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            bool xStartSuccess = double.TryParse(xStartBox.Text, out var xs);
            bool xEndSuccess = double.TryParse(xEndBox.Text, out var xe);

            view.XLeft = xStartSuccess ? xs : -10;
            view.XRight = xEndSuccess ? xe : 10;

            // OPTIONAL: reset Y as well
            view.YBottom = -10;
            view.YTop = 10;

            PlotFunction(FunctionBox.Text);
        }




        // function to draw axes
        private void DrawAxisTicks(double xStart, double xEnd, double yMin, double yMax, double canvasWidth, double canvasHeight, double xAxisPos)
        {
            int numXTicks = 10;     // number of ticks
            int numYTicks = 10;

            double plotWidth = canvasWidth - leftMargin - rightMargin;
            double plotHeight = canvasHeight - topMargin - bottomMargin;

            // Round tick step sizes to "nice" numbers
            double xRange = xEnd - xStart;
            double yRange = yMax - yMin;

            double xStep = Math.Round(xRange / numXTicks);
            double yStep = Math.Round(yRange / numYTicks);

            if (xStep == 0) xStep = 1;
            if (yStep == 0) yStep = 1;

            double xStartAligned = Math.Ceiling(xStart / xStep) * xStep;
            double xEndAligned = Math.Floor(xEnd / xStep) * xStep;
            double yStartAligned = Math.Ceiling(yMin / yStep) * yStep;
            double yEndAligned = Math.Floor(yMax / yStep) * yStep;

            // --- X-axis ticks ---
            for (double xVal = xStartAligned; xVal <= xEndAligned + 0.0001; xVal += xStep)
            {
                double xCanvas = leftMargin + (xVal - xStart) / xRange * plotWidth;
                double yCanvas = xAxisPos;

                // Tick line
                Line tick = new Line
                {
                    X1 = xCanvas,
                    X2 = xCanvas,
                    Y1 = yCanvas - 4,
                    Y2 = yCanvas + 4,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                PlotCanvas.Children.Add(tick);

                // Label
                TextBlock label = new TextBlock
                {
                    Text = xVal.ToString("0"),
                    FontSize = 10
                };
                Canvas.SetLeft(label, xCanvas - 8);
                Canvas.SetTop(label, yCanvas + 6);
                PlotCanvas.Children.Add(label);
            }

            // --- Y-axis ticks ---
            double xZero = leftMargin + (0 - xStart) / xRange * plotWidth; // X position of Y-axis

            for (double yVal = yStartAligned; yVal <= yEndAligned + 0.0001; yVal += yStep)
            {
                double yCanvas = topMargin + (yMax - yVal) / yRange * plotHeight;

                double tickX, labelX;

                if (xZero >= leftMargin && xZero <= canvasWidth - rightMargin)
                {
                    // Y-axis is visible
                    tickX = xZero;
                    labelX = xZero + 6; // label to the right of axis
                }
                else if (xZero < leftMargin)
                {
                    // Axis offscreen to the left
                    tickX = leftMargin;
                    labelX = leftMargin + 5; // small left margin
                }
                else // xZero > canvasWidth - rightMargin
                {
                    // Axis offscreen to the right
                    tickX = canvasWidth - rightMargin;
                    labelX = tickX - 25; // small right margin
                }

                // Tick line
                Line tick = new Line
                {
                    X1 = tickX - 4,
                    X2 = tickX + 4,
                    Y1 = yCanvas,
                    Y2 = yCanvas,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                PlotCanvas.Children.Add(tick);

                // Label
                TextBlock label = new TextBlock
                {
                    Text = yVal.ToString("0"),
                    FontSize = 10
                };
                Canvas.SetLeft(label, labelX);
                Canvas.SetTop(label, yCanvas - 8);
                PlotCanvas.Children.Add(label);
            }
        }

        private void DrawGridLines(double xStart, double xEnd, double yMin, double yMax, double canvasWidth, double canvasHeight)
        {
            int numXTicks = 10;
            int numYTicks = 10;

            double plotWidth = canvasWidth - leftMargin - rightMargin;
            double plotHeight = canvasHeight - topMargin - bottomMargin;

            double xStep = Math.Round((xEnd - xStart) / numXTicks);
            double yStep = Math.Round((yMax - yMin) / numYTicks);

            if (xStep == 0) xStep = 1;
            if (yStep == 0) yStep = 1;

            double xStartAligned = Math.Ceiling(xStart / xStep) * xStep;
            double xEndAligned = Math.Floor(xEnd / xStep) * xStep;
            double yStartAligned = Math.Ceiling(yMin / yStep) * yStep;
            double yEndAligned = Math.Floor(yMax / yStep) * yStep;

            // Draw vertical grid lines
            for (double xVal = xStartAligned; xVal <= xEndAligned + 0.0001; xVal += xStep)
            {
                double xCanvas = leftMargin + (xVal - xStart) / (xEnd - xStart) * plotWidth;

                Line gridLine = new Line
                {
                    X1 = xCanvas,
                    X2 = xCanvas,
                    Y1 = topMargin,
                    Y2 = canvasHeight - bottomMargin,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                PlotCanvas.Children.Add(gridLine);
            }

            // Draw horizontal grid lines
            for (double yVal = yStartAligned; yVal <= yEndAligned + 0.0001; yVal += yStep)
            {
                double yCanvas = topMargin + (yMax - yVal) / (yMax - yMin) * plotHeight;

                Line gridLine = new Line
                {
                    X1 = leftMargin,
                    X2 = canvasWidth - rightMargin,
                    Y1 = yCanvas,
                    Y2 = yCanvas,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                PlotCanvas.Children.Add(gridLine);
            }
        }



        private void DiffButton_click(object sender, RoutedEventArgs e)
        {
            string input = FunctionBox.Text;

            // Call into F#
            
            String result = Aststuff.differentiate(input);
            // Display result
            ResultBox.Text = result;
        }


        private void RootButton_click(object sender, RoutedEventArgs e)
        {
            try
            {
                string funcExpr = FunctionBox.Text;
                double left = view.XLeft;
                double right = view.XRight;

                var rootsArray = Aststuff.findAllRoots(funcExpr, left, right, symTable);

                if (rootsArray.Length == 0)
                {
                    ResultBox.Text = "No roots on screen";
                    return;
                }

                List<double> roots_double = new List<double>(rootsArray);
                ResultBox.Text = string.Join(" ", roots_double);
                drawRoots(roots_double);
            }
            catch (Exception ex)
            {
                ResultBox.Text = $"Error: {ex.Message}";
            }
        }





        private void drawRoots(List<double> xroots)
        {
            currentRoots = new List<double>(xroots);
            foreach (double x in currentRoots)
            {
                DrawRoot(x);
            }
        }

        private void DrawRoot(double x)
        {
            Point screenPoint = WorldToScreen(new Point(x, 0));
            Ellipse dot = new Ellipse
            {
                Width = 5,
                Height = 5,
                Fill = Brushes.Blue,
                Stroke = Brushes.Blue,
            };
            Canvas.SetLeft(dot, screenPoint.X - dot.Width / 2);
            Canvas.SetTop(dot, screenPoint.Y - dot.Height / 2);
            PlotCanvas.Children.Add(dot);
        }



        private void FunctionBox_Copy_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }


    }


}
