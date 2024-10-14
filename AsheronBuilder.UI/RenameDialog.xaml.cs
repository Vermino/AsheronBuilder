using System.Windows;

namespace AsheronBuilder.UI
{
    public partial class RenameDialog : Window
    {
        public string NewName { get; private set; }

        public RenameDialog(string currentName)
        {
            InitializeComponent();
            NameTextBox.Text = currentName;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            NewName = NameTextBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}