using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatePlayerBtnTest : MonoBehaviour
{
    Button button;

    // Start is called before the first frame update
    void Start()
    {
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(createPlayer);
    }

    // Create a player and assign a plot to the player
    private void createPlayer()
    {
        PlayerManager.instance.createPlayer(true);
    }
}
