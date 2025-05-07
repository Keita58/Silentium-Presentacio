using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NavKeypad
{
    public class KeypadButton : MonoBehaviour
    {
        [Header("Value")]
        [SerializeField] private string value;
        [Header("Component References")]
        [SerializeField] private Keypad keypad;
        [SerializeField] private MorseKeypad morseKeypad;

        private void OnMouseDown()
        {
            if (keypad !=null) 
                keypad.AddInput(value);
            else
                morseKeypad.AddInput(value);
        }

    }
}