using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.InteractionLogic
{
    /// <summary>
    /// Minimal screen prompt.
    /// If no Text assigned, tries to find one in children.
    /// </summary>
    public sealed class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI promptText;

        private void Awake()
        {
            if (promptText == null)
                promptText = GetComponentInChildren<TextMeshProUGUI>(true);

            if (promptText != null)
                promptText.enabled = false;
        }

        public void Show(string message)
        {
            if (promptText == null)
                return;

            promptText.text = message;
            promptText.enabled = true;
        }

        public void Hide()
        {
            if (promptText == null)
                return;

            promptText.enabled = false;
        }
    }
}