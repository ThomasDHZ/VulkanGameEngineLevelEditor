using GameScriptLibraryDLL.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulkanGameEngineLevelEditor.Component
{
    public class CameraFollowComponentView : ComponentView
    {
        public CameraFollowComponentView(uint id) : base(id, ComponentTypeEnum.kCameraFollowComponent)
        {
        }

        public CameraFollowComponentView(uint id, IntPtr componentPtr) : base(id, ComponentTypeEnum.kCameraFollowComponent)
        {
        }
    }
}
