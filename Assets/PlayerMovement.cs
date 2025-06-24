using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    public float jumpForce = 10f;
    public Transform groundCheckPoint;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayerMask;
    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayerMask);
        bool hitGround = hit.collider != null;
        return hitGround && rigid.velocity.y <= 0f;
    }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");

        rigid.velocity = new Vector2(h * speed, rigid.velocity.y);
    }

    private void LookForward()
    {
        if (Input.GetButton("Horizontal"))
        {
            spriteRenderer.flipX = Input.GetAxis("Horizontal") < 0f;
        }
    }
    private void Update()
    {
        LookForward();

        if (IsGrounded())
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }
        }
    }

    public void Jump()
    {
        rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    
}
