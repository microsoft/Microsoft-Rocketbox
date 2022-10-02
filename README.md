## MICROSOFT ROCKETBOX AVATAR LIBRARY
The Microsoft Rocketbox Avatar library consists of 115 characters and avatars fully rigged and with high definition that was developed over the course of 10 years. The diversity of the characters and the quality of the rigging together with a relatively low-poly meshes, makes this library the go-to asset among research laboratories worldwide from crowd simulation to real-time avatar embodiment and social Virtual Reality (VR). Ever since their launch, laboratories around the globe have been using the library and many of the lead authors in the VR community have extensively used these avatars during their research.

The announcement about the release here> https://www.microsoft.com/en-us/research/blog/microsoft-rocketbox-avatar-library-now-available-for-research-and-academic-use/

This release goes together with a paper that highlights and documents the creation of the library and reviews the research done with rigged avatars as well as explains the importance of having rigged avatars for Virtual Reality. If you are using this library for research you should consider citing it.

_Mar Gonzalez-Franco, Eyal Ofek, Ye Pan,  Angus Antley, Anthony Steed, Bernhard Spanlang,  Antonella Maselli, Domna Banakou, Nuria Pelechano, Sergio Orts Escolano, Veronica Orvahlo, Laura Trutoiu, Markus Wojcik, Maria V. Sanchez-Vives, Jeremy Bailenson, Mel Slater, and Jaron Lanier 
**"The Rocketbox library and the utility of freely available rigged avatars."** Frontiers in Virtual Reality DOI: [10.3389/frvir.2020.561558](https://www.frontiersin.org/articles/10.3389/frvir.2020.561558/abstract)_

[![AvatarsSample](Docs/AvatarsSample.jpg?raw=true)](https://www.youtube.com/watch?v=43OWnUQH_p4)
https://www.youtube.com/watch?v=43OWnUQH_p4

## UPDATES
6/2022: Release of ARkit compatible blendshapes. Contribution by Fang Ma and Matias Volonte

4/2022: Release of 417 animations compatible with the Microsoft Rocketbox Avatars (in Assets/Animations)

3/2022: Release of a new library: the Headbox to do facial blendshape animations on the avatars https://github.com/openVRlab/Headbox

2/2022: Release of avatars with facial blendshapes (15 visemes, 48 FACS, 30 for the Vive facial tracker).

2/2021: Added Unreal batch importer tool. Contribution by Joachim Tesch

12/2020: Release of a new library: the Movebox for Microsoft Rocketbox to animate the avatars https://github.com/microsoft/MoveBox-for-Microsoft-Rocketbox

12/2020: Updated license to MIT. 
## Setup
The FixRocketboxMaxImport.cs script under “Assets/Editor” needs to go in “Assets/Editor”  in the Unity project. This will fix the import of the 3dsMax materials to Unity. I.e. Max materials assume that diffuse material was set by the texture, whereas Unity multiplies the texture colour with the flat colour. Second Unity's transparent  materials still show specular highlights and thus hair looks like glass sheets. The material mode "Fade" goes to full transparent. The import tool also selects  the highest resolution mesh as being activated by default.
By editing this file you might choose another poly level (they are "hipoly", "midpoly", "lowpoly" and "ultralowpoly") Or you could choose not to import by changing OnPreprocessMeshHierarchy.

The FixRocketboxMaxImport was first contributed by Prof. Anthony Steed from University College London. 
In a more recent version we have updated the import tool to set the avatars into T-pose and reorganize the bones to optimize for the Unity humanoid rig.
 
## Running the sample

Import the desired avatar folder (including fbx files and textures of the avatar) to your unity project "Assets" folder.

Once the files are on the correct unity folders project you can open Unity and include the avatar to the scene.



## Contributors

Mar Gonzalez-Franco - Microsoft Research

Markus Wojcik - Rocketbox (Original avatar creation team)

Eyal Ofek - Microsoft Research

Anthony Steed - University College London, (Visiting Researcher at Microsoft Research when this repo was conceived)

Dave Garagan - Havok

Matias Volonte - Northeastern University & Clemenson University (Creation of the blendshapes, and the headbox tool to transfer blendshapes procedurally between avatars)

Fang Ma - Goldsmith University, Creation of blendshapes for Vive Tracker and ARkit

## November 2020 License Update

The library of avatars is now released under MIT License.


## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

