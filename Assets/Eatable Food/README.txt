
This file is released under Creative Commons CC BY 4.0

https://creativecommons.org/licenses/by/4.0/

You may copy, distribute, adapt, and use this material for any purpose, with attribution:
Fionna#5639 Disord, @jendaviswilson on Twitter

This prefab is similar to the one created in this tutorial: https://youtu.be/eADLJnd4PMM
This version is slightly different, and set up for ease of use and reliability.

Download the latest VRChat SDK: 
https://vrchat.com/home/download

Download CyanTriggers: 
https://cyanlaser.booth.pm/items/3194594
https://patreon.com/CyanLaser 
Download CyanEmu:
https://github.com/CyanLaser/CyanEmu
Prefab Database:
https://vrcprefabs.com/browse

-----------------
INSTRUCTIONS
-----------------
Before importing this package, please install CyanTriggers and the latest VRChat SDK3 from the above links.

To use this prefab, you need to do the following:

--------------
PlayerTracker
--------------
Drag ONE copy of "EatableFoodPlayerTracker" into the scene. 
 This is a modified copy of the PlayerTracker prefab provided in the CyanTrigger examples.  The position of this object does not matter, as it will follow the local player's camera at runtime.
  You can adjust the position and size of the "PlayerMouth" object in this prefab to change the size or position of the collider on they player's face.  However you must not rename PlayerMouth.

The collider on this object can be any shape or size, but should remain a "trigger" collider.  

-----------
Food Items
-----------
Drag as many copies of "EatableFoodItem" prefab into the scene. They have no visuals in the prefab, you will need to take your own food items and drag them into the object named "Visual--drag your food mesh here". Adjust the position and scale here.

Adjust the size and position of the collider component on the EatableFootItem parent object to match your visuals.  You can delete the sphere and use a box or capsule if you prefer. Note:
- Primitive colliders, not mesh colliders, should only be used.
- If you want your object to use physics, you can uncheck Trigger on the collider and disable Kinematic on the rigidbody.  
- You can put your own sound in the EatSound audio source. 
- You can adjust the particle system in the EatParticles object.


**Be careful when moving the items in the scene!**  Make sure you are moving the EatableFoodItem parent, and not moving the visual mesh itself. It is easy to make this mistake by clicking on the visual in Scene view instead of the object in the hierarchy.

If you are familiar with CyanTrigger, you can extend this prefab to add your own actions to the _Eat and _Respawn events! 




