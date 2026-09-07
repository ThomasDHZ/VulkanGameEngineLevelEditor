using GameScriptLibraryDLL.Components;
using GlmSharp;
using System.Runtime.InteropServices;
using VulkanEngineCS;
using VulkanGameEngineLevelEditor.Attributes;
using VulkanGameEngineLevelEditor.Component;

public unsafe class DirectionalLightComponentView : ComponentView
{
    public DirectionalLightComponentView(uint id) : base(id, ComponentTypeEnum.kDirectionalLightComponent) { }

    public DirectionalLightComponentView(uint id, IntPtr componentPtr) : base(id, ComponentTypeEnum.kDirectionalLightComponent, componentPtr) { }

    uint PoolIndex()
    {
        return base.GetComponent<DirectionalLightComponent>().DirectionalLightMemoryPoolIndex;
    }

    public uint DirectionalLightMemoryPoolIndex
    {
        get => PoolIndex();
        set
        {
            var c = base.GetComponent<DirectionalLightComponent>();
            c.DirectionalLightMemoryPoolIndex = value;
            base.SetComponent(c);
        }
    }

    [NumericUpDownLimitsAttribute(0.01f, 0.0f, 1.0f)]
    public vec3 LightColor
    {
        get => ReadLight().LightColor;
        set { var l = ReadLight(); l.LightColor = value; WriteLight(l); }
    }

    public vec3 LightDirection
    {
        get => ReadLight().LightDirection;
        set { var l = ReadLight(); l.LightDirection = value; WriteLight(l); }
    }

    [NumericUpDownLimitsAttribute(0.01f, 0.0f, 50.0f)]
    public float LightIntensity
    {
        get => ReadLight().LightIntensity;
        set { var l = ReadLight(); l.LightIntensity = value; WriteLight(l); }
    }

    public float ShadowStrength
    {
        get => ReadLight().ShadowStrength;
        set { var l = ReadLight(); l.ShadowStrength = value; WriteLight(l); }
    }

    public float ShadowBias
    {
        get => ReadLight().ShadowBias;
        set { var l = ReadLight(); l.ShadowBias = value; WriteLight(l); }
    }

    public float ShadowSoftness
    {
        get => ReadLight().ShadowSoftness;
        set { var l = ReadLight(); l.ShadowSoftness = value; WriteLight(l); }
    }

    GameScriptLibraryDLL.Components.DirectionalLight ReadLight()
    {
        IntPtr p = LightSystem.GetDirectionalLight(PoolIndex());
        return p == IntPtr.Zero ? default : Marshal.PtrToStructure<GameScriptLibraryDLL.Components.DirectionalLight>(p);
    }

    void WriteLight(GameScriptLibraryDLL.Components.DirectionalLight value)
    {
        IntPtr p = LightSystem.GetDirectionalLight(PoolIndex());
        if (p == IntPtr.Zero) return;
        Marshal.StructureToPtr(value, p, false);
        LightSystem.GetDirectionalLight(PoolIndex());
    }
}