using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VulkanEngineCoreCS;
using WeifenLuo.WinFormsUI.Docking;
using static System.Net.Mime.MediaTypeNames;

namespace VulkanGameEngineLevelEditor.EditorEnhancements
{
    public class ViewPortWindow : DockContent
    {
        public PictureBox RenderBox { get; } = new PictureBox { Dock = DockStyle.Fill };

        public ViewPortWindow()
        {
            Text = "Viewport";
            Controls.Add(RenderBox);
            RenderBox.ClientSizeChanged += (_, _) => { VulkanSystem.SetCustomFrameBufferSize(new ivec2(this.Width, this.Height)); };
        }
    }
}
