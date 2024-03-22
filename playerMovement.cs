using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class playerMovement : MonoBehaviour
{

    public Rigidbody rb;
    private float movementSpeed = 0.15f;
    public float movementMultiplier = 10f;
    [SerializeField]  float sprintSpeed;
    float GroundDrag = 6f;
    float AirDrag = 2f;
    float VerticalMovement;
    float HorizontalMovement;
    Vector3 moveDir = Vector3.zero;
   public bool isGrounded;
    float playerHeight = 2f;
    [SerializeField]float jumpForce = 6.5f;
    [SerializeField] int maxVelocity;
    public float maxSlopeAngle;
    [SerializeField] float walkSpeed;
    private RaycastHit slopeHit;
    [SerializeField] float RayLenght;
    [SerializeField] float SlopeRayLenght;
    public playerLook CameraLook;
    BoxCollider BoundBox;
    private enum PlayerMovementState
    {
        Crouched,
        Air,
        Walk,
        Sprint
    }
    PlayerMovementState state;

   



    [SerializeField] float crouchSpeed;



    public void PlayerState()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift) && state != PlayerMovementState.Crouched && state != PlayerMovementState.Air )
        {

            state = PlayerMovementState.Sprint;
            movementSpeed = sprintSpeed;


        }



        if (Input.GetKeyUp(KeyCode.LeftShift) && state == PlayerMovementState.Sprint )
        {


            state = PlayerMovementState.Walk;
            movementSpeed = walkSpeed;


        }


        if (Input.GetKeyDown(KeyCode.LeftControl))
        {


            state = PlayerMovementState.Crouched;
            transform.localScale -= new Vector3(0, playerHeight /6, 0);
            movementSpeed = crouchSpeed;


          //  rb.AddForce(Vector3.down * 6f,ForceMode.Impulse);
        }



        if (Input.GetKeyUp(KeyCode.LeftControl))
        {


            state = PlayerMovementState.Walk;
            transform.localScale += new Vector3(0, playerHeight / 6, 0);
            movementSpeed = walkSpeed;


        }


    }


    private void Start()
    {
        CameraLook = GetComponent<playerLook>();
        state = PlayerMovementState.Walk;
        rb = GetComponent<Rigidbody>();
        BoundBox = GetComponent<BoxCollider>();
        rb.freezeRotation = true;
        movementSpeed = walkSpeed;
       
    }
    void setDrag()
    {
         if (isGrounded)
         {
             rb.drag = GroundDrag;
         }
         else
         {
             rb.drag = AirDrag;
         }
         
    }
    public bool IsMoving;
    public void InputGetter()
    {
        HorizontalMovement = Input.GetAxisRaw("Horizontal");
        VerticalMovement = Input.GetAxisRaw("Vertical");
        if (HorizontalMovement != 0 || VerticalMovement != 0)
        {
            IsMoving = true;
        }
        else
        {
            IsMoving= false;
        }
        //moveDir = transform.forward * VerticalMovement + transform.right * HorizontalMovement;
        moveDir = (transform.forward * VerticalMovement) + transform.right * HorizontalMovement;
    }

 [SerializeField]   bool isJumping;
    RaycastHit groundcheck;
    void Jump()
    {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            //rb.velocity = new Vector3 (0, 5, 0) * jumpForce;
            isJumping = true;
    }

    RaycastHit hit;
    public LayerMask ground;
   
   
    [SerializeField] bool SlopeOn;
    private bool OnSlope()
    {


        //if (Physics.Raycast(transform.position - new Vector3(0, RayLenght, -0.2f), Vector3.down, out slopeHit, SlopeRayLenght, ground))
        //if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, SlopeRayLenght, ground))
        //if (Physics.Raycast(BoxContact, Vector3.down, out slopeHit, SlopeRayLenght, ground))
        if (Physics.Raycast(BoxContact, Vector3.down, out slopeHit, SlopeRayLenght, ground))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
          

            return angle < maxSlopeAngle && angle != 0;


        }
        
        return false;
    }
   
    void SnapPlayerToGround()
    {
        if (Onslope && isGrounded)
        {
            Vector3 temp = rb.position;
             temp.y = slopeHit.point.y + 1;
           // temp.y = BoxContact.y + 1;
            rb.position = temp;
            //rb.position = temp + slopeHit.normal * .5f;
        }
        else if (isGrounded)
        {

            Vector3 temp = rb.position;
             temp.y = hit.point.y + 1;
           // temp.y = BoxContact.y+1;
            rb.position = temp;
        }
        
       
    }

    private Vector3 GetSlopeMoveDirection()
    {
       if (SlopeOn && isGrounded)
        {
            return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
           // return Vector3.ProjectOnPlane(moveDir, hit.normal).normalized;
        }
        else if(isGrounded)
        {
           // return Vector3.ProjectOnPlane(moveDir, hit.normal).normalized;
            return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
        }
        return Vector3.ProjectOnPlane(moveDir, hit.normal).normalized; 
       // return Vector3.ProjectOnPlane(moveDir, Normal).normalized;
    }
    bool Onslope;
    float slopeAngle;
    Vector3 Normal;
    ContactPoint contactPoint;
    RaycastHit RaycastHit;
    void IsGround()
    {

        if(Onslope)
        {
            //isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, SlopeRayLenght, ground);
            // isGrounded = Physics.Raycast(new Vector3(BoxContact.x,transform.position.y,BoxContact.z), Vector3.down, out hit, SlopeRayLenght, ground);
            isGrounded = Physics.Raycast(BoxContact, Vector3.down, out hit, SlopeRayLenght, ground);
        }
       else
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, RayLenght, ground);
            //isGrounded = Physics.Raycast(BoxContact, Vector3.down, out hit, RayLenght, ground);
        }

            
        
        
    }

    Collision HitboxCollision;
    private void OnCollisionStay(Collision collision)
    {
        BoxContact = collision.contacts[0].point;
        HitboxCollision = collision;
        IsGround();
        SnapPlayerToGround();
    }
    private void OnCollisionEnter(Collision collision)
    {
        BoxContact = collision.contacts[0].point;
        SnapPlayerToGround();
        IsGround();
       
    }

    private void OnCollisionExit(Collision collision)
    {
        HitboxCollision = collision;
        IsGround();
        
        
    }
    public float maxdistance = 5;
    
   
    private void OnDrawGizmos()
    {


        Gizmos.DrawWireCube(transform.position-new Vector3(0,1,0), transform.lossyScale + new Vector3(0,-1,0));
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(BoxContact, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position - new Vector3(0, 0, -0.2f), transform.position - new Vector3(0, RayLenght, -0.2f));
         //Gizmos.DrawLine(new Vector3(BoxContact.x,transform.position.y,BoxContact.z), transform.position - new Vector3(0, RayLenght, -0.2f));
        Gizmos.color = Color.red;
       // Gizmos.DrawLine(transform.position - new Vector3(0, 1, 0.15f), transform.position - new Vector3(0, SlopeRayLenght, 0.15f));
        Gizmos.DrawLine(BoxContact, BoxContact - new Vector3(0, SlopeRayLenght, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0, 5, 0), transform.position + GetSlopeMoveDirection());
        //  Gizmos.DrawCube(transform.position - new Vector3(0,0.8f,0),new Vector3(1,0.5f,1));

    }

    Vector3 BoxContact;
    private void FixedUpdate()
    {
       
       //BoundBox.transform.rotation = Quaternion.Euler(0,0,0);

        Onslope = OnSlope();

        SlopeOn = Onslope;
        Debug.Log(rb.velocity);



        rb.MovePosition(rb.position + GetSlopeMoveDirection().normalized * movementSpeed * Time.fixedDeltaTime * movementMultiplier);
      
       
       
        //rb.MovePosition(rb.position + Normal * movementSpeed * Time.fixedDeltaTime * movementMultiplier);
        /* if (isGrounded)
         {

             rb.MovePosition(rb.position + moveDir.normalized *movementSpeed * Time.fixedDeltaTime * movementMultiplier);

         }
         else if (Onslope)
         {


                 rb.MovePosition(rb.position + GetSlopeMoveDirection().normalized * movementSpeed * Time.fixedDeltaTime *movementMultiplier);
         }
         else
         {
                  rb.MovePosition(rb.position + moveDir.normalized * movementSpeed * Time.fixedDeltaTime * movementMultiplier);
         }*/
    }
   
    void Update()
    {

        
        InputGetter();
        PlayerState();


        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || Onslope))
        {

            Jump();
        }
        else if (!Input.GetKeyDown(KeyCode.Space) && (isGrounded || Onslope))
        {
            isJumping = false;
        }
        

        setDrag();

    }
    

}
