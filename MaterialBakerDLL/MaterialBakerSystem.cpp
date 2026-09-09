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
    vulkan.VulkanSetUp(configSystem.WindowResolution, configSystem.RenderResolution);
    bufferSystem.SetUpVmaAllocation();
    materialMemoryPoolSystem.StartUp();
    AssetBakerId = renderSystem.LoadRenderPass("RenderPass/AssetCreatorRenderPass.json");
    LoadMaterial(importMaterialPath);
    textureSystem.GenerateTexture(AssetBakerId);
    textureBakerSystem.BakeTexture(importMaterialPath, exportMaterialPath, AssetBakerId);
    vkQueueWaitIdle(vulkan.GraphicsQueue());
    CleanRenderPass();
    materialMemoryPoolSystem.BakerResetMemoryPool();
}


void MaterialBakerSystem::CleanRenderPass()
{
    vkQueueWaitIdle(vulkan.GraphicsQueue());
    
    VulkanRenderPass renderPass = renderSystem.FindRenderPass(AssetBakerId);
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
    ivec2 materialSetResolution = ivec2(json["TextureSetResolution"][0], json["TextureSetResolution"][1]);

    materialMemoryPoolSystem.AllocateObject(BakerMaterialBuffer);
    ImportMaterialShader& material = materialMemoryPoolSystem.UpdateMaterial(0);
    material.Albedo = vec3(json["Albedo"][0], json["Albedo"][1], json["Albedo"][2]);
    material.SheenColor = vec3(json["SheenColor"][0], json["SheenColor"][1], json["SheenColor"][2]);
    material.SubSurfaceScatteringColor = vec3(json["SubSurfaceScatteringColor"][0], json["SubSurfaceScatteringColor"][1], json["SubSurfaceScatteringColor"][2]);
    material.Emission = vec3(json["Emission"][0], json["Emission"][1], json["Emission"][2]);
    material.ClearcoatTint = json["ClearcoatTint"];
    material.Metallic = json["Metallic"];
    material.Roughness = json["Roughness"];
    material.AmbientOcclusion = json["AmbientOcclusion"];
    material.ClearcoatStrength = json["ClearcoatStrength"];
    material.ClearcoatRoughness = json["ClearcoatRoughness"];
    material.Thickness = json["Thickness"];
    material.SheenIntensity = json["SheenIntensity"];
    material.NormalStrength = json["NormalStrength"];
    material.HeightScale = json["HeightScale"];
    material.Height = json["Height"];
    material.Alpha = json["Alpha"];

    if (!json["AlbedoMap"].is_null())
    {
        TextureLoader loader = json["AlbedoMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportAlbedoMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.AlbedoMap = TextureList.back().gpuTextureBufferIndex;
    }
    if (!json["MetallicMap"].is_null())
    {
        TextureLoader loader = json["MetallicMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportPackedORMMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.MetallicMap = TextureList.back().gpuTextureBufferIndex;
    }
    if (!json["RoughnessMap"].is_null())
    {
        TextureLoader loader = json["RoughnessMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportPackedORMMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.RoughnessMap = TextureList.back().gpuTextureBufferIndex;
    }
    if (!json["ThicknessMap"].is_null())
    {
        TextureLoader loader = json["ThicknessMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportThicknessMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.ThicknessMap = TextureList.back().gpuTextureBufferIndex;
    }
    if (!json["SubSurfaceScatteringColorMap"].is_null())
    {
        TextureLoader loader = json["SubSurfaceScatteringColorMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportSubSurfaceScatteringMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.SubSurfaceScatteringColorMap = TextureList.back().gpuTextureBufferIndex;
    }
    if (!json["SheenMap"].is_null())
    {
        TextureLoader loader = json["SheenMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportSheenMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.SheenMap = TextureList.back().gpuTextureBufferIndex;
    }
    if (!json["ClearCoatMap"].is_null())
    {
        TextureLoader loader = json["ClearCoatMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportClearCoatMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.ClearCoatMap = TextureList.back().gpuTextureBufferIndex;
    }
    if (!json["AmbientOcclusionMap"].is_null())
    {
        TextureLoader loader = json["AmbientOcclusionMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportPackedORMMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.AmbientOcclusionMap = TextureList.back().gpuTextureBufferIndex;
    }
    if (!json["NormalMap"].is_null())
    {
        TextureLoader loader = json["NormalMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportNormalMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.NormalMap = TextureList.back().gpuTextureBufferIndex;
    }
    if (!json["AlphaMap"].is_null())
    {
        TextureLoader loader = json["AlphaMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportAlphaMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.AlphaMap = TextureList.back().gpuTextureBufferIndex;
    }
    if (!json["EmissionMap"].is_null())
    {
        TextureLoader loader = json["EmissionMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportEmissionMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.EmissionMap = TextureList.back().gpuTextureBufferIndex;
    }
    if (!json["HeightMap"].is_null())
    {
        TextureLoader loader = json["HeightMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportParallaxMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.HeightMap = TextureList.back().gpuTextureBufferIndex;
    }
    materialMemoryPoolSystem.IsHeaderDirty = true;
    materialMemoryPoolSystem.IsDescriptorSetDirty = true;
}

void MaterialBakerSystem_BakeMaterial(const char* importMaterialPath, const char* exportMaterialPath)
{
    materialBakerSystem.BakeMaterial(importMaterialPath, exportMaterialPath);
}
