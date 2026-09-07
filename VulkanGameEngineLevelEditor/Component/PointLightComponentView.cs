using GameScriptLibraryDLL.Components;
using GlmSharp;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;
using VulkanEngineCS;
using VulkanGameEngineLevelEditor.Component;

public unsafe class PointLightComponentView : ComponentView
{
    public PointLightComponentView(uint id) : base(id, ComponentTypeEnum.kPointLightComponent) { }

    public PointLightComponentView(uint id, IntPtr componentPtr) : base(id, ComponentTypeEnum.kPointLightComponent, componentPtr) { }

    uint PoolIndex()
    {
        return base.GetComponent<PointLightComponent>().PointLightMemoryPoolIndex;
    }

    public uint PointLightMemoryPoolIndex
    {
        get => PoolIndex();
        set
        {
            var c = base.GetComponent<PointLightComponent>();
            c.PointLightMemoryPoolIndex = value;
            base.SetComponent(c);
        }
    }

    public vec3 LightPosition
    {
        get => ReadLight().LightPosition;
        set => SetPointLightWorldXY(GameObjectId, value.x, value.y);
    }

    public vec3 LightColor
    {
        get => ReadLight().LightColor;
        set { var l = ReadLight(); l.LightColor = value; WriteLight(l); }
    }

    public float LightRadius
    {
        get => ReadLight().LightRadius;
        set { var l = ReadLight(); l.LightRadius = value; WriteLight(l); }
    }

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

    GameScriptLibraryDLL.Components.PointLight ReadLight()
    {
        IntPtr p = LightSystem.GetPointLight(PoolIndex());
        return p == IntPtr.Zero ? default : Marshal.PtrToStructure<GameScriptLibraryDLL.Components.PointLight>(p);
    }

    void WriteLight(GameScriptLibraryDLL.Components.PointLight value)
    {
        IntPtr p = LightSystem.GetPointLight(PoolIndex());
        if (p == IntPtr.Zero) return;
        Marshal.StructureToPtr(value, p, false);
    }
}