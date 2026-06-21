using UnityEngine;
using UnityEngine.InputSystem;

namespace Deckbuilder.Debugging
{
    public class DebugClickModeSwitcher : MonoBehaviour
    {
        [SerializeField] private Key m_moveKey = Key.Digit1;
        [SerializeField] private Key m_playCardKey = Key.Digit2;

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current[m_moveKey].wasPressedThisFrame)
                SetMode(DebugClickMode.Move);

            if (Keyboard.current[m_playCardKey].wasPressedThisFrame)
                SetMode(DebugClickMode.PlayCard);
        }

        private static void SetMode(DebugClickMode _mode)
        {
            DebugClickRouter.Mode = _mode;
            Debug.Log($"[DebugClickModeSwitcher] Click mode set to {_mode}.");
        }
    }
}
