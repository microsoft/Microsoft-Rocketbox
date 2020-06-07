## MICROSOFT ROCKETBOX AVATAR LIBRARY
The Microsoft Rocketbox Avatar library consists of 115 characters and avatars fully rigged and with high definition that was developed over the course of 10 years. The diversity of the characters and the quality of the rigging together with a relatively low-poly meshes, makes this library the go-to asset among research laboratories worldwide from crowd simulation to real-time avatar embodiment and social Virtual Reality (VR). Ever since their launch, laboratories around the globe have been using the library and many of the lead authors in the VR community have extensively used these avatars during their research.

The announcement about the release here> https://www.microsoft.com/en-us/research/blog/microsoft-rocketbox-avatar-library-now-available-for-research-and-academic-use/

[![AvatarsSample](Docs/AvatarsSample.jpg?raw=true)](https://www.youtube.com/watch?v=43OWnUQH_p4)
https://www.youtube.com/watch?v=43OWnUQH_p4

## Setup
The FixRocketboxMaxImport.cs script under “Assets/Editor” needs to go in “Assets/Editor”  in the Unity project. This will fix the import of the 3dsMax materials to Unity. I.e. Max materials assume that diffuse material was set by the texture, whereas Unity multiplies the texture colour with the flat colour. Second Unity's transparent  materials still show specular highlights and thus hair looks like glass sheets. The material mode "Fade" goes to full transparent. The import tool also selects  the highest resolution mesh as being activated by default.
By editing this file you might choose another poly level (they are "hipoly", "midpoly", "lowpoly" and "ultralowpoly") Or you could choose not to import by changing OnPreprocessMeshHierarchy.

The FixRocketboxMaxImport was first contributed by Prof. Anthony Steed from University College London. 
In a more recent version we have updated the import tool to set the avatars into T-pose and reorganize the bones to optimize for the Unity humanoid rig.
 
## Running the sample

Import the desired avatar folder (including fbx files and textures of the avatar) to your unity project "Assets" folder.

Once the files are on the correct unity folders project you can open Unity and include the avatar to the scene.

## Documentation

This release goes together with a paper that is in preparation that highlights and reviews the research done with rigged avatars as well as explains the importance of having rigged avatars for Virtual Reality.

Mar Gonzalez-Franco, Eyal Ofek, Ye Pan,  Angus Antley, Anthony Steed, Bernhard Spanlang,  Antonella Maselli, Domna Banakou, Nuria Pelechano, Sergio Orts Escolano, Veronica Orvahlo, Laura Trutoiu, Markus Wojcik, Maria V. Sanchez-Vives, Jeremy Bailenson, Mel Slater, and Jaron Lanier "Importance of rigging for procedural avatars. Microsoft Rocketbox a public library."



## Contributors

Mar Gonzalez-Franco - Microsoft Research

Markus Wojcik - Rocketbox (Original avatar creation team)

Eyal Ofek - Microsoft Research

Anthony Steed - University College London, (Visiting Researcher at Microsoft Research when this was conceived)

Dave Garagan - Havok
