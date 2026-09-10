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
    memoryPoolSystem.StartUp();
    materialMemoryPoolSystem.StartUp();
    AssetBakerRenderPassId = renderSystem.LoadRenderPass("RenderPass/AssetCreatorRenderPass.json", materialMemoryPoolSystem.GetMemoryPoolInfo());
    LoadMaterial(importMaterialPath);
    textureSystem.GenerateTexture(AssetBakerRenderPassId);
    textureBakerSystem.BakeTexture(importMaterialPath, exportMaterialPath, AssetBakerRenderPassId);
    vkQueueWaitIdle(vulkan.GraphicsQueue());
    CleanRenderPass();
    materialMemoryPoolSystem.BakerResetMemoryPool();
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
    ivec2 materialSetResolution = ivec2(json["TextureSetResolution"][0], json["TextureSetResolution"][1]);

    uint materialId = materialMemoryPoolSystem.AllocateObject(BakerMaterialBuffer);
    ImportMaterial& material = materialMemoryPoolSystem.UpdateMaterial(materialId);
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
        material.AlbedoMap = AddToMaterialMemoryPool(TextureList.back().texture);
    }
    if (!json["MetallicMap"].is_null())
    {
        TextureLoader loader = json["MetallicMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportPackedORMMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.MetallicMap = AddToMaterialMemoryPool(TextureList.back().texture);
    }
    if (!json["RoughnessMap"].is_null())
    {
        TextureLoader loader = json["RoughnessMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportPackedORMMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.RoughnessMap = AddToMaterialMemoryPool(TextureList.back().texture);
    }
    if (!json["ThicknessMap"].is_null())
    {
        TextureLoader loader = json["ThicknessMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportThicknessMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.ThicknessMap = AddToMaterialMemoryPool(TextureList.back().texture);
    }
    if (!json["SubSurfaceScatteringColorMap"].is_null())
    {
        TextureLoader loader = json["SubSurfaceScatteringColorMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportSubSurfaceScatteringMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.SubSurfaceScatteringColorMap = AddToMaterialMemoryPool(TextureList.back().texture);
    }
    if (!json["SheenMap"].is_null())
    {
        TextureLoader loader = json["SheenMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportSheenMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.SheenMap = AddToMaterialMemoryPool(TextureList.back().texture);
    }
    if (!json["ClearCoatMap"].is_null())
    {
        TextureLoader loader = json["ClearCoatMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportClearCoatMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.ClearCoatMap = AddToMaterialMemoryPool(TextureList.back().texture);
    }
    if (!json["AmbientOcclusionMap"].is_null())
    {
        TextureLoader loader = json["AmbientOcclusionMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportPackedORMMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.AmbientOcclusionMap = AddToMaterialMemoryPool(TextureList.back().texture);
    }
    if (!json["NormalMap"].is_null())
    {
        TextureLoader loader = json["NormalMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportNormalMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.NormalMap = AddToMaterialMemoryPool(TextureList.back().texture);
    }
    if (!json["AlphaMap"].is_null())
    {
        TextureLoader loader = json["AlphaMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportAlphaMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.AlphaMap = AddToMaterialMemoryPool(TextureList.back().texture);
    }
    if (!json["EmissionMap"].is_null())
    {
        TextureLoader loader = json["EmissionMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportEmissionMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.EmissionMap = AddToMaterialMemoryPool(TextureList.back().texture);
    }
    if (!json["HeightMap"].is_null())
    {
        TextureLoader loader = json["HeightMap"].get<TextureLoader>();
        loader.SamplerCreateInfo = TextureSamplers::GetImportParallaxMapSamplerSettings();

        TextureList.emplace_back(textureSystem.LoadTexture(loader));
        material.HeightMap = AddToMaterialMemoryPool(TextureList.back().texture);
    }
    materialMemoryPoolSystem.IsHeaderDirty = true;
    materialMemoryPoolSystem.IsDescriptorSetDirty = true;
}

//void MaterialBakerSystem::UpdateDescriptorSets()
//{
//    auto GetDescriptorSet = [](const Texture& texture)
//        {
//            return VkDescriptorImageInfo
//            {
//                .sampler = texture.texture.TextureSampler(),
//                .imageView = texture.texture.TextureViews()[0],
//                .imageLayout = texture.texture.TextureImageLayout()
//            };
//        };
//
//    vkDeviceWaitIdle(vulkan.LogicalDevice());
//    VulkanRenderPass renderPass = renderSystem.FindRenderPass(AssetBakerRenderPassId);
//    auto a = renderPass.PipelineList();
//    VkDescriptorSet targetSet = renderSystem.FindRenderPipeline(renderPass.PipelineList()[0]).DescriptorSetList().front();
//
//    Vector<VkWriteDescriptorSet> descriptorSetList;
//    VkDescriptorBufferInfo materialInfo =
//    {
//        .buffer = bufferSystem.FindVulkanBuffer(materialMemoryPoolSystem.MaterialBakerBufferId).Buffer(),
//        .offset = 0,
//        .range = VK_WHOLE_SIZE
//    };
//    descriptorSetList.emplace_back(VkWriteDescriptorSet{ VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET, nullptr, targetSet, 0, 0, 1,  VK_DESCRIPTOR_TYPE_STORAGE_BUFFER, nullptr, &materialInfo,  nullptr });
//
//    for (uint x = 0; x < TextureList.size(); x++)
//    {
//        VkDescriptorImageInfo imageInfo = GetDescriptorSet(TextureList[x]);
//        descriptorSetList.emplace_back(VkWriteDescriptorSet{ VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET, nullptr, targetSet, x + 1, 0, 1,  VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, &imageInfo,  nullptr, nullptr });
//    }
//    vkUpdateDescriptorSets(vulkan.LogicalDevice(), static_cast<uint32_t>(descriptorSetList.size()), descriptorSetList.data(), 0, nullptr);
//}

uint32 MaterialBakerSystem::AddToMaterialMemoryPool(VulkanTexture& texture)
{
    uint32 gpuTextureIndex = materialMemoryPoolSystem.AllocateObject(MaterialBakerMemoryPoolTypes::BakerTexture2DMetadataBuffer);

    memoryPoolSystem.UpdateTextureDescriptorSet(gpuTextureIndex, texture, memoryPoolSystem.Texture2DBinding);
    return gpuTextureIndex;
}

void MaterialBakerSystem_BakeMaterial(const char* importMaterialPath, const char* exportMaterialPath)
{
    materialBakerSystem.BakeMaterial(importMaterialPath, exportMaterialPath);
}

Vector<RenderPassNode> MaterialBakerSystem::CreateDrawCommands(VkCommandBuffer& commandBuffer, const float& deltaTime)
{
    Vector<RenderPassNode> renderPassNodeList;
    for (auto& renderPassGuid : RenderPassDrawList)
    {
        const VulkanRenderPass& renderPass = renderSystem.FindRenderPass(renderPassGuid);

        uint32 maxMipLevelCount = 1;
        Vector<Vector<VulkanDrawMessage>> vulkanDrawMessageList;
        for (auto& renderPassList : renderPass.SubPassList())
        {
            Vector<VulkanDrawMessage> vulkanSubPassMessageList;
            for (auto& subPass : renderPassList)
            {
                for (auto& inputTexture : subPass.InputTextureList)
                {
                    const Texture& texture = renderSystem.FindRenderPassAttachment(inputTexture);
                    if (maxMipLevelCount < texture.texture.MipMapLevels()) maxMipLevelCount = texture.texture.MipMapLevels() - 1;
                }

                vulkanSubPassMessageList.emplace_back(VulkanDrawMessage
                    {
                        .RenderPassGuid = renderPassGuid,
                        .PipelinePackageGuid = subPass.PipelinePackageId,
                        .PushConstant = subPass.ShaderPushConstant,
                        .RenderPassInputs = subPass.InputTextureList,
                        .RenderPassOutputs = subPass.OutputTextureList,
                        .OffScreenRenderPass = subPass.OffScreenFrameBuffer
                    });
            }
            vulkanDrawMessageList.emplace_back(vulkanSubPassMessageList);
        }
        renderPassNodeList.emplace_back(RenderPassNode
            {
               .RenderPassGuid = renderPassGuid,
               .SubPassDrawMessage = vulkanDrawMessageList,
               .MipCount = maxMipLevelCount
            });
    }
    return renderPassNodeList;
}