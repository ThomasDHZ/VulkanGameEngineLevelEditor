#include "VulkanWindow.h"
#include <iostream>
#include "GameSystem.h"
#include "EngineConfigSystem.h"
#include <ImGuiSystem.h>
#include <DebugSystem.h>
#include <ktx/include/ktx.h>
#include "MaterialMemoryPoolSystem.h"
#include "MaterialBakerSystem.h"

int main(int argc, char** argv)
{
    if (!debugSystem.IsRenderDocInjected()) debugSystem.SetRootDirectory("../Assets");
    
    vulkan.VulkanSetUp(configSystem.WindowResolution, configSystem.RenderResolution);
    bufferSystem.SetUpVmaAllocation();
    memoryPoolSystem.StartUp();
  //  materialBakerSystem.BakeMaterial("AnimeGirlImportMaterial.json");
 //   materialBakerSystem.BakeMaterial("LightIconImportMaterial.json");
  // materialBakerSystem.BakeMaterial("MegaManShotImportMaterial.json");
    materialBakerSystem.BakeMaterial("SparkManTileSetImportMaterial.json");
}