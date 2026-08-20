using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulkanGameEngineLevelEditor.Enums
{
    public enum PipelineTypeEnum : uint
    {
        DefaultPipeline,
        DepthPipeline,
        WireFramePipeline,
        ReflectionPipeline,
        CollisionPipeline
    };
}
