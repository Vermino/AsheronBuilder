// AsheronBuilder.Tests/CommandTests.cs

using AsheronBuilder.Core.Commands;
using AsheronBuilder.Core.Dungeon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace AsheronBuilder.Tests
{
    [TestClass]
    public class CommandTests
    {
        [TestMethod]
        public void AddRoomCommand_ExecuteAndUndo()
        {
            var dungeonLayout = new DungeonLayout();
            var parentArea = dungeonLayout.Hierarchy.RootArea;
            var newRoom = new DungeonArea("TestRoom");
            newRoom.SetPosition(new Vector3(10, 20, 0));
            newRoom.SetScale(new Vector3(100, 100, 1));

            var command = new AddRoomCommand(dungeonLayout, newRoom, parentArea);

            // Execute
            command.Execute();
            Assert.IsTrue(parentArea.ChildAreas.Contains(newRoom));
            Assert.AreEqual(1, parentArea.ChildAreas.Count);

            // Undo
            command.Undo();
            Assert.IsFalse(parentArea.ChildAreas.Contains(newRoom));
            Assert.AreEqual(0, parentArea.ChildAreas.Count);
        }

        [TestMethod]
        public void AddCorridorCommand_ExecuteAndUndo()
        {
            var dungeonLayout = new DungeonLayout();
            var parentArea = dungeonLayout.Hierarchy.RootArea;
            var corridorPoints = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(10, 10, 0)
            };

            var command = new AddCorridorCommand(dungeonLayout, corridorPoints, parentArea);

            // Execute
            command.Execute();
            Assert.AreEqual(1, parentArea.EnvCells.Count);

            // Undo
            command.Undo();
            Assert.AreEqual(0, parentArea.EnvCells.Count);
        }

        [TestMethod]
        public void MoveAreaCommand_ExecuteAndUndo()
        {
            var area = new DungeonArea("TestArea");
            area.SetPosition(new Vector3(0, 0, 0));

            var newPosition = new Vector3(10, 20, 30);
            var command = new MoveAreaCommand(area, newPosition);

            // Execute
            command.Execute();
            Assert.AreEqual(newPosition, area.Position);

            // Undo
            command.Undo();
            Assert.AreEqual(new Vector3(0, 0, 0), area.Position);
        }
    }
}