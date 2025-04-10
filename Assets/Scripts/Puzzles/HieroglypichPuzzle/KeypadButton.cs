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


        private void OnMouseDown()
        {
            keypad.AddInput(value);

        }

    }
}