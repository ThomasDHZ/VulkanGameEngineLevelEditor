using GameScriptLibraryDLL.Components;
using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VulkanEngineCS;

namespace VulkanGameEngineLevelEditor.Component
{
    public unsafe class DirectionalLightComponentView : ComponentView
    {
        private uint directionalLightMemoryPoolIndex = UInt32.MaxValue;
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

        protected override T GetComponent<T>()
        {
            if (directionalLightMemoryPoolIndex == uint.MaxValue) directionalLightMemoryPoolIndex = LightSystem.FindDirectionalLightIndex(Ptr().ToPointer());
            LightSystem.GetDirectionalLight(directionalLightMemoryPoolIndex);
            IntPtr p = Ptr(); 
            return p == IntPtr.Zero ? default : Marshal.PtrToStructure<T>(p);
        }

        protected override void SetComponent<T>(T value)
        {
            if (directionalLightMemoryPoolIndex == uint.MaxValue) directionalLightMemoryPoolIndex = LightSystem.FindDirectionalLightIndex(Ptr().ToPointer());
            LightSystem.GetDirectionalLight(directionalLightMemoryPoolIndex);
            IntPtr p = Ptr();
            if (p == IntPtr.Zero) return;
            Marshal.StructureToPtr(value, p, false);
        }
    }
}
