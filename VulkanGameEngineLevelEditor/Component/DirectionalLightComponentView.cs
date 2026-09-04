using GameScriptLibraryDLL.Components;
using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulkanGameEngineLevelEditor.Component
{
    public class DirectionalLightComponentView : ComponentView
    {
        public DirectionalLightComponentView(uint id) : base(id, ComponentTypeEnum.kDirectionalLightComponent)
        {
        }

        public DirectionalLightComponentView(uint id, IntPtr componentPtr) : base(id, ComponentTypeEnum.kDirectionalLightComponent)
        {
        }

        public vec3 LightColor
        {
            get => GetComponent<DirectionalLightComponent>().LightColor;
            set
            {
                var c = GetComponent<DirectionalLightComponent>();
                c.LightColor = value;
                SetComponent(c);
            }
        }

        public vec3 LightDirection
        {
            get => GetComponent<DirectionalLightComponent>().LightDirection;
            set
            {
                var c = GetComponent<DirectionalLightComponent>();
                c.LightDirection = value;
                SetComponent(c);
            }
        }

        public float LightIntensity
        {
            get => GetComponent<DirectionalLightComponent>().LightIntensity;
            set
            {
                var c = GetComponent<DirectionalLightComponent>();
                c.LightIntensity = value;
                SetComponent(c);
            }
        }

        public float ShadowStrength
        {
            get => GetComponent<DirectionalLightComponent>().ShadowStrength;
            set
            {
                var c = GetComponent<DirectionalLightComponent>();
                c.ShadowStrength = value;
                SetComponent(c);
            }
        }

        public float ShadowBias
        {
            get => GetComponent<DirectionalLightComponent>().ShadowBias;
            set
            {
                var c = GetComponent<DirectionalLightComponent>();
                c.ShadowBias = value;
                SetComponent(c);
            }
        }

        public float ShadowSoftness
        {
            get => GetComponent<DirectionalLightComponent>().ShadowSoftness;
            set
            {
                var c = GetComponent<DirectionalLightComponent>();
                c.ShadowSoftness = value;
                SetComponent(c);
            }
        }
    }
}
