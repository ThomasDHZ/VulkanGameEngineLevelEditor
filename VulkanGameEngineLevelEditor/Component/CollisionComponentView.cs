using GameScriptLibraryDLL.Components;
using GlmSharp;
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

        public ivec2 Size
        {
            get => GetComponent<Collider2DComponent>().Size;
            set
            {
                var c = GetComponent<Collider2DComponent>();
                c.Size = value;
                SetComponent(c);
            }
        }

        public ivec2 Offset
        {
            get => GetComponent<Collider2DComponent>().Offset;
            set
            {
                var c = GetComponent<Collider2DComponent>();
                c.Offset = value;
                SetComponent(c);
            }
        }

        public bool Enabled
        {
            get => GetComponent<Collider2DComponent>().Enabled;
            set
            {
                var c = GetComponent<Collider2DComponent>();
                c.Enabled = value;
                SetComponent(c);
            }
        }

        public bool IsTrigger
        {
            get => GetComponent<Collider2DComponent>().IsTrigger;
            set
            {
                var c = GetComponent<Collider2DComponent>();
                c.IsTrigger = value;
                SetComponent(c);
            }
        }
    }
}
