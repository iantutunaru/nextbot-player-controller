using UnityEngine;

/// <summary>
/// Class that handles player sliding on the ground and slopes
/// </summary>
public class Sliding : MonoBehaviour
{
    // Reference to other parts of the first person player controller
    [Header("References")]
    // Transfor used to calculate player movement
    public Transform orientation;
    // Player object used for scaling of the player
    public Transform playerObj;
    // Rigid body used to move the player using force
    private Rigidbody rb;
    // Reference to the Player Movement script
    private PlayerMovement pm;
    // Sliding variables
    [Header("Sliding")]
    // Float representing the maximum amount of time that the player can slide
    public float maxSlideTime;
    // Float representing the different amount of force that influences the player when sliding
    public float slideForce;
    // Current amount of time spent sliding
    private float slideTimer;
    // Scale of the player when sliding
    public float slideYScale;
    // Scale of the player before sliding
    private float startYScale;
    // Input variables
    [Header("Input")]
    // Key binding to start sliding
    public KeyCode slideKey = KeyCode.LeftControl;
    // Float for horizontal input
    private float horizontalInput;
    // Float for vertical input
    private float verticalInput;

    /// <summary>
    /// Find Rigid Body and Player Movement and save player scale
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        startYScale = playerObj.localScale.y;
    }

    /// <summary>
    /// Check if player wants to slide or stop sliding
    /// </summary>
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Check if player wants to slide and he is not moving in any direction
        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
        {
            StartSlide();
        }

        // Check if player is sliding and wants to stop
        if (Input.GetKeyUp(slideKey) && pm.sliding)
        {
            StopSlide();
        }
    }

    /// <summary>
    /// Check if player is sliding and if they are then calculate sliding movement and speed
    /// </summary>
    private void FixedUpdate()
    {
        // Check if player is sliding
        if (pm.sliding)
        {
            SlidingMovement();
        }
    }

    /// <summary>
    /// Initiate sliding by changing the state in the state machine, scaling down the player and adding downwards force. Additionaly, reset the slide time
    /// </summary>
    private void StartSlide()
    {
        pm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    /// <summary>
    /// Calculate the slide speed depending if the player is on the slope and movement direction
    /// </summary>
    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Add force and decrease the slide timer if the player is not on the slope or if their velocity is more than -0.1f
        if (!pm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        // If the player is on the slope then just add force in the input direction
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }
        
        // If the slide timer runs out then stop sliding
        if (slideTimer < 0 )
        {
            StopSlide();
        }
    }

    /// <summary>
    /// When player stops sliding inform the state machine and reset player scale to default starting scale
    /// </summary>
    private void StopSlide()
    {
        pm.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}