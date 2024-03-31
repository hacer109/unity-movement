using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.AI;
using static UnityEditor.PlayerSettings;

public class PlayerMovement : MonoBehaviour
{
    Vector3 WishDir;
   public Rigidbody rb;
    float m_horizontal;
    float m_vertical;
    public float m_speed;
   // public BoxCollider BoundingBox;
    public float RayDistanceSlope;
    public float RayDistance;
    public float MaxSlopeAngle;
    RaycastHit slopeHit;
    RaycastHit Hit;
    [SerializeField] private bool onSlope;
    [SerializeField] private bool onGround;
    Vector3 ABSnormal;
    [SerializeField] LayerMask GroundLayer;
    Vector3 BBContact;
    [SerializeField] float m_JumpForce;
    RaycastHit ColisionHit;
    [SerializeField] CapsuleCollider colider;
    bool isGrounded;
    void Start()
    {
      //  rb = GetComponentInParent<Rigidbody>();
      colider = GetComponent<CapsuleCollider>();
    }

    Bounds bounds;
    void Update()
    {
       // Debug.Log(rb.position);
         bounds = colider.bounds;
        bounds.Expand(-2 * skinWidth);
        IsGround();
        WishDir = DirectionalLook.WishDir;
        onSlope = OnSlope();
        //SpeedControl();

     /*   if (Physics.Raycast(transform.position, WishDir.normalized, out ColisionHit, WishDir.magnitude, GroundLayer))
        {
            WishDir = WishDir.normalized * ColisionHit.distance;
        }*/

    }
    private void FixedUpdate()
    {
        // rb.MovePosition(rb.position+ GetMoveDirection().normalized * Time.fixedDeltaTime * m_speed);
        // rb.MovePosition(GetMoveDirection(WishDir).normalized);
        rb.MovePosition(GetMoveDirection(WishDir * Time.fixedDeltaTime * m_speed));
      //  transform.position = GetMoveDirection(WishDir * Time.fixedDeltaTime * m_speed) ;
       // rb.MovePosition(PlayerMove(WishDir));
       // Move(WishDir);
         //rb.AddForce(GetMoveDirection() * m_speed, ForceMode.Force);
        //Debug.Log(rb.velocity.ToString());
        IsGround();
        Jump();
        //SpeedControl();
        
    }
    void IsGround()
    {
        
        onGround = Physics.Raycast(new Vector3(BBContact.x,rb.position.y - 1,BBContact.z),Vector3.down,out Hit,RayDistance);
        /* if(onSlope)
        {
            onGround = Physics.Raycast(transform.position, Vector3.down, out slopeHit, RayDistanceSlope, GroundLayer);
        }
        else
        {
            onGround = Physics.Raycast(transform.position, Vector3.down, out Hit, RayDistance, GroundLayer);
        }*/
        
    }
   
   public float CheckDist;

    Vector3 PlayerMove(Vector3 WishDir)
    {
        Vector3 position = rb.position;
        Vector3 remaining = WishDir;
        Quaternion rotation = transform.rotation;
        int bounces = 0;
        while (bounces < maxBounces && remaining.magnitude > 0.001f)
        {
            float distance = remaining.magnitude;
            RaycastHit test;
            if (!Physics.CapsuleCast(rb.position + posY1, rb.position + posY2, bounds.extents.x, remaining.normalized, out test, CheckDist, GroundLayer))
            {
                position += remaining;
                break;
            }
            if(test.distance == 0)
            {
                break;
            }

            float fraction = test.distance / distance;

            position += (remaining * fraction);
            position += (test.normal * 0.001f * 2);
            remaining *= (1 - fraction);

            Vector3 planeNormal = test.normal;
            float angleBetween = Vector3.Angle(test.normal, remaining) - 90.0f;

            angleBetween = Mathf.Min(180.0f - 120.0f,Mathf.Abs(angleBetween));
            float normalizedAngle = angleBetween / (180.0f - 120.0f);
            float anglePower = 0.5f;
            remaining *= Mathf.Pow(1 - normalizedAngle, anglePower) * 0.9f + 0.1f;

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
        
        
        
       /* if (!Physics.CapsuleCast(rb.position + posY1, rb.position + posY2, bounds.extents.x, remaining.normalized, out test, RaycastDistance, GroundLayer))
        {
            return remaining;

        }
       
        
        
        

         if(test.distance == 0)
         {
            return Vector3.zero;
            //return Vector3.ProjectOnPlane(remaining,test.normal).normalized;
         }*/

        //return position + remaining;
        /*  if (onSlope)
          {
              position = Vector3.ProjectOnPlane(remaining, test.normal).normalized;
          }
          else
          {
              position = Vector3.ProjectOnPlane(remaining, test.normal).normalized;
          }*/

        //  return Vector3.ProjectOnPlane(remaining, test.normal);
     //   return remaining;

  /*          if (onSlope)
        {
            return Vector3.ProjectOnPlane(WishDir, slopeHit.normal);
        }
        else
        {
            return Vector3.ProjectOnPlane(WishDir, Hit.normal);
        }*/
        
        
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
        IEnumerable<RaycastHit> hits = Physics.CapsuleCastAll(
            top, bottom, radius, dir, dist, ~0, QueryTriggerInteraction.Ignore)
            .Where(hit => hit.collider.transform != transform);
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
  

    
    public float snapDist;
    void SnapPlayerToGround()
    {

        if(onSlope)
        {
            Vector3 temp = rb.position;
            temp.y = slopeHit.point.y+ snapDist;
            // temp.y = BoxContact.y+1;
            rb.position = temp;
        }
        else if (onGround)
        {
            Vector3 temp = rb.position;
            temp.y = Hit.point.y ;
            // temp.y = BoxContact.y+1;
            rb.position = temp;
        }
        


        
    }
   
        private void OnCollisionStay(Collision collision)
    {
       
       BBContact = collision.contacts[0].point;
        //SnapPlayerToGround();
    }
    private void OnCollisionExit(Collision collision)
    {
        BBContact = Vector3.zero;
    }

    //This handles collisions based on a tutorial,reason for removal is because its too glitchy on certain object,while yes it does improve collisions when speed is high the tradeoff is not worth it

      int maxBounces = 5;
      float skinWidth = 0.015f;
      public Vector3 posY1;
      public Vector3 posY2;
}
