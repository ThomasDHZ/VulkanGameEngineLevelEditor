#pragma once
#include "DLL.h"
#include <Platform.h>
#include <ktx/include/ktx.h>
#include <ktx/include/ktxvulkan.h>
#include <lodepng.h>
#include "TextureSamplers.h"
#include <TextureSystem.h>


enum RenderPassAttachmentEnum
{
    kAlbedoAttachment,
    kNormalDataAttachment,
    kMROAttachment,
    kFeatureAAttachment,
    kFeatureBAttachment,
    kFeatureCAttachment,
    kFeatureDAttachment,
    kEmissionAttachment
};

struct RawMipReadback
{
    void* data = nullptr;
    size_t size = 0;
    VkBuffer buffer = VK_NULL_HANDLE;
    VmaAllocation allocation = VK_NULL_HANDLE;
    bool needsUnmap = false;
};

struct ImportTexture
{
    TextureGuid              textureGuid = TextureGuid();
    RenderPassGuid           renderPassGuid = RenderPassGuid();
    size_t                   textureIndex = SIZE_MAX;

    int                      width = 1;
    int                      height = 1;
    int                      depth = 1;
    uint32                   mipMapLevels = 0;

    VkImage                  textureImage = VK_NULL_HANDLE;
    VkDeviceMemory           textureMemory = VK_NULL_HANDLE;
    Vector<VkImageView>      textureViewList;
    VkImageView              RenderedCubeMapView = VK_NULL_HANDLE;
    VkImageView              AttachmentArrayView = VK_NULL_HANDLE;
    VkSampler                textureSampler = VK_NULL_HANDLE;
    VkDescriptorSet          ImGuiDescriptorSet = VK_NULL_HANDLE;
    VmaAllocation            TextureAllocation = VK_NULL_HANDLE;

    TextureTypeEnum          textureType = TextureTypeEnum::kTextureType_Undefined;
    VkFormat                 textureByteFormat = VK_FORMAT_UNDEFINED;
    VkImageLayout            textureImageLayout = VK_IMAGE_LAYOUT_UNDEFINED;
    VkSampleCountFlagBits    sampleCount = VK_SAMPLE_COUNT_1_BIT;
    ColorChannelEnum         colorChannels = ColorChannelEnum::ChannelRGBA;
};

class TextureBakerSystem
{
public:
    static TextureBakerSystem& Get();

private:
    TextureBakerSystem() = default;
    ~TextureBakerSystem() = default;
    TextureBakerSystem(const TextureBakerSystem&) = delete;
    TextureBakerSystem& operator=(const TextureBakerSystem&) = delete;
    TextureBakerSystem(TextureBakerSystem&&) = delete;
    TextureBakerSystem& operator=(TextureBakerSystem&&) = delete;

    Vector<byte>    ConvertMipToRGBA8(const void* rawData, size_t rawSize, uint32 width, uint32 height, VkFormat srcFormat);
    String          GetAttachmentSuffix(uint x, uint materialBakerSubPassIndex);
    RawMipReadback  ConvertToRawTextureData(Texture& importTexture, uint32 mipLevel);
    void            DestroyVMATextureBuffer(RawMipReadback& data);
    void            ExportToPng(const String& fileName, Texture& texture, uint32 mipLevel = 0, bool flipY = true);
    void            ExportJson();
    float           HalfToFloat(uint16 h);
    void            FloatToRGBE(float r, float g, float b, byte out[4]);
    bool            ExportToHdr(const String& fileName, Texture& texture, uint32 mipLevel, bool flipY);
    nlohmann::json  TextureSlotJson(const String& path, VkFormat textureByteFormat);
    nlohmann::json  SamplerAtlasJson();

public:
    DLL_EXPORT  nlohmann::json BakeTexture(const String& MaterialName, VkGuid renderPassId, uint materialBakerSubPassIndex);
};
extern DLL_EXPORT TextureBakerSystem& textureBakerSystem;
inline TextureBakerSystem& TextureBakerSystem::Get()
{
    static TextureBakerSystem instance;
    return instance;
}