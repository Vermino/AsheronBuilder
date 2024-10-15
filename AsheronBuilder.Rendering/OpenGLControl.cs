// File: AsheronBuilder.Rendering/OpenGLControl.cs

using System;
using System.Windows;
using System.Windows.Media;
using OpenTK.Graphics.OpenGL;
using OpenTK.Wpf;
using OpenTK.Windowing.Common;
using AsheronBuilder.Core.Dungeon;
using AsheronBuilder.Core;
using OpenTK.Mathematics;


namespace AsheronBuilder.Rendering
{
    public class OpenGLControl : GLWpfControl
    {
        
        private SelectionMode _selectionMode = SelectionMode.None;
        private bool _wireframeMode = false;
        private bool _showCollision = false;
        private Vector3 _cameraPosition = new Vector3(0, 0, 10);
        private Vector3 _cameraTarget = Vector3.Zero;
        private Vector3 _cameraUp = Vector3.UnitY;
        private DungeonLayout _currentDungeon;
        private bool _showGrid = false;
        
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(OpenGLControl),
                new PropertyMetadata(Colors.Black, OnBackgroundColorChanged));

        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        private static void OnBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((OpenGLControl)d).InvalidateVisual();
        }

        private bool _isInitialized = false;

        public OpenGLControl()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 3,
                MinorVersion = 3,
                GraphicsProfile = ContextProfile.Core,
                GraphicsContextFlags = ContextFlags.ForwardCompatible,
            };
            Start(settings);

            Render += OnRender;
        }

        private void InitializeOpenGL()
        {
            // Set up any initial OpenGL state here
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }
        
        public void ResetCamera()
        {
            _cameraPosition = new Vector3(0, 0, 10);
            _cameraTarget = Vector3.Zero;
            _cameraUp = Vector3.UnitY;
            UpdateViewMatrix();
        }

        public void SetTopView()
        {
            _cameraPosition = new Vector3(0, 10, 0);
            _cameraTarget = Vector3.Zero;
            _cameraUp = Vector3.UnitZ;
            UpdateViewMatrix();
        }

        public void SetSelectionMode(SelectionMode mode)
        {
            _selectionMode = mode;
        }

        public void ApplyObjectChanges(string objectName, string objectType, string material)
        {
            // Implement applying changes to the selected object in the scene
            // This will depend on how you're storing and managing objects in your scene
        }

        public void SetSelectedObjectPosition(float x, float y, float z)
        {
            // Implement setting the position of the selected object
            // This will depend on how you're storing and managing objects in your scene
        }

        public void SetSelectedObjectScale(float scale)
        {
            // Implement setting the scale of the selected object
            // This will depend on how you're storing and managing objects in your scene
        }

        public void GoToLandblock(uint landblockId)
        {
            // Implement navigation to the specified landblock
            // This will involve loading the landblock data and updating the camera position
        }

        public void SetWireframeMode(bool enabled)
        {
            _wireframeMode = enabled;
        }

        public void SetCollisionVisibility(bool visible)
        {
            _showCollision = visible;
        }

        private void UpdateViewMatrix()
        {
            // Update the view matrix based on camera position, target, and up vector
            // This will be used in your shader to transform the scene
        }

        private void RenderGrid()
        {
            // Implement grid rendering
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(0.5f, 0.5f, 0.5f);

            for (int i = -10; i <= 10; i++)
            {
                GL.Vertex3(i, 0, -10);
                GL.Vertex3(i, 0, 10);
                GL.Vertex3(-10, 0, i);
                GL.Vertex3(10, 0, i);
            }

            GL.End();
        }

        private void RenderDungeon(DungeonLayout dungeon)
        {
            // Implement dungeon rendering
            // This will involve iterating through the dungeon structure and rendering each element
            foreach (var area in dungeon.Hierarchy.RootArea.GetAllAreas())
            {
                RenderArea(area);
            }
        }

        private void RenderArea(DungeonArea area)
        {
            // Implement area rendering
            foreach (var envCell in area.EnvCells)
            {
                RenderEnvCell(envCell);
            }

            foreach (var childArea in area.ChildAreas)
            {
                RenderArea(childArea);
            }
        }

        private void RenderEnvCell(EnvCell envCell)
        {
            // Implement EnvCell rendering
            // This will involve rendering the geometry, textures, and other properties of the EnvCell
            if (_wireframeMode)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            else
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }

            // Render EnvCell geometry here
            // You'll need to implement this based on how your EnvCell data is structured

            if (_showCollision)
            {
                RenderCollision(envCell);
            }
        }

        private void RenderCollision(EnvCell envCell)
        {
            // Implement collision rendering for the EnvCell
            // This might involve rendering bounding boxes, collision meshes, etc.
        }

        private void HandleSelection()
        {
            switch (_selectionMode)
            {
                case SelectionMode.Object:
                    // Implement object selection logic
                    break;
                case SelectionMode.Vertex:
                    // Implement vertex selection logic
                    break;
                case SelectionMode.Face:
                    // Implement face selection logic
                    break;
            }
        }

        protected void OnRender(TimeSpan e)
        {
            if (!_isInitialized)
            {
                InitializeOpenGL();
                _isInitialized = true;
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Set up view and projection matrices
            UpdateViewMatrix();
            SetupProjectionMatrix();

            if (_currentDungeon != null)
            {
                RenderDungeon(_currentDungeon);
            }

            if (_showGrid)
            {
                RenderGrid();
            }

            HandleSelection();
        }

        private void SetupProjectionMatrix()
        {
            float aspectRatio = (float)ActualWidth / (float)ActualHeight;
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45.0f),
                aspectRatio,
                0.1f,
                1000.0f);

            // Set the projection matrix in your shader
            // You'll need to implement this based on your shader setup
        }
    }
}