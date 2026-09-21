#pragma once
#include "DLL.h"
#include <Platform.h>
#include <JsonStruct.h>
#include <VulkanSystem.h>
#include "TextureBakerSystem.h"
#include <VulkanRenderPass.h>
#include <TextureSystem.h>
#include <VulkanTexture.h>
#include <RenderSystem.h>

class MaterialBakerSystem
{
public:
    static MaterialBakerSystem& Get();

private:
    MaterialBakerSystem() = default;
    ~MaterialBakerSystem() = default;
    MaterialBakerSystem(const MaterialBakerSystem&) = delete;
    MaterialBakerSystem& operator=(const MaterialBakerSystem&) = delete;
    MaterialBakerSystem(MaterialBakerSystem&&) = delete;
    MaterialBakerSystem& operator=(MaterialBakerSystem&&) = delete;

    Vector<Texture>                         TextureList;
    VkGuid                                  AssetBakerRenderPassId;
    Vector<VkGuid>                          RenderPassDrawList;

    void                                    LoadMaterial(const String& materialPath);
    uint                                    LoadTexture(nlohmann::json& json);
    uint32									AddToMaterialMemoryPool(Texture& texture);
    uint                                    TextureExists(nlohmann::json& j, const char* key);
    void                                    CleanRenderPass();
public:

    DLL_EXPORT void BakeMaterial(const String& importMaterialPath, const String& exportMaterialPath);
};
extern DLL_EXPORT MaterialBakerSystem& materialBakerSystem;
inline MaterialBakerSystem& MaterialBakerSystem::Get()
{
    static MaterialBakerSystem instance;
    return instance;
}