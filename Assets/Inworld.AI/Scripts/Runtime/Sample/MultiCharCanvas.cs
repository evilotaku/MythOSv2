using Inworld.Util;
using TMPro;
using UnityEngine;

namespace Inworld.Sample
{
    public class MultiCharCanvas : DemoCanvas
    {
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
        protected override void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
            if (!newCharacter && oldCharacter)
                m_Title.text = $"Inworld Disconnected!";
            else if (newCharacter && !oldCharacter)
                m_Title.text = $"Inworld Connected!";
            if (newCharacter)
                m_Content.text = $"Now Talking to <color=green>{newCharacter.CharacterName}</color>";
        }
    }
}
