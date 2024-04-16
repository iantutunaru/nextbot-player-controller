using System.Collections;
using UnityEngine;

/// <summary>
/// Class that handles player input, walking, running, jumping and states
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // Movement variables
    [Header("Movement")]
    // Speed of the player after calculations
    private float moveSpeed;
    // Speed of the player when walking
    public float walkSpeed;
    // Speed of the player when sprinting
    public float sprintSpeed; 
    // Speed of the player when sliding
    public float slideSpeed;
    // Float used in calculations of the move speed
    private float desiredMoveSpeed;
    // Float used in mathematical calculations of the move speed
    private float lastDesiredMoveSpeed;
    // Adjustable speed multiplier used in calculations
    public float speedIncreaseMultiplier;
    // Adjustable slope speed multiplier for calculations
    public float slopeIncreaseMultiplier;
    // Speed of the player when wall running
    public float wallRunSpeed;
    // Ground resistance experienced by the player when touching the ground
    public float groundDrag;
    // Float influencing jump height
    public float jumpForce;
    // Float representing how soon the player can jump again
    public float jumpCooldown;
    // Adjustable speed multiplier when player is in the air
    public float airMultiplier;
    // Bool used to signal to the script that player can jump again
    bool readyToJump;
    // Crouching variables
    [Header("Crouching")]
    // Speed of the player when crouching
    public float crouchSpeed;
    // Scale of the player when crouching
    public float crouchYScale;
    // Original scale of the player when spawned
    private float startYScale;
    // Keybinds used by the player input system
    // Note: I decided to use the old input system for practice and to understand legacy code if need be
    [Header("Keybinds")]
    // Binding of the jump key
    public KeyCode jumpKey = KeyCode.Space;
    // Binding of the sprint key
    public KeyCode sprintKey = KeyCode.LeftShift;
    // Binding of the crouch key
    public KeyCode crouchKey = KeyCode.LeftControl;
    // Variables used to check if player is on the ground
    [Header("Ground Check")]
    // Player height used for Raycasting
    public float playerHeight;
    // Layer mask to designate what is considered ground
    public LayerMask ground;
    // Boolean variable to represent if the player is on the ground
    bool grounded;
    // Variables used for movement on the slopes
    [Header("Slope Handling")]
    // Maximus slope angle on which the player can move
    public float maxSlopeAngle;
    // Raycast to check if player is on the slope
    private RaycastHit slopeHit;
    // Boolean representing if player is on the slope or exiting the slope
    private bool exitingSlope;
    // Transform used to calculate the move direction of the player 
    public Transform orientation;
    // Float representing horizontal input of the player
    float horizontalInput;
    // Float representing vertical input of the player
    float verticalInput;
    // Vectore representing the move direction of the player
    Vector3 moveDirection;
    // Rigid body component used to move the player using force
    Rigidbody rb;
    // Bool representing if the player is currently wallrunning
    public bool wallrunning;
    // Bool representing if the palyer is currently sliding
    public bool sliding;

    /// <summary>
    /// Possible states of the first person controller
    /// </summary>
    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        sliding,
        air
    }

    
    /// <summary>
    /// On start find Rigid Body, freeze rotation of the Rigid Body, allow jumping and save starting scale of the player
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    /// <summary>
    /// Check if player is jumping or crouching then check state and adjust drag depending if player is in the air
    /// </summary>
    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, ground);

        MyInput();
        SpeedControl();
        StateHandler();

        // Check if player is on the ground and adjust drag
        if (grounded)
        {
            rb.drag = groundDrag;
        } else
        {
            rb.drag = 0;
        }
            
    }

    /// <summary>
    /// Move player with Rigid Body
    /// </summary>
    private void FixedUpdate()
    {
        MovePlayer();
    }

    /// <summary>
    /// Get input axis's and check if the player wants to jump, start crouching or stop crouching
    /// </summary>
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Check if player wants to jump and is able to jump and is on the ground
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Check if player wants to crouch and start crouching
        // Force is added to make sure that player stays on the ground after they have been scaled down to the crouch size
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // Check if player wants to stop crouching and return their scale to normal
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    /// <summary>
    /// Check the current state and adjust move speed
    /// </summary>
    private void StateHandler()
    {
        // Check if player is wallrunning and adjust move speed
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallRunSpeed;
        }
        // Check if player is sliding and adjust move speed
        else if (sliding)
        {
            state = MovementState.sliding;

            // If player is on the slope and their speed is lower than 0.1f then increase it to sliding speeed
            // Otherwise make it equal to sprint speed
            if (OnSlope() && rb.velocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
            } else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }

        // Check if crouching and adjust move speed
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Check if player is on the ground and sprinting then adjust the move speed
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        // Check if the player is on the ground then adjust move speed to walk speed
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // If none other states are true then the player must be in the air
        else
        {
            state = MovementState.air;
        }

        // If the player suddenly gained a lot of move speed then adjust the speed smoothly
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        } else {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    /// <summary>
    /// Method to adjust player speed gradualy to the desired move speed
    /// </summary>
    /// <returns> Return value is used to go through the IEnumerator to adjust speed over time</returns>
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        // Continue loop while slowly adjusting the speed closer to the desired speed value
        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time/difference);

            // If the player is on slope then increase time by taking slope angle into account
            // This means that by moving downwards on a slope is faster than upwards
            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            } else
            {
                time += Time.deltaTime * speedIncreaseMultiplier;
            }
            
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    /// <summary>
    /// Move the player using Rigid Body depending if they are on the slope, ground, air or wallrunning
    /// </summary>
    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // If the player is on the slope then add a bit of force to keep them tied to a slope and prevent jagged movement
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(20f * moveSpeed * GetSlopeMoveDirection(moveDirection), ForceMode.Force);

            // If player is moving on a slope then add downwards force
            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // If the player is on the ground then adjust move speed to accordingly
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        } 
        
        // If the player is not on the ground then increase speed faster using air multiplier
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // Turn gravity off on slope to prevent jagged movement
        if (!wallrunning)
        {
            rb.useGravity = !OnSlope();
        }
    }

    /// <summary>
    /// Speed control is used to limit speed gain of the player to prevent excessive speed on slopes, ground and air
    /// </summary>
    private void SpeedControl()
    {
        // Check if player is on the slope
        if (OnSlope() && !exitingSlope)
        {
            // Limit velocity by adjusting it with the move speed
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        } 
        
        // If player is not on slope then they are either on the ground or air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // If player velocity on the ground exceeds move speed then adjust velocity accordingly
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    /// <summary>
    /// Move the player up by adding upwards force and set exiting the slope to true
    /// </summary>
    private void Jump()
    {
        exitingSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Used to signal the game that player can jump again
    /// </summary>
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    /// <summary>
    /// Used to check if the player is on a slope by perfoming a Raycast under the player
    /// </summary>
    /// <returns> True - if player is on an uneven ground, False -if Raycast doesn't detect a slope </returns>
    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    /// <summary>
    /// Used to check what direction the player is moving on a slope
    /// </summary>
    /// <param name="direction"> Direction of the player movement </param>
    /// <returns> Vector3 normalized result of a slopeHit </returns>
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
