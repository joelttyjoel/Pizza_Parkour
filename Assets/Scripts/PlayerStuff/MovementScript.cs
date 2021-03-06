using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class MovementScript : MonoBehaviour
{
    PlayerSounds playerSounds;

    Collider2D col;
    Rigidbody2D rb;
    Animator animCtrl;
    SpriteRenderer sr;
    Vector2 groundedOrigin;
    Vector2 groundRayDir;
    Vector2 wallRayOrigin;
    Vector2 wallRayDir;
    Vector2 newPos;

    enum animState { idle, running, jumping, falling };
    animState currentAnim = animState.idle;
    string[] groundTypes = new string[] { "Snow", "Ice" };
    string currentGroundType; //= groundType.Snow;

    bool animHolding;
    bool grounded;
    bool isJumping;
    bool jumpBuffer;
    bool facingLeft;   //make sure to initilize to correct direction
    bool hasAirJumped;

    //TEMP
    public bool canMove = true;

    float groundRayOriginOffset;
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
    //sorry but just need this in quick for working, make so that its "what to hit" not "what to avoid", is better
    public LayerMask groundMaskThingsToAvoidTemp;
    public LayerMask onWallMaskThingsToAvoid;
    #region jumping
    [HideInInspector, SerializeField,
        Tooltip("The amount of upwards-acceleration applied to the player on jump-frame.")]
    float jumpVelocity = 20.5f;
    [HideInInspector, SerializeField,
        Tooltip("The amount of upwards-acceleration applied to the player on air-jump-frame.")]
    float airJumpVelocity = 18f;
    [HideInInspector, SerializeField,
        Tooltip("Gravity multiplier for short-jump, determines how high a short-jump is."),
        Min(0f)]
    float shortJumpMultiplier = 13f; // early fall
    [HideInInspector, SerializeField,
        Tooltip("NOT IN USE."),
        Min(0f)]
    float shortAirJumpMultiplier = 15f; // early fall (double jump)
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

    #region General Movement
    [Header("General Movement")]
    [SerializeField,
        Tooltip("The minimum distance from the ground the collider bottom of the player needs to be to count as grounded.")]
    float groundedOffset = 0.1f;
    [SerializeField,
        Tooltip("The minimum distance from the wall to walljump for example.")]
    float wallOffset = 0.1f;
    [SerializeField]
    float gratvityScale = 5f;
    [SerializeField]
    bool canDoubleJump = true;
    [SerializeField, Min(0.01f), Tooltip("Minimum falling speed, used to determine when to switch to falling animation")]
    float minFallingSpeed = 2.5f;
    #endregion

    public bool IsFacingLeft()
    {
        return facingLeft;
    }

    public void SetHolding(bool v)
    {
        animHolding = v;
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

        animCtrl = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        lastIn = 1;     //initialize facingLeft to correct direction

        groundRayOriginOffset = col.bounds.min.y;
        //init timers
        ResetCoyote();

        currentGroundType = groundTypes[0];

        playerSounds = GetComponent<PlayerSounds>();
        if (playerSounds == null)
            Debug.LogWarning("No PlayerSounds script attached to player object, no player-sounds will play.", this);
    }

    void Update()
    {
        rb.gravityScale = gratvityScale;    //temp, move to start when value is tweaked enough

        if (!canMove) return;

        int hInt = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        bool j = Input.GetButtonDown("Jump");                           //record j until jumpBuffer end

        JumpInput(j);

        Movement(hInt);

        Jumping();

        CatchHeight();

        SpeedClamp();

        AnimationUpdate();
    }

    private void Movement(int hInt)
    {
        ///Horizontal movement (ground)
        //TODO: use a friction value when adding velocity and stopping:
        //Friction = 1 == normal snappy, full stop on same frame as release of key,
        //Friction = 0 == never stops and cant accelerate. 
        /*
         * switch(currentGroundType)
         *      case "Ice": 
         *          slippery, smal friction number
         *              break;
         *      default:
         *          normal friction = maxFriction (1?);
         *      break;
         */


        if (hInt != 0)
        {
            if (lastIn != hInt)     //switched direction
            {
                accelTimer = 0;
                facingLeft = !facingLeft;
            }

            float currentAccel = GetAcceleration();
            Vector2 addedVelocity = new Vector2(maxSpeed * currentAccel * Time.deltaTime, 0);
            rb.velocity += addedVelocity * hInt;

            if (lastIn == hInt && lastVel > Mathf.Abs(rb.velocity.x))    //didnt switch direction, but somehow decelerated (can happen when landing)
                rb.velocity = new Vector2(lastVel * hInt, rb.velocity.y);

            lastVel = Mathf.Abs(rb.velocity.x);
            lastIn = hInt;
            //if (grounded)
            currentAnim = animState.running;
        }
        else
        {
            accelTimer = 0;
            rb.velocity = new Vector2(0, rb.velocity.y);    //Player instantly stops when not moving horizontally.
                                                            //Need to stop the player using friction or something, cant affect velocity directly.
            currentAnim = animState.idle;
        }

        if (IsAtWall(hInt))      //Prevents player from getting stuck mid-air when moving towards a wall in-air
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    private void JumpInput(bool j)
    {
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
    }

    /// <summary>
    /// If the player barly misses the platform, hitting its legs to the side, it is helped up to reach the platform.
    /// </summary>
    void CatchHeight()
    {
        //if player isnt grounded and collided with a platform
        //raycast from bottom of player collider, towards xdir of platform (ydir:0)
        //get the platform-colliders top vertex
        //distance = platformTopVertexCol.pos.y - bottomOfPlayerCol
        //(distance should always be positive, others "top" vertex is beneath player)
        //move player up by distance.
    }

    void AnimationUpdate()
    {
        sr.flipX = !facingLeft;

        if (animCtrl != null)
        {
            if (animHolding)
            {
                animCtrl.SetBool("Holding", true);
            }
            else
            {
                animCtrl.SetBool("Holding", false);
            }

            switch (currentAnim)
            {
                case animState.idle:
                    animCtrl.SetBool("Running", false);
                    animCtrl.SetBool("Falling", false);
                    animCtrl.SetBool("Jumping", false);
                    break;
                case animState.running:
                    animCtrl.SetBool("Running", true);
                    animCtrl.SetBool("Falling", false);
                    animCtrl.SetBool("Jumping", false);
                    break;
                case animState.jumping:
                    animCtrl.SetBool("Falling", false);
                    animCtrl.SetBool("Jumping", true);
                    break;
                case animState.falling:
                    animCtrl.SetBool("Jumping", false);
                    animCtrl.SetBool("Falling", true);
                    break;
                default:
                    Debug.LogError("anim state not defined", this);
                    break;
            }
        }
        else
            Debug.LogWarning("Animation Controller not found on player.", this);
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
        float bottomOfCollider = transform.position.y - col.bounds.extents.y + col.offset.y * 0.5f;
        groundedOrigin = new Vector3(transform.position.x, bottomOfCollider);
        Vector2 boxSize = new Vector2(col.bounds.size.x - 0.01f, groundedOffset);

        LayerMask mask = LayerMask.GetMask(LayerMask.LayerToName(0));
        if (GetComponent<InteractObjectiveController>() != null)     //temporary
            mask = ~groundMaskThingsToAvoidTemp;

        RaycastHit2D hit = Physics2D.BoxCast(groundedOrigin, boxSize, 0, groundRayDir, groundedOffset, mask);

        if (hit.collider != null)
        {
            if (hit.collider == col)
            {
                Debug.LogError("Grounded raycast hit player-collider, try changing groundRayOriginOffset.", this);
            }
            else
            {
                TilemapCollider2D tmCol;
                hit.collider.TryGetComponent(out tmCol);
                if (tmCol != null)   //if hit ground tilemap
                {
                    currentGroundType = GetGroundTileType(hit.point, tmCol.GetComponent<Tilemap>());
                }
                else
                    currentGroundType = groundTypes[0]; //default to snow

                hasAirJumped = false;
                //ResetCoyote();
                return true;
            }
        }
        return false;
    }

    bool IsAtWall(int dir)
    {
        float sideOfCollider = transform.position.x + dir * col.bounds.extents.x;
        wallRayOrigin = new Vector3(sideOfCollider, transform.position.y + col.offset.y * 0.5f);
        Vector2 boxSize = new Vector2(wallOffset * 2, col.bounds.size.y);

        LayerMask mask = LayerMask.GetMask(LayerMask.LayerToName(0));
        if (GetComponent<InteractObjectiveController>() != null)     //temporary
            mask = ~onWallMaskThingsToAvoid;

        RaycastHit2D hit = Physics2D.BoxCast(wallRayOrigin, boxSize, 0, wallRayDir, wallOffset, mask);

        #region Trashy visualisation of boxcast for copy-pasted, remove later
#if UNITY_EDITOR

        //Setting up the points to draw the cast
        Vector2 p1, p2, p3, p4, p5, p6, p7, p8, size = new Vector2(wallOffset * 2, col.bounds.size.y);
        float w = size.x * 0.5f;
        float h = size.y * 0.5f;
        p1 = new Vector2(-w, h);
        p2 = new Vector2(w, h);
        p3 = new Vector2(w, -h);
        p4 = new Vector2(-w, -h);
        Quaternion q = Quaternion.AngleAxis(0, new Vector3(0, 0, 1));
        p1 = q * p1;
        p2 = q * p2;
        p3 = q * p3;
        p4 = q * p4;
        p1 += wallRayOrigin;
        p2 += wallRayOrigin;
        p3 += wallRayOrigin;
        p4 += wallRayOrigin;
        Vector2 realDistance = wallRayDir.normalized * wallOffset;
        p5 = p1 + realDistance;
        p6 = p2 + realDistance;
        p7 = p3 + realDistance;
        p8 = p4 + realDistance;
        //Drawing the cast
        Color castColor = Color.red;//hit ? Color.red : Color.green;
        Debug.DrawLine(p1, p2, castColor);
        Debug.DrawLine(p2, p3, castColor);
        Debug.DrawLine(p3, p4, castColor);
        Debug.DrawLine(p4, p1, castColor);
        Debug.DrawLine(p5, p6, castColor);
        Debug.DrawLine(p6, p7, castColor);
        Debug.DrawLine(p7, p8, castColor);
        Debug.DrawLine(p8, p5, castColor);
        Debug.DrawLine(p1, p5, Color.grey);
        Debug.DrawLine(p2, p6, Color.grey);
        Debug.DrawLine(p3, p7, Color.grey);
        Debug.DrawLine(p4, p8, Color.grey);
#endif
        #endregion

        if (hit.collider != null)
        {
            //Debug.Log(hit.collider.name);
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
        Gizmos.DrawRay(groundedOrigin, groundRayDir);
    }
#endif
    void Jumping()
    {
        #region jumpCurve
        /*
        Draw a jumpCurve directly using this formula? By setting height at time t directly. 
        This way we easily can set max jump height and have it affect velocity and time, and draw the curve, or change one of the other variables etc. 
        Vertical Jump Physics Equation:
        https://sciencing.com/calculate-jump-height-acceleration-8771263.html
        v0 is jumpVelocity
        */
        #endregion

        if (isJumping)
        {
            isJumping = false;
            if (hasAirJumped)
                rb.velocity = Vector2.up * airJumpVelocity;
            else
                rb.velocity = Vector2.up * jumpVelocity;

            playerSounds?.PlayJump();   //null conditional operator '?.': if playerSounds != null, PlayJump(). Probably remove before deploy/release. 
        }

        //falling
        if (rb.velocity.y < 0)
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;

        //air-jump & realeased jump-button (not max jump)
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump") && hasAirJumped)
            rb.velocity += Vector2.up * Physics2D.gravity.y * (shortAirJumpMultiplier - 1) * Time.deltaTime;

        //jumped & realeased jump-button (not max jump)
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
            rb.velocity += Vector2.up * Physics2D.gravity.y * (shortJumpMultiplier - 1) * Time.deltaTime;

        if (!grounded)
            if (rb.velocity.y < -minFallingSpeed)
                currentAnim = animState.falling;
            else
                currentAnim = animState.jumping;
    }

    string GetGroundTileType(Vector3 position, Tilemap tm)
    {
        string tName = "";
        Vector3Int tilePos = tm.WorldToCell(new Vector3(transform.position.x, position.y - 0.5f));
        var tile = tm?.GetTile(tilePos);

        if (tile == null)   //try offsetting boxcast.hit.point insted of using player position if no tile found
        {
            int offsetSide = transform.position.x - position.x > 0 ? -1 : 1;
            tilePos = tm.WorldToCell(new Vector3(position.x + 0.5f * offsetSide, position.y - 0.5f));    //offset to get the middle of the tile, otherwise sometimes the tile above or adjacent the intended might be returned
            tile = tm?.GetTile(tilePos);
        }

        //added this to avoid error "no instance of object"
        if (tile == null) return groundTypes[0];

        tName = tile.name;
        for (int i = 0; i < groundTypes.Length; i++)
        {
            if (tName.Contains(groundTypes[i]))     //Kind of expensive, would be nice if we could set groundType in tileData directly.. which is kind of possible but may be overkill.
            {
                return groundTypes[i];
                break;
            }
            if (i == groundTypes.Length - 1)
            {
                Debug.LogWarning("Tile does not contain name of any known groundTypes, defaulting to 'snow'.", this);
            }
        }
        return groundTypes[0];  //shoudlnt be reached.
    }

    public void PlayFootstepSound()
    {
        for(int i = 0; i < groundTypes.Length; i++)
        {
            if (currentGroundType == groundTypes[i])
                playerSounds.PlayFootstepOnGroundType(groundTypes[i]);

            if (i == groundTypes.Length)
                Debug.LogError("GroundType was never found, make spellcheck and make sure all ground types are added to groundTypes array.", this);
        }
    }
}