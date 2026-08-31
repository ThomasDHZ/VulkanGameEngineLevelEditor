using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeifenLuo.WinFormsUI.Docking;
using static System.Net.Mime.MediaTypeNames;

namespace VulkanGameEngineLevelEditor.EditorEnhancements
{
    public class TreeWindow : DockContent
    {
        public TreeWindow() 
        { 
            Text = "Render Passes"; 
            /* add tree, Dock = Fill */ 
        }
    }
}
