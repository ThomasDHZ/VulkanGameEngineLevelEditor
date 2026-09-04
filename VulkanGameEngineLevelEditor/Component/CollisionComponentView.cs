using GameScriptLibraryDLL.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulkanGameEngineLevelEditor.Component
{
    public class CollisionComponentView : ComponentView
    {
        public CollisionComponentView(uint id) : base(id, ComponentTypeEnum.kCollisionComponent)
        {
        }

        public CollisionComponentView(uint id, IntPtr componentPtr) : base(id, ComponentTypeEnum.kCollisionComponent)
        {
        }
    }
}
