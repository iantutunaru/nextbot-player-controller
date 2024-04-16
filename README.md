# nextbot-player-controller
The NextBot Player Controller is designed to provide a robust and dynamic movement system for a collaborative game project. It integrates various advanced movement features such as sliding, wall running, and camera controls to enhance the player's interaction with the game environment.

## Components
### PlayerMovement.cs
Handles basic movements like walking, running, and jumping. It relies on Unity's physics components to simulate realistic movements.

**Dependencies:** Unity's CharacterController or Rigidbody.

**Key Features:** Configurable speeds and jump height.

### Sliding.cs
Enables the player to perform a sliding motion, typically used when running and crouching. Adds a dynamic aspect to player movement allowing for evasion or navigating under obstacles.

**Dependencies:** PlayerMovement.cs.

**Key Features:** Sliding duration and cooldown.

### WallRunning.cs
Allows the player to run on walls for a short duration. This script checks for wall proximity and player speed to initiate wall running, enhancing vertical gameplay.

**Dependencies:** PlayerMovement.cs.

**Key Features:** Wall run duration, speed requirement.

### PlayerCam.cs
Manages the first-person camera tied directly to the player’s head movement, providing a more immersive experience.

**Dependencies:** None.

**Key Features:** Camera sensitivity, smoothing.

### MoveCamera.cs
A more general camera script that can be used for broader camera movements and adjustments, providing flexibility in how the game environment is viewed.

**Dependencies:** None.

**Key Features:** Camera range, offset adjustments.

## Setup Instructions
**Environment Setup:** Ensure your Unity project is set up with the latest version of Unity. Import all necessary Unity packages and ensure your project settings are configured for a 3D game environment.

## Configuration:
* Configure all movement parameters (like speed and jump height) in the Unity inspector for PlayerMovement.cs.
* Set up sliding and wall running parameters based on the desired gameplay mechanics.

## Testing:
* Run the game and test all movement features.
* Adjust configurations as needed based on test results.

## Contributing
Your contributions are welcome! Please feel free to submit pull requests or open issues to discuss potential improvements or fixes.

## References
@DaveGameDevelopment - https://www.youtube.com/@davegamedevelopment
