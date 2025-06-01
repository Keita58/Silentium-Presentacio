using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace NavKeypad
{
    public class Keypad : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private UnityEvent onAccessGranted;
        [SerializeField] private UnityEvent onAccessDenied;
        [Header("Combination Code (9 Numbers Max)")]
        [SerializeField] private int keypadCombo = 12345;

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
        private string currentInput;
        private bool displayingResult = false;
        private bool accessWasGranted = false;
        [SerializeField] GameObject panelCollider;


        public InputSystem_Actions inputActions { get; private set; }
        private void Awake()
        {
            door.SetLocked(true);
            inputActions = new InputSystem_Actions();
            inputActions.Hieroglyphic.Exit.started += ExitPuzzle;
            ClearInput();
        }

        private void OnDestroy()
        {
            inputActions.Hieroglyphic.Exit.started -= ExitPuzzle;
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
                    if (currentInput != null && currentInput.Length == 9) // 9 max passcode size 
                    {
                        return;
                    }
                    currentInput += input;
                    keypadDisplayText.text = currentInput;
                    break;
            }

        }
        public void CheckCombo()
        {
            if (int.TryParse(currentInput, out var currentKombo))
            {
                bool granted = currentKombo == keypadCombo;
                if (!displayingResult)
                {
                    StartCoroutine(DisplayResultRoutine(granted));
                }
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
            door.Open(new Vector3(0, 0, 0));
            accessWasGranted = true;
            keypadDisplayText.text = accessGrantedText;
            PuzzleManager.instance.HieroglyphicPuzzleExitAnimation();
            audioSource.PlayOneShot(accessGrantedSfx);
            this.transform.gameObject.layer = 0;
            panelCollider.gameObject.layer = 0;
        }
        private void ExitPuzzle(InputAction.CallbackContext context)
        {
            PuzzleManager.instance.HieroglyphicPuzzleExit(false);
        }
    }
}