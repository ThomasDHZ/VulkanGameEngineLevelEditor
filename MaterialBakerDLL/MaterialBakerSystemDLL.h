#pragma once
#include "MaterialBakerSystem.h"

#ifdef __cplusplus
extern "C" {
#endif
	DLL_EXPORT void MaterialBakerSystem_BakeMaterial(const char* importMaterialPath, const char* exportMaterialPath);
#ifdef __cplusplus
}
#endif
