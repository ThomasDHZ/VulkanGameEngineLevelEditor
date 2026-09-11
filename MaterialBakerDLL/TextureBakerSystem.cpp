#define FMT_HEADER_ONLY

#include "TextureBakerSystem.h"
#include <EngineConfigSystem.h>
#include <TextureSystem.h>
#include <VulkanSystem.h>
#include <BufferSystem.h>
#include <ktx/include/ktx.h>
#include <ktx/include/ktxvulkan.h>
#include <algorithm>
#include <thread>
#include <fstream>
#include <filesystem>
#include <fmt/format.h>
#include <shellapi.h>
#include <windows.h> 
#include <RenderSystem.h>

TextureBakerSystem& textureBakerSystem = TextureBakerSystem::Get();

void TextureBakerSystem::BakeTexture(const String& materialLoader, const String& baseFilePath, VkGuid renderPassId)
{
    Vector<Texture> attachmentTextureList = renderSystem.FindRenderPassAttachmentList(renderPassId);
    if (attachmentTextureList.empty())
    {
        fprintf(stderr, "No rendered textures found for render pass %s\n", renderPassId.ToString().c_str());
        return;
    }

    nlohmann::json importJson = fileSystem.LoadJsonFile(materialLoader.c_str());

    String textureName = baseFilePath;
    if (const size_t pos = textureName.find("Import"); pos != String::npos)
        textureName.erase(pos, 6);

    uint32 requestedMips = 1;
    importJson.at("ExportMipMapCount").get_to(requestedMips);

    const char* nvttExe = R"(C:\Program Files\NVIDIA Corporation\NVIDIA Texture Tools\nvtt_export.exe)";

    for (size_t x = 0; x < attachmentTextureList.size(); ++x)
    {
        Texture& importTexture = attachmentTextureList[x];

        const bool isAlbedo = (x == AlbedoAttachment);
        const bool isEmission = (x == EmissionAttachment);

        String textureAttachment;
        switch (x)
        {
        case AlbedoAttachment:         textureAttachment = "AlbedoData";         break;
        case NormalDataAttachment:     textureAttachment = "NormalData";         break;
        case PackedMROAttachment:      textureAttachment = "PackedMROData";      break;
        case PackedSheenSSSAttachment: textureAttachment = "PackedSheenSSSData"; break;
        case UnusedAttachment:         textureAttachment = "UnusedData";         break;
        case EmissionAttachment:       textureAttachment = "EmissionData";       break;
        default:
            fprintf(stderr, "Unknown attachment index %zu\n", x);
            continue;
        }

        const String suffix = GetAttachmentSuffix(x);
        const std::filesystem::path ktxPath = textureName + suffix + ".ktx2";
        const std::filesystem::path previewPngPath = textureName + suffix + ".png";

        if (std::filesystem::exists(ktxPath))
        {
            std::filesystem::remove(ktxPath);
            printf("Removed existing ktx: %s\n", ktxPath.string().c_str());
        }

        ExportToPng(previewPngPath.string(), importTexture, 0, false);
        if (!std::filesystem::exists(previewPngPath) || std::filesystem::file_size(previewPngPath) == 0)
        {
            fprintf(stderr, "ExportToPng failed for %s\n", textureAttachment.c_str());
            continue;
        }

        const uint32 maxDim = std::max(importTexture.texture.TextureSize().x, importTexture.texture.TextureSize().y);
        const uint32 maxMips = static_cast<uint32>(std::floor(std::log2(static_cast<double>(maxDim)))) + 1;
        const uint32 actualMips = (requestedMips == UINT32_MAX) ? maxMips : std::clamp(requestedMips, 1u, maxMips);
        const bool generateMips = (actualMips > 1);

        const char* format = "bc7";
        const char* transfer = isAlbedo ? "srgb" : "linear";
        std::string cmd = fmt::format("\"{}\" \"{}\" -o \"{}\" --format {} --quality production --zcmp 22 --export-transfer-function {}", nvttExe, previewPngPath.string(), ktxPath.string(), format, transfer);
        if (generateMips)
        {
            cmd += fmt::format(" --mips --mip-filter kaiser --max-mip-count {}", actualMips);
            if (isAlbedo) cmd += " --mip-gamma-correct --mip-pre-alpha";
        }
        else
        {
            cmd += " --no-mips --max-mip-count 1";
        }
        printf("Launching NVTT for %s:\n%s\n", textureAttachment.c_str(), cmd.c_str());

        STARTUPINFOA si{};
        si.cb = sizeof(si);
        PROCESS_INFORMATION pi{};
        if (!CreateProcessA(nullptr, cmd.data(), nullptr, nullptr, FALSE, 0, nullptr, nullptr, &si, &pi))
        {
            fprintf(stderr, "CreateProcess failed (%lu): %s\n", GetLastError(), cmd.c_str());
            std::filesystem::remove(previewPngPath);
            continue;
        }

        DWORD waitResult;
        do
        {
            waitResult = WaitForSingleObject(pi.hProcess, 30 * 1000);
            if (waitResult == WAIT_TIMEOUT) printf("Still baking %s...\n", textureAttachment.c_str());
        } while (waitResult == WAIT_TIMEOUT);

        DWORD exitCode = 1;
        GetExitCodeProcess(pi.hProcess, &exitCode);
        CloseHandle(pi.hThread);
        CloseHandle(pi.hProcess);

        std::error_code ec;
        std::filesystem::remove(previewPngPath, ec);

        if (waitResult != WAIT_OBJECT_0 || exitCode != 0 ||
            !std::filesystem::exists(ktxPath) || std::filesystem::file_size(ktxPath) == 0)
        {
            fprintf(stderr, "NVTT failed for %s (wait=%lu exit=%lu)\n", textureAttachment.c_str(), waitResult, exitCode);
            continue;
        }

        printf("Baked %s -> %s\n", textureAttachment.c_str(), ktxPath.string().c_str());
    }
}

void TextureBakerSystem::ExportToPng(const String& fileName, Texture& texture, uint32 mipLevel, bool flipY)
{
    if (mipLevel >= texture.texture.MipMapLevels()) {
        std::cerr << "Invalid mip level " << mipLevel << " (max " << texture.texture.MipMapLevels() << ")\n";
        return;
    }

    VmaAllocator allocator = bufferSystem.VmaAllocatorHandle();

    uint32 width = std::max(1u, static_cast<uint32>(texture.texture.TextureSize().x) >> mipLevel);
    uint32 height = std::max(1u, static_cast<uint32>(texture.texture.TextureSize().y) >> mipLevel);

    bool is16Bit = false;
    bool is32BitFloat = false;
    size_t bytesPerPixel = 4;
    if (texture.texture.TextureByteFormat() == VK_FORMAT_R32G32B32A32_SFLOAT ||
        texture.texture.TextureByteFormat() == VK_FORMAT_R32G32B32A32_UINT ||
        texture.texture.TextureByteFormat() == VK_FORMAT_R32G32B32A32_SINT)
    {
        bytesPerPixel = 16;
        is32BitFloat = true;
        std::cerr << "32-bit float formats not supported for PNG export yet\n";
        return;
    }
    else if (texture.texture.TextureByteFormat() >= VK_FORMAT_R16G16B16A16_UNORM &&
        texture.texture.TextureByteFormat() <= VK_FORMAT_R16G16B16A16_SFLOAT) {
        bytesPerPixel = 8;
        is16Bit = true;
    }

    VkImageMemoryBarrier barrier = {
        .sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER,
        .srcAccessMask = 0,
        .dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT,
        .oldLayout = texture.texture.TextureImageLayout(),
        .newLayout = VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
        .srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
        .dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
        .image = texture.texture.TextureImage(),
        .subresourceRange = {
            .aspectMask = VK_IMAGE_ASPECT_COLOR_BIT,
            .baseMipLevel = mipLevel,
            .levelCount = 1,
            .baseArrayLayer = 0,
            .layerCount = 1
        }
    };

    if (texture.texture.TextureImageLayout() == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL) barrier.srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
    else if (texture.texture.TextureImageLayout() == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL) barrier.srcAccessMask = VK_ACCESS_SHADER_READ_BIT;
    else if (texture.texture.TextureImageLayout() == VK_IMAGE_LAYOUT_GENERAL) barrier.srcAccessMask = VK_ACCESS_SHADER_READ_BIT | VK_ACCESS_SHADER_WRITE_BIT;
    else barrier.srcAccessMask = VK_ACCESS_MEMORY_READ_BIT | VK_ACCESS_MEMORY_WRITE_BIT;

    VkCommandBuffer cmd = vulkan.CommandBuffer().BeginSingleUseCommand();
    vkCmdPipelineBarrier(cmd, VK_PIPELINE_STAGE_ALL_COMMANDS_BIT, VK_PIPELINE_STAGE_TRANSFER_BIT, 0, 0, nullptr, 0, nullptr, 1, &barrier);

    VkBufferCreateInfo bufferInfo = {
        .sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO,
        .size = static_cast<VkDeviceSize>(width) * height * bytesPerPixel,
        .usage = VK_BUFFER_USAGE_TRANSFER_DST_BIT,
        .sharingMode = VK_SHARING_MODE_EXCLUSIVE
    };

    VmaAllocationCreateInfo allocInfo = {
        .flags = VMA_ALLOCATION_CREATE_HOST_ACCESS_RANDOM_BIT | VMA_ALLOCATION_CREATE_MAPPED_BIT,
        .usage = VMA_MEMORY_USAGE_AUTO
    };

    VmaAllocationInfo allocInfoOut{};
    VkBuffer stagingBuffer = VK_NULL_HANDLE;
    VmaAllocation stagingAlloc = VK_NULL_HANDLE;
    VULKAN_THROW_IF_FAIL(vmaCreateBuffer(allocator, &bufferInfo, &allocInfo, &stagingBuffer, &stagingAlloc, &allocInfoOut));

    VkBufferImageCopy region = {
        .bufferOffset = 0,
        .bufferRowLength = 0,
        .bufferImageHeight = 0,
        .imageSubresource = {
            .aspectMask = VK_IMAGE_ASPECT_COLOR_BIT,
            .mipLevel = mipLevel,
            .baseArrayLayer = 0,
            .layerCount = 1
        },
        .imageOffset = {0, 0, 0},
        .imageExtent = {width, height, 1}
    };

    vkCmdCopyImageToBuffer(cmd, texture.texture.TextureImage(), VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
        stagingBuffer, 1, &region);

    vulkan.CommandBuffer().EndSingleUseCommand(cmd);

    void* mapped = allocInfoOut.pMappedData;
    bool needsUnmap = false;
    if (!mapped) {
        VULKAN_THROW_IF_FAIL(vmaMapMemory(allocator, stagingAlloc, &mapped));
        needsUnmap = true;
    }

    Vector<byte> pngData;
    if (!is16Bit)
    {
        // 8-bit RGBA
        Vector<byte> pixels(width * height * 4);

        const byte* src = static_cast<const byte*>(mapped);
        byte* dst = pixels.data();

        for (uint32 y = 0; y < height; ++y)
        {
            uint32 srcY = flipY ? (height - 1 - y) : y;
            const byte* srcRow = src + srcY * width * 4;
            byte* dstRow = dst + y * width * 4;
            memcpy(dstRow, srcRow, width * 4);
        }

        unsigned error = lodepng::encode(pngData, pixels.data(), width, height, LCT_RGBA, 8);
        if (error) std::cerr << "lodepng encode error (8-bit): " << lodepng_error_text(error) << "\n";
    }
    else
    {
        // 16-bit RGBA (PNG big-endian)
        Vector<uint16> pixels(width * height * 4);

        const uint16* src = static_cast<const uint16*>(mapped);
        uint16* dst = pixels.data();

        for (uint32 y = 0; y < height; ++y) {
            uint32 srcY = flipY ? (height - 1 - y) : y;
            const uint16* srcRow = src + srcY * width * 4;
            uint16* dstRow = dst + y * width * 4;

            for (uint32 x = 0; x < width * 4; ++x)
            {
                uint16 val = srcRow[x];
                dstRow[x] = ((val >> 8) & 0xFF) | ((val & 0xFF) << 8);
            }
        }

        unsigned error = lodepng::encode(pngData, reinterpret_cast<unsigned char*>(pixels.data()), width, height, LCT_RGBA, 16);
        if (error) std::cerr << "lodepng encode error (16-bit): " << lodepng_error_text(error) << "\n";
    }

    if (!pngData.empty()) {
        unsigned saveError = lodepng::save_file(pngData, fileName);
        if (saveError) {
            std::cerr << "Failed to save PNG: " << lodepng_error_text(saveError) << "\n";
        }
        else {
            std::cout << "Exported PNG: " << fileName << "\n";
        }
    }

    if (needsUnmap) vmaUnmapMemory(allocator, stagingAlloc);
    vmaDestroyBuffer(allocator, stagingBuffer, stagingAlloc);
}

Vector<byte> TextureBakerSystem::ConvertMipToRGBA8(const void* rawData, size_t rawSize, uint32 width, uint32 height, VkFormat srcFormat)
{
    Vector<byte> rgba8(width * height * 4);

    size_t bytesPerPixel = 4;
    if (srcFormat == VK_FORMAT_R32G32B32A32_SFLOAT ||
        srcFormat == VK_FORMAT_R32G32B32A32_UINT ||
        srcFormat == VK_FORMAT_R32G32B32A32_SINT)
    {
        bytesPerPixel = 16;
    }
    else if (srcFormat >= VK_FORMAT_R16G16B16A16_UNORM &&
        srcFormat <= VK_FORMAT_R16G16B16A16_SFLOAT)
    {
        bytesPerPixel = 8;
    }

    size_t expectedSize = static_cast<size_t>(width) * height * bytesPerPixel;
    if (rawSize != expectedSize)
    {
        fprintf(stderr, "ConvertMipToRGBA8: size mismatch (expected %zu, got %zu)\n", expectedSize, rawSize);
        return {};
    }

    if (bytesPerPixel == 4)
    {
        memcpy(rgba8.data(), rawData, rawSize);
    }
    else if (bytesPerPixel == 8)
    {
        const uint16_t* src16 = static_cast<const uint16_t*>(rawData);
        for (size_t p = 0; p < static_cast<size_t>(width) * height; ++p)
        {
            for (int c = 0; c < 4; ++c)
            {
                uint16_t val = src16[p * 4 + c];
                float norm = 0.0f;

                if (srcFormat == VK_FORMAT_R16G16B16A16_UNORM ||
                    srcFormat == VK_FORMAT_R16G16B16A16_UINT)
                {
                    norm = static_cast<float>(val) / 65535.0f;
                }
                else if (srcFormat == VK_FORMAT_R16G16B16A16_SNORM)
                {
                    int16 s = static_cast<int16_t>(val);
                    norm = std::max(static_cast<float>(s) / 32767.0f, -1.0f);
                    norm = norm * 0.5f + 0.5f;
                }
                else if (srcFormat == VK_FORMAT_R16G16B16A16_SFLOAT)
                {
                    uint32 sign = (val >> 15) & 0x01;
                    uint32 exp = (val >> 10) & 0x1F;
                    uint32 mant = val & 0x3FF;
                    float fval;
                    if (exp == 0) fval = (mant / 1024.0f) * std::pow(2.0f, -14);
                    else if (exp == 31) fval = mant ? NAN : (sign ? -INFINITY : INFINITY);
                    else fval = ((1.0f + mant / 1024.0f) * std::pow(2.0f, exp - 15));

                    if (sign) fval = -fval;
                    norm = std::clamp(fval, 0.0f, 1.0f);
                }

                rgba8[p * 4 + c] = static_cast<byte>(norm * 255.0f + 0.5f);
            }
        }
    }
    else if (bytesPerPixel == 16)
    {
        const float* src32 = static_cast<const float*>(rawData);
        for (size_t p = 0; p < static_cast<size_t>(width) * height; ++p)
        {
            for (int c = 0; c < 4; ++c)
            {
                float val = src32[p * 4 + c];
                val = std::clamp(val, 0.0f, 1.0f);
                rgba8[p * 4 + c] = static_cast<byte>(val * 255.0f + 0.5f);
            }
        }
    }

    return rgba8;
}

RawMipReadback TextureBakerSystem::ConvertToRawTextureData(Texture& importTexture, uint32 mipLevel)
{
    VmaAllocator allocator = bufferSystem.VmaAllocatorHandle();
    uint32 mipWidth = std::max(1u, static_cast<uint32>(importTexture.texture.TextureSize().x) >> mipLevel);
    uint32 mipHeight = std::max(1u, static_cast<uint32>(importTexture.texture.TextureSize().y) >> mipLevel);

    size_t bytesPerPixel = 4;
    if (importTexture.texture.TextureByteFormat() == VK_FORMAT_R32G32B32A32_SFLOAT ||
        importTexture.texture.TextureByteFormat() == VK_FORMAT_R32G32B32A32_UINT ||
        importTexture.texture.TextureByteFormat() == VK_FORMAT_R32G32B32A32_SINT) {
        bytesPerPixel = 16;
    }
    else if (importTexture.texture.TextureByteFormat() >= VK_FORMAT_R16G16B16A16_UNORM &&
        importTexture.texture.TextureByteFormat() <= VK_FORMAT_R16G16B16A16_SFLOAT)
    {
        bytesPerPixel = 8;
    }

    VkBufferCreateInfo bufferInfo =
    {
        .sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO,
        .size = static_cast<VkDeviceSize>(mipWidth) * mipHeight * bytesPerPixel,
        .usage = VK_BUFFER_USAGE_TRANSFER_DST_BIT,
        .sharingMode = VK_SHARING_MODE_EXCLUSIVE
    };

    VmaAllocationCreateInfo allocInfo =
    {
        .flags = VMA_ALLOCATION_CREATE_HOST_ACCESS_RANDOM_BIT | VMA_ALLOCATION_CREATE_MAPPED_BIT,
        .usage = VMA_MEMORY_USAGE_AUTO
    };

    VmaAllocationInfo allocOut{};
    VkBuffer stagingBuffer = VK_NULL_HANDLE;
    VmaAllocation stagingAlloc = VK_NULL_HANDLE;
    VULKAN_THROW_IF_FAIL(vmaCreateBuffer(allocator, &bufferInfo, &allocInfo, &stagingBuffer, &stagingAlloc, &allocOut));

    VkBufferImageCopy region =
    {
        .bufferOffset = 0,
        .bufferRowLength = 0,
        .bufferImageHeight = 0,
        .imageSubresource
            {
                .aspectMask = VK_IMAGE_ASPECT_COLOR_BIT,
                .mipLevel = mipLevel,
                .baseArrayLayer = 0,
                .layerCount = 1,
            },
        .imageOffset = { 0, 0, 0 },
        .imageExtent =
            {
                static_cast<uint32>(mipWidth),
                static_cast<uint32>(mipHeight),
                1
            }
    };

    VkImageMemoryBarrier barrier =
    {
        .sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER,
        .dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT,
        .oldLayout = importTexture.texture.TextureImageLayout(),
        .newLayout = VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
        .srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
        .dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
        .image = importTexture.texture.TextureImage(),
        .subresourceRange =
        {
            .aspectMask = VK_IMAGE_ASPECT_COLOR_BIT,
            .baseMipLevel = mipLevel,
            .levelCount = 1,
            .baseArrayLayer = 0,
            .layerCount = 1,
        }
    };
    if (importTexture.texture.TextureImageLayout() == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL) barrier.srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
    else if (importTexture.texture.TextureImageLayout() == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL) barrier.srcAccessMask = VK_ACCESS_SHADER_READ_BIT;
    else if (importTexture.texture.TextureImageLayout() == VK_IMAGE_LAYOUT_GENERAL) barrier.srcAccessMask = VK_ACCESS_SHADER_READ_BIT | VK_ACCESS_SHADER_WRITE_BIT;
    else barrier.srcAccessMask = VK_ACCESS_MEMORY_READ_BIT | VK_ACCESS_MEMORY_WRITE_BIT;

    VkCommandBuffer command = vulkan.CommandBuffer().BeginSingleUseCommand();
    vkCmdPipelineBarrier(command, VK_PIPELINE_STAGE_ALL_GRAPHICS_BIT, VK_PIPELINE_STAGE_TRANSFER_BIT, 0, 0, nullptr, 0, nullptr, 1, &barrier);
    vkCmdCopyImageToBuffer(command, importTexture.texture.TextureImage(), VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, stagingBuffer, 1, &region);
    vulkan.CommandBuffer().EndSingleUseCommand(command);

    bool needsUnmap = false;
    void* mappedData = allocOut.pMappedData;
    if (!mappedData)
    {
        VULKAN_THROW_IF_FAIL(vmaMapMemory(allocator, stagingAlloc, &mappedData));
        needsUnmap = true;
    }

    return RawMipReadback
    {
        .data = mappedData,
        .size = static_cast<VkDeviceSize>(mipWidth) * mipHeight * bytesPerPixel,
        .buffer = stagingBuffer,
        .allocation = stagingAlloc,
        .needsUnmap = needsUnmap,
    };
}

void TextureBakerSystem::DestroyVMATextureBuffer(RawMipReadback& data)
{
    if (data.needsUnmap)
    {
        vmaUnmapMemory(bufferSystem.VmaAllocatorHandle(), data.allocation);
    }
    vmaDestroyBuffer(bufferSystem.VmaAllocatorHandle(), data.buffer, data.allocation);
}

String TextureBakerSystem::GetAttachmentSuffix(uint x)
{
    String suffix;
    if (x == 0)      suffix = "_Albedo";
    else if (x == 1) suffix = "_NormalHeight";
    else if (x == 2) suffix = "_MRO";
    else if (x == 3) suffix = "_SheenSSS";
    else if (x == 4) suffix = "_Unused";
    else if (x == 5) suffix = "_Emission";
    else             suffix = "_Attachment" + std::to_string(x);
    return suffix;
}