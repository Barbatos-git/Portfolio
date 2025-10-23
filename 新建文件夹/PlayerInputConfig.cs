using UnityEngine;
using UnityEngine.InputSystem;

public enum ControlScheme
{
    Gamepad,
    Keyboard
}

[System.Serializable]
public class PlayerInputConfig
{
    public ControlScheme controlScheme;
    public InputDevice device;

    public bool IsConfirmPressed()
    {
        if (InputLock.IsLocked) return false;

        if (controlScheme == ControlScheme.Gamepad && device is Gamepad gamepad)
        {
            return gamepad.buttonSouth.wasPressedThisFrame;
        }
        if (controlScheme == ControlScheme.Keyboard && device is Keyboard kb)
        {
            return kb.spaceKey.wasPressedThisFrame;
        }
        return false;
    }

    public bool IsCancelPressed()
    {
        if (InputLock.IsLocked) return false;

        if (controlScheme == ControlScheme.Gamepad && device is Gamepad gamepad)
        {
            return gamepad.buttonEast.wasPressedThisFrame;
        }
        if (controlScheme == ControlScheme.Keyboard && device is Keyboard kb)
        {
            return kb.escapeKey.wasPressedThisFrame;
        }
        return false;
    }

    public Vector2 GetMoveInput()
    {
        if (InputLock.IsLocked) return Vector2.zero;

        if (controlScheme == ControlScheme.Gamepad && device is Gamepad gamepad)
        {
            // 方向キー（DPad）への戻りを優先し、
            // 押されていない場合はジョイスティックに戻ります
            Vector2 dpad = gamepad.dpad.ReadValue();
            if (dpad.sqrMagnitude > 0.01f)
                return dpad;

            return gamepad.leftStick.ReadValue();
        }
        if (controlScheme == ControlScheme.Keyboard && device is Keyboard)
        {
            Vector2 dir = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) dir.y += 1;
            if (Keyboard.current.sKey.isPressed) dir.y -= 1;
            if (Keyboard.current.aKey.isPressed) dir.x -= 1;
            if (Keyboard.current.dKey.isPressed) dir.x += 1;
            return dir;
        }
        return Vector2.zero;
    }
}
