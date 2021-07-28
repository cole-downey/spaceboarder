using System.Collections;
using UnityEngine;

namespace Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class ExamplePlayerMovement : MonoBehaviour
    {
        #region PUBLIC FIELDS

        [Header("Walk / Run Setting")] public float walkSpeed;
        public float runSpeed;

        [Header("Jump Settings")] public float playerJumpForce;
        public ForceMode appliedForceMode;


        [Header("Jumping State")] public bool playerIsJumping;

        [Header("Current Player Speed")] public float currentSpeed;

        [Header("Ground LayerMask name")] public string groundLayerMask;

        [Header("Raycast Distance")] public float raycastDistance;

        #endregion


        #region PRIVATE FIELDS

        private float m_xAxis;
        private float m_zAxis;
        private Rigidbody m_rb;
        private RaycastHit m_hit;
        private Vector3 m_groundLocation;
        private bool m_leftShiftPressed;
        private int m_groundLayerMask;
        private float m_distanceFromPlayerToGround;
        private bool m_playerIsGrounded;
        private bool m_playerJumpStarted;
        #endregion


        #region jump presets

        private const int MaxAllowJump = 2; //maximum allowed jumps
        private int m_currentNumberOfJumpsMade; //current number of jumps processed

        #endregion

        #region MONODEVELOP ROUTINES

        private void Start()
        {
            #region initializing components

            m_rb = GetComponent<Rigidbody>();

            #endregion

            #region get layer mask [env ground layer]

            m_groundLayerMask = LayerMask.GetMask(groundLayerMask);

            #endregion

            #region jump press initiated
            
            m_playerJumpStarted = true;

            #endregion
        }

        private void Update()
        {
            #region controller Input [horizontal | vertical ] movement

            m_xAxis = Input.GetAxis("Horizontal");
            m_zAxis = Input.GetAxis("Vertical");

            #endregion

            #region adjust player move speed [walk | run]

            currentSpeed = m_leftShiftPressed ? runSpeed : walkSpeed;

            #endregion

            #region play jump input

            playerIsJumping = Input.GetKeyDown(KeyCode.Space);

            #endregion

            #region Shift To Change Speed

            m_leftShiftPressed = Input.GetKey(KeyCode.LeftShift);

            #endregion

            #region compute player distance from ground

            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * raycastDistance, Color.blue);
            //added layermask for those dealing with complex ground objects.
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out m_hit,
                raycastDistance, m_groundLayerMask))
            {
                m_groundLocation = m_hit.point;
                m_distanceFromPlayerToGround = transform.position.y - m_groundLocation.y;
            }

            #endregion
        }

        private void FixedUpdate()
        {
            #region move player

            m_rb.MovePosition(transform.position + Time.deltaTime * currentSpeed *
                transform.TransformDirection(m_xAxis, 0f, m_zAxis));

            #endregion

            #region apply single / double jump

            m_playerIsGrounded = m_distanceFromPlayerToGround <= 1f;

            if (playerIsJumping && m_playerJumpStarted &&
                (m_playerIsGrounded || MaxAllowJump > m_currentNumberOfJumpsMade)) StartCoroutine(ApplyJump());

            if (m_playerIsGrounded) m_currentNumberOfJumpsMade = 0;

            #endregion
        }

        #endregion

        #region HELPER ROUTINES
        /// <summary>
        /// applies force in the upward direction
        /// using jump force and supplied force mode
        /// </summary>
        /// <param name="jumpForce"></param>
        /// <param name="forceMode"></param>
        private void PlayerJump(float jumpForce, ForceMode forceMode)
        {
            m_rb.AddForce(jumpForce * m_rb.mass * Time.deltaTime * Vector3.up, forceMode);
        }

        /// <summary>
        /// handles single and double jump
        /// waits until space bar pressed is terminated before
        /// next jump is initiated.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ApplyJump()
        {
            PlayerJump(playerJumpForce, appliedForceMode);
            m_playerIsGrounded = false;
            m_playerJumpStarted = false;
            yield return new WaitUntil(() => !playerIsJumping);
            ++m_currentNumberOfJumpsMade;
            m_playerJumpStarted = true;
        }

        #endregion
    }
}