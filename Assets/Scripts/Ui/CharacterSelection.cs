using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
   
    public string currentCharacter;
    public List<CharacterUi> characterGos = new();
    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var butt = transform.GetChild(i).GetComponent<Button>();
            characterGos.Add(new CharacterUi(transform.GetChild(i).GetComponent<Image>(), butt));
            butt.onClick.AddListener(() => { OnClick(butt.gameObject.name); });
        }
        LoadCharacterName();
        SaveCharacterName();
    }
    private void Start()
    {
        OnClick(currentCharacter,true);
    }

    void LoadCharacterName()
    {
        currentCharacter = PlayerPrefs.GetString("char", Characters.PlayerCyborg.ToString());
    }
    void OnClick(string name, bool firstTime = false)
    {
        if (currentCharacter == name && !firstTime)
        {
            return;
        }
        currentCharacter = name;
        foreach (var item in characterGos)
        {
            var color = item.image.color;
            if (item.button.gameObject.name == name)
            {
                color.a = 1;
            }
            else
            {
                color.a = 0.5f;
            }
            item.image.color = color;
        }
        SaveCharacterName();
    }
    void SaveCharacterName()
    {
        if (!string.IsNullOrEmpty(currentCharacter))
        {
            PlayerPrefs.SetString("char", currentCharacter);
        }
        else
        {
            PlayerPrefs.SetString("char", Characters.PlayerCyborg.ToString());
        }
    }
}
public enum Characters
{
    PlayerCyborg, PlayerPunk
}
[Serializable]
public class CharacterUi
{
    public Image image;
    public Button button;

    public CharacterUi(Image image, Button button)
    {
        this.image = image;
        this.button = button;
    }
}
