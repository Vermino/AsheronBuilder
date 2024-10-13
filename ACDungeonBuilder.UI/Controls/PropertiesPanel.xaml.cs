using System.Windows;
using System.Windows.Controls;
using ACDungeonBuilder.Core.Dungeon;

namespace ACDungeonBuilder.UI.Controls
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

            if (item is Room room)
            {
                AddProperty("Room ID", room.Id.ToString());
                AddProperty("X", room.X.ToString());
                AddProperty("Y", room.Y.ToString());
                AddProperty("Width", room.Width.ToString());
                AddProperty("Height", room.Height.ToString());
                AddProperty("Environment ID", room.EnvironmentId.ToString());
            }
            else if (item is Corridor corridor)
            {
                AddProperty("Corridor ID", corridor.Id.ToString());
                AddProperty("Start Room ID", corridor.StartRoomId.ToString());
                AddProperty("End Room ID", corridor.EndRoomId.ToString());
            }
            else if (item is Door door)
            {
                AddProperty("Door ID", door.Id.ToString());
                AddProperty("Room ID", door.RoomId.ToString());
                AddProperty("X", door.X.ToString());
                AddProperty("Y", door.Y.ToString());
                AddProperty("Type", door.Type.ToString());
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