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
    public struct VulkanPipelinePackageLoader
    {
        public Guid PipelinePackageId { get; set; } = new Guid();
        public Guid RenderPassId { get; set; } = new Guid();
        public ivec2 RenderPassResolution { get; set; } = new ivec2();
        public VkRenderPass RenderPass { get; set; } = VulkanCSConst.VK_NULL_HANDLE;
        public VkDescriptorPool GlobalBindlessPool { get; set; } = VulkanCSConst.VK_NULL_HANDLE;
        public VkDescriptorSet GlobalBindlessDescriptorSet { get; set; } = VulkanCSConst.VK_NULL_HANDLE;
        public VkDescriptorSetLayout GlobalBindlessDescriptorSetLayout { get; set; } = VulkanCSConst.VK_NULL_HANDLE;
        public Vector<VkDescriptorImageInfo> RenderPassInputTextures { get; set; } = new Vector<VkDescriptorImageInfo>();
        public Dictionary<PipelineTypeEnum, VulkanPipelineLoader> PipelineMap { get; set; } = new Dictionary<PipelineTypeEnum, VulkanPipelineLoader>();
        public bool UseGlobalBindlessSet { get; set; } = false;
        public bool UseCubeMapMultiview { get; set; } = false;

        public VulkanPipelinePackageLoader()
        {
        }
    }
}
