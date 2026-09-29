#include "MaterialMemoryPoolSystem.h"

MaterialMemoryPoolSystem& materialMemoryPoolSystem = MaterialMemoryPoolSystem::Get();

void MaterialMemoryPoolSystem::StartUp()
{
    for (int x = 0; x < static_cast<int>(MaterialBakerMemoryPoolTypes::BakerEndofPool); x++)
    {
        MaterialBakerMemoryPoolTypes type = (MaterialBakerMemoryPoolTypes)x;
        switch (x)
        {
        case MaterialBakerMemoryPoolTypes::BakerMaterialBuffer:
        {
            MemorySubPoolHeader[type] = MemoryPoolSubBufferHeader
            {
                .ActiveCount = 0,
                .Capacity = BakerMaterialCapacity,
                .Size = sizeof(ImportMaterial),
                .IsSlotActive = Vector<byte>(BakerMaterialCapacity, 0x00),
                .FreeIndices = Vector<uint32>(),
                .IsDirty = true
            };
            break;
        }
        case MaterialBakerMemoryPoolTypes::BakerTexture2DMetadataBuffer:
        {
            MemorySubPoolHeader[type] = MemoryPoolSubBufferHeader
            {
                .ActiveCount = 0,
                .Capacity = BakerTexture2DCapacity,
                .Size = sizeof(TextureMetadataHeader),
                .IsSlotActive = Vector<byte>(BakerTexture2DCapacity, 0x00),
                .FreeIndices = Vector<uint32>(),
                .IsDirty = true
            };
            break;
        }
        }
    }

    UpdateMemoryPoolHeader(BakerMaterialBuffer, BakerMaterialCapacity);

    Vector<byte> GpuDataBufferMemoryPool2 =
        Vector<byte>(sizeof(MaterialBakerBufferHeader) + MaterialMemoryPoolSize, 0xFF);
    memcpy(GpuDataBufferMemoryPool2.data(), &MaterialPoolHeader, sizeof(MaterialBakerBufferHeader));

    MaterialBakerBufferId = bufferSystem.CreateDynamicBuffer(
        GpuDataBufferMemoryPool2.data(),
        GpuDataBufferMemoryPool2.size(),
        VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT | VK_BUFFER_USAGE_STORAGE_BUFFER_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT);

    VulkanBuffer& buffer = bufferSystem.FindVulkanBuffer(MaterialBakerBufferId);
    MaterialBufferPtr = buffer.BufferMappedData();
    vmaFlushAllocation(bufferSystem.VmaAllocatorHandle(), buffer.BufferAllocation(), 0, GpuDataBufferMemoryPool2.size());
    CreateMaterialBakerBindlessDescriptorSet();
}

void MaterialMemoryPoolSystem::ResizeMemoryPool(MaterialBakerMemoryPoolTypes memoryPoolToUpdate, uint32 resizeCount)
{
    void* oldMappedPtr = MaterialBufferPtr;
    uint32 oldBufferId = MaterialBakerBufferId;
    auto oldSubHeaders = MemorySubPoolHeader;

    UpdateMemoryPoolHeader(memoryPoolToUpdate, resizeCount);

    size_t newTotalSize = sizeof(MaterialBakerBufferHeader) + MaterialMemoryPoolSize;
    uint32 newBufferId = bufferSystem.CreateDynamicBuffer(
        nullptr,
        newTotalSize,
        VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT | VK_BUFFER_USAGE_STORAGE_BUFFER_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT);

    VulkanBuffer& newBuf = bufferSystem.FindVulkanBuffer(newBufferId);
    MaterialBufferPtr = newBuf.BufferMappedData();

    std::memcpy(MaterialBufferPtr, &MaterialPoolHeader, sizeof(MaterialBakerBufferHeader));

    for (const auto& [type, sub] : MemorySubPoolHeader)
    {
        const auto& oldSub = oldSubHeaders[type];
        size_t bytesToCopy = oldSub.ActiveCount * oldSub.Size;
        if (bytesToCopy > 0)
        {
            std::byte* dst = static_cast<std::byte*>(MaterialBufferPtr) + sub.Offset;
            const std::byte* src = static_cast<const std::byte*>(oldMappedPtr) + oldSub.Offset;
            std::memcpy(dst, src, bytesToCopy);
        }
    }

    vmaFlushAllocation(bufferSystem.VmaAllocatorHandle(), newBuf.BufferAllocation(), 0, newTotalSize);

    if (oldBufferId != UINT32_MAX)
    {
        bufferSystem.DestroyBuffer(bufferSystem.FindVulkanBuffer(oldBufferId));
    }

    MaterialBakerBufferId = newBufferId;
    IsDescriptorSetDirty = true;
    IsHeaderDirty = true;
}

void MaterialMemoryPoolSystem::UpdateMemoryPoolHeader(MaterialBakerMemoryPoolTypes memoryPoolTypeToUpdate, uint32 newPoolSize)
{
    for (int x = static_cast<int>(memoryPoolTypeToUpdate);
        x < static_cast<int>(MaterialBakerMemoryPoolTypes::BakerEndofPool);
        x++)
    {
        const MaterialBakerMemoryPoolTypes memoryPoolType = (MaterialBakerMemoryPoolTypes)x;
        const MaterialBakerMemoryPoolTypes lastMemoryPoolType = (x == 0)
            ? MaterialBakerMemoryPoolTypes::BakerEndofPool
            : (MaterialBakerMemoryPoolTypes)(x - 1);

        const MemoryPoolSubBufferHeader oldMemoryPoolSubHeader = MemorySubPoolHeader[memoryPoolType];

        MemorySubPoolHeader[memoryPoolType] = MemoryPoolSubBufferHeader
        {
           .ActiveCount = MemorySubPoolHeader[memoryPoolType].ActiveCount,
           .Offset = lastMemoryPoolType == MaterialBakerMemoryPoolTypes::BakerEndofPool
                ? sizeof(MaterialBakerBufferHeader)
                : MemorySubPoolHeader[lastMemoryPoolType].Offset
                    + (MemorySubPoolHeader[lastMemoryPoolType].Capacity * MemorySubPoolHeader[lastMemoryPoolType].Size),
           .Capacity = memoryPoolType == memoryPoolTypeToUpdate
                ? newPoolSize
                : MemorySubPoolHeader[memoryPoolType].Capacity,
           .Size = MemorySubPoolHeader[memoryPoolType].Size,
           .IsSlotActive = memoryPoolType == memoryPoolTypeToUpdate
                ? Vector<byte>(newPoolSize, 0x00)
                : MemorySubPoolHeader[memoryPoolType].IsSlotActive,
           .FreeIndices = MemorySubPoolHeader[memoryPoolType].FreeIndices,
           .IsDirty = true
        };

        const uint32 bytesToCopy = std::min(
            oldMemoryPoolSubHeader.ActiveCount,
            static_cast<uint32>(MemorySubPoolHeader[memoryPoolType].IsSlotActive.size()));
        if (bytesToCopy > 0 && !oldMemoryPoolSubHeader.IsSlotActive.empty())
        {
            memcpy(MemorySubPoolHeader[memoryPoolType].IsSlotActive.data(),
                oldMemoryPoolSubHeader.IsSlotActive.data(),
                bytesToCopy);
        }
    }

    MemoryPoolSubBufferHeader lastHeader =
        MemorySubPoolHeader[(MaterialBakerMemoryPoolTypes)((int)MaterialBakerMemoryPoolTypes::BakerEndofPool - 1)];
    MaterialMemoryPoolSize = lastHeader.Offset + (lastHeader.Size * lastHeader.Capacity);

    MaterialPoolHeader = MaterialBakerBufferHeader
    {
        .MaterialOffset = MemorySubPoolHeader[BakerMaterialBuffer].Offset,
        .MaterialCount = MemorySubPoolHeader[BakerMaterialBuffer].ActiveCount,
        .MaterialSize = MemorySubPoolHeader[BakerMaterialBuffer].Size,
        .Texture2DOffset = MemorySubPoolHeader[BakerTexture2DMetadataBuffer].Offset,
        .Texture2DCount = MemorySubPoolHeader[BakerTexture2DMetadataBuffer].ActiveCount,
        .Texture2DSize = MemorySubPoolHeader[BakerTexture2DMetadataBuffer].Size,
    };
}

uint32 MaterialMemoryPoolSystem::AllocateObject(MaterialBakerMemoryPoolTypes memoryPoolToUpdate)
{
    MemoryPoolSubBufferHeader& subPoolHeader = MemorySubPoolHeader[memoryPoolToUpdate];

    if (!subPoolHeader.FreeIndices.empty())
    {
        uint32 index = subPoolHeader.FreeIndices.back();
        subPoolHeader.FreeIndices.pop_back();
        subPoolHeader.IsSlotActive[index] = 0x01;
        if (index + 1 > subPoolHeader.ActiveCount)
        {
            subPoolHeader.ActiveCount = index + 1;
        }
        subPoolHeader.IsDirty = true;
        return index;
    }

    if (subPoolHeader.ActiveCount == subPoolHeader.Capacity)
    {
        ResizeMemoryPool(memoryPoolToUpdate, subPoolHeader.Capacity * 2);
    }

    uint32 index = subPoolHeader.ActiveCount++;
    subPoolHeader.IsSlotActive[index] = 0x01;
    subPoolHeader.IsDirty = true;
    return index;
}

void MaterialMemoryPoolSystem::UpdateMemoryPool()
{
    if (!MaterialBufferPtr)
    {
        return;
    }

    VulkanBuffer& buffer = bufferSystem.FindVulkanBuffer(MaterialBakerBufferId);
    for (auto& [type, sub] : MemorySubPoolHeader)
    {
        if (sub.IsDirty)
        {
            size_t start = sub.Offset;
            size_t len = sub.ActiveCount * sub.Size;
            if (len > 0)
            {
                vmaFlushAllocation(bufferSystem.VmaAllocatorHandle(), buffer.BufferAllocation(), start, len);
            }
            sub.IsDirty = false;
        }
    }

    if (IsHeaderDirty)
    {
        memcpy(MaterialBufferPtr, &MaterialPoolHeader, sizeof(MaterialBakerBufferHeader));
        vmaFlushAllocation(bufferSystem.VmaAllocatorHandle(), buffer.BufferAllocation(), 0, sizeof(MaterialBakerBufferHeader));
        IsHeaderDirty = false;
    }

    if (IsDescriptorSetDirty)
    {
        UpdateDataBufferDescriptorSet(MaterialBakerBufferId, BakerMaterialDescriptorBinding);
        IsDescriptorSetDirty = false;
    }
}

ImportMaterial& MaterialMemoryPoolSystem::UpdateMaterial(uint32 index)
{
    MemoryPoolSubBufferHeader& materialSubPool = MemorySubPoolHeader[BakerMaterialBuffer];
    if (index >= materialSubPool.Capacity)
        throw std::out_of_range("Material index out of range: " + std::to_string(index) + " >= " + std::to_string(materialSubPool.Capacity));
    if (index >= materialSubPool.IsSlotActive.size() || !materialSubPool.IsSlotActive[index])
        throw std::runtime_error("Material slot inactive at index " + std::to_string(index));

    uint32 offset = materialSubPool.Offset + (index * sizeof(ImportMaterial));
    materialSubPool.IsDirty = true;
    return *reinterpret_cast<ImportMaterial*>(static_cast<byte*>(MaterialBufferPtr) + offset);
}

void MaterialMemoryPoolSystem::UpdateTextureDescriptorSet(Texture& texture, uint binding)
{
    if (texture.texture.TextureViews().empty())
    {
        std::cerr << "ERROR: Trying to update descriptor with invalid image view for texture index " << texture.gpuTextureBufferIndex << std::endl;
        return;
    }
    if (texture.texture.TextureSampler() == VK_NULL_HANDLE)
    {
        std::cerr << "ERROR: Null sampler for texture index " << texture.gpuTextureBufferIndex << std::endl;
        return;
    }

    VkDescriptorImageInfo textureUpdate = VkDescriptorImageInfo
    {
        .sampler = texture.texture.TextureSampler(),
        .imageView = texture.texture.TextureViews().front(),
        .imageLayout = texture.texture.m_colorChannels == ColorChannelEnum::ChannelR ? VK_IMAGE_LAYOUT_DEPTH_STENCIL_READ_ONLY_OPTIMAL : VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL
    };

    VkWriteDescriptorSet descriptorUpdate = VkWriteDescriptorSet
    {
        .sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET,
        .dstSet = MaterialBakerBindlessDescriptorSet,
        .dstBinding = binding,
        .dstArrayElement = static_cast<uint32>(texture.gpuTextureBufferIndex),
        .descriptorCount = 1,
        .descriptorType = VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER,
        .pImageInfo = &textureUpdate,
    };
    vkUpdateDescriptorSets(vulkan.LogicalDevice(), 1, &descriptorUpdate, 0, nullptr);
}

void MaterialMemoryPoolSystem::UpdateDataBufferDescriptorSet(uint32 vulkanBufferIndex, uint binding)
{
    VkDescriptorBufferInfo bufferUpdate = VkDescriptorBufferInfo
    {
        .buffer = bufferSystem.FindVulkanBuffer(vulkanBufferIndex).Buffer(),
        .offset = 0,
        .range = VK_WHOLE_SIZE
    };

    VkWriteDescriptorSet descriptorUpdate = VkWriteDescriptorSet
    {
        .sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET,
        .dstSet = MaterialBakerBindlessDescriptorSet,
        .dstBinding = binding,
        .dstArrayElement = 0,
        .descriptorCount = 1,
        .descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER,
        .pBufferInfo = &bufferUpdate,
    };
    vkUpdateDescriptorSets(vulkan.LogicalDevice(), 1, &descriptorUpdate, 0, nullptr);
}

void MaterialMemoryPoolSystem::BakerResetMemoryPool()
{
    MaterialMemoryPoolSize = UINT32_MAX;
    MemorySubPoolHeader.clear();
    MaterialPoolHeader = MaterialBakerBufferHeader();
    MaterialBufferMemoryPool.clear();
}

const MemoryPoolLoader MaterialMemoryPoolSystem::GetMemoryPoolInfo()
{
    return MemoryPoolLoader
    {
        .GlobalBindlessPool = MaterialBakerBindlessPool,
        .GlobalBindlessDescriptorSet = MaterialBakerBindlessDescriptorSet,
        .GlobalBindlessDescriptorSetLayout = MaterialBakerBindlessDescriptorSetLayout
    };
}

void MaterialMemoryPoolSystem::FreeObject(MaterialBakerMemoryPoolTypes memoryPoolToUpdate, uint32 index)
{
    MemoryPoolSubBufferHeader& sub = MemorySubPoolHeader[memoryPoolToUpdate];
    if (index >= sub.Capacity || index >= sub.IsSlotActive.size() || !sub.IsSlotActive[index])
    {
        return;
    }

    sub.IsSlotActive[index] = 0x00;
    sub.FreeIndices.push_back(index);
    sub.IsDirty = true;

    while (sub.ActiveCount > 0 && sub.IsSlotActive[sub.ActiveCount - 1] == 0)
    {
        sub.ActiveCount--;
    }
}

void MaterialMemoryPoolSystem::CreateMaterialBakerBindlessDescriptorSet()
{
    VkDescriptorBufferInfo materialInfo =
    {
        .buffer = bufferSystem.FindVulkanBuffer(MaterialBakerBufferId).Buffer(),
        .offset = 0,
        .range = VK_WHOLE_SIZE
    };

    Vector<VkDescriptorPoolSize> poolSizes = {
        {VK_DESCRIPTOR_TYPE_STORAGE_BUFFER, 512},
        {VK_DESCRIPTOR_TYPE_STORAGE_BUFFER, 512},
        {VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, BakerTexture2DCapacity + 1024},
        {VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT, 64}
    };

    Vector<VkDescriptorSetLayoutBinding> bindings =
    {
        { BakerMaterialDescriptorBinding,   VK_DESCRIPTOR_TYPE_STORAGE_BUFFER,          1, VK_SHADER_STAGE_ALL },
        { BakerTexture2DBinding,            VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, BakerTexture2DCapacity,      VK_SHADER_STAGE_ALL },
        //{ BakerTexture3DBinding,            VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, BakerTexture3DCapacity,      VK_SHADER_STAGE_ALL },  
        //{ BakerTextureCubeMapBinding,       VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, BakerTextureCubeMapCapacity, VK_SHADER_STAGE_ALL }
    };

    Vector<VkDescriptorBindingFlags> flags =
    {
        VkDescriptorBindingFlags { VK_DESCRIPTOR_BINDING_PARTIALLY_BOUND_BIT | VK_DESCRIPTOR_BINDING_UPDATE_AFTER_BIND_BIT | VK_DESCRIPTOR_BINDING_UPDATE_UNUSED_WHILE_PENDING_BIT },
        VkDescriptorBindingFlags { VK_DESCRIPTOR_BINDING_PARTIALLY_BOUND_BIT | VK_DESCRIPTOR_BINDING_UPDATE_AFTER_BIND_BIT | VK_DESCRIPTOR_BINDING_UPDATE_UNUSED_WHILE_PENDING_BIT },
        //VkDescriptorBindingFlags { VK_DESCRIPTOR_BINDING_PARTIALLY_BOUND_BIT | VK_DESCRIPTOR_BINDING_UPDATE_AFTER_BIND_BIT | VK_DESCRIPTOR_BINDING_UPDATE_UNUSED_WHILE_PENDING_BIT },
        //VkDescriptorBindingFlags { VK_DESCRIPTOR_BINDING_PARTIALLY_BOUND_BIT | VK_DESCRIPTOR_BINDING_UPDATE_AFTER_BIND_BIT | VK_DESCRIPTOR_BINDING_UPDATE_UNUSED_WHILE_PENDING_BIT }
    };

    VkDescriptorSetLayoutBindingFlagsCreateInfo flagsInfo
    {
        .sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_BINDING_FLAGS_CREATE_INFO,
        .bindingCount = static_cast<uint32>(flags.size()),
        .pBindingFlags = flags.data()
    };

    VkDescriptorPoolCreateInfo poolInfo =
    {
        .sType = VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO,
        .flags = VK_DESCRIPTOR_POOL_CREATE_UPDATE_AFTER_BIND_BIT,
        .maxSets = 64,
        .poolSizeCount = static_cast<uint32_t>(poolSizes.size()),
        .pPoolSizes = poolSizes.data()
    };
    VULKAN_THROW_IF_FAIL(vkCreateDescriptorPool(vulkan.LogicalDevice(), &poolInfo, nullptr, &MaterialBakerBindlessPool));

    VkDescriptorSetLayoutCreateInfo layoutInfo =
    {
        .sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO,
        .pNext = &flagsInfo,
        .flags = VK_DESCRIPTOR_SET_LAYOUT_CREATE_UPDATE_AFTER_BIND_POOL_BIT,
        .bindingCount = static_cast<uint32_t>(bindings.size()),
        .pBindings = bindings.data()
    };
    VULKAN_THROW_IF_FAIL(vkCreateDescriptorSetLayout(vulkan.LogicalDevice(), &layoutInfo, nullptr, &MaterialBakerBindlessDescriptorSetLayout));

    VkDescriptorSetAllocateInfo allocInfo =
    {
        .sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO,
        .descriptorPool = MaterialBakerBindlessPool,
        .descriptorSetCount = 1,
        .pSetLayouts = &MaterialBakerBindlessDescriptorSetLayout
    };
    VULKAN_THROW_IF_FAIL(vkAllocateDescriptorSets(vulkan.LogicalDevice(), &allocInfo, &MaterialBakerBindlessDescriptorSet));

    VkWriteDescriptorSet materialWrite =
    {
        .sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET,
        .dstSet = MaterialBakerBindlessDescriptorSet,
        .dstBinding = BakerMaterialDescriptorBinding,
        .dstArrayElement = 0,
        .descriptorCount = 1,
        .descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER,
        .pBufferInfo = &materialInfo
    };

    vkUpdateDescriptorSets(vulkan.LogicalDevice(), 1, &materialWrite, 0, nullptr);
}