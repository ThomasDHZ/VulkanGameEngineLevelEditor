using GameScriptLibraryDLL.Components;
using GlmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using VulkanCS;
using VulkanEngineCoreCS;
using VulkanEngineCS;
using VulkanGameEngineLevelEditor.Component;
using VulkanGameEngineLevelEditor.LevelEditor;
using WeifenLuo.WinFormsUI.Docking;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static VulkanGameEngineLevelEditor.RenderViewForm;

namespace VulkanGameEngineLevelEditor.EditorEnhancements
{
    public class ViewPortWindow : DockContent
    {
        public volatile bool HasPickRequest;
        public volatile int PickX;
        public volatile int PickY;
        public volatile uint SelectedSpriteIndex = uint.MaxValue;
        public float PendingMoveX;
        public float PendingMoveY;
        public float PendingCamX;
        public float PendingCamY;
        public float PendingZoom;
        public readonly object InputLock = new();
        public PictureBox RenderBox { get; private set; }
        public LevelEditorTreeView TreeView { get; set; } = null;
        public PropertiesPanel PropertiesPanel { get; set; } = null;
        private float KeyBoardCameraSpeed = 25.0f;
        private bool IsDragging { get; set; } = false;
        private bool LeftMouseButtonDown { get; set; } = false;
        public Guid ObjectSamplerTexture { get; set; } = new Guid("7047804f-d32e-4cb5-ba95-90783b28d1df");
        private ivec2 RenderResolutionSize = new ivec2(3840, 2160);
        private System.Drawing.Point LastMousePosition { get; set; }
        public volatile int PendingWidth;
        public volatile int PendingHeight;
        public volatile bool SizeDirty;

        public ViewPortWindow()
        {
            InitializeComponent();
        }
        public ViewPortWindow(string title)
        {
            Text = title;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            RenderBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)RenderBox).BeginInit();
            SuspendLayout();
            // 
            // RenderBox
            // 
            RenderBox.Dock = DockStyle.Fill;
            RenderBox.Location = new Point(0, 0);
            RenderBox.Name = "RenderBox";
            RenderBox.Size = new Size(278, 244);
            RenderBox.TabIndex = 0;
            RenderBox.TabStop = false;
            RenderBox.AllowDrop = true;
            RenderBox.ClientSizeChanged += RenderBox_ClientSizeChanged;
            RenderBox.DragDrop += RenderBox_DragDrop;
            RenderBox.DragEnter += RenderBox_DragEnter;
            RenderBox.MouseDown += RendererBox_MouseDown;
            RenderBox.MouseMove += RendererBox_MouseMove;
            RenderBox.MouseUp += RendererBox_MouseUp;
            RenderBox.MouseWheel += RendererBox_MouseWheel;
            RenderBox.KeyDown += RenderBox_KeyDown;
            // 
            // ViewPortWindow
            // 
            ClientSize = new Size(278, 244);
            Controls.Add(RenderBox);
            Name = "ViewPortWindow";
            ((System.ComponentModel.ISupportInitialize)RenderBox).EndInit();
            ResumeLayout(false);

        }

        private void RendererBox_MouseDown(object sender, MouseEventArgs e)
        {
            RenderBox.Focus();
            if (e.Button != MouseButtons.Left)
            {
                IsDragging = true;
                LastMousePosition = e.Location;
                return;
            }

            lock (InputLock)
            {
                PickX = e.X;
                PickY = e.Y;
                HasPickRequest = true;
            }
            IsDragging = true;
            LastMousePosition = e.Location;
        }

        private void RenderBox_ClientSizeChanged(object sender, EventArgs e)
        {
            int width = RenderBox.ClientSize.Width;
            int height = RenderBox.ClientSize.Height;
            if (width <= 0 || height <= 0)
            {
                SizeDirty = true;
                PendingWidth = 0;
                PendingHeight = 0;
                return;
            }

            PendingWidth = width;
            PendingHeight = height;
            SizeDirty = true;
        }

        private void RenderBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DragAssetData)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void RenderBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(DragAssetData)) is not DragAssetData asset) return;

            Point client = RenderBox.PointToClient(new Point(e.X, e.Y));
            vec2 dropPos = ClientToWorld(client);

            if (asset.AssetType == AssetDataTypeEnum.kAssetTypeGameObject)
            {
                uint id = GameObjectSystem.CreateGameObject(asset.JsonPath, dropPos);
                TreeView?.AddGameObject(id);
            }
        }

        private void RendererBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsDragging) return;

            int dx = e.X - LastMousePosition.X;
            int dy = e.Y - LastMousePosition.Y;
            LastMousePosition = e.Location;

            lock (InputLock)
            {
                if (e.Button == MouseButtons.Left)
                {
                    PendingMoveX += dx;
                    PendingMoveY += dy;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    PendingCamX -= dx;
                    PendingCamY += dy;
                }
            }
        }

        private void RendererBox_MouseUp(object sender, MouseEventArgs e)
        {
            IsDragging = false;
        }

        private void RendererBox_MouseWheel(object sender, MouseEventArgs e)
        {
            lock (InputLock)
            {
                PendingZoom += e.Delta / 1200.0f;
            }
        }

        private void RenderBox_KeyDown(object sender, KeyEventArgs e)
        {
            ref var cameraTransform = ref CameraSystem.UpdateActiveCamera();
            if (e.KeyCode == Keys.W) cameraTransform.Position = new vec3(cameraTransform.Position.x, cameraTransform.Position.y - KeyBoardCameraSpeed, 0.0f);
            if (e.KeyCode == Keys.A) cameraTransform.Position = new vec3(cameraTransform.Position.x + KeyBoardCameraSpeed, cameraTransform.Position.y, 0.0f);
            if (e.KeyCode == Keys.D) cameraTransform.Position = new vec3(cameraTransform.Position.x - KeyBoardCameraSpeed, cameraTransform.Position.y, 0.0f);
            if (e.KeyCode == Keys.S) cameraTransform.Position = new vec3(cameraTransform.Position.x, cameraTransform.Position.y + KeyBoardCameraSpeed, 0.0f);
            if ((e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back) && SelectedSpriteIndex != uint.MaxValue)
            {
                GameObjectSystem.DestroyGameObject(SelectedSpriteIndex);
            }
        }

        private vec2 ClientToWorld(Point clientPos)
        {
            int cw = Math.Max(1, RenderBox.ClientSize.Width);
            int ch = Math.Max(1, RenderBox.ClientSize.Height);

            ref var camera = ref CameraSystem.UpdateActiveCamera();
            float zoom = camera.Zoom != 0f ? camera.Zoom : 1f;

            float sx = camera.Width / cw / zoom;
            float sy = camera.Height / ch / zoom;

            float worldX = camera.Position.x + clientPos.X * sx;
            float worldY = camera.Position.y + (ch - clientPos.Y) * sy;

            return new vec2(worldX, worldY);
        }

        private System.Drawing.Point WorldToClient(vec2 worldPos)
        {
            ref Camera camera = ref CameraSystem.UpdateActiveCamera();

            float clientX = (worldPos.x - camera.Position.x) / camera.Zoom;
            float clientY = (worldPos.y - camera.Position.y) / camera.Zoom;

            clientY = RenderBox.ClientSize.Height - clientY;

            return new System.Drawing.Point((int)MathF.Round(clientX), (int)MathF.Round(clientY));
        }

        public unsafe ref T ConvertPtrToObject<T>(IntPtr ptr) where T : unmanaged
        {
            if (ptr == IntPtr.Zero)
            {
                Debug.WriteLine($"Warning: {typeof(T).Name} not found (null pointer)");
                return ref Unsafe.NullRef<T>();
            }
            return ref Unsafe.AsRef<T>(ptr.ToPointer());
        }
    }
}
