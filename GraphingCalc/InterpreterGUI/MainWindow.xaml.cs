using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.FSharp.Collections;

namespace InterpreterGUI
{
    public partial class MainWindow : Window


    {

        private FSharpMap<string, InterpreterCore.v> symTable = MapModule.Empty<string, InterpreterCore.v>();
        
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Evaluate_Click(object sender, RoutedEventArgs e)
        {
            string input = InputBox.Text;
            // Call the F# evaluator; Item1 is Success (bool), Item2 is Result/Error message
            var s = InterpreterCore.eval(symTable, input);

            if (s.Item1) // Success Case
            {
                // 1. Update UI Content
                ResultBox.Text = s.Item2;
                symTable = s.Item3;

                // 2. Toggle Visibility: Show Result, Hide Error
                ResultBox.Visibility = Visibility.Visible;
                ErrorBox.Visibility = Visibility.Collapsed;

                // 3. Sync with Plot Window
                foreach (Window w in this.OwnedWindows)
                {
                    if (w is PlotWindow pw)
                    {
                        pw.UpdateSymbolTable(symTable);
                    }
                }
            }
            else // Error Case
            {
                // 1. Update UI Content
                ErrorBox.Text = s.Item2;

                // 2. Toggle Visibility: Show Error, Hide Result
                ErrorBox.Visibility = Visibility.Visible;
                ResultBox.Visibility = Visibility.Collapsed;
            }
        }
        private void InputBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (InputBox.Text == "Enter expression")
            {
                InputBox.Text = "";
            }
        }
        private void btnOpenPlot_Click(object sender, RoutedEventArgs e)
        {
            PlotWindow plotWindow = new PlotWindow(symTable);
            plotWindow.Owner = this;

            plotWindow.Show();
        }


        private void ResultBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            String s = "y = 1x + c";

        }

    }
}