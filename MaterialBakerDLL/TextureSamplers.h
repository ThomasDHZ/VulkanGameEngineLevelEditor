#pragma once
#include "DLL.h"
#include <Platform.h>
#include <VulkanSystem.h>

class TextureSamplers
{
public:
    static VkSamplerCreateInfo GetImportAlbedoMapSamplerSettings();
    static VkSamplerCreateInfo GetImportNormalMapSamplerSettings();
    static VkSamplerCreateInfo GetImportPackedORMMapSamplerSettings();
    static VkSamplerCreateInfo GetImportParallaxMapSamplerSettings();
    static VkSamplerCreateInfo GetImportAlphaMapSamplerSettings();
    static VkSamplerCreateInfo GetImportThicknessMapSamplerSettings() { return GetImportPackedORMMapSamplerSettings(); }
    static VkSamplerCreateInfo GetImportSubSurfaceScatteringMapSamplerSettings() { return GetImportPackedORMMapSamplerSettings(); }
    static VkSamplerCreateInfo GetImportSheenMapSamplerSettings() { return GetImportPackedORMMapSamplerSettings(); }
    static VkSamplerCreateInfo GetImportClearCoatMapSamplerSettings() { return GetImportPackedORMMapSamplerSettings(); }
    static VkSamplerCreateInfo GetImportEmissionMapSamplerSettings() { return GetImportAlbedoMapSamplerSettings(); }

    static nlohmann::json GetAlbedoMaterialSamplerSettings(nlohmann::json& json);
    static nlohmann::json GetNormalMaterialSamplerSettings(nlohmann::json& json);
    static nlohmann::json GetMROMaterialSamplerSettings(nlohmann::json& json);
    static nlohmann::json GetSheenSSSSamplerSettings(nlohmann::json& json) { return GetMROMaterialSamplerSettings(json); }
    static nlohmann::json GetUnusedSamplerSettings(nlohmann::json& json) { return GetMROMaterialSamplerSettings(json); }
    static nlohmann::json GetEmissionSamplerSettings(nlohmann::json& json);

    static nlohmann::json GetTiledAlbedoMaterialSamplerSettings(nlohmann::json& json);
    static nlohmann::json GetTiledNormalMaterialSamplerSettings(nlohmann::json& json);
    static nlohmann::json GetTiledMROMaterialSamplerSettings(nlohmann::json& json);
    static nlohmann::json GetTiledSheenSSSSamplerSettings(nlohmann::json& json) { return GetTiledMROMaterialSamplerSettings(json); }
    static nlohmann::json GetTiledUnusedSamplerSettings(nlohmann::json& json) { return GetTiledMROMaterialSamplerSettings(json); }
    static nlohmann::json GetTiledEmissionSamplerSettings(nlohmann::json& json);
};
