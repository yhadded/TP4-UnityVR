using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

/// Exercice 3 : joystick gauche = déplacement, joystick droit = rotation (snap 30/45° ou fluide).
/// À placer sur le GameObject "XR Origin (XR Rig)".
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(XROrigin))]
public class JoystickLocomotion : MonoBehaviour
{
    [Header("Déplacement (joystick gauche)")]
    public float moveSpeed = 2f;
    public float gravity = -9.81f;
    [Range(0f, 0.5f)] public float deadzone = 0.15f;

    [Header("Rotation (joystick droit)")]
    public bool snapTurn = true;
    public float snapAngle = 45f;          // 30 ou 45
    public float smoothTurnSpeed = 90f;    // °/s si snapTurn = false
    [Range(0.1f, 1f)] public float turnThreshold = 0.7f;

    [Header("Inputs (Input System)")]
    public InputActionProperty moveAction = new InputActionProperty(
        new InputAction("Move", InputActionType.Value,
            "<XRController>{LeftHand}/{Primary2DAxis}", expectedControlType: "Vector2"));

    public InputActionProperty turnAction = new InputActionProperty(
        new InputAction("Turn", InputActionType.Value,
            "<XRController>{RightHand}/{Primary2DAxis}", expectedControlType: "Vector2"));

    XROrigin origin;
    CharacterController cc;
    Transform cam;
    float verticalVelocity;
    bool turnReady = true;

    void Awake()
    {
        origin = GetComponent<XROrigin>();
        cc = GetComponent<CharacterController>();
        cam = origin.Camera.transform;
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        turnAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        turnAction.action.Disable();
    }

    void Update()
    {
        UpdateCapsule();
        Move();
        Turn();
    }

    // La capsule de collision suit la position/hauteur de la tête
    void UpdateCapsule()
    {
        float h = Mathf.Clamp(origin.CameraInOriginSpaceHeight, 1f, 2.2f);
        Vector3 camLocal = origin.CameraInOriginSpacePos;
        cc.height = h;
        cc.center = new Vector3(camLocal.x, h / 2f + cc.skinWidth, camLocal.z);
    }

    void Move()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        if (input.magnitude < deadzone) input = Vector2.zero;

        // Direction relative au regard, projetée sur le sol
        Vector3 fwd = cam.forward; fwd.y = 0; fwd.Normalize();
        Vector3 right = cam.right; right.y = 0; right.Normalize();
        Vector3 dir = fwd * input.y + right * input.x;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        if (cc.isGrounded && verticalVelocity < 0) verticalVelocity = -1f;
        verticalVelocity += gravity * Time.deltaTime;

        cc.Move((dir * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);
    }

    void Turn()
    {
        float x = turnAction.action.ReadValue<Vector2>().x;

        if (snapTurn)
        {
            if (turnReady && Mathf.Abs(x) > turnThreshold)
            {
                origin.RotateAroundCameraUsingOriginUp(Mathf.Sign(x) * snapAngle);
                turnReady = false;              // un seul cran par poussée
            }
            else if (Mathf.Abs(x) < 0.2f)
            {
                turnReady = true;               // joystick revenu au centre
            }
        }
        else if (Mathf.Abs(x) > deadzone)
        {
            origin.RotateAroundCameraUsingOriginUp(x * smoothTurnSpeed * Time.deltaTime);
        }
    }
}
