#pragma once
#include "DLL.h"
#include <Platform.h>
#include <JsonStruct.h>
#include <VulkanSystem.h>
#include "TextureBakerSystem.h"
#include <VulkanRenderPass.h>
#include <TextureSystem.h>
#include <VulkanTexture.h>

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
    VkGuid                                  AssetBakerId;

    void                                    LoadMaterial(const String& materialPath);
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

#ifdef __cplusplus
extern "C" {
#endif
    DLL_EXPORT void MaterialBakerSystem_BakeMaterial(const char* importMaterialPath, const char* exportMaterialPath);
#ifdef __cplusplus
}
#endif