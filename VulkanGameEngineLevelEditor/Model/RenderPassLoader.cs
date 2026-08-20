using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VulkanCS;

namespace VulkanGameEngineLevelEditor.Model
{
    public class RenderPassLoader
    {
        public Guid RenderPassId { get; set; } = new Guid();
        public ivec2 RenderPassResolution { get; set; } = new ivec2(0);
        public List<RenderPassAttachmentLoader> AttachmentList { get; set; } = new List<RenderPassAttachmentLoader>();
        public List<VkSubpassDependency> SubpassDependencyList { get; set; } = new List<VkSubpassDependency>();
        public List<VulkanPipelinePackageLoader> PipelinePackageList { get; set; } = new List<VulkanPipelinePackageLoader>();
        public List<List<VulkanSubPassLoader>> SubPassList { get; set; } = new List<List<VulkanSubPassLoader>>();
        public List<VkClearValue> ClearValueList { get; set; } = new List<VkClearValue>();
        public VkSampleCountFlagBits SampleCount { get; set; } = VkSampleCountFlagBits.VK_SAMPLE_COUNT_1_BIT;
        public bool UseGlobalBindlessSet { get; set; } = false;
        public bool UseCubeMapMultiView { get; set; } = false;
        public bool IsCubeMapRenderPass { get; set; } = false;
    }
}
