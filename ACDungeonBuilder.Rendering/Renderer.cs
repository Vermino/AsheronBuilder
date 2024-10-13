using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using ACDungeonBuilder.Core.Dungeon;

namespace ACDungeonBuilder.Rendering
{
    public class Renderer
    {
        private GameWindow _gameWindow;
        private Shader _shader;
        private DungeonLayout _dungeonLayout;
        private Camera _camera;

        public Renderer(GameWindow gameWindow)
        {
            _gameWindow = gameWindow;
            _camera = new Camera(Vector3.UnitZ * 3, _gameWindow.Size.X / (float)_gameWindow.Size.Y);
        }

        public void Run()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            _shader = new Shader("ACDungeonBuilder.Rendering.Shaders.shader.vert", "ACDungeonBuilder.Rendering.Shaders.shader.frag");
        }

        public void OnRenderFrame()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();

            var model = Matrix4.Identity;
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            RenderDungeon();
        }

        public void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            _camera.AspectRatio = e.Width / (float)e.Height;
        }

        public void SetDungeonLayout(DungeonLayout dungeonLayout)
        {
            _dungeonLayout = dungeonLayout;
        }

        private void RenderDungeon()
        {
            if (_dungeonLayout == null) return;

            foreach (var room in _dungeonLayout.Rooms)
            {
                RenderRoom(room);
            }

            foreach (var corridor in _dungeonLayout.Corridors)
            {
                RenderCorridor(corridor);
            }

            foreach (var door in _dungeonLayout.Doors)
            {
                RenderDoor(door);
            }
        }

        private void RenderRoom(Room room)
        {
            // Implement room rendering logic
            // For now, let's just render a simple cube as a placeholder
            RenderCube(new Vector3(room.X, room.Y, 0), new Vector3(room.Width, room.Height, 1));
        }

        private void RenderCorridor(Corridor corridor)
        {
            // Implement corridor rendering logic
            // For now, let's render a line between start and end points
            RenderLine(new Vector3(corridor.Path[0].X, corridor.Path[0].Y, 0),
                       new Vector3(corridor.Path[^1].X, corridor.Path[^1].Y, 0));
        }

        private void RenderDoor(Door door)
        {
            // Implement door rendering logic
            // For now, let's render a small cube at the door position
            RenderCube(new Vector3(door.X, door.Y, 0), new Vector3(0.5f, 0.5f, 0.5f));
        }

        private void RenderCube(Vector3 position, Vector3 scale)
        {
            // Implement cube rendering logic
        }

        private void RenderLine(Vector3 start, Vector3 end)
        {
            // Implement line rendering logic
        }
    }
}