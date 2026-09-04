using GameScriptLibraryDLL.Components;
using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulkanGameEngineLevelEditor.Component
{
    public class SpriteComponentView : ComponentView
    {
        public SpriteComponentView(uint id) : base(id, ComponentTypeEnum.kSpriteComponent)
        {
        }

        public SpriteComponentView(uint id, IntPtr componentPtr) : base(id, ComponentTypeEnum.kSpriteComponent)
        {
        }

        public Guid SpriteVramId
        { 
            get => GetComponent<SpriteComponent>().SpriteVramId;
            set
            {
                var c = GetComponent<SpriteComponent>();
                c.SpriteVramId = value;
                SetComponent(c);
            }
        }
    }
}
