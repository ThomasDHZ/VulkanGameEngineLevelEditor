#include "MaterialBakerSystem.h"
#include <EngineConfigSystem.h>
#include <TextureSystem.h>
#include <ShaderSystem.h>
#include <FileSystem.h>
#include "MaterialMemoryPoolSystem.h"
#include <from_json.h>
#include <regex>
#include "TextureSamplers.h"
#include <RenderSystem.h>
#include <EngineConfigSystem.h>

MaterialBakerSystem& materialBakerSystem = MaterialBakerSystem::Get();

void MaterialBakerSystem::BakeMaterial(const String& importMaterialJson)
{
    materialMemoryPoolSystem.StartUp();
    nlohmann::json importJson = fileSystem.LoadJsonFile(configSystem.BakerImportMaterialPath + importMaterialJson);

    String textureName = importMaterialJson;
    if (const size_t pos = textureName.find(".json"); pos != String::npos) textureName.erase(pos, 5);
    if (const size_t pos = textureName.find("Import"); pos != String::npos) textureName.erase(pos, 6);

    ivec2 materialSetResolution = ivec2(importJson["TextureSetResolution"][0], importJson["TextureSetResolution"][1]);
    RenderPassLoader renderPassLoader = fileSystem.LoadJsonFile("RenderPass/AssetCreatorRenderPass.json").get<RenderPassLoader>();
    renderPassLoader.RenderPassResolution = materialSetResolution;
    AssetBakerRenderPassId = renderSystem.LoadRenderPass(renderPassLoader, materialMemoryPoolSystem.GetMemoryPoolInfo());

    VulkanRenderPass renderPass = renderSystem.FindRenderPass(AssetBakerRenderPassId);
    Vector<PushConstantUpdateRule> pushConstantRules = renderPass.SubPassList().front().front().PushConstantUpdates;
    ImportMaterial material = LoadMaterial(importJson);

    nlohmann::json exportMaterial;
    exportMaterial["MaterialId"] = VkGuid::Generate().ToString();
    for (int x = 0; x < 2; x++)
    {
        for (int y = 0; y < pushConstantRules.size(); y++)
        {
            if (pushConstantRules[y].Variable == "MaterialBakerSubPassIndex") pushConstantRules[y].Value[0] = std::to_string(x);
        }

        textureSystem.GenerateTexture(AssetBakerRenderPassId, &pushConstantRules);
        vkQueueWaitIdle(vulkan.GraphicsQueue());
        nlohmann::json bakedSlots = textureBakerSystem.BakeTexture(importMaterialJson, AssetBakerRenderPassId, x);
        exportMaterial.update(bakedSlots);
    }
    vkQueueWaitIdle(vulkan.GraphicsQueue());
    CleanRenderPass();
    materialMemoryPoolSystem.BakerResetMemoryPool();

    nlohmann::json root;
    exportMaterial["ShadingModel"]   = material.ShadingModel;
    exportMaterial["FeatureMask"]    = material.FeatureMask;
    exportMaterial["ClearcoatTint"]  = material.ClearcoatTint;
    exportMaterial["IOR"]            = material.IOR;
    exportMaterial["AlphaCutOff"]    = material.AlphaCutOff;
    std::ofstream(std::filesystem::current_path().string() + "/../../VulkanGameEngine/Assets/" + configSystem.BakerExportMaterialPath + textureName + ".json") << exportMaterial.dump(2);

    std::cout << "Material Baking Finished" << std::endl;
}

void MaterialBakerSystem::CleanRenderPass()
{
    vkQueueWaitIdle(vulkan.GraphicsQueue());
    
    VulkanRenderPass renderPass = renderSystem.FindRenderPass(AssetBakerRenderPassId);
    for (auto& renderPipelineIdList : renderPass.PipelineList())
    {
        VulkanPipeline pipeline = renderSystem.FindRenderPipeline(renderPipelineIdList);
        for (auto dsl : pipeline.DescriptorSetLayoutList())
        {
            vkDestroyDescriptorSetLayout(vulkan.LogicalDevice(), dsl, nullptr);
        }
        pipeline.DescriptorSetLayoutList().clear();
        pipeline.Destroy();
    }
    renderPass.Destroy();

    textureSystem.Destroy();
    TextureList.clear();
}

ImportMaterial MaterialBakerSystem::LoadMaterial(nlohmann::json& materialJson)
{
    uint materialId = materialMemoryPoolSystem.AllocateObject(BakerMaterialBuffer);
    ImportMaterial& material = materialMemoryPoolSystem.UpdateMaterial(materialId);

    auto v3 = [](const nlohmann::json& a, float x, float y, float z) 
        {
            if (a.is_array() && a.size() >= 3) return std::array<float, 3>{ a[0].get<float>(), a[1].get<float>(), a[2].get<float>() };
            return std::array<float, 3>{ x, y, z };
        };

    auto Albedo = v3(materialJson["Albedo"], 1, 1, 1);
    auto ClearcoatTint = v3(materialJson["ClearcoatTint"], 1, 1, 1);
    auto SheenColor = v3(materialJson["SheenColor"], 1, 1, 1);
    auto SSSColor = v3(materialJson["SubSurfaceScatteringColor"], 1, 0.45f, 0.35f);
    auto AttenuationColor = v3(materialJson["AttenuationColor"], 1, 1, 1);
    auto Emission = v3(materialJson["Emission"], 0, 0, 0);

    material.Metallic = materialJson.value("Metallic", 0.0f);
    material.Roughness = materialJson.value("Roughness", 0.5f);
    material.AmbientOcclusion = materialJson.value("AmbientOcclusion", 1.0f);
    material.IOR = materialJson.value("IOR", 1.45f);
    material.NormalStrength = materialJson.value("NormalStrength", 1.0f);
    material.Height = materialJson.value("HeightScale", 0.0f);
    material.CoatWeight = materialJson.value("ClearcoatWeight", 0.0f);
    material.CoatRoughness = materialJson.value("ClearcoatRoughness", 0.08f);
    material.CoatDarkening = materialJson.value("CoatDarkening", 1.0f);
    material.SheenWeight = materialJson.value("SheenWeight", 0.0f);
    material.SheenRoughness = materialJson.value("SheenRoughness", 0.5f);
    material.SSSWeight = materialJson.value("SSSWeight", 0.0f);
    material.SSSProfile = materialJson.value("SSSProfile", 0.0f);
    material.Thickness = materialJson.value("Thickness", 0.5f);
    material.TransmissionWeight = materialJson.value("TransmissionWeight", 0.0f);
    material.AttenuationDistance = materialJson.value("AttenuationDistance", 1.0f);
    material.Anisotropy = materialJson.value("Anisotropy", 0.0f);
    material.AnisotropyRotation = materialJson.value("AnisotropyRotation", 0.0f);
    material.ThinFilmWeight = materialJson.value("ThinFilmWeight", 0.0f);
    material.ThinFilmThickness = materialJson.value("ThinFilmThickness", 0.5f);
    material.EmissionIntensity = materialJson.value("EmissionIntensity", 0.0f);
    material.AlphaCutOff = materialJson.value("AlphaCutoff", 0.1f);
    material.AlbedoMap = TextureExists(materialJson, "AlbedoMap");
    material.NormalMap = TextureExists(materialJson, "NormalMap");
    material.HeightMap = TextureExists(materialJson, "HeightMap");
    material.AlphaMap = TextureExists(materialJson, "AlphaMap");
    material.MetallicMap = TextureExists(materialJson, "MetallicMap");
    material.RoughnessMap = TextureExists(materialJson, "RoughnessMap");
    material.AmbientOcclusionMap = TextureExists(materialJson, "AmbientOcclusionMap");
    material.EmissionMap = TextureExists(materialJson, "EmissionMap");
    material.ClearCoatColorMap = TextureExists(materialJson, "ClearCoatColorMap");
    material.ClearCoatPropertiesMap = TextureExists(materialJson, "ClearCoatPropertiesMap");
    material.SheenMap = TextureExists(materialJson, "SheenMap");
    material.SheenPropertiesMap = TextureExists(materialJson, "SheenPropertiesMap");
    material.SSSColorMap = TextureExists(materialJson, "SubSurfaceScatteringColorMap");
    material.SSSPropertiesMap = TextureExists(materialJson, "SubSurfaceScatteringPropertiesMap");
    material.AttenuationColorMap = TextureExists(materialJson, "AttenuationColorMap");
    material.AttenuationPropertiesMap = TextureExists(materialJson, "AttenuationPropertiesTexture");
    material.AnisotropyPropertiesMap = TextureExists(materialJson, "AnisotropyPropertiesMap");
    material.IORMap = TextureExists(materialJson, "IORTexture");
    material.ShadingModel = materialJson.value("ShadingModel", 0u);
    material.FeatureMask = materialJson.value("FeatureMask", 0u);
    memcpy(material.Albedo, Albedo.data(), 12);
    memcpy(material.ClearcoatTint, ClearcoatTint.data(), 12);
    memcpy(material.SheenColor, SheenColor.data(), 12);
    memcpy(material.SSSColor, SSSColor.data(), 12);
    memcpy(material.AttenuationColor, AttenuationColor.data(), 12);
    memcpy(material.Emission, Emission.data(), 12);

    materialMemoryPoolSystem.IsHeaderDirty = true;
    materialMemoryPoolSystem.IsDescriptorSetDirty = true;
    return material;
}

uint MaterialBakerSystem::TextureExists(nlohmann::json& j, const char* key)
{
    if (!j.contains(key) || j[key].is_null()) return 0xFFFFFFFFu;
    return LoadTexture(j[key]);
}

uint MaterialBakerSystem::LoadTexture(nlohmann::json& json)
{
    TextureLoader loader = json.get<TextureLoader>();
    TextureList.emplace_back(textureSystem.LoadTexture(loader));
    return AddToMaterialMemoryPool(TextureList.back());
}

uint32 MaterialBakerSystem::AddToMaterialMemoryPool(Texture& texture)
{
    texture.gpuTextureBufferIndex = materialMemoryPoolSystem.AllocateObject(MaterialBakerMemoryPoolTypes::BakerTexture2DMetadataBuffer);
    materialMemoryPoolSystem.UpdateTextureDescriptorSet(texture, materialMemoryPoolSystem.BakerTexture2DBinding);
    materialMemoryPoolSystem.UpdateMemoryPool();
    return texture.gpuTextureBufferIndex;
}