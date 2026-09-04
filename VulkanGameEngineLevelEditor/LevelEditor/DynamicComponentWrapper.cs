using GameScriptLibraryDLL.Components;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using VulkanEngineCS;
using VulkanGameEngineLevelEditor.Component;

namespace VulkanGameEngineLevelEditor.LevelEditor
{
    public unsafe class DynamicComponentWrapper
    {
        public uint GameObjectId { get; }
        public ComponentTypeEnum ComponentType { get; }
        public ComponentView? View { get; }
        public Type? ComponentStructType { get; }

        public string DisplayName =>
            ComponentType.ToString().Replace("k", "").Replace("Component", "");

        public DynamicComponentWrapper(uint gameObjectId, ComponentTypeEnum componentType, ComponentView? view = null)
        {
            GameObjectId = gameObjectId;
            ComponentType = componentType;
            View = view;
            ComponentStructType = view?.GetType();
        }

        public object? GetMemberValue(MemberInfo member)
        {
            try
            {
                if (View == null) return null;

                var vp = View.GetType().GetProperty(member.Name, BindingFlags.Public | BindingFlags.Instance);
                if (vp == null || !vp.CanRead) return null;
                return vp.GetValue(View);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetMemberValue] {ComponentType}.{member.Name}: {ex}");
                return null;
            }
        }

        public void SetMemberValue(MemberInfo member, object? value)
        {
            if (View == null || value == null) return;

            try
            {
                var vp = View.GetType().GetProperty(member.Name, BindingFlags.Public | BindingFlags.Instance);
                if (vp == null || !vp.CanWrite) return;
                vp.SetValue(View, ConvertValue(value, vp.PropertyType));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SetMemberValue] {ComponentType}.{member.Name}: {ex.Message}");
            }
        }

        static object ConvertValue(object value, Type target)
        {
            if (target.IsInstanceOfType(value)) return value;
            return Convert.ChangeType(value, target);
        }
    }
}