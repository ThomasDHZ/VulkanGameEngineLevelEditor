using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeifenLuo.WinFormsUI.Docking;
using static System.Net.Mime.MediaTypeNames;

namespace VulkanGameEngineLevelEditor.EditorEnhancements
{
    public class BottomToolsWindow : DockContent
    {
        public BottomToolsWindow() 
        { 
            Text = "Tools"; 
            /* tabControl Dock = Fill */ 
        }
    }
}
