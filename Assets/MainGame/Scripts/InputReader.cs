using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class InputReader : MonoBehaviour
{
    public static InputReader Instance;

    public DefaultControls touchControls;

    public delegate void StartTouchEvent(Vector2 position, float time);
    public event StartTouchEvent OnStartTouch;
    public delegate void EndTouchEvent(Vector2 position, float time);
    public event EndTouchEvent OnEndTouch;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            touchControls = new DefaultControls();
            return;
        }
        else 
        { 
            Destroy(Instance); 
        }
    }
    private void OnEnable()
    {
        touchControls.Enable();
    }
    private void OnDisable()
    {
        touchControls.Disable();
    }
    private void Start()
    {
        touchControls.Player.TouchPress.started += ctx => StartTouch(ctx);
        touchControls.Player.TouchPress.canceled += ctx => EndTouch(ctx);
    }

    private void StartTouch(InputAction.CallbackContext _context)
    {
        if (OnStartTouch != null) OnStartTouch(touchControls.Player.TouchPosition.ReadValue<Vector2>(), (float) _context.startTime);
        Debug.Log("Meep :" + touchControls.Player.TouchPosition.ReadValue<Vector2>());
    }
    private void EndTouch(InputAction.CallbackContext _context)
    {
        if (OnEndTouch != null) OnEndTouch(touchControls.Player.TouchPosition.ReadValue<Vector2>(), (float)_context.time);
    }
    
}
