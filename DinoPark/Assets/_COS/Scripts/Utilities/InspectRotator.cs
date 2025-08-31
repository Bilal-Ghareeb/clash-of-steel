using UnityEngine;
using UnityEngine.InputSystem;

public class InspectRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;

    private InputAction _dragAction;
    private InputAction _pressAction;
    private bool _isDragging;

    private void OnEnable()
    {
        _dragAction = new InputAction(type: InputActionType.Value, binding: "<Pointer>/delta");
        _pressAction = new InputAction(type: InputActionType.Button, binding: "<Pointer>/press");

        _dragAction.Enable();
        _pressAction.Enable();

        _pressAction.performed += ctx => _isDragging = true;
        _pressAction.canceled += ctx => _isDragging = false;
    }

    private void OnDisable()
    {
        _dragAction.Disable();
        _pressAction.Disable();
    }

    private void Update()
    {
        if (_isDragging)
        {
            Vector2 delta = _dragAction.ReadValue<Vector2>();
            float rotX = delta.x * rotationSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, -rotX, Space.World);
        }
    }
}
