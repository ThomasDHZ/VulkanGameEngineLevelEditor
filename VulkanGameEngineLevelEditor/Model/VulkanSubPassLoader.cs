using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VulkanEngineCoreCS.Models;

namespace VulkanGameEngineLevelEditor.Model
{
    public struct VulkanSubPassLoader
    {
        public Guid PipelinePackageGuid { get; set; } = Guid.Empty;
        public MeshTypeEnum MeshType { get; set; } = MeshTypeEnum.kMesh_Undefined;
        public string? ShaderPushConstant { get; set; } = string.Empty;
        public List<PushConstantUpdateRule> PushConstantUpdates { get; set; } = new List<PushConstantUpdateRule>();
        public List<Guid> InputTextureList { get; set; } = new List<Guid>();
        public List<Guid> OutputTextureList { get; set; } = new List<Guid>();
        public bool OffScreenRenderPass { get; set; } = false;

        public VulkanSubPassLoader()
        {
        }
    }
}
