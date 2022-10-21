using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoneUtilities {

/// <summary>
/// Recursively searches a transform hierarchy for a bone and returns it.
/// </summary>
/// <param name="current">The root of the bone hierarchy (or any transform above the bone)</param>
/// <param name="name">The name of the bone, e.g. "Bip01 R Forearm"</param>
/// <returns>The transform of the bone if found, otherwise null.</returns>
    public static Transform SearchHierarchyForBone(Transform current, string name)   
    {
        // Check if the current bone is the bone we're looking for, returning it if so.
        if (current.name == name)
            return current;
        // Search through the current transform's children for the bone.
        for (int i = 0; i < current.childCount; ++i)
        {
            // The recursive step; repeat the search one step deeper in the hierarchy
            Transform found = SearchHierarchyForBone(current.GetChild(i), name);
            // If the bone was found, return it.
            if (found != null)
                return found;
        }
    
        // Bone was not found.
        return null;
    }
}
