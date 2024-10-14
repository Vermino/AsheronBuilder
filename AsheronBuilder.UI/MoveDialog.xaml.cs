// AsheronBuilder.UI/MoveDialog.xaml.cs
using System.Windows;
using AsheronBuilder.Core.Dungeon;
using System.Collections.Generic;
using System.Windows.Controls;

namespace AsheronBuilder.UI
{
    public partial class MoveDialog : Window
    {
        public string DestinationPath { get; private set; }

        public MoveDialog(string currentPath, DungeonHierarchy hierarchy)
        {
            InitializeComponent();
            PopulateTreeView(hierarchy);
            CurrentPathTextBlock.Text = currentPath;
        }

        private void PopulateTreeView(DungeonHierarchy hierarchy)
        {
            var rootItem = new TreeViewItem { Header = "Root" };
            HierarchyTreeView.Items.Add(rootItem);
            AddChildAreas(rootItem, hierarchy.RootArea);
        }

        private void AddChildAreas(TreeViewItem parentItem, DungeonArea area)
        {
            foreach (var childArea in area.ChildAreas)
            {
                var childItem = new TreeViewItem { Header = childArea.Name };
                parentItem.Items.Add(childItem);
                AddChildAreas(childItem, childArea);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = HierarchyTreeView.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                DestinationPath = GetPath(selectedItem);
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please select a destination area.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private string GetPath(TreeViewItem item)
        {
            var path = new List<string>();
            while (item != null)
            {
                path.Insert(0, item.Header.ToString());
                item = item.Parent as TreeViewItem;
            }
            return string.Join("/", path);
        }
    }
}