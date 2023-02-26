using UnityEngine;

namespace Wiggy
{
  public class input_handler : MonoBehaviour
  {
    Player_controls input_actions;

    // Set by callbacks in OnEnable 
    public Vector2 l_analogue { get; private set; }
    public Vector2 r_analogue { get; private set; }

    public void OnEnable()
    {
      if (input_actions == null)
      {
        input_actions = new Player_controls();
        input_actions.PlayerMovement.LStick.performed += i => l_analogue = i.ReadValue<Vector2>();
        input_actions.PlayerMovement.RStick.performed += i => r_analogue = i.ReadValue<Vector2>();
        // input_actions.PlayerActions.DPadRight.performed += i => d_pad_r = true;
      }

      input_actions.Enable();
    }

    private void OnDisable()
    {
      input_actions.Disable();
    }
  }
}