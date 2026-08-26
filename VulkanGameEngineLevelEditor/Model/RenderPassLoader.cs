using GlmSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VulkanCS;
using VulkanGameEngineLevelEditor.Attributes;

namespace VulkanGameEngineLevelEditor.Model
{
    [Serializable]
    public class RenderPassLoader
    {
        public string Name = string.Empty;
        [ReadOnlyAttribute(true)]
        [DisplayName("Render Pass Id")]
        public Guid RenderPassId { get; set; } = new Guid();
        [DisplayName("Render Pass Resolution")]
        [VisibleWhen(nameof(RenderPassLoader.UseDefaultRenderPassSize), false)]
        public ivec2 RenderPassResolution { get; set; } = new ivec2(0);
        [DisplayName("Attachments")]
        public List<RenderPassAttachmentLoader> AttachmentList { get; set; } = new List<RenderPassAttachmentLoader>();
        [DisplayName("Subpass Dependencies")]
        public List<VkSubpassDependency> SubpassDependencyList { get; set; } = new List<VkSubpassDependency>();
        [DisplayName("Pipelines")]
        public List<VulkanPipelinePackageLoader> PipelinePackageList { get; set; } = new List<VulkanPipelinePackageLoader>();
        [DisplayName("Sub Pass")]
        public List<List<VulkanSubPassLoader>> SubPassList { get; set; } = new List<List<VulkanSubPassLoader>>();
        [DisplayName("Attachment Clear Colors")]
        public List<VkClearValue> ClearValueList { get; set; } = new List<VkClearValue>();
        public VkSampleCountFlagBits SampleCount { get; set; } = VkSampleCountFlagBits.VK_SAMPLE_COUNT_1_BIT;
        [DisplayName("Use Global Bindless Set")]
        [TooltipAttribute("Map descriptor sets to a memory pool in the child pipelines.")]
        public bool UseGlobalBindlessSet { get; set; } = false;
        [DisplayName("Multiview RenderPass")]
        [TooltipAttribute("Render 6 diffrent views at once to create a cubemap")]
        public bool UseVkMultiview { get; set; } = false;
        [TooltipAttribute("Specifies the Vulkan structure type. Must be set to the image creation type.")]
        [DisplayName("Render as cubemap")]
        public bool RenderAsCubemap { get; set; } = false;
        [DisplayName("Use Default Render Pass Size")]
        [TooltipAttribute("True - Use native render pass size. False - Custom render pass size")]
        public bool UseDefaultRenderPassSize { get; set; } = true;
    }
}
