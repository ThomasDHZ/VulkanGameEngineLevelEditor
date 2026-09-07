using GameScriptLibraryDLL.Components;
using GlmSharp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VulkanEngineCS;

namespace VulkanGameEngineLevelEditor.Component
{
    public class ComponentView
    {
        private IntPtr _componentPtr;
        public uint GameObjectId { get; private set; } = uint.MaxValue;
        public ComponentTypeEnum ComponentType { get; private set; }

        public ComponentView(uint id, ComponentTypeEnum type) : this(id, type, GameObjectSystem.GetGameObjectComponentPtr(id, type))
        {
        }

        public ComponentView(uint id, ComponentTypeEnum type, IntPtr componentPtr)
        {
            GameObjectId = id;
            ComponentType = type;
            _componentPtr = componentPtr;
        }

        public void Rebind(IntPtr componentPtr) => _componentPtr = componentPtr;

        protected IntPtr Ptr()
        {
            if (_componentPtr == IntPtr.Zero) _componentPtr = GameObjectSystem.GetGameObjectComponentPtr(GameObjectId, ComponentType);
            return _componentPtr;
        }

        protected virtual T GetComponent<T>() where T : unmanaged
        {
            IntPtr p = Ptr();
            return p == IntPtr.Zero ? default : Marshal.PtrToStructure<T>(p);
        }

        protected virtual void SetComponent<T>(T value) where T : unmanaged
        {
            IntPtr p = Ptr();
            if (p == IntPtr.Zero) return;
            Marshal.StructureToPtr(value, p, false);
        }

        protected void SetField<TField>(Action<IntPtr, TField> write, TField value)
        {
            IntPtr p = Ptr();
            if (p == IntPtr.Zero) return;
            write(p, value);
        }

        public static T GetComponent<T>(uint gameObjectId, IntPtr ptr, ComponentTypeEnum ComponentType) where T : unmanaged
        {
            IntPtr p = GameObjectSystem.GetGameObjectComponentPtr(gameObjectId, ComponentType);
            return p == IntPtr.Zero ? default : Marshal.PtrToStructure<T>(p);
        }

        public static void SetPointLightWorldXY(uint gameObjectId, float x, float y, float? z = null)
        {
            IntPtr transformPtr = GameObjectSystem.GetGameObjectComponentPtr(gameObjectId, ComponentTypeEnum.kTransform2DComponent);
            if (transformPtr != IntPtr.Zero)
            {
                var transform = Marshal.PtrToStructure<Transform2DComponent>(transformPtr);
                transform.Position = new vec2(x, y);
                Marshal.StructureToPtr(transform, transformPtr, false);
            }

            IntPtr indexPtr = GameObjectSystem.GetGameObjectComponentPtr(gameObjectId, ComponentTypeEnum.kPointLightComponent);
            if (indexPtr == IntPtr.Zero) return;

            var indexComp = Marshal.PtrToStructure<PointLightComponent>(indexPtr);
            IntPtr lightPtr = LightSystem.GetPointLight(indexComp.PointLightMemoryPoolIndex);
            if (lightPtr == IntPtr.Zero) return;

            var light = Marshal.PtrToStructure<GameScriptLibraryDLL.Components.PointLight>(lightPtr);
            light.LightPosition = new vec3(x, y, z ?? light.LightPosition.z);
            Marshal.StructureToPtr(light, lightPtr, false);
            LightSystem.GetPointLight(indexComp.PointLightMemoryPoolIndex);
        }
    }
}
