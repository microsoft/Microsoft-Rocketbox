#
# Import Microsoft Rocketbox avatars into Unreal Engine with material instances
#
# Requirements: Unreal Engine 4.24
# 
# Usage:
# 1. Create new empty Unreal project "RocketboxImport"
# 2. Copy "Rocketbox" folder which contains the master material .uasset files to your Unreal project filesystem content folder (RocketboxImport/Content).
#    The following folder should then contain the .uasset files: RocketboxImport/Content/Rocketbox/Materials/
# 3. Open Unreal project and enable "Python" and "Editor Scripting Utilities" plugins, then restart editor.
# 4. Run this Python script with UnrealEditor>File>Execute Python Script
# 5. Save all remaining new assets with UnrealEditor>Content Browser>Save All
#
# Notes:
#   + Already existing assets will not be reimported or regenerated unless you delete them from Unreal Editor
#   + Tested with Unreal Engine 4.24 on Windows 10
#
# TODO:
#   + Proper use of opacity textures in material (currently fully transparent)
#

import fnmatch
import os
import re
import unreal

rocketbox_root = os.path.normpath(os.path.join(os.path.dirname(__file__), "../../.."))
rocketbox_root_unreal = "/Game/Rocketbox"

def import_textures(rocketbox_root, rocketbox_root_unreal):
    data_root = os.path.join(rocketbox_root, "Assets", "Avatars")

    unreal.log('Importing textures: ' + data_root)

    import_texture_paths = []
    for root, dirnames, filenames in os.walk(data_root):
        for filename in fnmatch.filter(filenames, '*.tga'):
            import_texture_paths.append((root, filename))


    import_tasks = []
    result = []

    for import_texture_dir, import_texture_name in import_texture_paths:
        import_texture_path = os.path.join(import_texture_dir, import_texture_name)

        # Check if texture is already imported
        texture_name = import_texture_name.replace(".tga", "")
        texture_dir = import_texture_dir.replace(data_root, "").replace("\\", "/").lstrip("/")
        texture_dir = "%s/%s" % (rocketbox_root_unreal, texture_dir)
        texture_path = "%s/%s" % (texture_dir, texture_name)

        if unreal.EditorAssetLibrary.does_asset_exist(texture_path):
            unreal.log("  Already imported: " + texture_path)
        else:
            unreal.log("  Importing: " + texture_path)
            task = unreal.AssetImportTask()
            task.set_editor_property("filename", import_texture_path)
            task.set_editor_property("destination_path", texture_dir)
            task.set_editor_property('save', True)
            import_tasks.append(task)

        result.append((texture_name, texture_dir))

    unreal.AssetToolsHelpers.get_asset_tools().import_asset_tasks(import_tasks)
    result.sort()
    return result


def set_texture(material_instance, texture_name, texture_dir):

    texture_path = texture_dir + "/" + texture_name

    unreal.log("    Loading texture: " + texture_path)

    if unreal.EditorAssetLibrary.does_asset_exist(texture_path):
        texture = unreal.EditorAssetLibrary.load_asset(texture_path)
        parameter_name = ""

        if "color" in texture_name:
            parameter_name = "Color"
        elif "normal" in texture_name:
            parameter_name = "Normal"
        elif "specular" in texture_name:
            parameter_name = "Specular"
        else:
            unreal.log_error("Unknown texture extension: " + texture_name)
            return None

        unreal.MaterialEditingLibrary.set_material_instance_texture_parameter_value(material_instance, parameter_name, texture)
        return texture
    else:
        unreal.log_error("Texture not found")
        return None
    return

def load_master_material(data_root, name):
    name = "%s/Materials/%s" % (data_root, name)
    unreal.log("Loading asset: " + name)
    master_material = unreal.EditorAssetLibrary.load_asset("Material'%s'" % name)
    if not master_material:
        unreal.log_error("Cannot load master material: " + name)
        return None
    
    return master_material

def create_materials(data_root, imported_textures_info):

    # Load master material
    master_material = load_master_material(data_root, "M_Rocketbox_Master")
    if master_material is None:
        return

    master_material_opacity = load_master_material(data_root, "M_Rocketbox_Master_Opacity")
    if master_material_opacity is None:
        return

    # Build material dictionary
    materials = {}
    for imported_texture in imported_textures_info:
        (texture_name, texture_dir) = imported_texture

        # Extract material name from texture name
        #   f001_body_color => f001_body
        #   f204_color => f204
        #   sm002_body_color_acu => sm002_body
        match = re.search(r"^(.+)_(col|nor|opa|spec)", texture_name)
        if not match:
            unreal.log_error("Unsupported texture name: " + texture_name)
            exit(1)
        
        material_name = match.group(1)
        if material_name not in materials:
            material_dir = texture_dir.replace("/Textures", "")
            textures = [(texture_name, texture_dir)]
        else:
            (material_dir, textures) = materials[material_name]
            textures.append((texture_name, texture_dir))

        materials[material_name] = (material_dir, textures)            


    # Create material instances
    unreal.log("Creating material instances")
    for material_name in materials:
        (material_dir, textures) = materials[material_name]

        material_path = material_dir + "/" + material_name
        if unreal.EditorAssetLibrary.does_asset_exist(material_path):
            unreal.log("  Already existing: " + material_path)
        else:
            unreal.log("  Creating: " + material_path)

            material_instance = unreal.AssetToolsHelpers.get_asset_tools().create_asset(asset_name=material_name, package_path=material_dir, asset_class=unreal.MaterialInstanceConstant, factory=unreal.MaterialInstanceConstantFactoryNew())

            # Set master material
            if "opacity" in material_name:
                unreal.MaterialEditingLibrary.set_material_instance_parent(material_instance, master_material_opacity)
            else:
                unreal.MaterialEditingLibrary.set_material_instance_parent(material_instance, master_material)
                # Set textures for current material instance
                for (texture_name, texture_dir) in textures:
                    texture = set_texture(material_instance, texture_name, texture_dir)
                    if texture is None:
                        exit(1)

            unreal.EditorAssetLibrary.save_loaded_asset(material_instance)

    return

def import_fbx(data_root):
    data_root = os.path.join(rocketbox_root, "Assets", "Avatars")

    unreal.log('Importing FBX: ' + data_root)

    import_fbx_paths = []
    for root, dirnames, filenames in os.walk(data_root):
        for filename in fnmatch.filter(filenames, '*.fbx'):
            import_fbx_paths.append((root, filename))

    import_tasks = []
    for import_fbx_dir, import_fbx_name in import_fbx_paths:
        import_fbx_path = os.path.join(import_fbx_dir, import_fbx_name)

        # Check if fbx is already imported
        fbx_name = import_fbx_name.replace(".fbx", "")
        fbx_dir = import_fbx_dir.replace(data_root, "").replace("\\", "/").lstrip("/").replace("/Export", "")
        fbx_dir = "%s/%s" % (rocketbox_root_unreal, fbx_dir)
        fbx_path = "%s/%s" % (fbx_dir, fbx_name)

        if unreal.EditorAssetLibrary.does_asset_exist(fbx_path):
            unreal.log("  Already imported: " + fbx_path)
        else:
            unreal.log("  Importing: " + fbx_path)

            fbx_options = unreal.FbxImportUI()
            fbx_options.set_editor_property('import_materials', False)            
            fbx_options.set_editor_property('import_textures', False)

            task = unreal.AssetImportTask()
            task.set_editor_property("automated", True)
            task.set_editor_property("filename", import_fbx_path)
            task.set_editor_property("destination_path", fbx_dir)
            task.set_editor_property("destination_name", "")
            task.set_editor_property("replace_existing", True)
            task.set_editor_property("save", True)
            task.set_editor_property("options", fbx_options)

            import_tasks.append(task)

    unreal.AssetToolsHelpers.get_asset_tools().import_asset_tasks(import_tasks)

######################################################################
# Main
######################################################################
if __name__ == '__main__':        
    unreal.log("============================================================")
    unreal.log("Running: %s" % __file__)
    unreal.log("rocketbox_root: " + rocketbox_root)

    # Import textures into Unreal
    imported_textures_info = import_textures(rocketbox_root, rocketbox_root_unreal)

    # Create material instances for imported textures
    create_materials(rocketbox_root_unreal, imported_textures_info)

    # Import skeletal mesh data
    import_fbx(rocketbox_root_unreal)
