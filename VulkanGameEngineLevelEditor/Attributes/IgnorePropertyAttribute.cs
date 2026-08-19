using System;
namespace VulkanGameEngineLevelEditor.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class IgnorePropertyAttribute : Attribute { }
}