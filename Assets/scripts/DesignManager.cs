using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesignManager : MonoBehaviour
{
    // Start is called before the first frame update
    public int id;
    public GameManager gameManager;
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void setId(int _id) {
        id = _id;
    }
    public void item_click()
    {
        if (!gameManager.playbtnFlag) {
            if (gameManager.playflag) {
                if (gameManager.beforeid < id) {
                    if ((gameManager.clickNum-1)*5 < id) { 
                        gameManager.getId(id);
                        if (gameManager.clickNum < 10)
                        {
                            gameManager.clickNum = gameManager.clickNum + 1;
                        }
                    }
                }
            }
        }
    }
}
