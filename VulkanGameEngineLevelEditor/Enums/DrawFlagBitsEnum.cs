using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulkanGameEngineLevelEditor.Enums
{
    public enum DrawFlagBits : UInt64
    {
        kDrawFlag_DrawDefault = 1 << 0,
        kDrawFlag_DrawMesh = 1 << 1,
        kDrawFlag_DrawInstance = 1 << 2,
        kDrawFlag_DrawLine = 1 << 3,
        kDrawFlag_DrawCubeMap = 1 << 4,
        kDrawFlag_DrawDebugObjects = 1 << 5,
        kDrawFlag_DrawShadow = 1 << 6,
        kDrawFlag_DrawRenderAttachment = 1 << 7
    };
}
