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

MaterialBakerSystem& materialBakerSystem = MaterialBakerSystem::Get();

void MaterialBakerSystem::BakeMaterial(const String& importMaterialPath, const String& exportMaterialPath)
{
    materialMemoryPoolSystem.StartUp();

    nlohmann::json json = fileSystem.LoadJsonFile(importMaterialPath.c_str());
    ivec2 materialSetResolution = ivec2(json["TextureSetResolution"][0], json["TextureSetResolution"][1]);

    RenderPassLoader renderPassLoader = fileSystem.LoadJsonFile("RenderPass/AssetCreatorRenderPass.json").get<RenderPassLoader>();
    renderPassLoader.RenderPassResolution = materialSetResolution;
    AssetBakerRenderPassId = renderSystem.LoadRenderPass(renderPassLoader, materialMemoryPoolSystem.GetMemoryPoolInfo());
    
    VulkanRenderPass renderPass = renderSystem.FindRenderPass(AssetBakerRenderPassId);
    Vector<PushConstantUpdateRule> pushConstantRules = renderPass.SubPassList().front().front().PushConstantUpdates;

    LoadMaterial(importMaterialPath);
    for (int x = 0; x < 2; x++)
    {
        for (int y = 0; y < pushConstantRules.size(); y++)
        {
            if (pushConstantRules[y].Variable == "MaterialBakerSubPassIndex")
            {
                pushConstantRules[y].Value[0] = std::to_string(x);
            }
        }

        textureSystem.GenerateTexture(AssetBakerRenderPassId, &pushConstantRules);
        vkQueueWaitIdle(vulkan.GraphicsQueue());
        textureBakerSystem.BakeTexture(importMaterialPath, exportMaterialPath, AssetBakerRenderPassId, x);
    }
    vkQueueWaitIdle(vulkan.GraphicsQueue());
    CleanRenderPass();
    materialMemoryPoolSystem.BakerResetMemoryPool();
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

void MaterialBakerSystem::LoadMaterial(const String& materialPath)
{
    nlohmann::json json = fileSystem.LoadJsonFile(materialPath.c_str());

    uint materialId = materialMemoryPoolSystem.AllocateObject(BakerMaterialBuffer);
    ImportMaterial& m = materialMemoryPoolSystem.UpdateMaterial(materialId);

    auto v3 = [](const nlohmann::json& a, float x, float y, float z) 
        {
            if (a.is_array() && a.size() >= 3) return std::array<float, 3>{ a[0].get<float>(), a[1].get<float>(), a[2].get<float>() };
            return std::array<float, 3>{ x, y, z };
        };

    auto Albedo = v3(json["Albedo"], 1, 1, 1);
    auto ClearcoatTint = v3(json["ClearcoatTint"], 1, 1, 1);
    auto SheenColor = v3(json["SheenColor"], 1, 1, 1);
    auto SSSColor = v3(json.contains("SSSColor") ? json["SSSColor"] : json["SubSurfaceScatteringColor"], 1, 0.45f, 0.35f);
    auto AttenuationColor = v3(json["AttenuationColor"], 1, 1, 1);
    auto Emission = v3(json["Emission"], 0, 0, 0);

    memcpy(m.Albedo, Albedo.data(), 12);
    memcpy(m.ClearcoatTint, ClearcoatTint.data(), 12);
    memcpy(m.SheenColor, SheenColor.data(), 12);
    memcpy(m.SSSColor, SSSColor.data(), 12);
    memcpy(m.AttenuationColor, AttenuationColor.data(), 12);
    memcpy(m.Emission, Emission.data(), 12);

    m.Metallic = json.value("Metallic", 0.0f);
    m.Roughness = json.value("Roughness", 0.5f);
    m.AmbientOcclusion = json.value("AmbientOcclusion", 1.0f);
    m.IOR = json.value("IOR", 1.45f);
    m.NormalStrength = json.value("NormalStrength", 1.0f);
    m.Height = json.value("Height", json.value("HeightScale", 0.0f));

    m.CoatWeight = json.value("CoatWeight", json.value("ClearcoatWeight", 0.0f));
    m.CoatRoughness = json.value("CoatRoughness", json.value("ClearcoatRoughness", 0.08f));
    m.CoatDarkening = json.value("CoatDarkening", 1.0f);

    m.SheenWeight = json.value("SheenWeight", 0.0f);
    m.SheenRoughness = json.value("SheenRoughness", 0.5f);

    m.SSSWeight = json.value("SSSWeight", 0.0f);
    m.SSSProfile = json.value("SSSProfile", 0.0f);
    m.Thickness = json.value("Thickness", 0.5f);

    m.TransmissionWeight = json.value("TransmissionWeight", 0.0f);
    m.AttenuationDistance = json.value("AttenuationDistance", 1.0f);

    m.Anisotropy = json.value("Anisotropy", 0.0f);
    m.AnisotropyRotation = json.value("AnisotropyRotation", 0.0f);
    m.ThinFilmWeight = json.value("ThinFilmWeight", 0.0f);
    m.ThinFilmThickness = json.value("ThinFilmThickness", 0.5f);
    m.EmissionIntensity = json.value("EmissionIntensity", 0.0f);

    m.AlbedoMap = TextureExists(json, "AlbedoMap");
    m.NormalMap = TextureExists(json, "NormalMap");
    m.HeightMap = TextureExists(json, "HeightMap");
    m.AlphaMap = TextureExists(json, "AlphaMap");
    m.MetallicMap = TextureExists(json, "MetallicMap");
    m.RoughnessMap = TextureExists(json, "RoughnessMap");
    m.AmbientOcclusionMap = TextureExists(json, "AmbientOcclusionMap");
    m.EmissionMap = TextureExists(json, "EmissionMap");
    m.ClearCoatColorMap = TextureExists(json, "ClearCoatColorMap");
    m.ClearCoatPropertiesMap = TextureExists(json, "ClearCoatPropertiesMap");
    m.SheenMap = TextureExists(json, "SheenMap");
    m.SheenPropertiesMap = TextureExists(json, "SheenPropertiesMap");
    m.SSSColorMap = TextureExists(json, "SubSurfaceScatteringColorMap");
    m.SSSPropertiesMap = TextureExists(json, "SubSurfaceScatteringPropertiesMap");
    m.AttenuationColorMap = TextureExists(json, "AttenuationColorMap");
    m.AttenuationPropertiesMap = TextureExists(json, "AttenuationPropertiesTexture");
    m.AnisotropyPropertiesMap = TextureExists(json, "AnisotropyPropertiesMap");
    m.IORMap = TextureExists(json, "IORTexture");

    m.ShadingModel = json.value("ShadingModel", 0u);
    m.FeatureMask = json.value("FeatureMask", 0u);

    materialMemoryPoolSystem.IsHeaderDirty = true;
    materialMemoryPoolSystem.IsDescriptorSetDirty = true;
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