using UnityEngine;

namespace Wiggy
{
  public class input_handler : MonoBehaviour
  {
    Player_controls input_actions;

    // Set by callbacks in OnEnable 
    public Vector2 l_analogue { get; private set; }
    public Vector2 r_analogue { get; private set; }
    public bool a_input;
    public bool b_input;
    public bool x_input;
    public bool y_input;
    public bool rb_input;
    public bool rt_input;
    public bool d_pad_u;
    public bool d_pad_d;
    public bool d_pad_l;
    public bool d_pad_r;

    public void OnEnable()
    {
      if (input_actions == null)
      {
        input_actions = new Player_controls();
        input_actions.PlayerMovement.LStick.performed += i => l_analogue = i.ReadValue<Vector2>();
        input_actions.PlayerMovement.RStick.performed += i => r_analogue = i.ReadValue<Vector2>();
        input_actions.PlayerActions.A.performed += i => a_input = true;
        input_actions.PlayerActions.B.performed += i => b_input = true;
        input_actions.PlayerActions.X.performed += i => x_input = true;
        input_actions.PlayerActions.Y.performed += i => y_input = true;
        input_actions.PlayerActions.RB.performed += i => rb_input = true;
        input_actions.PlayerActions.RT.performed += i => rt_input = true;
        input_actions.PlayerActions.DPadUp.performed += i => d_pad_u = true;
        input_actions.PlayerActions.DPadDown.performed += i => d_pad_d = true;
        input_actions.PlayerActions.DPadLeft.performed += i => d_pad_l = true;
        input_actions.PlayerActions.DPadRight.performed += i => d_pad_r = true;
      }

      input_actions.Enable();
    }

    private void OnDisable()
    {
      input_actions.Disable();
    }

    public void DoLateUpdate()
    {
      a_input = false;
      b_input = false;
      x_input = false;
      y_input = false;
      rb_input = false;
      rt_input = false;
      d_pad_u = false;
      d_pad_d = false;
      d_pad_l = false;
      d_pad_r = false;
    }

  }
}