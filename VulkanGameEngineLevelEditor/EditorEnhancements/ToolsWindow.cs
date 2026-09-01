using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using WeifenLuo.WinFormsUI.Docking;
using static System.Net.Mime.MediaTypeNames;

namespace VulkanGameEngineLevelEditor.EditorEnhancements
{
    public class ToolsWindow : DockContent
    {
        public ToolsWindow()
        {
            HideOnClose = true;
            DockAreas = DockAreas.DockBottom
                      | DockAreas.DockLeft
                      | DockAreas.DockRight
                      | DockAreas.DockTop
                      | DockAreas.Float
                      | DockAreas.Document;
        }

        public ToolsWindow(string title, Control content)
        {
            Text = title;
            HideOnClose = true;       
            DockAreas = DockAreas.DockBottom
                      | DockAreas.DockLeft
                      | DockAreas.DockRight
                      | DockAreas.DockTop
                      | DockAreas.Float
                      | DockAreas.Document;
        
            content.Dock = DockStyle.Fill;
            Controls.Add(content);
        }
    }
}
