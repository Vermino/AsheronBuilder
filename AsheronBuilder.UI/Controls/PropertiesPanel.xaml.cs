// AsheronBuilder.UI/Controls/PropertiesPanel.xaml.cs
using System.Windows;
using System.Windows.Controls;
using AsheronBuilder.Core.Dungeon;

namespace AsheronBuilder.UI.Controls
{
    public partial class PropertiesPanel : UserControl
    {
        public PropertiesPanel()
        {
            InitializeComponent();
        }

        public void DisplayProperties(object item)
        {
            PropertiesStack.Children.Clear();
            PropertiesStack.Children.Add(new TextBlock { Text = "Properties", FontWeight = FontWeights.Bold, Margin = new System.Windows.Thickness(0, 0, 0, 10) });

            if (item == null)
            {
                PropertiesStack.Children.Add(new TextBlock { Text = "No item selected" });
                return;
            }

            if (item is EnvCell envCell)
            {
                AddProperty("EnvCell ID", envCell.Id.ToString());
                AddProperty("Environment ID", envCell.EnvironmentId.ToString());
                AddProperty("Position X", envCell.Position.X.ToString());
                AddProperty("Position Y", envCell.Position.Y.ToString());
                AddProperty("Position Z", envCell.Position.Z.ToString());
                AddProperty("Rotation X", envCell.Rotation.X.ToString());
                AddProperty("Rotation Y", envCell.Rotation.Y.ToString());
                AddProperty("Rotation Z", envCell.Rotation.Z.ToString());
                AddProperty("Rotation W", envCell.Rotation.W.ToString());
                AddProperty("Scale X", envCell.Scale.X.ToString());
                AddProperty("Scale Y", envCell.Scale.Y.ToString());
                AddProperty("Scale Z", envCell.Scale.Z.ToString());
            }
        }

        private void AddProperty(string name, string value)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new System.Windows.Thickness(0, 0, 0, 5) };
            panel.Children.Add(new TextBlock { Text = name + ": ", FontWeight = FontWeights.Bold, Width = 100 });
            panel.Children.Add(new TextBox { Text = value, Width = 100 });
            PropertiesStack.Children.Add(panel);
        }
    }
}