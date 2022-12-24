using Inworld.Grpc;
using Inworld.Util;
using UnityEngine;
using GestureEvent = Inworld.Packets.GestureEvent;
using GestureType = Inworld.Grpc.GestureEvent.Types.Type;
using InworldPacket = Inworld.Packets.InworldPacket;

namespace Inworld.Sample
{
    public class EmotionCanvas : DemoCanvas
    {
        static readonly int s_Emotion = Animator.StringToHash("Emotion");
        static readonly int s_Gesture = Animator.StringToHash("Gesture");
        static readonly int s_Motion = Animator.StringToHash("MainStatus");
        Animator m_Animator;

        string m_CharName;
        GestureType m_CurrentGesture;
        EmotionEvent.Types.SpaffCode m_CurrentSpaff;
        GestureType m_LastGesture;
        EmotionEvent.Types.SpaffCode m_LastSpaff;
        public EmotionEvent.Types.SpaffCode Emotion
        {
            get => m_CurrentSpaff;
            private set
            {
                m_LastSpaff = m_CurrentSpaff;
                m_CurrentSpaff = value;
            }
        }

        public GestureType Gesture
        {
            get => m_CurrentGesture;
            private set
            {
                m_LastGesture = m_CurrentGesture;
                m_CurrentGesture = value;
            }
        }

        string _Statement
        {
            get
            {
                string emotion = m_LastSpaff == m_CurrentSpaff ? m_CurrentSpaff.ToString() : $"Last: {m_LastSpaff} Current: {m_CurrentSpaff}";
                string gesture = m_LastGesture == m_CurrentGesture && m_CurrentGesture == GestureType.Greeting //YAN: Enum can't be null. By default is greeting.
                    ? ""
                    : m_LastGesture == m_CurrentGesture
                        ? $"Gesture: {m_CurrentGesture.ToString()}"
                        : $"Gesture\nLast: {m_LastGesture} Current: {m_CurrentGesture}";
                return $"Emotion\n{emotion}\n\n{gesture}";
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            InworldController.Instance.OnStateChanged += OnStatusChanged;
            InworldController.Instance.OnCharacterChanged += OnCharacterChanged;
            InworldController.Instance.OnPacketReceived += OnPacketEvents;
        }
        void Update()
        {
            if (!m_Animator)
                return;
            m_Content.text = $"Current Emotion: <color=green>{(Emotion)m_Animator.GetInteger(s_Emotion)}</color>\nCurrent Gesture: <color=green>{(Gesture)m_Animator.GetInteger(s_Gesture)}</color>";
        }
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnStateChanged -= OnStatusChanged;
            InworldController.Instance.OnCharacterChanged -= OnCharacterChanged;
            InworldController.Instance.OnPacketReceived -= OnPacketEvents;
        }

        protected override void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
            if (!newCharacter && oldCharacter)
                m_Title.text = $"{oldCharacter.transform.name} Disconnected!";
            else
            {
                m_Title.text = $"{newCharacter.transform.name} connected!";
                m_Animator = newCharacter.GetComponent<Animator>();
            }
        }
        void OnPacketEvents(InworldPacket packet)
        {
            string charID = InworldController.Instance.CurrentCharacter.ID;
            if (packet.Routing.Target.Id != charID && packet.Routing.Source.Id != charID)
                return;
            switch (packet)
            {
                case Packets.EmotionEvent emotionEvent:
                    HandleEmotion(emotionEvent.SpaffCode);
                    break;
                case GestureEvent gestureEvent:
                    HandleGesture(gestureEvent.Simple);
                    break;
            }
        }
        void HandleEmotion(EmotionEvent.Types.SpaffCode incomingSpaff)
        {
            Emotion = incomingSpaff;
            m_Title.text = $"Get Emotion {incomingSpaff}";
        }
        void HandleGesture(GestureType incomingGesture)
        {
            Gesture = incomingGesture;
            m_Title.text = $"Get Gesture {incomingGesture}";
        }
        public void SendEmotion(int emotion)
        {
            if (!InworldController.Instance.CurrentCharacter || !m_Animator)
                return;
            m_Animator.SetInteger(s_Emotion, emotion);
            m_Title.text = $"Set Emotion {(Emotion)emotion}";
        }
        public void SendGesture(int gesture)
        {
            if (!InworldController.Instance.CurrentCharacter || !m_Animator)
                return;
            m_Animator.SetInteger(s_Gesture, gesture);
            m_Title.text = $"Set Gesture {(Gesture)gesture}";
        }
        public void SetMainStatus(int mainStatus)
        {
            if (!InworldController.Instance.CurrentCharacter || !m_Animator)
                return;
            m_Animator.SetInteger(s_Motion, mainStatus);
            m_Title.text = $"Set Gesture {(AnimMainStatus)mainStatus}";
        }
    }
}
