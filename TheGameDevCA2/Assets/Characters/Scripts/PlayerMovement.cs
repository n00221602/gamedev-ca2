using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_12_animatedJump : MonoBehaviour
{
    //v9 freeFollow Camera
    [SerializeField]
    private Transform cameraTransform;

    //v11maximumSpeed no longer needed now controlled by animator
    //public float maximumSpeed; //rename public var speed
    [SerializeField]
    private float rotationSpeed;

    //v4 jump
    [SerializeField]
    private float jumpSpeed;

    //v5 - improve jump
    [SerializeField]
    private float jumpButtonGracePeriod;

    //v12 - animated jump
    [SerializeField]
    private float jumpHorizontalSpeed;


    private float ySpeed;
    private float originalStepOffset;
    private float? lastGroundedTime; //nullable floattype
    private float? jumpButtonPressedTime;

    //v6. animation
    private Animator animator;
    private CharacterController characterController;

    //v12 animated jumps
    private bool isJumping;
    private bool isGrounded;

    void Start()
    {

        characterController = GetComponent<CharacterController>();
        //v4 jump
        originalStepOffset = characterController.stepOffset;
        //v6. animation
        animator = GetComponent<Animator>();

    }

    void Update()
    {
        //old input system
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);

        //v8 1D Blend Anim tree
        float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);


        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            inputMagnitude /= 2; //1/2 speed
        }

        //set Float of animator component to blend animations.
        animator.SetFloat("Input Magnitude", inputMagnitude, 0.05f, Time.deltaTime);

        //v9 freeFollow camera -> update movementDirection with rotation from the CameraTransform
        movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up) * movementDirection;

        //v11 speed does not need to be calculated anylonger
        //float speed = inputMagnitude * maximumSpeed;

        //Normalize diretion vector so that it has a range of 0-1
        movementDirection.Normalize();

        //v4 - Jump. update ySpeed with Gravity
        ySpeed += Physics.gravity.y * Time.deltaTime;
        //Debug.Log(ySpeed);

        //v5. improve jump
        if (characterController.isGrounded)
        {
            lastGroundedTime = Time.time; // assign time since game started
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpButtonPressedTime = Time.time; // assign time since game started
        }


        //v5. improve jump replace  if (characterController.isGrounded)
        if (Time.time - lastGroundedTime <= jumpButtonGracePeriod)
        {

            characterController.stepOffset = originalStepOffset;//reset characterController stepOffset
            ySpeed = -0.5f;  //reset ySpeed 
            //v12 animated jump - player is grounded
            animator.SetBool("isGrounded", true);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            isGrounded = true;
            isJumping = false;



            //v5. improve jump. replace  Input.GetButtonDown("Jump")
            if (Time.time - jumpButtonPressedTime <= jumpButtonGracePeriod)
            {
                ySpeed = jumpSpeed;   //apply jumpSpeed to ySpeed
                //v5. improve jump. reset nullables back to null
                // in order to avoid multiple jumps inside gracePeriod
                jumpButtonPressedTime = null;
                lastGroundedTime = null;
                //v12 animated jump - player is jumping
                animator.SetBool("isJumping", true);
                isJumping = true;

            }
        }
        else
        {
            characterController.stepOffset = 0;
            //v12 animated jump - player is not grounded
            animator.SetBool("isGrounded", false);
            isGrounded = false;

            //v12 animated jump - is player falling?
            if ((isJumping && ySpeed < 0) || ySpeed < -2)
            {
                animator.SetBool("isFalling", true);

            }



        }







        if (movementDirection != Vector3.zero)
        {
            //changes character to point in direction of movement.
            //v11 -> change state to blendtree in animator 
            animator.SetBool("isMoving", true);

            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            //v11 -> default idle state in animator
            animator.SetBool("isMoving", false);
        }

        //v12 apply motion to jump state

        if (isGrounded == false)
        {
            Vector3 velocity = movementDirection * jumpHorizontalSpeed;
            velocity.y = ySpeed;
            characterController.Move(velocity * Time.deltaTime);
        }
    }

    /// <summary>
    /// v11 root motion
    /// method gets triggered when animator moves. (caused by root motion).
    /// overrides default AnimatorMove behaviour. velocity calculation from update 
    /// gets moved here. 
    /// </summary>
    private void OnAnimatorMove()
    {
        if (isGrounded)
        {
            //v11 velocity gets assigned from animator deltaPos -> positon change
            Vector3 velocity = animator.deltaPosition;
            // v4 - Jump. Local var vector3 velocity
            // add ySpeed to velocity
            velocity.y = ySpeed * Time.deltaTime;
            //Time.deltaTime is  required for the charControll Move method
            characterController.Move(velocity);
        }
    }

    /// <summary>
    /// Method that locks or unlocks cursor based on focus
    /// </summary>
    /// <param name="focus">boolean that determines the focus</param>
    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}

