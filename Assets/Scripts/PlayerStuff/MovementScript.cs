using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class MovementScript : MonoBehaviour
{
    Collider2D col;
    Rigidbody2D rb;
    Vector2 groundRayDir;

    bool grounded;
    bool isJumping;
    bool jumpBuffer;
    bool facingRight;   //make sure to initilize to correct direction
    bool hasAirJumped;
    bool acceleratedJumpLastFrame;
    float heightBeforeJump;
    float lastInput;

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
    float maxSpeed = 10f;
    [HideInInspector, SerializeField,
        Tooltip("How long (in seconds) from holding movement key to reach max acceleration.")]
    float timeToMaxAccel = 0.1f;
    [HideInInspector, SerializeField]
    float maxAcceleration = 150f;
    [HideInInspector, SerializeField,
        Tooltip("How the acceleration curve looks like. Current Acceleration = (Max Acceleration * y-value).\n" +
        "Y-value at x-value 1 is reached after 'Time To Max Accel' seconds.")]
    AnimationCurve accelCurve = new AnimationCurve(new Keyframe(0, 0.7f), new Keyframe(1, 1));
    #endregion

    #region air movement
    [HideInInspector, SerializeField]
    float airVelocity;
    [HideInInspector, SerializeField]
    float maxAirSpeed;
    [HideInInspector, SerializeField,
        Tooltip("Maximum fallspeed.")]
    float maxFallSpeed = -1;    //clamp falling speed
    [HideInInspector, SerializeField, /* Range(0, 1), */
        Tooltip("By how much height can the player miss a jump (legs hit top part of platform-side) and still be helped over it.")]
    float catchHeight = -1;  //catch missed jumps, could be a percentage of the player-height
    [HideInInspector, SerializeField, /*[Range(0, 1), */
        Tooltip("By how much width can the player be off the collider-side on above platforms while jumping, and still be helped past it.")]
    float bumpedHeadWidth = -1; //bumped head correction, could be percentage of player-width
    #endregion

    #region jumping
    [HideInInspector, SerializeField,
        Tooltip("The amount of upwards-acceleration applied to the player each frame while jumping")]
    float jumpAccel = 5f;
    [HideInInspector, SerializeField,
        Tooltip("The height of jump when tapping the jump button (while grounded), opposed to holding it until apex is reached.")]
    float minJumpHeight = -1; // early fall
    [HideInInspector, SerializeField,
        Tooltip("The height of jump when tapping the jump button (while airbourne), opposed to holding it until apex is reached.")]
    float minAirJumpHeight = -1; // early fall
    [HideInInspector, SerializeField,
        Tooltip("How many milliseconds after running of a platform can the player still perform a normal jump."),
        Range(0, 500)]
    float coyoteTime = -1;  //coyote time in ms
    [HideInInspector, SerializeField,
        Tooltip("Duration (in milliseconds) before being grounded that the jump-input still registers, and happens when ground is hit."),
        Range(0, 300)]
    float jumpBufferTime = -1;  //jump buffering
    //[HideInInspector, SerializeField,
    //    Tooltip("")]
    //float stickyFeetFriction = -1;  //sticky feet on land, skip this?
    [HideInInspector, SerializeField,
        Tooltip("Speed of direction change mid-jump. Percentage of normal direction-change."),
    Range(0.01f, 1)]
    float apexSpeed = -1;   //speed apex
    [HideInInspector, SerializeField,
        Tooltip("Percentage of normal gravity the player experiences at the apex of jumps. 1 is normal gravity."),
        Range(0.01f, 1)]
    float antiGravityApexMagnitude = -1; // anti gravity apex
    #endregion

    [Header("General Movement")]
    [SerializeField,
        Tooltip("The minimum distance from the ground the collider bottom of the player needs to be to count as grounded.")]
    float groundedOffset = 0.1f;

    public bool IsFacingRight()
    {
        return facingRight;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;    //smoothes out movement

        //float halvedHeight = col.bounds.extents.y;
        groundRayDir = new Vector2(0, -groundedOffset);

        //init timers
        ResetCoyote();

    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        //record j until jumpBuffer end
        bool j = Input.GetButton("Jump");

        // jumpBuffer &= JumpBuffer(); //jumpBuffer = started buffer AND within buffer-time

        //bool withinCoyote = false;
        //if (!grounded)
        //    withinCoyote = Coyote();    //only count coyote-time while not grounded

        if (j/* || jumpBuffer*/)
        {
            grounded = IsPlayerGrounded();  //Only raycast when player tries to jump
            if (grounded/* || withinCoyote*/)
            {
                isJumping = true;   //StartJump();
            }
            //else    //Tried to jump while airbourne
            //{
            //    if (!hasAirJumped)      //"Double jump"
            //    {
            //        //remember to reset hasAirJumped after touching ground again (grounded)
            //        hasAirJumped = true;    //StartAirJump();
            //    }
            //    else if (!jumpBuffer)   //Jump-Buffering, cant be triggered by a buffered jump
            //    {
            //        jumpBuffer = true;
            //    }
            //    //StartJumpBuffer();  //Reset buffer-timer every ungrounded jump-input, whether already buffering or not.
            //}
        }

        Jumping();


        if (!Mathf.Approximately(h, 0))
        {
            float currentAccel = GetAcceleration();
            Vector2 addedVelocity = new Vector2(maxSpeed * currentAccel * Time.deltaTime, 0);

            if (Mathf.Abs(rb.velocity.x) + addedVelocity.x >= maxSpeed)
            {
                print("max");
                rb.velocity = new Vector2(maxSpeed * h, rb.velocity.y);
            }
            else
            {
                //Vector2 moveForce = new Vector2(h, 0) * accel * rb.mass;    //F = m * a 
                //rb.AddForce(moveForce * 100 * Time.deltaTime);              //mult by 100 to cancel out deltaTime, to avoid having large numbers for acceleration.
                //facingRight = rb.velocity.x > 0 ? true : false;

                rb.velocity += addedVelocity * h;

                lastInput = h;
                print("accel");
            }

            if (lastInput > h)   //Decelerating or switch of direction
            {
                if (lastInput > 0 && h < 0)     //Switched direction
                {
                    rb.velocity = new Vector2(0, rb.velocity.y);
                }
                else
                {

                }
            }
            else if (lastInput < h)
            {
                if (lastInput < 0 && h > 0)
                {
                    rb.velocity = new Vector2(0, rb.velocity.y);
                }
            }

        }
        else
        {
            accelTimer = 0;
            rb.velocity = new Vector2(0, rb.velocity.y);    //Player instantly stops when not moving in horizontally.
        }

        /*
        //horizontal movement
        if grounded
            ground-movement
        else
            air-movement
        */
    }

    private float GetAcceleration()
    {
        float t;
        if (accelTimer > timeToMaxAccel)
        {
            t = 1;
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

        Ray2D r = new Ray2D(transform.position, groundRayDir); //use -normal of ground instead of v.down for slopes?

        /*                                      
        groundRayLength
        if raycast hit ground
            hasAirJumped = false;
            ResetCoyote();
            return true;

        else        //coyote time
            start coyote-timer
            if !on timer-end: return true;
            on timer-end: grounded = false
            */

        return true;    //temp
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
        Gizmos.DrawRay(transform.position, groundRayDir);
    }
#endif

    void Jumping()
    {

        if (isJumping)
        {
            if (!acceleratedJumpLastFrame)   //initial jump
            {
                //s = v*t
                //t = s/v       

                if (jAccelTimer < minJumpHeight / jumpAccel)    //need to change if variable jump-acceleration
                {
                    jAccelTimer += Time.deltaTime;

                    Vector2 jumpVel = new Vector2(0, jumpAccel /* * maxJumpSpeed*/);    //implement similar acceleration curve as horizontal movement
                    rb.velocity = jumpVel * Time.deltaTime;
                }
                else    //initial jump completed, probably need to change before implementing continous jumping
                {
                    jAccelTimer = 0;
                    isJumping = false;
                }
            }
            //else
            //{
            //    if not reached maxjump
            //    Vector2 jumpVel = new Vector2(0, jumpAccel /* * maxJumpSpeed*/);    //implement similar acceleration curve as horizontal movement
            //    rb.velocity = jumpVel * Time.deltaTime;
            //    Keep adding jumpAccel until maxJump is reached.
            //    Reach maxJump after 
            //}

            acceleratedJumpLastFrame = true;
        }


    }

}