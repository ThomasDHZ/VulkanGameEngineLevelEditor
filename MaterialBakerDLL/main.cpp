#include "VulkanWindow.h"
#include "SystemClock.h"
#include <iostream>
#include "FrameTimer.h"
#include "GameSystem.h"
#include "EngineConfigSystem.h"
#include <ImGuiSystem.h>
#include <DebugSystem.h>
#include <ktx/include/ktx.h>
#include "MaterialMemoryPoolSystem.h"
#include "MaterialBakerSystem.h"

int main(int argc, char** argv)
{
    materialBakerSystem.BakeMaterial("C:\\Users\\DHZ\\Documents\\GitHub\\VulkanGameEngine\\Assets\\ImportMaterials\\AnimeGirlImportMaterial.json", "C:\\Users\\DHZ\\Documents\\GitHub\\VulkanGameEngine\\Assets\\Textures\\AnimeGirlMaterial");
    return 0;
}