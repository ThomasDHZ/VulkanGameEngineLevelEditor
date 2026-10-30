using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VulkanEngineCoreCS.Models;
using VulkanEngineCoreCS.Models.VulkanEngineCS;
using VulkanEngineCS;
using VulkanGameEngineLevelEditor.Enums;

namespace VulkanGameEngineLevelEditor.Model
{
    public struct VulkanSubPassLoader
    {
        public Guid PipelinePackageGuid { get; set; } = Guid.Empty;
        public MeshTypeEnum MeshType { get; set; } = MeshTypeEnum.kMesh_Undefined;
        public DrawFlagBits RenderPassDrawFlags;
        public string? ShaderPushConstant { get; set; } = string.Empty;
        public List<PushConstantUpdateRule> PushConstantUpdates { get; set; } = new List<PushConstantUpdateRule>();
        public List<Guid> InputAttachmentList;
        public List<Guid> InputTextureList { get; set; } = new List<Guid>();
        public List<Guid> OutputTextureList { get; set; } = new List<Guid>();
        public String CustomDrawCommand;
        public CustomDrawInitContext CustomDrawCommandContext;
        public bool OffScreenRenderPass { get; set; } = false;
        public bool UseCustomDrawCommand = false;
        public bool AlphaRenderPass = false;

        public VulkanSubPassLoader()
        {
        }
    }
}
