using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace StartPosition.Scripts
{
    [RequireComponent(typeof(Button))]
    public class Switcher : MonoBehaviour
    {
        [SerializeField] private Text buttonText;
        [SerializeField] private string switchedOnText = "Stop";
        [SerializeField] private string switchedOffText = "Start";
        [SerializeField] private State state = State.SwitchedOff;
        [SerializeField] private UnityEvent switchOn;
        [SerializeField] private UnityEvent switchOff;

        public void OnButtonClicked()
        {
            switch (state)
            {
                case State.SwitchedOn:
                    switchOff.Invoke();
                    state = State.SwitchedOff;
                    if (buttonText != null)
                        buttonText.text = switchedOffText;
                    break;
                case State.SwitchedOff:
                    switchOn.Invoke();
                    state = State.SwitchedOn;
                    if (buttonText != null)
                        buttonText.text = switchedOnText;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum State
        {
            SwitchedOn,
            SwitchedOff
        }
    }
}
