using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AsheronBuilder.Core.Dungeon
{
    public class DungeonValidator
    {
        public static List<string> ValidateDungeon(DungeonLayout dungeonLayout)
        {
            var errors = new List<string>();

            // Check for overlapping EnvCells
            errors.AddRange(CheckOverlappingEnvCells(dungeonLayout));

            // Check for disconnected EnvCells
            errors.AddRange(CheckDisconnectedEnvCells(dungeonLayout));

            // Check for invalid scales
            errors.AddRange(CheckInvalidScales(dungeonLayout));

            // Check for EnvCells outside the valid range
            errors.AddRange(CheckEnvCellsOutOfBounds(dungeonLayout));

            // Check for duplicate EnvCell IDs
            errors.AddRange(CheckDuplicateEnvCellIds(dungeonLayout));

            // Check for invalid hierarchy
            errors.AddRange(CheckInvalidHierarchy(dungeonLayout));

            return errors;
        }

        private static IEnumerable<string> CheckOverlappingEnvCells(DungeonLayout dungeonLayout)
        {
            var envCells = dungeonLayout.GetAllEnvCells().ToList();
            for (int i = 0; i < envCells.Count; i++)
            {
                for (int j = i + 1; j < envCells.Count; j++)
                {
                    if (EnvCellsOverlap(envCells[i], envCells[j]))
                    {
                        yield return $"EnvCell {envCells[i].Id} overlaps with EnvCell {envCells[j].Id}";
                    }
                }
            }
        }

        private static bool EnvCellsOverlap(EnvCell cell1, EnvCell cell2)
        {
            // Implement a more sophisticated collision detection algorithm here
            // This is a simple bounding box check and may not be sufficient for complex EnvCell shapes
            Vector3 min1 = cell1.Position - cell1.Scale / 2;
            Vector3 max1 = cell1.Position + cell1.Scale / 2;
            Vector3 min2 = cell2.Position - cell2.Scale / 2;
            Vector3 max2 = cell2.Position + cell2.Scale / 2;

            return (min1.X <= max2.X && max1.X >= min2.X) &&
                   (min1.Y <= max2.Y && max1.Y >= min2.Y) &&
                   (min1.Z <= max2.Z && max1.Z >= min2.Z);
        }

        private static IEnumerable<string> CheckDisconnectedEnvCells(DungeonLayout dungeonLayout)
        {
            var envCells = dungeonLayout.GetAllEnvCells().ToList();
            var connectedCells = new HashSet<uint>();
            var stack = new Stack<EnvCell>();

            if (envCells.Count > 0)
            {
                stack.Push(envCells[0]);
                while (stack.Count > 0)
                {
                    var cell = stack.Pop();
                    connectedCells.Add(cell.Id);

                    foreach (var neighbor in GetNeighbors(cell, envCells))
                    {
                        if (!connectedCells.Contains(neighbor.Id))
                        {
                            stack.Push(neighbor);
                        }
                    }
                }
            }

            if (connectedCells.Count != envCells.Count)
            {
                yield return "Some EnvCells are disconnected from the main dungeon structure";
            }
        }

        private static IEnumerable<EnvCell> GetNeighbors(EnvCell cell, List<EnvCell> allCells)
        {
            // Implement a method to find neighboring EnvCells
            // This is a simple distance-based check and may need to be refined based on your specific requirements
            float neighborThreshold = 1.0f; // Adjust this value as needed
            return allCells.Where(c => c.Id != cell.Id && Vector3.Distance(c.Position, cell.Position) <= neighborThreshold);
        }

        private static IEnumerable<string> CheckInvalidScales(DungeonLayout dungeonLayout)
        {
            foreach (var cell in dungeonLayout.GetAllEnvCells())
            {
                if (cell.Scale.X <= 0 || cell.Scale.Y <= 0 || cell.Scale.Z <= 0)
                {
                    yield return $"EnvCell {cell.Id} has an invalid scale: {cell.Scale}";
                }
            }
        }

        private static IEnumerable<string> CheckEnvCellsOutOfBounds(DungeonLayout dungeonLayout)
        {
            // Define the valid range for EnvCell positions
            Vector3 minBounds = new Vector3(-1000, -1000, -1000);
            Vector3 maxBounds = new Vector3(1000, 1000, 1000);

            foreach (var cell in dungeonLayout.GetAllEnvCells())
            {
                if (cell.Position.X < minBounds.X || cell.Position.X > maxBounds.X ||
                    cell.Position.Y < minBounds.Y || cell.Position.Y > maxBounds.Y ||
                    cell.Position.Z < minBounds.Z || cell.Position.Z > maxBounds.Z)
                {
                    yield return $"EnvCell {cell.Id} is out of the valid position range: {cell.Position}";
                }
            }
        }

        private static IEnumerable<string> CheckDuplicateEnvCellIds(DungeonLayout dungeonLayout)
        {
            var envCells = dungeonLayout.GetAllEnvCells().ToList();
            var idCounts = envCells.GroupBy(c => c.Id).Where(g => g.Count() > 1);

            foreach (var group in idCounts)
            {
                yield return $"Duplicate EnvCell ID found: {group.Key} (Count: {group.Count()})";
            }
        }

        private static IEnumerable<string> CheckInvalidHierarchy(DungeonLayout dungeonLayout)
        {
            var allEnvCells = dungeonLayout.GetAllEnvCells().ToHashSet();
            var hierarchyEnvCells = new HashSet<EnvCell>();

            void TraverseHierarchy(DungeonArea area)
            {
                foreach (var cell in area.EnvCells)
                {
                    hierarchyEnvCells.Add(cell);
                }

                foreach (var childArea in area.ChildAreas)
                {
                    TraverseHierarchy(childArea);
                }
            }

            TraverseHierarchy(dungeonLayout.Hierarchy.RootArea);

            var missingFromHierarchy = allEnvCells.Except(hierarchyEnvCells);
            var extraInHierarchy = hierarchyEnvCells.Except(allEnvCells);

            foreach (var cell in missingFromHierarchy)
            {
                yield return $"EnvCell {cell.Id} is missing from the dungeon hierarchy";
            }

            foreach (var cell in extraInHierarchy)
            {
                yield return $"EnvCell {cell.Id} is in the dungeon hierarchy but not in the main EnvCell collection";
            }
        }
    }
}