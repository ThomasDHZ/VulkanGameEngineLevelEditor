using GameScriptLibraryDLL.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VulkanGameEngineLevelEditor;
using VulkanGameEngineLevelEditor.Component;

namespace VulkanGameEngineLevelEditor.Registries
{
    public static class ComponentViewRegistry
    {
        public static ComponentView? TryCreate(uint gameObjectId, ComponentTypeEnum type, IntPtr componentPtr)
        {
            ComponentView? view = type switch
            {
                ComponentTypeEnum.kInputComponent => new InputComponentView(gameObjectId, componentPtr),
                ComponentTypeEnum.kSpriteComponent => new SpriteComponentView(gameObjectId, componentPtr),
                ComponentTypeEnum.kTransform2DComponent => new Transform2DComponentView(gameObjectId, componentPtr),
               // ComponentTypeEnum.kTransform3DComponent => new Transform3DComponentView(),
                ComponentTypeEnum.kCameraFollowComponent => new CameraFollowComponentView(gameObjectId, componentPtr),
                ComponentTypeEnum.kDirectionalLightComponent => new DirectionalLightComponentView(gameObjectId, componentPtr),
                ComponentTypeEnum.kPointLightComponent => new PointLightComponentView(gameObjectId, componentPtr),
                ComponentTypeEnum.kDebugObjectComponent => new DebugComponentView(gameObjectId, componentPtr),
                ComponentTypeEnum.kCollisionComponent => new CollisionComponentView(gameObjectId, componentPtr),
                _ => null
            };
            return view;
        }
    }
}
