using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace Hill30.BooProject.BooISh
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class Control : UserControl
    {
        public Control()
        {
            InitializeComponent();
        }

        private BooIShWrapper booIShWrapper;
        private void BooIShScreen_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox textBox = BooIShScreen;
            booIShWrapper =
                new BooIShWrapper(
                    s => textBox.Dispatcher.BeginInvoke(
                        new Action(
                            () =>
                                {
                                    textBox.AppendText(s);
                                    caretIndex = textBox.Text.Length - 1;
                                }
                            )));
        }

        private int caretIndex;

        private void BooIShScreen_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                caretIndex = BooIShScreen.CaretIndex+1;
                booIShWrapper.Input.WriteLine(BooIShScreen.Text.Substring(caretIndex));
                BooIShScreen.AppendText("\n");
            }

            if (BooIShScreen.CaretIndex < caretIndex)
                BooIShScreen.CaretIndex = BooIShScreen.Text.Length - 1;
        }

        private void BooIShScreen_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}