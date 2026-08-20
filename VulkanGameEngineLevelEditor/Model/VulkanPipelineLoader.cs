using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VulkanCS;

namespace VulkanGameEngineLevelEditor.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct VulkanPipelineLoader
    {
        public string Name = string.Empty;
        public Guid PipelineId { get; set; } = new Guid();
        public Guid RenderPassId { get; set; } = new Guid();
        public Guid LevelId { get; set; } = new Guid();
        public uint SubPassId { get; set; } = uint.MaxValue;
        public uint BindlessDescriptorSetIndex { get; set; } = uint.MaxValue;
        public ivec2 RenderPassResolution { get; set; } = new ivec2();
        public VkRenderPass RenderPass { get; set; } = VulkanCSConst.VK_NULL_HANDLE;
        public VkDescriptorPool GlobalBindlessPool { get; set; } = VulkanCSConst.VK_NULL_HANDLE;
        public VkDescriptorSet GlobalBindlessDescriptorSet { get; set; } = VulkanCSConst.VK_NULL_HANDLE;
        public VkDescriptorSetLayout GlobalBindlessDescriptorSetLayout { get; set; } = VulkanCSConst.VK_NULL_HANDLE;
        List<VkDescriptorImageInfo> RenderPassInputTextures { get; set; } = new List<VkDescriptorImageInfo>();
        List<VkViewport> ViewportList { get; set; } = new List<VkViewport>();
        List<VkRect2D> ScissorList { get; set; } = new List<VkRect2D>();
        List<ShaderLoader> ShaderLoaderList { get; set; } = new List<ShaderLoader>();
        List<VkPipelineColorBlendAttachmentState> PipelineColorBlendAttachmentStateList { get; set; } = new List<VkPipelineColorBlendAttachmentState>();
        public VkPipelineInputAssemblyStateCreateInfo PipelineInputAssemblyStateCreateInfo { get; set; } = new VkPipelineInputAssemblyStateCreateInfo();
        public VkPipelineRasterizationStateCreateInfo PipelineRasterizationStateCreateInfo { get; set; } = new VkPipelineRasterizationStateCreateInfo();
        public VkPipelineMultisampleStateCreateInfo PipelineMultisampleStateCreateInfo { get; set; } = new VkPipelineMultisampleStateCreateInfo();
        public VkPipelineDepthStencilStateCreateInfo PipelineDepthStencilStateCreateInfo { get; set; } = new VkPipelineDepthStencilStateCreateInfo();
        public VkPipelineColorBlendStateCreateInfo PipelineColorBlendStateCreateInfoModel { get; set; } = new VkPipelineColorBlendStateCreateInfo();
        public bool UseGlobalBindlessSet { get; set; } = false;
        public bool UseDynamicColorWrite { get; set; } = false;
        public bool UseCubeMapMultiview { get; set; } = false;
        public VulkanPipelineLoader()
        {
        }
    }
}
