#pragma once
#include "DLL.h"
#include "Platform.h"
#include "MemoryPoolSystem.h"

enum MaterialBakerMemoryPoolTypes
{
    BakerMaterialBuffer,
    BakerTexture2DMetadataBuffer,
    //BakerTexture3DMetadataBuffer,
    //BakerTextureCubeMapMetadataBuffer,
    BakerEndofPool
};

struct MaterialBakerBufferHeader
{
    uint64 MaterialOffset = UINT64_MAX;
    uint32 MaterialCount = UINT32_MAX;
    uint32 MaterialSize = UINT32_MAX;
    uint64 Texture2DOffset = UINT64_MAX;
    uint32 Texture2DCount = UINT32_MAX;
    uint32 Texture2DSize = UINT32_MAX;
    //uint64 Texture3DOffset = UINT64_MAX;
    //uint32 Texture3DCount = UINT32_MAX;
    //uint32 Texture3DSize = UINT32_MAX;
    //uint64 TextureCubeMapOffset = UINT64_MAX;
    //uint32 TextureCubeMapCount = UINT32_MAX;
    //uint32 TextureCubeMapSize = UINT32_MAX;
};

struct ImportMaterial
{
    float  Albedo[3];
    float  ClearcoatTint[3];
    float  SheenColor[3];
    float  SSSColor[3];
    float  AttenuationColor[3];
    float  Emission[3];

    float Metallic;
    float Roughness;
    float AmbientOcclusion;
    float IOR;
    float NormalStrength;
    float Height;

    float CoatWeight;
    float CoatRoughness;
    float CoatDarkening;

    float SheenWeight;
    float SheenRoughness;

    float SSSWeight;
    float SSSProfile;
    float Thickness;

    float TransmissionWeight;
    float AttenuationDistance;

    float Anisotropy;
    float AnisotropyRotation;
    float ThinFilmWeight;
    float ThinFilmThickness;
    float EmissionIntensity;

    uint  AlbedoMap;
    uint  NormalMap;
    uint  HeightMap;
    uint  AlphaMap;
    uint  MetallicMap;
    uint  RoughnessMap;
    uint  AmbientOcclusionMap;
    uint  EmissionMap;
    uint  ClearCoatColorMap;
    uint  ClearCoatPropertiesMap;
    uint  SheenMap;
    uint  SheenPropertiesMap;
    uint  SSSColorMap;
    uint  SSSPropertiesMap;
    uint  AttenuationColorMap;
    uint  AttenuationPropertiesMap;
    uint  AnisotropyPropertiesMap;
    uint  IORMap;

    uint  ShadingModel;
    uint  FeatureMask;
};
static_assert(sizeof(ImportMaterial) == 59 * 4);

class MaterialMemoryPoolSystem
{
public:
    static MaterialMemoryPoolSystem& Get();

private:
    MaterialMemoryPoolSystem() = default;
    ~MaterialMemoryPoolSystem() = default;
    MaterialMemoryPoolSystem(const MaterialMemoryPoolSystem&) = delete;
    MaterialMemoryPoolSystem& operator=(const MaterialMemoryPoolSystem&) = delete;
    MaterialMemoryPoolSystem(MaterialMemoryPoolSystem&&) = delete;
    MaterialMemoryPoolSystem& operator=(MaterialMemoryPoolSystem&&) = delete;

    VkDescriptorPool										              MaterialBakerBindlessPool = VK_NULL_HANDLE;
    VkDescriptorSet											              MaterialBakerBindlessDescriptorSet = VK_NULL_HANDLE;
    VkDescriptorSetLayout									              MaterialBakerBindlessDescriptorSetLayout = VK_NULL_HANDLE;

    void													              UpdateMemoryPoolHeader(MaterialBakerMemoryPoolTypes memoryPoolType, uint32 newPoolSize);
    void													              ResizeMemoryPool(MaterialBakerMemoryPoolTypes memoryPoolToUpdate, uint32 resizeCount);
    void													              CreateMaterialBakerBindlessDescriptorSet();

public:
    static constexpr size_t									              BakerMaterialCapacity = 1;
    static constexpr size_t									              BakerTexture2DCapacity = 20;
    static constexpr size_t									              BakerTexture3DCapacity = 4;
    static constexpr size_t									              BakerTextureCubeMapCapacity = 4;

    static constexpr uint									              BakerMaterialDescriptorBinding = 0;
    static constexpr uint									              BakerTexture2DBinding = 1;
    //static constexpr uint									              BakerTexture3DBinding = 2;
    //static constexpr uint									              BakerTextureCubeMapBinding = 3;

    void* MaterialBufferPtr = nullptr;
    bool													              IsHeaderDirty = true;
    bool													              IsDescriptorSetDirty = true;


    uint32                                                                MaterialBakerBufferId = UINT32_MAX;
    size_t													              MaterialMemoryPoolSize = UINT32_MAX;
    UnorderedMap<MaterialBakerMemoryPoolTypes, MemoryPoolSubBufferHeader> MemorySubPoolHeader;
    MaterialBakerBufferHeader									          MaterialPoolHeader;
    Vector<byte>											              MaterialBufferMemoryPool;

    DLL_EXPORT void											              StartUp();
    DLL_EXPORT uint32										              AllocateObject(MaterialBakerMemoryPoolTypes memoryPoolToUpdate);
    DLL_EXPORT void											              UpdateMemoryPool();
    DLL_EXPORT ImportMaterial&                                            UpdateMaterial(uint32 index);
    DLL_EXPORT void											              UpdateTextureDescriptorSet(Texture& texture, uint binding);
    DLL_EXPORT void											              UpdateDataBufferDescriptorSet(uint32 vulkanBufferIndex, uint binding);
    DLL_EXPORT void											              FreeObject(MaterialBakerMemoryPoolTypes memoryPoolToUpdate, uint32 index);
    DLL_EXPORT void                                                       BakerResetMemoryPool();
    const MemoryPoolLoader									 GetMemoryPoolInfo();
};
extern DLL_EXPORT MaterialMemoryPoolSystem& materialMemoryPoolSystem;
inline MaterialMemoryPoolSystem& MaterialMemoryPoolSystem::Get()
{
    static MaterialMemoryPoolSystem instance;
    return instance;
}