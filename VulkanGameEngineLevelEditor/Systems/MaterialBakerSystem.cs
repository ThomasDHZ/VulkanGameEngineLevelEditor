using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VulkanEngineCoreCS;
using VulkanEngineCS;

namespace VulkanGameEngineLevelEditor.Systems
{
    public unsafe static class MaterialBakerSystem
    {

        public static void BakeMaterial(string importMaterialPath, string exportMaterialPath)
        {
            DLLSystem.CallDLLFunc(() => MaterialBakerSystem_BakeMaterial(importMaterialPath, exportMaterialPath));
        }
        [DllImport("MaterialBakerDLL.dll", CallingConvention = CallingConvention.StdCall)] private static extern void MaterialBakerSystem_BakeMaterial([MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)] string importMaterialPath, [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)] string exportMaterialPath);
    }
}
