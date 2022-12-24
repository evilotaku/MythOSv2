using Inworld.Util;
using TMPro;
using UnityEngine;

namespace Inworld.Sample
{
    public class DemoCanvas : MonoBehaviour
    {
        [SerializeField] protected TMP_Text m_Title;
        [SerializeField] protected TMP_Text m_Content;
        // Start is called before the first frame update
        void Start()
        {
            InworldController.Instance.OnStateChanged += OnStatusChanged;
            InworldController.Instance.OnCharacterChanged += OnCharacterChanged;
        }
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnStateChanged -= OnStatusChanged;
            InworldController.Instance.OnCharacterChanged -= OnCharacterChanged;
        }
        protected virtual void OnStatusChanged(ControllerStates incomingStatus)
        {
            m_Title.text = $"Inworld {incomingStatus}";
        }
        protected virtual void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
            if (!newCharacter && oldCharacter)
                m_Title.text = $"Inworld Disconnected!";
            else if (newCharacter && !oldCharacter)
                m_Title.text = $"Inworld Connected!";
        }
    }
}
