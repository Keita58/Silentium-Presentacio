using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace NavKeypad
{
    public class MorseKeypad : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private UnityEvent onAccessGranted;
        [SerializeField] private UnityEvent onAccessDenied;
        [Header("Combination Code")]
        [SerializeField] private string keypadCombo = "DEDPIERTA PAPA";

        [Header("Settings")]
        [SerializeField] private string accessGrantedText = "Granted";
        [SerializeField] private string accessDeniedText = "Denied";

        [Header("Visuals")]
        [SerializeField] private float displayResultTime = 1f;
        [Header("SoundFx")]
        [SerializeField] private AudioClip buttonClickedSfx;
        [SerializeField] private AudioClip accessDeniedSfx;
        [SerializeField] private AudioClip accessGrantedSfx;
        [Header("Component References")]
        [SerializeField] private Renderer panelMesh;
        [SerializeField] private TMP_Text keypadDisplayText;
        [SerializeField] private AudioSource audioSource;

        [SerializeField] Door door;
        [SerializeField]
        private string currentInput;
        private string currentMorseCharacter = "";
        private bool displayingResult = false;
        private bool accessWasGranted = false;

        private Dictionary<string, char> morseCode = new Dictionary<string, char>()
        {
            {".-", 'A'},    {"-...", 'B'},  {"-.-.", 'C'},  {"-..", 'D'},
            {".", 'E'},     {"..-.", 'F'},  {"--.", 'G'},   {"....", 'H'},
            {"..", 'I'},    {".---", 'J'},  {"-.-", 'K'},   {".-..", 'L'},
            {"--", 'M'},    {"-.", 'N'},    {"---", 'O'},   {".--.", 'P'},
            {"--.-", 'Q'},  {".-.", 'R'},   {"...", 'S'},   {"-", 'T'},
            {"..-", 'U'},   {"...-", 'V'},  {".--", 'W'},   {"-..-", 'X'},
            {"-.--", 'Y'},  {"--..", 'Z'}

        };


    public InputSystem_Actions inputActions { get; private set; }
        private void Awake()
        {
            door.SetLocked(true);
            inputActions = new InputSystem_Actions();
            inputActions.Morse.Exit.started += ExitPuzzle;
            ClearInput();
        }


        //Gets value from pressedbutton
        public void AddInput(string input)
        {
            audioSource.PlayOneShot(buttonClickedSfx);
            if (displayingResult || accessWasGranted) return;
            switch (input)
            {
                case "enter":
                    CheckCombo();
                    break;
                default:
                    
                    if (currentInput != null && currentInput.Length ==14)
                    {
                        return;
                    }

                    if (input == "." || input == "-")
                    {
                        currentMorseCharacter += input;
                    }
                    else if (input == "endLetter") 
                    {
                        if (morseCode.ContainsKey(currentMorseCharacter))
                        {
                            currentInput += morseCode[currentMorseCharacter];
                        }
                        else if (currentMorseCharacter != "")
                        {
                            currentInput += "?";
                        }

                        currentMorseCharacter = "";
                    }
                    else if (input == "/")
                    {
                        currentInput += " ";
                    }else if (input == "delete")
                    {
                        char[] aux= currentInput.ToCharArray();
                        currentInput = "";
                        for (int i = 0; i < aux.Length; i++)
                        {
                            if (i < aux.Length - 1)
                            {
                                currentInput += aux[i];
                            }
                        }
                    }

                    keypadDisplayText.text = currentInput;
                    break;
            }

        }
        public void CheckCombo()
        {
            bool granted = currentInput == keypadCombo;
            if (!displayingResult)
            {
                StartCoroutine(DisplayResultRoutine(granted));
            }
        }

        //mainly for animations 
        private IEnumerator DisplayResultRoutine(bool granted)
        {
            displayingResult = true;

            if (granted) AccessGranted();
            else AccessDenied();

            yield return new WaitForSeconds(displayResultTime);
            displayingResult = false;
            if (granted) yield break;
            ClearInput();
        }

        private void AccessDenied()
        {
            keypadDisplayText.text = accessDeniedText;
            onAccessDenied?.Invoke();
            audioSource.PlayOneShot(accessDeniedSfx);
        }

        private void ClearInput()
        {
            currentInput = "";
            keypadDisplayText.text = currentInput;
        }

        private void AccessGranted()
        {
            door.SetLocked(false);
            door.Open(new Vector3(0,0,0));
            accessWasGranted = true;
            keypadDisplayText.text = accessGrantedText;
            PuzzleManager.instance.ExitMorsePuzzle();
            audioSource.PlayOneShot(accessGrantedSfx);
        }
        private void ExitPuzzle(InputAction.CallbackContext context)
        {
            PuzzleManager.instance.ExitMorsePuzzle();
        }
    }
}
