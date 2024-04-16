using UnityEngine;

/// <summary>
/// Class that handles player movement on the wall as well as exiting the wall
/// </summary>
public class WallRunning : MonoBehaviour
{
    // Variables used to calculate wall running
    [Header("Wallrunning")]
    // Layer which represents the wall
    public LayerMask wall;
    // Layer which represents the ground
    public LayerMask ground;
    // Force applied to the player when wall running
    public float wallRunForce;
    // Force applied to the player when wall jumping
    public float wallJumpUpForce;
    // Force applied to the player when jumping to the side from a wall
    public float wallJumpSideForce;
    // Float representing a speed with which player clims the wall
    public float wallClimbSpeed;
    // Float representing the maximum amount of time player can run on the wall
    public float maxWallRunTime;
    // Float representing the current amount of time spent running on the wall
    private float wallRunTimer;
    // Variables used for wall running input
    [Header("Input")]
    // Key binding for jump action
    public KeyCode jumpKey = KeyCode.Space;
    // Key binding for upwards movement while wallrunning
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    // Key binding for downards movement while wallrunning
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    // Bool representing if player is currently wallruning in the upwards direction
    private bool upwardsRunning;
    // Bool representing if the player is currently wallrunning in the downwards direction
    private bool downwardsRunning;
    // Float for horizontal user input
    private float horizontalInput;
    // Float for vertical user input
    private float verticalInput;
    // Variables used for wall detection before/while wallrunning
    [Header("Detection")]
    // Distance at which the wall is detected
    public float wallCheckDistance;
    // Minimum distance from the ground player needs to start wall running
    public float minJumpHeight;
    // Raycast to check if there is a wall to the left of the player
    private RaycastHit leftWallHit;
    // Rayacst to check if there is a wall to the right of the player
    private RaycastHit rightWallHit;
    // Bool representing if there is a wall on the left
    private bool wallLeft;
    // Bool representin if there is a wall on the right
    private bool wallRight;
    // Variables for wall exiting calculations
    [Header("Exiting")]
    // Bool representing if the player is in the exiting wall state
    private bool exitingWall;
    // Amount of time needed for the player to exit the wall
    public float exitWallTime;
    // Timer used to track the amount of time that player spent in the exiting the wall state
    private float exitWallTimer;
    // Variables used to calculate influence of gravity on wall running
    [Header("Gravity")]
    // Bool representing if gravity is infuencing the player
    public bool useGravity;
    // The amount of gravity influencing the player
    public float gravityCounterForce;
    // Reference to various game objects and scripts used in wall running calculations
    [Header("References")]
    // Transform used in calculating movement direction
    public Transform orientation;
    // Player camera used for camera effects
    public PlayerCam cam;
    // Player Movements script and state machine
    private PlayerMovement pm;
    // Rigid Body used to move the player with force
    private Rigidbody rb;

    /// <summary>
    /// Find Rigid body and Player Movement components
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    /// <summary>
    /// Check for walls on the left and right of the player and update the wall running state machine
    /// </summary>
    void Update()
    {
        CheckForWall();
        StateMachine();
    }

    /// <summary>
    /// Check if player is wall running and if true then start the wall running movement
    /// </summary>
    private void FixedUpdate()
    {
        // Check if player is wall running
        if (pm.wallrunning)
        {
            WallRunningMovement();
        }
    }

    /// <summary>
    /// Check if there is a wall on the right and left of the player by performing Raycasts to the right and left
    /// </summary>
    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, wall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, wall);
    }

    /// <summary>
    /// Check if the player is above ground by perfoming a Raycast under the player
    /// </summary>
    /// <returns> True - if the player is above the ground, False - if the player is on the ground </returns>
    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, ground);
    }

    /// <summary>
    /// Wall running state machine with 3 stages: wallrunning, exiting the wallrun and not wallrunning
    /// </summary>
    private void StateMachine()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        // Check if there is a wall to the left and right of the player and player is trying to move and player is above ground and not ending the wall run
        // If true then check if player is not wallrunning already. If not then start a wall run
        // If player is already wallrunning then check for how long and if it exits the max wall run timer then stop wallrunning
        // Finally, check if player pressed a jump key and wants to exit the wallrun
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            // If player is not wallrunning then start wallrunning
            if (!pm.wallrunning)
            {
                StartWallRun();
            }
            
            // Check if player is already wallrunning and if they are then increase the timer
            if (wallRunTimer > 0)
            {
                wallRunTimer -= Time.deltaTime;
            }

            // Check if the timer has run out and player is wallrunning then stop the wallrunning
            if (wallRunTimer <= 0 && pm.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            // Check if player wants to jump and perform a wall jump
            if (Input.GetKeyDown(jumpKey))
            {
                WallJump();
            }
        }

        // Check if player is exiting the wall and if they are then check if they are wallrunning to stop wallrunning
        // Also check if the exit wall timer is more than 0 which means that the player is exitin the wall and increase the exiting wall timer
        // Finally, if the exiting wall timer is 0 or less then the player is not exiting the wall
        else if (exitingWall)
        {
            // Check if the player is wallrunning and stop wallrunning if they are
            if (pm.wallrunning)
            {
                StopWallRun();
            }

            // If player are already exiting wallrunning then increase the exiting wallrunning timer
            if (exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }

            // If the exit wall timer is less or equal to 0 then the player is not exiting the wall and stop exiting the wall
            if (exitWallTimer <= 0)
            {
                exitingWall = false;
            }
        }

        // If all other states are false then the player is standing still and stop wallrunning
        else
        {
            // If the player is wallrunning then stop wallrunning
            if (pm.wallrunning)
            {
                StopWallRun();
            }
        }
    }

    /// <summary>
    /// Start wallruning by setting the state to wallrunning, reseting the wall run timer, applying additional velocity to the player and tilting the camera depending if wall is on the
    /// left or right of the player.
    /// </summary>
    private void StartWallRun()
    {
        pm.wallrunning = true;

        wallRunTimer = maxWallRunTime;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        cam.DoFov(90f);

        // If the wall is to the left of the player then tilt the camera 5 degrees to the left
        if (wallLeft)
        {
            cam.DoTilt(-5f);
        }

        // If the wall is to the right of the player then tilt the camera five degrees to the right
        if (wallRight)
        {
            cam.DoTilt(5f);
        }
    }

    /// <summary>
    /// While wallrunning apply force to the player depending on the direction
    /// </summary>
    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        // Allows the player to move on curved walls
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude) {
            wallForward = -wallForward; 
        }

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // If player is running up on the wall then adjust force accordingly
        if (upwardsRunning)
        {
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        }

        // If player is running down the wall then adjust force accordingly
        if (downwardsRunning)
        {
            rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);
        }

        // Push the player to the wall on which they are running at the moment
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
        {
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
        }

        // Counter gravity to allow for more arcadey feel
        if (useGravity)
        {
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        }
    }

    /// <summary>
    /// Stop wallrunning by informing the state machine and reseting the camera
    /// </summary>
    private void StopWallRun()
    {
        pm.wallrunning = false;

        cam.DoFov(80f);
        cam.DoTilt(0f);
    }

    /// <summary>
    /// Handle wall jumping while wall running by exiting the wall and applying upwards force to the player
    /// </summary>
    private void WallJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
