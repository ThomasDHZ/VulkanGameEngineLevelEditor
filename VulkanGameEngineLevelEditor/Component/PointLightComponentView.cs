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
    public unsafe class PointLightComponentView : ComponentView
    {
        private uint pointLightMemoryPoolIndex = UInt32.MaxValue;
        public PointLightComponentView(uint id ) : base(id, ComponentTypeEnum.kPointLightComponent)
        {

        }

        public PointLightComponentView(uint id, IntPtr componentPtr) : base(id, ComponentTypeEnum.kPointLightComponent) 
        {

        }

        public vec3 LightPosition
        {
            get => GetComponent<PointLightComponent>().LightPosition;
            set
            {
                var c = GetComponent<PointLightComponent>();
                c.LightPosition = value;
                SetComponent(c);
            }
        }

        public vec3 LightColor
        {
            get => GetComponent<PointLightComponent>().LightColor;
            set
            {
                var c = GetComponent<PointLightComponent>();
                c.LightColor = value;
                SetComponent(c);
            }
        }

        public float LightRadius
        {
            get => GetComponent<PointLightComponent>().LightRadius;
            set
            {
                var c = GetComponent<PointLightComponent>();
                c.LightRadius = value;
                SetComponent(c);
            }
        }

        public float LightIntensity
        {
            get => GetComponent<PointLightComponent>().LightIntensity;
            set
            {
                var c = GetComponent<PointLightComponent>();
                c.LightIntensity = value;
                SetComponent(c);
            }
        }

        public float ShadowStrength
        {
            get => GetComponent<PointLightComponent>().ShadowStrength;
            set
            {
                var c = GetComponent<PointLightComponent>();
                c.ShadowStrength = value;
                SetComponent(c);
            }
        }

        public float ShadowBias
        {
            get => GetComponent<PointLightComponent>().ShadowBias;
            set
            {
                var c = GetComponent<PointLightComponent>();
                c.ShadowBias = value;
                SetComponent(c);
            }
        }

        public float ShadowSoftness
        {
            get => GetComponent<PointLightComponent>().ShadowSoftness;
            set
            {
                var c = GetComponent<PointLightComponent>();
                c.ShadowSoftness = value;
                SetComponent(c);
            }
        }

        protected override T GetComponent<T>()
        {
            if(pointLightMemoryPoolIndex == uint.MaxValue) pointLightMemoryPoolIndex = LightSystem.FindPointLightIndex(Ptr().ToPointer());
            IntPtr p = LightSystem.GetPointLight(pointLightMemoryPoolIndex);
            return p == IntPtr.Zero ? default : Marshal.PtrToStructure<T>(p);
        }

        protected override void SetComponent<T>(T value)
        {
            if (pointLightMemoryPoolIndex == uint.MaxValue) pointLightMemoryPoolIndex = LightSystem.FindPointLightIndex(Ptr().ToPointer());
            IntPtr p = LightSystem.GetPointLight(pointLightMemoryPoolIndex);
            if (p == IntPtr.Zero) return;
            Marshal.StructureToPtr(value, p, false);
        }
    }
}
