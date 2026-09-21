#include "MaterialBakerSystemDLL.h"

void MaterialBakerSystem_BakeMaterial(const char* importMaterialPath, const char* exportMaterialPath)
{
    materialBakerSystem.BakeMaterial(importMaterialPath, exportMaterialPath);
}