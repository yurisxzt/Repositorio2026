using UnityEngine;
using UnityEngine.InputSystem;

namespace Script
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("Reference to the Move action (Vector2) from the Input Actions asset")]
        public InputActionReference moveAction;

        [Tooltip("Multiplier for the force applied to the ball")]
        public float moveSpeed = 5f;

        Rigidbody _rb;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        void OnEnable()
        {
            if (moveAction != null && moveAction.action != null)
                moveAction.action.Enable();
        }

        void OnDisable()
        {
            if (moveAction != null && moveAction.action != null)
                moveAction.action.Disable();
        }

        void FixedUpdate()
        {
            if (_rb == null) return;
            if (moveAction == null || moveAction.action == null) return;

            Vector2 input = moveAction.action.ReadValue<Vector2>();
            Vector3 force = new Vector3(input.x, 0f, input.y) * moveSpeed;
            _rb.AddForce(force, ForceMode.Force);
        }

        // Convenience: ensure rb is assigned when editing the component in the inspector
#if UNITY_EDITOR
        void OnValidate()
        {
            if (_rb == null) _rb = GetComponent<Rigidbody>();
        }
#endif
    }
}


