using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class MovementScript : MonoBehaviour
{
    Collider2D col;
    Rigidbody2D rb;
    Vector2 groundedOrigin;
    Vector2 groundRayDir;
    Vector2 wallRayDir;
    Vector2 newPos;

    bool grounded;
    bool isJumping;
    bool jumpBuffer;
    bool facingRight;   //make sure to initilize to correct direction
    bool hasAirJumped;

    float lastVel;

    int lastIn;
    

    #region timers
    float jAccelTimer;
    float jBufferTimer;
    float accelTimer;
    float cTimer;
    #endregion

    //These properties are grouped in a foldout with a custom editor, 
    //and therefore needs the 'HideInInspector' attribute (to avoid serializing twice)
    #region ground movement
    [HideInInspector, SerializeField]
    float maxSpeed = 14f;
    [HideInInspector, SerializeField,
        Tooltip("How long (in seconds) from holding movement key to reach max acceleration.")]
    float timeToMaxAccel = 0.1f;
    [HideInInspector, SerializeField]
    float maxAcceleration = 7f;
    [HideInInspector, SerializeField,
        Tooltip("How the acceleration curve looks like. Current Acceleration = (Max Acceleration * y-value).\n" +
        "Y-value at x-value 1 is reached after 'Time To Max Accel' seconds.")]
    AnimationCurve accelCurve = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(1, 1f));
    #endregion

    #region air movement
    [HideInInspector, SerializeField,
        Tooltip("NOT IN USE.")]
    float airVelocity;
    [HideInInspector, SerializeField,
        Tooltip("NOT IN USE.")]
    float maxAirSpeed;
    [HideInInspector, SerializeField,
        Min(5),
        Tooltip("Maximum vertical speed.")]
    float maxFallSpeed = 50;    //clamp falling speed
    [HideInInspector, SerializeField, /* Range(0, 1), */
        Tooltip("NOT IN USE. By how much height can the player miss a jump (legs hit top part of platform-side) and still be helped over it.")]
    float catchHeight = -1;  //catch missed jumps, could be a percentage of the player-height
    [HideInInspector, SerializeField, /*[Range(0, 1), */
        Tooltip("NOT IN USE. By how much width can the player be off the collider-side on above platforms while jumping, and still be helped past it.")]
    float bumpedHeadWidth = -1; //bumped head correction, could be percentage of player-width
    #endregion

    #region jumping
    [HideInInspector, SerializeField,
        Tooltip("The amount of upwards-acceleration applied to the player each frame while jumping")]
    float jumpVelocity = 20.5f;
    [HideInInspector, SerializeField,
        Tooltip("Gravity multiplier for short-jump, determines how high a short-jump is."),
        Min(0f)]
    float shortJumpMultiplier = 13; // early fall
    [HideInInspector, SerializeField,
        Tooltip("NOT IN USE."),
        Min(0f)]
    float shortAirJumpMultiplier = 1; // early fall (double jump)
    [HideInInspector, SerializeField,
        Tooltip("Gravity multiplier"),
        Min(0f)]
    float fallMultiplier = 11.3f; // early fall
    [HideInInspector, SerializeField,
        Tooltip("NOT IN USE. How many milliseconds after running of a platform can the player still perform a normal jump."),
        Range(0, 500)]
    float coyoteTime = -1;  //coyote time in ms
    [HideInInspector, SerializeField,
        Tooltip("NOT IN USE. Duration (in milliseconds) before being grounded that the jump-input still registers, and happens when ground is hit."),
        Range(0, 300)]
    float jumpBufferTime = -1;  //jump buffering
    //[HideInInspector, SerializeField,
    //    Tooltip("")]
    //float stickyFeetFriction = -1;  //sticky feet on land, skip this?
    [HideInInspector, SerializeField,
        Tooltip("NOT IN USE. Determines the apex-height of the jump, percentage of max jump-height."),
    Range(0.01f, 1)]
    float apexHeight = 0.9f;   //apex
    [HideInInspector, SerializeField,
        Tooltip("NOT IN USE. Speed of direction change mid-jump. Percentage of normal direction-change."),
    Range(0.01f, 1)]
    float apexSpeed = -1;   //speed apex
    [HideInInspector, SerializeField,
        Tooltip("NOT IN USE. Percentage of normal gravity the player experiences at the apex of jumps. 1 is normal gravity."),
        Range(0.01f, 1)]
    float antiGravityApexMagnitude = -1; // anti gravity apex
    #endregion

    [Header("General Movement")]
    [SerializeField,
        Tooltip("The minimum distance from the ground the collider bottom of the player needs to be to count as grounded.")]
    float groundedOffset = 0.1f;
    [SerializeField]
    float groundRayOriginOffset = 0f;
    [SerializeField,
        Tooltip("The minimum distance from the wall to walljump for example.")]
    float wallOffset = 0.1f;
    [SerializeField]
    float gratvityScale = 5f;
    [SerializeField]
    bool canDoubleJump = true;

    public bool IsFacingRight()
    {
        return facingRight;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;    //smoothes out movement
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;    //prevent rb from moving through/into stuff at high velocities
        rb.freezeRotation = true;   //freeze z-rotation

        //float halvedHeight = col.bounds.extents.y;
        groundRayDir = new Vector2(0, -groundedOffset);
        wallRayDir = new Vector2(wallOffset, 0);

        //init timers
        ResetCoyote();

        //initialize facingRight to correct direction according to graphics
    }

    void Update()
    {
        rb.gravityScale = gratvityScale;    //temp, move to start when value is tweaked enough

        int hInt = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        bool j = Input.GetButtonDown("Jump");                           //record j until jumpBuffer end

        // jumpBuffer &= JumpBuffer(); //jumpBuffer = started buffer AND within buffer-time

        //bool withinCoyote = false;
        //if (!grounded)
        //    withinCoyote = Coyote();    //only count coyote-time while not grounded

        if (j/* || jumpBuffer*/)
        {
            grounded = IsPlayerGrounded();
            if (grounded/* || withinCoyote*/)
            {
                isJumping = true;   //StartJump();
            }
            else    //Tried to jump while airbourne
            {
                if (!hasAirJumped && canDoubleJump)      //"Double jump"
                {
                    hasAirJumped = true;    //reset hasAirJumped when grounded again
                    isJumping = true;
                }
                //    else if (!jumpBuffer)   //Jump-Buffering, cant be triggered by a buffered jump
                //    {
                //        jumpBuffer = true;
                //    }
                //    //StartJumpBuffer();  //Reset buffer-timer every ungrounded jump-input, whether already buffering or not.
            }
        }
        else if (!j)
            isJumping = false;

        Jumping();

        ///Horizontal movement (ground)
        //TODO: use a friction value when adding velocity and stopping:
        //Friction = 1 == normal snappy, full stop on same frame as release of key,
        //Friction = 0 == never stops and cant accelerate. 

        if (hInt != 0 && !IsAtWall(hInt))
        {
            if (lastIn != hInt)     //switched direction
            {
                accelTimer = 0;
                facingRight = !facingRight;
            }

            float currentAccel = GetAcceleration();
            Vector2 addedVelocity = new Vector2(maxSpeed * currentAccel * Time.deltaTime, 0);
            rb.velocity += addedVelocity * hInt;

            if (lastIn == hInt && lastVel > Mathf.Abs(rb.velocity.x))    //didnt switch direction, but somehow decelerated (can happen when landing)
            {
                rb.velocity = new Vector2(lastVel * hInt, rb.velocity.y);
            }
            lastVel = Mathf.Abs(rb.velocity.x);
        }
        else
        {
            accelTimer = 0;
            rb.velocity = new Vector2(0, rb.velocity.y);    //Player instantly stops when not moving horizontally.
                                                            //Need to stop the player using friction or something, cant affect velocity directly.
        }
        lastIn = hInt;

        SpeedClamp();
    }

    private void FixedUpdate()
    {
        grounded = IsPlayerGrounded();  //kind of expensive, but we may have the budget for it. 
    }

    private void SpeedClamp()
    {
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
            rb.velocity = new Vector2(maxSpeed * Mathf.Sign(rb.velocity.x), rb.velocity.y);
        if (Mathf.Abs(rb.velocity.y) > maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed * Mathf.Sign(rb.velocity.y));
    }

    private float GetAcceleration()
    {
        float t;
        if (accelTimer > timeToMaxAccel)
        {
            t = timeToMaxAccel;
        }
        else
        {
            accelTimer += Time.deltaTime;
            t = Mathf.Lerp(0, timeToMaxAccel, accelTimer);
        }
        float currentAccel = accelCurve.Evaluate(t) * maxAcceleration;

#if UNITY_EDITOR
        if (currentAccel > maxAcceleration)
        {
            Debug.LogWarning("Acceleration above maxAccel, make sure accelCurves y-value never exceeds 1.", this);
        }
        else if (currentAccel <= 0)
        {
            Debug.LogError("Make sure the accelerationCurves y-value is always above 0.", this);
            UnityEditor.EditorGUIUtility.PingObject(this);
        }
#endif
        return currentAccel;
    }

    void StartJumpBuffer()
    {
        jBufferTimer = jumpBufferTime * 1000;   //ms * 1000 = seconds
    }

    bool JumpBuffer()
    {
        if (jBufferTimer > 0)
        {
            jBufferTimer -= Time.deltaTime;     //Within buffer-time, record input
            return true;
        }
        else
        {
            jBufferTimer = 0f;      //Buffer-time expired, scrap jump-input
            return false;
        }
    }

    bool Coyote()
    {
        //was the player grounded last frame?
        //if not, start coyote timer
        if (cTimer > 0)
        {
            //still 'grounded, within coyote-time
            cTimer -= Time.deltaTime;
            return true;
        }
        else
        {
            //not grounded, end of coyote time
            //Reset coyote-time when grounded
            return false;
        }
    }

    private void ResetCoyote()  //Called when hitting ground
    {
        cTimer = coyoteTime * 1000;             //ms * 1000 = seconds
    }

    bool IsPlayerGrounded()
    {
        groundedOrigin = new Vector3(transform.position.x, transform.position.y + groundRayOriginOffset);
        Vector2 boxSize = new Vector2(col.bounds.size.x - 0.01f, groundedOffset);
        RaycastHit2D hit = Physics2D.BoxCast(new Vector3(transform.position.x, transform.position.y + groundRayOriginOffset), boxSize, 0, groundRayDir, groundedOffset);

        if (hit.collider != null)
        {
            hasAirJumped = false;
            //ResetCoyote();
            //if hit ground

            if(hit.collider == col)
            {
                Debug.LogError("Grounded raycast hit player-collider, try changing groundRayOriginOffset.", this);

            }

            return true;
        }
        return false;
    }

    bool IsAtWall(int dir)
    {
        Vector3 rayOrigin = transform.position + new Vector3(col.bounds.extents.x * dir, col.bounds.extents.y);
        Vector2 boxSize = new Vector2(wallOffset, col.bounds.size.y);
        RaycastHit2D hit = Physics2D.BoxCast(rayOrigin, boxSize, 0, wallRayDir, wallOffset);

        if (hit.collider != null)
        {            
            if (Mathf.Round(Vector2.Dot(Vector2.left * dir, hit.normal)) == 1)  //check if wall
            {
                return true;
            }
        }
        return false;
    }

#if UNITY_EDITOR    //Redundant?
    private void OnDrawGizmos()
    {
        //Debug movement speed/acceleration
        if (UnityEditor.EditorApplication.isPlaying)    //needed to avoid use of uninitilized local variable when in editor
        {
            Gizmos.color = Color.Lerp(Color.red, Color.blue, Mathf.Abs(rb.velocity.x) / maxSpeed);
            Gizmos.DrawWireSphere(transform.position, Mathf.Abs(rb.velocity.x) / maxSpeed + 0.1f);
        }

        //Debug grounded raycast
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(new Vector3(transform.position.x, transform.position.y + groundRayOriginOffset), groundRayDir);

        if (Application.isPlaying)
        {
            Vector3 rayOrigin = transform.position + new Vector3(col.bounds.extents.x, col.bounds.extents.y);
            Debug.DrawRay(rayOrigin, wallRayDir, Color.blue);
            rayOrigin = transform.position + new Vector3(col.bounds.extents.x * -1, col.bounds.extents.y);
            Debug.DrawRay(rayOrigin, wallRayDir * -1, Color.blue);
        }

    }
#endif

    void Jumping()
    {
        //Draw a jumpCurve directly using this formula? By setting height at time t directly. 
        //This way we easily can set max jump height and have it affect velocity and time, and draw the curve, or change one of the other variables etc. 
        //Vertical Jump Physics Equation:
        //https://sciencing.com/calculate-jump-height-acceleration-8771263.html

        //v0 is jumpVelocity

        if (isJumping)
        {
            rb.velocity = Vector2.up * jumpVelocity;
            isJumping = false;
        }

        if (rb.velocity.y < 0)   //falling
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))  //upwards velocity & realeased jump-button (not max jump)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (shortJumpMultiplier - 1) * Time.deltaTime;
        }
    }

}