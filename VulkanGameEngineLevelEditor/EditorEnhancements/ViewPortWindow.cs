using GameScriptLibraryDLL.Components;
using GlmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VulkanCS;
using VulkanEngineCoreCS;
using VulkanEngineCS;
using WeifenLuo.WinFormsUI.Docking;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static VulkanGameEngineLevelEditor.RenderViewForm;

namespace VulkanGameEngineLevelEditor.EditorEnhancements
{
    public class ViewPortWindow : DockContent
    {
        public PictureBox RenderBox;
        private Label label1;

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
            //ivec2 worldPos = ClientToWorld(clientPos);
            //ivec2 dropPos = new ivec2(worldPos.x / 2, worldPos.y / 2);
            //if (asset.AssetType == AssetDataTypeEnum.kAssetTypeGameObject)
            //{
            //    uint newGoId = GameObjectSystem.CreateGameObject(asset.JsonPath, dropPos);
            //    treeView1.AddGameObject(newGoId);
            //}
            //else if (asset.AssetType == AssetDataTypeEnum.kAssetTypeLight)
            //{
            //    uint newLightId = LightSystem.LoadLight(asset.JsonPath);
            //    treeView1.AddDirectionalLightObject(newLightId);
            //}
        }

        private void RendererBox_MouseMove(object sender, MouseEventArgs e)
        {
     
            //if (SelectedSpriteIndex == uint.MaxValue) return;

            //Point currentPos = e.Location;

            //if (e.Button == MouseButtons.Left)
            //{
            //    int deltaX = currentPos.X - LastMousePosition.X;
            //    int deltaY = currentPos.Y - LastMousePosition.Y;


            //    List<ComponentTypeEnum> gameObjectComponents = GameObjectSystem.GetGameObjectComponentList(SelectedSpriteIndex);
            //    if (gameObjectComponents.Contains(ComponentTypeEnum.kPointLightComponent))
            //    {
            //        ref var pointLightComponent = ref GameObjectSystem.UpdateGameObjectComponent<PointLightComponent>(SelectedSpriteIndex, ComponentTypeEnum.kPointLightComponent);
            //        ref var pointLight = ref ConvertPtrToObject<PointLight>(LightSystem.GetPointLight(pointLightComponent.PointLightId));
            //        pointLight.LightPosition = new vec3(pointLight.LightPosition.x + deltaX, pointLight.LightPosition.y - deltaY, pointLight.LightPosition.z);
            //    }
            //    if (gameObjectComponents.Contains(ComponentTypeEnum.kTransform2DComponent))
            //    {
            //        ref var transform = ref GameObjectSystem.UpdateGameObjectComponent<Transform2DComponent>(SelectedSpriteIndex, ComponentTypeEnum.kTransform2DComponent);
            //        transform.GameObjectPosition = new vec2(transform.GameObjectPosition.x + deltaX, transform.GameObjectPosition.y - deltaY);
            //    }
            //    if (gameObjectComponents.Contains(ComponentTypeEnum.kTransform3DComponent))
            //    {
            //        ref var transform = ref GameObjectSystem.UpdateGameObjectComponent<Transform3DComponent>(SelectedSpriteIndex, ComponentTypeEnum.kTransform3DComponent);
            //        transform.GameObjectPosition = new vec3(transform.GameObjectPosition.x + deltaX, transform.GameObjectPosition.y - deltaY, 0.0f);
            //    }
            //    // if (Math.Abs(deltaX) > 1 || Math.Abs(deltaY) > 1)
            //}
            //else if (e.Button == MouseButtons.Right)
            //{
            //    int deltaX = currentPos.X - LastMousePosition.X;
            //    int deltaY = currentPos.Y - LastMousePosition.Y;

            //    ref var cameraTransform = ref CameraSystem.UpdateActiveCamera();
            //    cameraTransform.Position = new vec3(cameraTransform.Position.x - deltaX, cameraTransform.Position.y + deltaY, 0.0f);
            //}

            //LastMousePosition = currentPos;
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
            //System.Drawing.Point mousePos = e.Location;
            //float scrollDelta = e.Delta / 1200.0f;
            //ref var cameraTransform = ref CameraSystem.UpdateActiveCamera();
            //cameraTransform.Zoom += scrollDelta;
        }

        private void InitializeComponent()
        {
            RenderBox = new PictureBox();
            label1 = new Label();
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
            RenderBox.MouseHover += RenderBox_MouseHover;
            RenderBox.MouseMove += RendererBox_MouseMove;
            RenderBox.MouseUp += RendererBox_MouseUp;
            RenderBox.MouseWheel += RendererBox_MouseWheel;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = SystemColors.ControlDarkDark;
            label1.ForeColor = Color.White;
            label1.Location = new Point(44, 195);
            label1.Name = "label1";
            label1.Size = new Size(59, 25);
            label1.TabIndex = 1;
            label1.Text = "label1";
            // 
            // ViewPortWindow
            // 
            ClientSize = new Size(278, 244);
            Controls.Add(label1);
            Controls.Add(RenderBox);
            Name = "ViewPortWindow";
            ((System.ComponentModel.ISupportInitialize)RenderBox).EndInit();
            ResumeLayout(false);
            PerformLayout();

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

            uint raw = RenderSystem.SampleRenderPassPixel(ObjectSamplerTexture, new ivec2(x,y));
            BeginInvoke(() => label1.Text = $"texel={x},{y} id={raw}");
           // MessageLogger.LogMessage($@"SwapChain Size: {swapChainSize.x}, {swapChainSize.y} \n id:{texSize.x},{texSize.y} \n Box: {RenderBox.Width},{RenderBox.Height} \n PresentTexture Before Blit: {presentTexture.x},{presentTexture.y}", VkDebugUtilsMessageSeverityFlagBitsEXT.VK_DEBUG_UTILS_MESSAGE_SEVERITY_INFO_BIT_EXT);
        }

        private void RenderBox_MouseHover(object sender, EventArgs e)
        {
  
        }

        //private bool ClientToTexel(Point client, ivec2 texSize, out ivec2 texel)
        //{
         
        //    if (img == null || texSize.x <= 0 || texSize.y <= 0) return false;

        //    int cw = Math.Max(1, RenderBox.ClientSize.Width);
        //    int ch = Math.Max(1, RenderBox.ClientSize.Height);

        //    // Change this to match how you actually draw the Vulkan image.
        //    // StretchImage:
        //    int dx = 0, dy = 0, dw = cw, dh = ch;

        //    // Zoom (letterbox) — use this if you preserve aspect:
        //    // float scale = Math.Min(cw / (float)texSize.x, ch / (float)texSize.y);
        //    // dw = Math.Max(1, (int)(texSize.x * scale));
        //    // dh = Math.Max(1, (int)(texSize.y * scale));
        //    // dx = (cw - dw) / 2;
        //    // dy = (ch - dh) / 2;

        //    int lx = client.X - dx;
        //    int ly = client.Y - dy;
        //    if ((uint)lx >= (uint)dw || (uint)ly >= (uint)dh) return false;

        //    int x = (int)((long)lx * texSize.x / dw);
        //    int y = (int)((long)ly * texSize.y / dh);

        //    // Uncomment if the blit flips Y (D3D-style / some host copies):
        //    // y = texSize.y - 1 - y;

        //    texel.x = Math.Clamp(x, 0, texSize.x - 1);
        //    texel.y = Math.Clamp(y, 0, texSize.y - 1);
        //    return true;
        //}
    }
}
