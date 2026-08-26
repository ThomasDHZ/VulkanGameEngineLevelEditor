using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulkanGameEngineLevelEditor.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class VisibleWhenAttribute : Attribute
    {
        public string PropertyName { get; }
        public object ExpectedValue { get; }

        public VisibleWhenAttribute(string propertyName, object expectedValue)
        {
            PropertyName = propertyName;
            ExpectedValue = expectedValue;
        }
    }
}
