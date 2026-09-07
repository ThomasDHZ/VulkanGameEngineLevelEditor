using GameScriptLibraryDLL.Components;
using GlmSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VulkanEngineCoreCS.Vulkan;
using VulkanEngineCS;

namespace VulkanGameEngineLevelEditor.Component
{
    public class Transform2DComponentView : ComponentView
    {
        public Transform2DComponentView(uint id) : base(id, ComponentTypeEnum.kTransform2DComponent)
        {
        }

        public Transform2DComponentView(uint id, IntPtr componentPtr) : base(id, ComponentTypeEnum.kTransform2DComponent, componentPtr)
        {
        }

        public vec2 Position
        {
            get => GetComponent<Transform2DComponent>().Position;
            set => SetPointLightWorldXY(GameObjectId, value.x, value.y);
        }

        public vec2 Rotation
        {
            get => GetComponent<Transform2DComponent>().Rotation;
            set
            {
                var c = GetComponent<Transform2DComponent>();
                c.Rotation = value;
                SetComponent(c);
            }
        }

        public vec2 Scale
        {
            get => GetComponent<Transform2DComponent>().Scale;
            set
            {
                var c = GetComponent<Transform2DComponent>();
                c.Scale = value;
                SetComponent(c);
            }
        }
    }
}
    
