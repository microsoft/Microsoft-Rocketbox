# Import Microsoft Rocketbox avatars into Unreal Engine with material instances

# Requirements: Unreal Engine 4.24 and 4.25
 
# Usage:
 1. Create new empty Unreal project "UnrealProject"
 3. The Microsoft-Rocketbox/Assets folder need to be located three levels up from the UnrealProject/Content folder 
I.e. Microsoft-Rocketbox/Unreal/UnrealProject/Content
 2. Copy "Tool/Unreal/Import/Rocketbox" folder which contains the master material .uasset files to your Unreal project filesystem content folder (UnrealProject/Content/Rocketbox).
    The Python code needs to be directly at Content
    The following folder should then contain the .uasset files: UnrealProject/Content/Rocketbox/Materials/
 3. Open Unreal project and enable "Python" and "Editor Scripting Utilities" Plugins, then restart editor.
 4. Run this Python script with UnrealEditor>File>Execute Python Script
 5. Save all remaining new assets with UnrealEditor>Content Browser>Save All

# Notes:
  + Already existing assets will not be reimported or regenerated unless you delete them from Unreal Editor
  + Tested with Unreal Engine 4.24 on Windows 10

# TODO:
   + Proper use of opacity textures in material (currently fully transparent)


