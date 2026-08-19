using GameScriptLibraryDLL.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VulkanGameEngineLevelEditor;

namespace VulkanGameEngineLevelEditor.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GameObjectComponentAttribute : Attribute
    {
        public ComponentTypeEnum ComponentType;
        public GameObjectComponentAttribute(ComponentTypeEnum componentType)
        {
            ComponentType = componentType;
        }
    }
}
