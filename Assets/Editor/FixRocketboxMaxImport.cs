using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class FixRocketboxMaxImport : AssetPostprocessor
{
    //  If you plan on using a biped avatar, set this to false to fix the "triangle pelvis/triangle neck" issue.
    private bool _usingMixamoAnimations = true; 
    // Set true if you want the forearm to be split into two bones, which aids twist correction scripts
    // that reduce the mesh deformation at the wrist in inverse kinematics packages like FinalIK or Unity's own.
    bool _twistCorrection = false;
    // Setting to "ModelImporterAnimationType.Human" will generate a human skeleton for the avatar.
    // Setting to generic will skip this step. If using a human avatar, set _usingMixamoAnimations to false
    // for t-pose enforcement.
    ModelImporterAnimationType _animationType = ModelImporterAnimationType.Generic;
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

    void OnPostprocessModel(GameObject g)
    {
        //Make sure this is only applied to the .fbx humanoid avatars
        if (g.transform.Find("Bip02") != null) RenameBip(g);
        if (!assetPath.ToLower().Contains("avatars")) return;
        Transform rootBone = g.transform.Find("Bip01");

        var importer = (ModelImporter)assetImporter;

        if(!_usingMixamoAnimations){
            FixBones(rootBone);
        }
        
        if(_animationType == ModelImporterAnimationType.Human){
            if (g.GetComponent(typeof(Animator)) == null)
            {
                g.AddComponent<Animator>();
            }
            importer.animationType = _animationType;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            var avatarMappings = GenerateAvatarBoneMappings(g);
            importer.humanDescription = avatarMappings;
            var avatar = AvatarBuilder.BuildHumanAvatar(g, avatarMappings);
            g.GetComponent<Animator>().avatar = avatar;

        }
        else
        {
            importer.animationType = _animationType;
        }
        
        if(!_usingMixamoAnimations){
            FixBones(rootBone);
        }
    }

    private void RenameBip(GameObject currentBone)
    {
        currentBone.name = currentBone.name.Replace("Bip02", "Bip01");
        for (int i = 0; i < currentBone.transform.childCount; i++)
        {
            RenameBip(currentBone.transform.GetChild(i).gameObject);
        }

    }
    
    /// <summary>
    /// Updates the transforms of the rocketbox avatar bones to place it in t-pose, including the hands and fingers. If "twistCorrection"
    /// is true, divides the forearm bones into two pieces (enables twist relaxers/corrections, e.g. in FinalIK, to work more effectively).
    /// </summary>
    /// <param name="avatarBase">The root of the avatar hierarchy.</param>
    private void FixBones(Transform avatarBase)
    {
        
        Transform pelvis = avatarBase.Find("Bip01 Pelvis");

        if (pelvis == null) return;

        avatarBase.eulerAngles = new Vector3(-90, 90, 0);
        Transform spine2 = BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 Spine2");

        // Fix the parents of the thigh and clavicle bones if not already done.
        if (spine2.Find("Bip01 L Clavicle") == null)
        {
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Clavicle").SetParent(spine2);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Clavicle").SetParent(spine2);
        }

        if (pelvis.Find("Bip01 L Thigh") == null)
        {
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Thigh").SetParent(pelvis);
            BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Thigh").SetParent(pelvis);
        }
        
        // Ensure t-pose
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Clavicle").localEulerAngles = new Vector3(160, 90, 0);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Clavicle").localEulerAngles = new Vector3(-160, -90, 0);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Clavicle").localPosition = new Vector3(-0.1f, -0.01f, 0.075f);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Clavicle").localPosition = new Vector3(-0.1f, -0.01f, -0.075f);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L UpperArm").localEulerAngles = Vector3.zero;
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R UpperArm").localEulerAngles = Vector3.zero;
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Forearm").localEulerAngles = Vector3.zero;
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Forearm").localEulerAngles = Vector3.zero;
        
        // If _twistCorrection is set to true, split the forearm bones into two pieces. This is needed for the FinalIK twist relaxers to work.
        if (_twistCorrection)
        {
            // If not already made, create the new forearm bones.
            var rWrist = (BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Wrist") == null) ? new GameObject
            {
                name = "Bip01 R Wrist"
            } : BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Wrist").gameObject;
            
            var lWrist = (BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Wrist") == null) ? new GameObject
            {
                name = "Bip01 L Wrist"
            } : BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Wrist").gameObject;
            
            // Parent them to the forearm bone and set their position to halfway between the forearm and hand bones.
            rWrist.transform.SetParent(BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Forearm"));
            rWrist.transform.localPosition = new Vector3(
                -.12f,
                0,
                0);
            rWrist.transform.localEulerAngles = Vector3.zero;
            // Parent the wrist to the hand, and set the hand's position such that it remains the same length from
            // the forearm as it was before.
            var rHand = BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Hand");
            rHand.SetParent(rWrist.transform);
            rHand.localPosition = new Vector3(
                -.12f,
                0,
                0
                );
            
            lWrist.transform.SetParent(BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Forearm"));
            lWrist.transform.localPosition = new Vector3(
                -.12f,
                0,
                0);
            lWrist.transform.localEulerAngles = Vector3.zero;
            var lHand = BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Hand");
            lHand.SetParent(lWrist.transform);
            lHand.localPosition = new Vector3(
                -.12f,
                0,
                0
                );
        }
        
        // Fix the finger bones, adding a very slight curl to the tips
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Hand").localEulerAngles = new Vector3(310, 340, 20);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger0").localEulerAngles =
            assetPath.ToLower().Contains("female") ? new Vector3(87, -31, 8) : new Vector3(55, -31, 8);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger1").localEulerAngles = new Vector3(4, 4, -3);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger2").localEulerAngles = new Vector3(-13, 7, -6);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger3").localEulerAngles = new Vector3(-15, 7, -6);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger4").localEulerAngles = new Vector3(-34, 11, -2);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger01").localEulerAngles = new Vector3(0, 0, -4);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger11").localEulerAngles = new Vector3(0, 0, -4);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger21").localEulerAngles = new Vector3(0, 0, -4);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger31").localEulerAngles = new Vector3(0, 0, -4);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 L Finger41").localEulerAngles = new Vector3(0, 0, -4);


        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Hand").localEulerAngles = new Vector3(50, 20, 20);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger0").localEulerAngles = 
            assetPath.ToLower().Contains("female") ? new Vector3(-87, 31, 8) : new Vector3(-55, 31, 8);;
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger1").localEulerAngles = new Vector3(-4, -4, -3);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger2").localEulerAngles = new Vector3(13, -7, -6);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger3").localEulerAngles = new Vector3(15, -7, -6);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger4").localEulerAngles = new Vector3(34, -11, -2);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger01").localEulerAngles = new Vector3(0, 0, -4);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger11").localEulerAngles = new Vector3(0, 0, -4);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger21").localEulerAngles = new Vector3(0, 0, -4);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger31").localEulerAngles = new Vector3(0, 0, -4);
        BoneUtilities.SearchHierarchyForBone(avatarBase, "Bip01 R Finger41").localEulerAngles = new Vector3(0, 0, -4);
    }


    /// <summary>
    /// Pairs the model's bones with avatar bones and forms a human description. If "twistCorrection" is true,
    /// additionally adds a "wrist bone" to help with mesh deformation upon twisting.
    /// </summary>
    /// <param name="g">The model with bones.</param>
    /// <returns>A mapped HumanDescription to be used in generating an avatar.</returns>
    private HumanDescription GenerateAvatarBoneMappings(GameObject g)
    {
        // Define the bone collection.
        Dictionary<string, string> boneName = new Dictionary<string, string>
        {
            ["Hips"] = "Bip01 Pelvis",
            ["Spine"] = "Bip01 Spine",
            ["Chest"] = "Bip01 Spine1",
            ["UpperChest"] = "Bip01 Spine2",
            ["RightShoulder"] = "Bip01 R Clavicle",
            ["RightUpperArm"] = "Bip01 R UpperArm",
            ["RightLowerArm"] = "Bip01 R Forearm",
            ["RightHand"] = "Bip01 R Hand",
            ["LeftShoulder"] = "Bip01 L Clavicle",
            ["LeftUpperArm"] = "Bip01 L UpperArm",
            ["LeftLowerArm"] = "Bip01 L Forearm",
            ["LeftHand"] = "Bip01 L Hand",
            ["Neck"] = "Bip01 Neck",
            ["Head"] = "Bip01 Head",
            ["Jaw"] = "Bip01 MJaw",
            ["LeftEye"] = "Bip01 LEye",
            ["RightEye"] = "Bip01 REye",
            ["LeftUpperLeg"] = "Bip01 L Thigh",
            ["LeftLowerLeg"] = "Bip01 L Calf",
            ["LeftFoot"] = "Bip01 L Foot",
            ["LeftToes"] = "Bip01 L Toe0",
            ["RightUpperLeg"] = "Bip01 R Thigh",
            ["RightLowerLeg"] = "Bip01 R Calf",
            ["RightFoot"] = "Bip01 R Foot",
            ["RightToes"] = "Bip01 R Toe0",
            ["Left Thumb Proximal"] = "Bip01 L Finger0",
            ["Left Thumb Intermediate"] = "Bip01 L Finger01",
            ["Left Thumb Distal"] = "Bip01 L Finger02",
            ["Left Index Proximal"] = "Bip01 L Finger1",
            ["Left Index Intermediate"] = "Bip01 L Finger11",
            ["Left Index Distal"] = "Bip01 L Finger12",
            ["Left Middle Proximal"] = "Bip01 L Finger2",
            ["Left Middle Intermediate"] = "Bip01 L Finger21",
            ["Left Middle Distal"] = "Bip01 L Finger22",
            ["Left Ring Proximal"] = "Bip01 L Finger3",
            ["Left Ring Intermediate"] = "Bip01 L Finger31",
            ["Left Ring Distal"] = "Bip01 L Finger32",
            ["Left Little Proximal"] = "Bip01 L Finger4",
            ["Left Little Intermediate"] = "Bip01 L Finger41",
            ["Left Little Distal"] = "Bip01 L Finger42",
            ["Right Thumb Proximal"] = "Bip01 R Finger0",
            ["Right Thumb Intermediate"] = "Bip01 R Finger01",
            ["Right Thumb Distal"] = "Bip01 R Finger02",
            ["Right Index Proximal"] = "Bip01 R Finger1",
            ["Right Index Intermediate"] = "Bip01 R Finger11",
            ["Right Index Distal"] = "Bip01 R Finger12",
            ["Right Middle Proximal"] = "Bip01 R Finger2",
            ["Right Middle Intermediate"] = "Bip01 R Finger21",
            ["Right Middle Distal"] = "Bip01 R Finger22",
            ["Right Ring Proximal"] = "Bip01 R Finger3",
            ["Right Ring Intermediate"] = "Bip01 R Finger31",
            ["Right Ring Distal"] = "Bip01 R Finger32",
            ["Right Little Proximal"] = "Bip01 R Finger4",
            ["Right Little Intermediate"] = "Bip01 R Finger41",
            ["Right Little Distal"] = "Bip01 R Finger42"
        };


        string[] humanName = boneName.Keys.ToArray();
        HumanBone[] humanBones = new HumanBone[boneName.Count];


        var rootBoneTransform = g.transform.Find("Bip01");

        var skeletonBones = new List<SkeletonBone>();
        
        var parentObject = new SkeletonBone
        {
            name = g.name,
            position = Vector3.zero,
            rotation = Quaternion.identity,
            scale = g.transform.lossyScale
        };
        var rootBone = new SkeletonBone
        {
            name = "Bip01",
            position = rootBoneTransform.localPosition,
            rotation = Quaternion.Euler(-90, 90, 0),
            scale = rootBoneTransform.lossyScale
        };
        
        skeletonBones.Add(parentObject);
        skeletonBones.Add(rootBone);

        int j = 0;
        int i = 0;
        while (i < humanName.Length)
        {
            if (boneName.ContainsKey(humanName[i]))
            {
                HumanBone humanBone = new HumanBone
                {
                    humanName = humanName[i],
                    boneName = boneName[humanName[i]]
                };
                humanBone.limit.useDefaultValues = true;
                humanBones[j++] = humanBone;
                
                string currentBoneName = boneName[humanName[i]];
                Transform currentBone = BoneUtilities.SearchHierarchyForBone(g.transform, currentBoneName);
                SkeletonBone skeletonBone = new SkeletonBone
                {
                    name = currentBoneName,
                    position = currentBone.localPosition,
                    rotation = currentBone.localRotation,
                    scale = currentBone.lossyScale
                };
                skeletonBones.Add(skeletonBone);
            }
            
            i++;
        }
        
        //Add additional bones for wrist to reduce mesh deformation
        if (BoneUtilities.SearchHierarchyForBone(g.transform, "Bip01 R Wrist") != null & _twistCorrection)
        {
            SkeletonBone rightWrist = new SkeletonBone
            {
                name = "Bip01 R Wrist"
            };
            Transform rightWristTransform = BoneUtilities.SearchHierarchyForBone(g.transform, rightWrist.name);
            rightWrist.position = rightWristTransform.localPosition;
            rightWrist.rotation = rightWristTransform.localRotation;
            rightWrist.scale = BoneUtilities.SearchHierarchyForBone(g.transform, "Bip01 R Forearm").lossyScale;
            skeletonBones.Add(rightWrist);
                
            SkeletonBone leftWrist = new SkeletonBone
            {
                name = "Bip01 L Wrist"
            };
            Transform leftWristTransform = BoneUtilities.SearchHierarchyForBone(g.transform, leftWrist.name);
            leftWrist.position = leftWristTransform.localPosition;
            leftWrist.rotation = leftWristTransform.localRotation;
            leftWrist.scale = BoneUtilities.SearchHierarchyForBone(g.transform, "Bip01 L Forearm").lossyScale;
            skeletonBones.Add(leftWrist);
        }
        // Use the generated HumanBone array and the SkeletonBone list (as array) to complete
        // the HumanDescription and assign it to the AvatarBuilder.
        var humanDescription = new HumanDescription
        {
            human = humanBones,
            hasTranslationDoF = true,
            skeleton = skeletonBones.ToArray()
        };
        return humanDescription;
    }


}


