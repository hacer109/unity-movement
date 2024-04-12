using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class PlayerMovement : MonoBehaviour
{
    Vector3 WishDir;
    Vector3 Velocity;
    float WishSpeed;
   public Rigidbody rb;
    float m_horizontal;
    float m_vertical;
    private float m_speed;
   
    public float RayDistanceSlope;
    public float RayDistance;
    public float MaxSlopeAngle;
    public float crouchSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    RaycastHit slopeHit;
    RaycastHit Hit;
    Vector3 PlayerScale;
    [SerializeField] private bool onSlope;
    [SerializeField] private bool onGround;
    Vector3 ABSnormal;
    [SerializeField] LayerMask GroundLayer;
    Vector3 BBContact;
    [SerializeField] float m_JumpForce;
    RaycastHit ColisionHit;
    [SerializeField] CapsuleCollider colider;
    [SerializeField] float RayCheckAbove;
    [SerializeField]bool GroundCheckGravity;
    [SerializeField] float Raydist2;
    [SerializeField] bool objectAbovePlayer;
    bool isGrounded;
    float gravity = -9.81f;
    float defaultRayDist;
    

    public enum PlayerState
    {
        Crouched,
        Air,
        Walk,
        Sprint
    }
    PlayerState state;

    
    void PlayerMovementState()
    {
        objectAbovePlayer = CheckAbovePlayer(out RaycastHit UpHit);

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            RayDistance /= 7;
         
            
           //     m_speed = crouchSpeed;
            
            state = PlayerState.Crouched;
            
            transform.localScale = new Vector3(PlayerScale.x, PlayerScale.y / 2, PlayerScale.z);
        }

        if (!objectAbovePlayer && Input.GetKey(KeyCode.LeftControl) == false && state == PlayerState.Crouched)
        {
            RayDistance = defaultRayDist;
            state = PlayerState.Walk;
           
            
           //     m_speed = walkSpeed;
            
            
            transform.localScale = PlayerScale;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && state == PlayerState.Walk && state != PlayerState.Crouched)
        {
            state = PlayerState.Sprint;
          //  m_speed = sprintSpeed;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && state == PlayerState.Sprint)
        {
            state = PlayerState.Walk;
          //  m_speed = walkSpeed;
        }

        switch(state)
        {
            case PlayerState.Sprint:
                m_speed = sprintSpeed;

                break;

                case PlayerState.Walk:
                m_speed = walkSpeed;
                break;

                case PlayerState.Crouched: 
                if (isGrounded)
                {

                    m_speed = crouchSpeed;
                }
                else
                {

                   
                }
                break;
        }


    }
    void Start()
    {
      
      colider = GetComponent<CapsuleCollider>();
     PlayerScale = transform.localScale;
        defaultRayDist = RayDistance;
        m_speed = walkSpeed;
        Velocity = Vector3.zero;
    }
   [SerializeField] float friction;
    [SerializeField] float groundAccel;
    [SerializeField] float maxGroundAccel;
    [SerializeField] float airAccel;
    [SerializeField] float maxAirAccel;
    


    Bounds bounds;
    bool IsMoving;

    public float tempFloat;

    void AirAcceleration(Vector3 wishDir,float accel,float wishSpeed)
    {
        
        
        float currentspeed = Vector3.Dot(Velocity, wishDir);
        Debug.Log(currentspeed);
        if(currentspeed > MaxAirWishSpeed)
        {
            currentspeed = MaxAirWishSpeed;
        }
        float addSpeed = wishSpeed - currentspeed;
        if (addSpeed <= 0)
        {
            return;
        }
        float AirAccelerate = accel * wishSpeed * Time.fixedDeltaTime; 
        if (AirAccelerate > addSpeed)
        {
            AirAccelerate = addSpeed;
        }
        Velocity += AirAccelerate * wishDir;
        
       
    }
    void Moveplayer()
    {
        Vector3 wishdir = WishDir * m_speed;
       // wishdir.Normalize();
        //wishdir = transform.TransformDirection(wishdir);
        float wishSpeed = wishdir.magnitude;
        wishSpeed = m_speed;
        Velocity = WishDir;
        
       AirAcceleration(wishdir, airAccel,wishSpeed);
       
    }
    void Update()
    {

        
       
        GroundCheckGravity = Physics.Raycast(transform.position, Vector3.down, Raydist2);
       // Debug.Log(rb.position);
         bounds = colider.bounds;
        bounds.Expand(-2 * skinWidth);
        (bool onGround, float groundAngle) = IsGround(out RaycastHit groundHit);
        bool falling = !(onGround && groundAngle <= 60f);
        WishDir = DirectionalLook.WishDir;
        Moveplayer();
       
        WishSpeed = WishDir.magnitude;
        onSlope = OnSlope();
        isGrounded = !falling;
        PlayerMovementState();
        
        

        if (onSlope)
        {
            WishDir = Vector3.ProjectOnPlane(WishDir, slopeHit.normal);
        }

        if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0 && isGrounded)
        {
            IsMoving = false;
        }
        else
        {
            IsMoving = true;
        }

       
    }
    public float MaxAirWishSpeed;
    

    float GravityVelocity;
    private void FixedUpdate()
    {

        rb.MovePosition(GetMoveDirection(Velocity * Time.fixedDeltaTime));





        // rb.AddForce(WishDir * m_speed,ForceMode.Force);



        Jump();
     
        
    }
    (bool,float) IsGround(out RaycastHit GroundHit)
    {

        onGround = CastSelf(transform.position, transform.rotation, Vector3.down, RayDistance, out GroundHit);
        float angle = Vector3.Angle(GroundHit.normal, Vector3.up);
       return (onGround, angle); 
    }
   
   public float CheckDist;
    bool CheckAbovePlayer(out RaycastHit UpHit)
    {
        bool aboveDetect = CastSelf(transform.position,transform.rotation,Vector3.up, RayCheckAbove, out UpHit);
        return aboveDetect;
    }
   
    Vector3 GetMoveDirection(Vector3 movement)
    {
        // CheckDist = WishDir.magnitude + skinWidth;
        Vector3 position = transform.position;
        Vector3 remaining = movement;
        Quaternion rotation = DirectionalLook.TransformRotation;
        int bounces = 0;
         while (bounces < maxBounces && remaining.magnitude > 0.001f)
        {
            float RaycastDistance = remaining.magnitude;
            CheckDist = RaycastDistance;

            //test = Physics.CapsuleCastAll(rb.position + posY1, rb.position + posY2, bounds.extents.x, remaining.normalized, RaycastDistance,GroundLayer);
            if (!CastSelf(rb.position, rotation, remaining, RaycastDistance, out RaycastHit hit))
            {
                position += remaining;
                    break;
            }

            if (hit.distance == 0)
            {
                break;
            }

            float fraction = hit.distance / RaycastDistance;
            position += remaining * (fraction);
            position += hit.normal * 0.001f * 2;
            remaining *= (1-fraction);
            Vector3 planeNormal = hit.normal;

            float angleBetween = Vector3.Angle(hit.normal, remaining) - 90.0f;
            float maxAngleShoveDegrees = 180.0f - 120.0f;
            angleBetween = Mathf.Min(maxAngleShoveDegrees, Mathf.Abs(angleBetween));
            float normalizedAngle = angleBetween / maxAngleShoveDegrees;
            float anglePower = 0.5f;
            remaining *= Mathf.Pow(1 - normalizedAngle, anglePower)* 0.9f + 0.1f;

            Vector3 projected = Vector3.ProjectOnPlane(remaining, planeNormal).normalized * remaining.magnitude;
            if(projected.magnitude + 0.001f < remaining.magnitude)
            {
                remaining = Vector3.ProjectOnPlane(remaining,Vector3.up).normalized * remaining.magnitude;
            }
            else
            {
                remaining = projected;
            }
            bounces++;
        }
        return position;
        
       
      
        
    }

    public bool CastSelf(Vector3 pos, Quaternion rot, Vector3 dir, float dist, out RaycastHit hit)
    {
        // Get Parameters associated with the KCC
        Vector3 center = rot * colider.center + pos;
        float radius = colider.radius;
        float height = colider.height;

        // Get top and bottom points of collider
        Vector3 bottom = center + rot * Vector3.down * (height / 2 - radius);
        Vector3 top = center + rot * Vector3.up * (height / 2 - radius);

        // Check what objects this collider will hit when cast with this configuration excluding itself
        IEnumerable<RaycastHit> hits = Physics.CapsuleCastAll(top, bottom, radius, dir, dist, ~0, QueryTriggerInteraction.Ignore).Where(hit => hit.collider.transform != transform); // layer ~0
        bool didHit = hits.Count() > 0;

        // Find the closest objects hit
        float closestDist = didHit ? Enumerable.Min(hits.Select(hit => hit.distance)) : 0;
        IEnumerable<RaycastHit> closestHit = hits.Where(hit => hit.distance == closestDist);

        // Get the first hit object out of the things the player collides with
        hit = closestHit.FirstOrDefault();

        // Return if any objects were hit
        return didHit;
    }
    public Mesh mesh;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position - new Vector3(0,RayDistanceSlope,0));
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(BBContact, 0.3f);
        // Gizmos.DrawMesh(mesh,rb.position+ moveam);
        Gizmos.DrawWireMesh(mesh, rb.position + GetMoveDirection(WishDir));// * CheckDist);
        RaycastHit test;
        Physics.CapsuleCast(rb.position + posY1, rb.position + posY2, bounds.extents.x, WishDir.normalized, out test, CheckDist * WishDir.magnitude, GroundLayer);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireMesh(mesh, test.point);
        Gizmos.color= Color.green;
        Gizmos.DrawCube(new Vector3(BBContact.x, rb.position.y - 1, BBContact.z), new Vector3(0.5f, 0.5f, 0.5f));
        //Gizmos.DrawLine(BoundingBox.BoundingBoxCollision.contacts[0].point, BoundingBox.BoundingBoxCollision.contacts[0].point - new Vector3(0, RayDistance,0));
    }
    void Jump()
    {
        Vector3 vel = rb.velocity;
        vel.y = m_JumpForce;
        if(Input.GetKey(KeyCode.Space) && onGround)
        {
            rb.velocity = vel;
        }
    }
    public bool OnSlope()
    {
        
        if (Physics.Raycast(transform.position, Vector3.down,out slopeHit, RayDistanceSlope,GroundLayer))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);


            return angle < MaxSlopeAngle && angle != 0;
        }
        return false;
    }
  

    
    
   
    [SerializeField] float forceMagnitude;

    
    private void OnCollisionStay(Collision collision)
    {

        Rigidbody rigidbody = collision.collider.attachedRigidbody;
        if (rigidbody != null && IsMoving == true)//&& Vector3.Dot(collision.contacts[0].normal,Vector3.up) == 1)
        {

            Vector3 PushForceDir = collision.gameObject.transform.position - transform.position;
            PushForceDir.y = 0;
            PushForceDir.Normalize();
            rigidbody.AddForceAtPosition(PushForceDir * forceMagnitude, collision.contacts[0].point, ForceMode.Impulse);
        }


        BBContact = collision.contacts[0].point;
       // SnapPlayerToGround();
    }
    private void OnCollisionExit(Collision collision)
    {
        BBContact = Vector3.zero;
    }

    

      int maxBounces = 5;
      float skinWidth = 0.015f;
      public Vector3 posY1;
      public Vector3 posY2;
}
