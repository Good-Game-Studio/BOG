using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyStartButton : MonoBehaviour
{
    public TMP_InputField inputField;
     Button startButton;
    private void Awake()
    {
        if (PlayerPrefs.HasKey("gamerTag"))
        {
            inputField.text = PlayerPrefs.GetString("gamerTag");
        }
        startButton = GetComponent<Button>();
        startButton.onClick.AddListener(() => {

            if (!string.IsNullOrEmpty( inputField.text) )
            {
                PlayerPrefs.SetString("gamerTag",inputField.text);
                SceneManager.LoadScene(1);
            }
        });
    }
}
