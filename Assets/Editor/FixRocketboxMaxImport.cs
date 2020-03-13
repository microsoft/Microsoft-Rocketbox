using UnityEngine;
using UnityEditor;

public class FixRocketboxMaxImport : AssetPostprocessor
{
    void OnPostprocessMaterial(Material material)
    {
        // This fixes two problems with importing 3DSMax materials. The first is that the Max materials
        // assumed that diffuse material was set by the texture, whereas Unity multiplies the texture 
        // colour with the flat colour. 
        material.color = Color.white;
        // Second Unity's transparent  materials still show specular highlights and thus hair looks 
        // like glass sheets. The material mode "Fade" goes to full transparent. 
        if (material.GetFloat("_Mode") == 3f)
            material.SetFloat("_Mode", 2f);
    }

    void OnPostprocessMeshHierarchy(GameObject gameObject)
    {
        // This function selects only the highest resolution mesh as being activated by default.
        // You might choose another poly level (they are "hipoly", "midpoly", "lowpoly" and "ultralowpoly")
        // to be selected. Or you could choose not to import, by changing OnPreprocessMeshHierarchy
        if (gameObject.name.ToLower().Contains("poly") &&
            !gameObject.name.ToLower().Contains("hipoly"))
            gameObject.SetActive(false);
    }
    
    void OnPreprocessTexture()
    {
        // This function changes textures that are labelled with "normal" in their title to be loaded as 
        // NormalMaps. This just avoids a warning dialogue box that would otherwise fix it.
        if (assetPath.ToLower().Contains("normal"))
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.textureType = TextureImporterType.NormalMap;
            textureImporter.convertToNormalmap = false;
        }
    }
}

