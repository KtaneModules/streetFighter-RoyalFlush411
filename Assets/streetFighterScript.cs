using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class streetFighterScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] fighterButton;
    public Renderer surface;
    public GameObject player2MenuObject;
    public Renderer player2Selection;
    public GameObject selectionBlocker;
    public GameObject player2Stuff;
    public Renderer player2Head;
    private bool player1Selected;
    private bool player2Selected;
    public string[] listOfFighterNames;
    public string[] listOfFighterNamesLog;
    private List<string> lettersInChosenCountry = new List<string>();

    private bool aaBatts;
    private bool vowel;
    private bool ports;
    private bool lit;
    private string mustContain = "";
    private int matchingCountryLetters = 0;
    private int occurrences = 0;
    private int opponentIndex = 0;
    private string correctOpponent = "";
    private string correctOpponentLog = "";

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable button in fighterButton)
        {
            KMSelectable pressedButton = button;
            button.OnHighlight += delegate () { FighterHighlight(pressedButton); };
            button.OnInteract += delegate () { FighterPress(pressedButton); return false; };
        }
    }

    void Start()
    {
        player2MenuObject.SetActive(false);
        selectionBlocker.SetActive(false);
        player2Stuff.SetActive(false);
        CalculateLetter();
    }

    void CalculateLetter()
    {
        if(Bomb.GetBatteryCount(Battery.AA) + Bomb.GetBatteryCount(Battery.AAx3) + Bomb.GetBatteryCount(Battery.AAx4) >= 2)
        {
            aaBatts = true;
        }
        if(Bomb.GetSerialNumberLetters().Any(x => x == 'A' || x == 'E' || x == 'I' || x == 'O' || x == 'U'))
        {
            vowel = true;
        }
        if(Bomb.GetPortCount(Port.RJ45) >= 1 && Bomb.GetPortCount(Port.Serial) >= 1)
        {
            ports = true;
        }
        if(Bomb.GetOnIndicators().Count() > 0)
        {
            lit = true;
        }

        if(aaBatts && vowel && ports && lit)
        {
            mustContain = "t";
        }

        else if(aaBatts && vowel && ports)
        {
            mustContain = "g";
        }
        else if(aaBatts && vowel && lit)
        {
            mustContain = "c";
        }
        else if(aaBatts && lit && ports)
        {
            mustContain = "n";
        }
        else if(vowel && ports && lit)
        {
            mustContain = "s";
        }

        else if(aaBatts && vowel)
        {
            mustContain = "l";
        }
        else if(aaBatts && lit)
        {
            mustContain = "k";
        }
        else if(aaBatts && ports)
        {
            mustContain = "d";
        }
        else if(vowel && lit)
        {
            mustContain = "m";
        }
        else if(vowel && ports)
        {
            mustContain = "u";
        }
        else if(lit && ports)
        {
            mustContain = "f";
        }

        else if(aaBatts)
        {
            mustContain = "h";
        }
        else if(vowel)
        {
            mustContain = "v";
        }
        else if(ports)
        {
            mustContain = "o";
        }
        else if(lit)
        {
            mustContain = "b";
        }

        else
        {
            mustContain = "r";
        }
        Debug.LogFormat("[Street Fighter #{0}] Select a fighter that has the letter '{1}' in their name.", moduleId, mustContain);
    }

    void CalculateOpponent(KMSelectable pressedButton)
    {
        for(int i = 0; i < pressedButton.GetComponent<ButtonMaterials>().lettersInCountry.Count(); i++)
        {
            lettersInChosenCountry.Add(pressedButton.GetComponent<ButtonMaterials>().lettersInCountry[i]);
        }

        for(int i = 0; i <= 11; i++)
        {
            for(int j = 0; j < listOfFighterNames[i].Count(); j++)
            {
                for(int k = 0; k < lettersInChosenCountry.Count(); k++)
                {
                    if(lettersInChosenCountry[k].Contains(listOfFighterNames[i][j]))
                    {
                        occurrences++;
                    }
                }
            }
            if(occurrences > 0)
            {
                matchingCountryLetters++;
            }
            occurrences = 0;
        }
        opponentIndex = (matchingCountryLetters + pressedButton.GetComponent<ButtonMaterials>().fighterName.Count() + Bomb.GetModuleNames().Count()) % 12;
        correctOpponent = listOfFighterNames[opponentIndex];
        correctOpponentLog = listOfFighterNamesLog[opponentIndex];
        Debug.LogFormat("[Street Fighter #{0}] You need to fight {1}.", moduleId, correctOpponentLog);
    }

    void FighterHighlight(KMSelectable pressedButton)
    {
        if(moduleSolved)
        {
            return;
        }
        if(!player1Selected)
        {
            surface.material = pressedButton.GetComponent<ButtonMaterials>().highlightBackground;
            Audio.PlaySoundAtTransform("highlight", transform);
        }
        else if(!player2Selected)
        {
            player2Selection.material = pressedButton.GetComponent<ButtonMaterials>().player2MenuBackground;
            player2Head.material = pressedButton.GetComponent<ButtonMaterials>().player2Headshot;
            Audio.PlaySoundAtTransform("highlight", transform);
        }
    }

    void FighterPress(KMSelectable pressedButton)
    {
        if(moduleSolved)
        {
            return;
        }
        pressedButton.AddInteractionPunch();
        if(!player1Selected)
        {
            if(pressedButton.GetComponent<ButtonMaterials>().lettersInName.Any((x) => x.Equals(mustContain)))
            {
                Debug.LogFormat("[Street Fighter #{0}] You have selected {1} as your fighter. That is correct.", moduleId, pressedButton.GetComponent<ButtonMaterials>().loggingName);
                player1Selected = true;
                surface.material = pressedButton.GetComponent<ButtonMaterials>().highlightBackground;
                player2Selection.material = pressedButton.GetComponent<ButtonMaterials>().player2MenuBackground;
                player2Head.material = pressedButton.GetComponent<ButtonMaterials>().player2Headshot;
                player2MenuObject.SetActive(true);
                selectionBlocker.SetActive(true);
                player2Stuff.SetActive(true);
                Audio.PlaySoundAtTransform("selected", transform);
                Audio.PlaySoundAtTransform("plane", transform);
                Audio.PlaySoundAtTransform(pressedButton.GetComponent<ButtonMaterials>().countrySFX.name, transform);
                CalculateOpponent(pressedButton);
            }
            else
            {
                Debug.LogFormat("[Street Fighter #{0}] Strike! You tried to select {1}. That does not contain the letter '{2}'.", moduleId, pressedButton.GetComponent<ButtonMaterials>().loggingName, mustContain);
                GetComponent<KMBombModule>().HandleStrike();
                Audio.PlaySoundAtTransform("laugh", transform);
            }

        }
        else if(!player2Selected)
        {
            if(pressedButton.GetComponent<ButtonMaterials>().fighterName == correctOpponent)
            {
                Debug.LogFormat("[Street Fighter #{0}] You have selected to fight {1}. That is correct. Module defused.", moduleId, pressedButton.GetComponent<ButtonMaterials>().loggingName);
                GetComponent<KMBombModule>().HandlePass();
                Audio.PlaySoundAtTransform("selected", transform);
                Audio.PlaySoundAtTransform("win", transform);
                player2Selected = true;
                player2Selection.material = pressedButton.GetComponent<ButtonMaterials>().player2MenuBackground;
                player2Head.material = pressedButton.GetComponent<ButtonMaterials>().player2Headshot;
            }
            else
            {
                Debug.LogFormat("[Street Fighter #{0}] Strike! You tried to select {1} to fight. That is not correct.", moduleId, pressedButton.GetComponent<ButtonMaterials>().loggingName);
                GetComponent<KMBombModule>().HandleStrike();
                Audio.PlaySoundAtTransform("laugh", transform);
            }
        }
    }
}
