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
using VulkanGameEngineLevelEditor.LevelEditor;
using WeifenLuo.WinFormsUI.Docking;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static VulkanGameEngineLevelEditor.RenderViewForm;

namespace VulkanGameEngineLevelEditor.EditorEnhancements
{
    public class ViewPortWindow : DockContent
    {
        public PictureBox RenderBox { get; private set; }
        public LevelEditorTreeView TreeView { get; set; } = null;
        public PropertiesPanel PropertiesPanel { get; set; } = null;
        private float KeyBoardCameraSpeed = 25.0f;
        private bool IsDragging { get; set; } = false;
        private uint SelectedSpriteIndex { get; set; } = uint.MaxValue;
        private bool LeftMouseButtonDown { get; set; } = false;
        private Guid ObjectSamplerTexture { get; set; } = new Guid("7047804f-d32e-4cb5-ba95-90783b28d1df");
        private ivec2 RenderResolutionSize = new ivec2(3840, 2160);
        private System.Drawing.Point LastMousePosition { get; set; }

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

            ivec2 texSize = RenderSystem.GetAttachmentSize(ObjectSamplerTexture);
            int x = (int)((long)e.X * texSize.x / Math.Max(1, RenderBox.ClientSize.Width));
            int y = (int)((long)e.Y * texSize.y / Math.Max(1, RenderBox.ClientSize.Height));

            uint pickedId = RenderSystem.SampleRenderPassPixel(ObjectSamplerTexture, new ivec2(x, y));
            if (pickedId != uint.MaxValue)
            {
                SelectedSpriteIndex = pickedId;
                PropertiesPanel.SetSelectedEntity(pickedId);
                TreeView.SelectGameObject(pickedId);

                IsDragging = true;
                LastMousePosition = e.Location;
            }
        }

        private void RenderBox_ClientSizeChanged(object sender, EventArgs e)
        {
            if (RenderBox.Width <= 0 || RenderBox.Height <= 0) return;
            VulkanSystem.SetCustomFrameBufferSize(new ivec2(RenderBox.Width, RenderBox.Height));
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

            Point clientPos = RenderBox.PointToClient(new Point(e.X, e.Y));
            ivec2 worldPos = ClientToWorld(clientPos);
            ivec2 dropPos = new ivec2(worldPos.x / 2, worldPos.y / 2);
            if (asset.AssetType == AssetDataTypeEnum.kAssetTypeGameObject)
            {
                int a = 234;
              //  uint newGoId = GameObjectSystem.CreateGameObject(asset.JsonPath, dropPos);
               // _treeView.AddGameObject(newGoId);
            }
            //else if (asset.AssetType == AssetDataTypeEnum.kAssetTypeLight)
            //{
            //    uint newLightId = LightSystem.LoadLight(asset.JsonPath);
            //    _treeView.AddDirectionalLightObject(newLightId);
            //}
        }

        private void RendererBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (SelectedSpriteIndex == uint.MaxValue) return;

            Point currentPos = e.Location;

            if (e.Button == MouseButtons.Left)
            {
                int deltaX = currentPos.X - LastMousePosition.X;
                int deltaY = currentPos.Y - LastMousePosition.Y;

                //ivec2 texSize = RenderSystem.GetAttachmentSize(ObjectSamplerTexture);
                //int cw = Math.Max(1, RenderBox.ClientSize.Width);
                //int ch = Math.Max(1, RenderBox.ClientSize.Height);

                //float worldDx = deltaX * (texSize.x / (float)cw);
                //float worldDy = deltaY * (texSize.y / (float)ch);

                //ref var camera = ref CameraSystem.UpdateActiveCamera();
                //float zoom = camera.Zoom != 0 ? camera.Zoom : 1.0f;

                //worldDx /= zoom;
                //worldDy /= zoom;

                int cw = Math.Max(1, RenderBox.ClientSize.Width);
                int ch = Math.Max(1, RenderBox.ClientSize.Height);

                ref var camera = ref CameraSystem.UpdateActiveCamera();

                // width/height of what the 2D camera shows, in the same units as transform.Position
                float worldW = camera.Width;   // or RenderPassResolution.x if position is in render pixels
                float worldH = camera.Height;  // or RenderPassResolution.y
                float zoom = camera.Zoom != 0 ? camera.Zoom : 1f;

                float worldDx = deltaX * (worldW / cw) / zoom;
                float worldDy = deltaY * (worldH / ch) / zoom;

                List<ComponentTypeEnum> gameObjectComponents = GameObjectSystem.GetGameObjectComponentList(SelectedSpriteIndex);
                //if (gameObjectComponents.Contains(ComponentTypeEnum.kPointLightComponent))
                //{
                //    ref var pointLightComponent = ref GameObjectSystem.UpdateGameObjectComponent<PointLightComponent>(SelectedSpriteIndex, ComponentTypeEnum.kPointLightComponent);
                //    ref var pointLight = ref ConvertPtrToObject<PointLight>(LightSystem.GetPointLight(pointLightComponent.PointLightId));
                //    pointLight.LightPosition = new vec3(pointLight.LightPosition.x + deltaX, pointLight.LightPosition.y - deltaY, pointLight.LightPosition.z);
                //}
                if (gameObjectComponents.Contains(ComponentTypeEnum.kTransform2DComponent))
                {
                    ref var transform = ref GameObjectSystem.UpdateGameObjectComponent<Transform2DComponent>(SelectedSpriteIndex, ComponentTypeEnum.kTransform2DComponent);
                    transform.Position = new vec2(transform.Position.x + worldDx, transform.Position.y - worldDy);
                }
                //if (Math.Abs(deltaX) > 1 || Math.Abs(deltaY) > 1)
            }
            else if (e.Button == MouseButtons.Right)
            {
                int deltaX = currentPos.X - LastMousePosition.X;
                int deltaY = currentPos.Y - LastMousePosition.Y;

                ref var cameraTransform = ref CameraSystem.UpdateActiveCamera();
                cameraTransform.Position = new vec3(cameraTransform.Position.x - deltaX, cameraTransform.Position.y + deltaY, 0.0f);
            }

            LastMousePosition = currentPos;
        }

        private void RendererBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                IsDragging = false;
                SelectedSpriteIndex = 0;
            }
            else if (e.Button == MouseButtons.Right)
            {
                IsDragging = false;
            }
        }

        private void RendererBox_MouseWheel(object sender, MouseEventArgs e)
        {
            System.Drawing.Point mousePos = e.Location;
            float scrollDelta = e.Delta / 1200.0f;
            ref var cameraTransform = ref CameraSystem.UpdateActiveCamera();
            cameraTransform.Zoom += scrollDelta;
        }

        private void RenderBox_KeyDown(object sender, KeyEventArgs e)
        {
            ref var cameraTransform = ref CameraSystem.UpdateActiveCamera();
            if (e.KeyCode == Keys.W) cameraTransform.Position = new vec3(cameraTransform.Position.x, cameraTransform.Position.y + KeyBoardCameraSpeed, 0.0f);
            if (e.KeyCode == Keys.A) cameraTransform.Position = new vec3(cameraTransform.Position.x - KeyBoardCameraSpeed, cameraTransform.Position.y, 0.0f);
            if (e.KeyCode == Keys.D) cameraTransform.Position = new vec3(cameraTransform.Position.x + KeyBoardCameraSpeed, cameraTransform.Position.y, 0.0f);
            if (e.KeyCode == Keys.S) cameraTransform.Position = new vec3(cameraTransform.Position.x, cameraTransform.Position.y - KeyBoardCameraSpeed, 0.0f);
            if ((e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back) && SelectedSpriteIndex != uint.MaxValue)
            {
                GameObjectSystem.DestroyGameObject(SelectedSpriteIndex);
            }
        }

        private ivec2 ClientToWorld(System.Drawing.Point clientPos)
        {
            ref Camera camera = ref CameraSystem.UpdateActiveCamera();

            float camX = camera.Position.x;
            float camY = camera.Position.y;

            ivec2 texSize = RenderSystem.GetAttachmentSize(ObjectSamplerTexture);
            float scaleX = texSize.x / (float)RenderBox.ClientSize.Width;
            float scaleY = texSize.y / (float)RenderBox.ClientSize.Height;

            float renderX = clientPos.X * scaleX;
            float renderY = (RenderBox.ClientSize.Height - clientPos.Y) * scaleY;

            float worldX = camX + renderX;
            float worldY = camY + renderY;

            return new ivec2((int)Math.Round(worldX), (int)Math.Round(worldY));
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
