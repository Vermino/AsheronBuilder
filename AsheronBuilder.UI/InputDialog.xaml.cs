using System.Windows;

namespace AsheronBuilder.UI
{
    public partial class InputDialog : Window
    {
        public string InputText { get; private set; }

        public InputDialog(string title, string prompt)
        {
            InitializeComponent();
            Title = title;
            PromptTextBlock.Text = prompt;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            InputText = InputTextBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}