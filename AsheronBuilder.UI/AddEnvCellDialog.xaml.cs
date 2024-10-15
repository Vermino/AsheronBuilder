// File: AsheronBuilder.UI/AddEnvCellDialog.xaml.cs

using System.Windows;
using System.Linq;
using AsheronBuilder.Core.Assets;

namespace AsheronBuilder.UI
{
    public partial class AddEnvCellDialog : Window
    {
        public uint SelectedEnvironmentId { get; private set; }

        public AddEnvCellDialog(AssetManager assetManager)
        {
            InitializeComponent();
            PopulateEnvironmentList(assetManager);
        }

        private void PopulateEnvironmentList(AssetManager assetManager)
        {
            var environmentIds = assetManager.GetAllEnvironmentFileIds();
            EnvironmentListBox.ItemsSource = environmentIds.Select(id => new { Id = id, Name = $"Environment {id}" });
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (EnvironmentListBox.SelectedItem != null)
            {
                SelectedEnvironmentId = (uint)EnvironmentListBox.SelectedItem.GetType().GetProperty("Id").GetValue(EnvironmentListBox.SelectedItem);
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please select an environment.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}