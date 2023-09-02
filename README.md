An easy example using Job System, for a pathfinding 2D in a 3D Game (x,z)

[ GridManager ] Generates a grid of variable sizes, and scale.

Then Check if this point collides with the LayerMask and mark it unabled. And expose a Native Array with all the points, so it can be used by other classes.

[ Agent ] Patrol the Waypoints, and if anyone finds the player Alert the others and everyone goes to the point where it was seen. If they lose the trail they will return to patrol.

[ Path2D ] Executes a Job with Burst Compile that generates a path to the target point, Using the native array of the GridManager.

[ WaypointList ] To Make easy to the Level Design, you can simply put the gameobjects and the script will generate the list in the desired order. And rename them accordingly. 
