using GameScriptLibraryDLL.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulkanGameEngineLevelEditor.Component
{
    public class DebugComponentView : ComponentView
    {
        public DebugComponentView(uint id) : base(id, ComponentTypeEnum.kDebugObjectComponent)
        {
        }

        public DebugComponentView(uint id, IntPtr componentPtr) : base(id, ComponentTypeEnum.kDebugObjectComponent)
        {
        }
    }
}
