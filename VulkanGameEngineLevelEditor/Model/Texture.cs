using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VulkanCS;
using VulkanGameEngineLevelEditor.Enums;

namespace VulkanGameEngineLevelEditor.Model
{
    public class Texture
    {
        public Guid TextureId { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        List<string> TextureFilePaths { get; set; } = new List<string>();
        public TextureTypeEnum TextureType { get; set; } = TextureTypeEnum.kTextureType_Undefined;
        public TextureUsageTypeEnum TextureUsageType { get; set; } = TextureUsageTypeEnum.kUsageType_Undefined;
        public VkImageType ImageType { get; set; } = VkImageType.VK_IMAGE_TYPE_2D;
        public VkSamplerCreateInfo SamplerCreateInfo { get; set; } = new VkSamplerCreateInfo();
        public bool IsRenderPassAttachment { get; set; } = false;
        public bool IsCubeMap {  get; set; } = false;
        public bool UseMipMaps { get; set; } = false;

    }
}
