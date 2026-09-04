using GameScriptLibraryDLL.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulkanGameEngineLevelEditor.Component
{
    public class InputComponentView : ComponentView
    {
        public InputComponentView(uint id) : base(id, ComponentTypeEnum.kInputComponent)
        {
        }

        public InputComponentView(uint id, IntPtr componentPtr) : base(id, ComponentTypeEnum.kInputComponent)
        {
        }
    }
}
