using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorsePuzzle : MonoBehaviour
{
    private Light morseLight;

    //Morse durations
    private float dotDuration = 0.2f;
    private float dashDuration = 0.6f;
    private float spaceDuration = 0.2f;
    private float letterSpace = 0.6f;
    private string message = "Despierta papa";

    [SerializeField] DetectMorseEntry detectMorseEntry;

    private Coroutine morseCoroutine;

    //El dictionari de cada lletra traduida a morse 
    private Dictionary<char, string> morseCode = new Dictionary<char, string>()
    {
        {'A', ".-"}, {'D', "-.."},
        {'E', "."},   {'I', ".."},  {'P', ".--."},
        {'R', ".-."},   {'S', "..."},  {'T', "-"},
        {' ', " "} 
    };


    private void Awake()
    {
        morseLight = GetComponent<Light>();
        morseLight.enabled = false;
        detectMorseEntry.onMorseRoomEnter += StartMorse;
        detectMorseEntry.onMorseRoomLeave += EndMorse;
    }

    private void StartMorse()
    {
        morseCoroutine = StartCoroutine(PlayMorse(message.ToUpper()));
    }

    private void EndMorse()
    {
        StopAllCoroutines();
        morseLight.enabled = false;
    }

    private IEnumerator PlayMorse(string message)
    {
        while (true)
        {

            foreach (char c in message)
            {
                string code = morseCode[c];

                if (code == " ")
                {
                    yield return new WaitForSeconds(letterSpace * 2); //espai entre paraula
                    continue;
                }
                foreach (char symbol in code)
                {
                    morseLight.enabled = true;

                    if (symbol == '.')
                    {
                        yield return new WaitForSeconds(dotDuration);
                    }
                    else if (symbol == '-')
                    {
                        yield return new WaitForSeconds(dashDuration);
                    }

                    morseLight.enabled = false;

                    yield return new WaitForSeconds(spaceDuration); //espai entre simbols
                }

                yield return new WaitForSeconds(letterSpace); //espai entre lletres
            }

            morseLight.enabled = false;
            yield return new WaitForSeconds(10f);
        }

    }
}
