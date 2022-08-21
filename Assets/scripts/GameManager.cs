using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Timers;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;
using SimpleJSON;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //Start is called before the first frame update
    public static APIForm apiform;
    public static Globalinitial _global;
    public Transform prefab1;
    public Transform prefab2;
    private Transform card;
    private Transform score;
    public Button increasebtn;
    public Button decreasebtn;
    public Button maxbtn;
    public Button minbtn;
    public Button pitPlusbtn;
    public Button pitMinusbtn;
    public static int pitNum;
    public TMP_InputField pitCount;
    private float betValue;
    public TMP_InputField inputPriceText;
    private float totalValue;
    public TMP_Text totalPriceText;
    public TMP_Text alertText;
    public GameObject alertPan;
    public Button playbtn;
    public Sprite first_card;
    public Sprite pitImg;
    public Sprite explodeImg;
    public Sprite clickImg;
    public static int[] pitArray;
    public bool playflag;
    public bool playbtnFlag;
    public int clickNum;
    public string[] allCardName;
    public int beforeid;
    string[,] ScoreArray;
    [DllImport("__Internal")]
    private static extern void GameReady(string msg);
    BetPlayer _player;
    void Start()
    {
        _player = new BetPlayer();
        #if UNITY_WEBGL == true && UNITY_EDITOR == false
                    GameReady("Ready");
        #endif
        playflag = true;
        playbtnFlag = true;
        pitNum = 1;
        ScoreArray = new string[4, 10] {
            { "X1.21", "X1.52", "X1.89", "X2.37", "X2.96", "X3.7", "X4.62", "X5.78", "X7.23", "X9.03" },
            { "X1.62", "X2.69", "X4.49", "X7.48", "X12.47", "X20.79", "X34.65", "X57.75", "X96.25", "X160.42" },
            { "X2.42", "X6.06", "X15.15", "X37.89", "X94.73", "X236.82", "X592.04", "X1480.1", "X3700.26", "X9250.64" },
            { "X4.85", "X24.25", "X121.25", "X606.25", "X3031.25", "X15156.25", "X75781.25", "X378906","X1894531", "X9472656" }
        };
        pitArray = new int[10*pitNum];
        allCardName = new string[50];
        pitCount.text = pitNum.ToString();
        betValue = 10f;
        inputPriceText.text = betValue.ToString("f2");
        alertPan.SetActive(false);
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                card = Instantiate(prefab1, Vector2.zero, Quaternion.identity);
                card.transform.SetParent(GameObject.FindGameObjectWithTag("pitArea").transform);
                card.name = "card" + (i * 5 + j + 1);
                allCardName[i * 5 + j] = card.name;
                card.GetComponent<RectTransform>().anchorMin = new Vector2(0.335f + j * 0.12f, 0.865f - (i * 0.09f));
                card.GetComponent<RectTransform>().anchorMax = new Vector2(0.435f + j * 0.12f, 0.945f - (i * 0.09f));
                card.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
                card.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
                card.localScale = new Vector2(1f, 1f);
                card.GetComponent<DesignManager>().setId(i * 5 + j + 1);
            }
            score = Instantiate(prefab2, Vector2.zero, Quaternion.identity);
            score.transform.SetParent(GameObject.FindGameObjectWithTag("scores").transform);
            score.name = "score" + (i + 1);
            score.GetComponent<RectTransform>().anchorMin = new Vector2(0.1f, 0.894f - (i * 0.1f));
            score.GetComponent<RectTransform>().anchorMax = new Vector2(0.913f, 0.972f - (i * 0.1f));
            score.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            score.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            score.localScale = new Vector2(1f, 1f);
            GameObject.Find(score.name).transform.GetChild(0).GetComponent<TMP_Text>().text = ScoreArray[pitNum - 1, i];
        }
    }
    // Update is called once per frame
    public void RequestToken(string data)
    {
        JSONNode usersInfo = JSON.Parse(data);
        _player.token = usersInfo["token"];
        _player.username = usersInfo["userName"];
        float i_balance = float.Parse(usersInfo["amount"]);
        totalValue = i_balance;
        totalPriceText.text = totalValue.ToString("F2");
    }
    void Update()
    {
        
    }
    public void play() {
        
        if (playbtnFlag)
        {
            if (totalValue >= betValue)
            {
                if (totalValue >= 10)
                {
                    betClear();
                    StartCoroutine(UpdateCoinsAmount(totalValue, totalValue - betValue));
                    StartCoroutine(beginServer());
                }
                else
                {
                    StartCoroutine(alert("Insufficient balance!", "other"));
                }
            }else{
                StartCoroutine(alert("Insufficient balance!", "other"));
            }
        }else{
            collectClear();
            StartCoroutine(Server());
        }
    }
    IEnumerator beginServer()
    {
        WWWForm form = new WWWForm();
        form.AddField("userName", _player.username);
        form.AddField("betAmount", betValue.ToString("F2"));
        form.AddField("token", _player.token);
        form.AddField("amount", totalValue.ToString("F2"));
        form.AddField("pitNum", pitNum.ToString());
        _global = new Globalinitial();
        UnityWebRequest www = UnityWebRequest.Post(_global.BaseUrl + "api/start-Minesweeper", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<APIForm>(strdata);
            if (apiform.serverMsg == "Success")
            {
                pitArray = new int[10 * pitNum];
                pitArray = apiform.pitArray;
                StartCoroutine(UpdateCoinsAmount(totalValue, apiform.total));
            }
            else
            {
                StartCoroutine(alert(apiform.serverMsg, "other"));
                StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + betValue));
            }
        }
        else
        {
            StartCoroutine(alert("Can't find server!", "other"));
            StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + betValue));
        }
        yield return new WaitForSeconds(0.1f);
    }
    IEnumerator Server()
    {
        if (clickNum == 10) {
            clickNum = 11;
        }
        WWWForm form = new WWWForm();
        form.AddField("userName", _player.username);
        form.AddField("betAmount", betValue.ToString("F2"));
        form.AddField("token", _player.token);
        form.AddField("amount", totalValue.ToString("F2"));
        form.AddField("pitNum", pitNum.ToString("F2"));
        form.AddField("loop", clickNum.ToString("F2"));
        _global = new Globalinitial();
        UnityWebRequest www = UnityWebRequest.Post(_global.BaseUrl + "api/game-result", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<APIForm>(strdata);
            if (apiform.serverMsg == "Success")
            {
                StartCoroutine(alert(apiform.msg, "win"));
                StartCoroutine(UpdateCoinsAmount(totalValue, apiform.total));
                collectClear();
            }
            else
            {
                StartCoroutine(alert(apiform.serverMsg, "other"));
                StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + betValue));
            }
            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            StartCoroutine(UpdateCoinsAmount(totalValue, totalValue + betValue));
            StartCoroutine(alert("Can't find server!", "other"));
        }
    }
    public void setClickAble(int from, int since,bool flag) {
        for (int i = from; i < since; i++) {
            GameObject.Find(allCardName[i]).GetComponent<Button>().interactable = flag;
        }
    }
    public void pitIncrease()
    {
        if (pitNum < 5)
        {
            pitNum = pitNum + 1;
        }
        pitCount.text = pitNum.ToString();
    }
    public void pitDecrease()
    {
        if (pitNum > 1)
        {
            pitNum = pitNum - 1;
        }
        pitCount.text = pitNum.ToString();
    }
    public void pitInputChanged()
    {
        pitNum = int.Parse(string.IsNullOrEmpty(pitCount.text) ? "0" : pitCount.text);
        Transform scores = GameObject.Find("scores").transform;
        for (int i = 0; i < scores.childCount; i++)
        {
            Transform scoreObject = scores.transform.GetChild(i);
            scoreObject.GetChild(0).GetComponent<TMP_Text>().text = ScoreArray[pitNum - 1, i];
        }
        if (pitNum >= 4)
        {
            pitPlusbtn.interactable = false;
        }
        else if (pitNum <= 1)
        {
            pitMinusbtn.interactable = false;
        }
        else
        {
            pitPlusbtn.interactable = true;
            pitMinusbtn.interactable = true;
        }
    }
    public void halfControll()
    {
        betValue = betValue / 2;
        inputPriceText.text = betValue.ToString("F2");
    }
    public void doubleControll()
    {
        if (totalValue >= 2*betValue)
        {
            betValue = betValue * 2;
            inputPriceText.text = betValue.ToString("F2");
        }
    }
    public void maxControll()
    {
        if (totalValue >= 100000)
        {
            betValue = 100000f;
        }
        else
        {
            betValue = totalValue;
        }
        inputPriceText.text = betValue.ToString("F2");
    }
    public void minControll()
    {
        betValue = 10f;
        inputPriceText.text = betValue.ToString("F2");
    }
    public void inputChanged()
    {
        betValue = float.Parse(string.IsNullOrEmpty(inputPriceText.text) ? "0" : inputPriceText.text);
        if (betValue <= 10)
        {
            betValue = 10;
            inputPriceText.text = betValue.ToString("F2");
            decreasebtn.interactable = false;
            minbtn.interactable = false;
            maxbtn.interactable = true;
            increasebtn.interactable = true;
        }
        else if (betValue >= 100000)
        {
            betValue = 100000;
            inputPriceText.text = betValue.ToString("F2");
            increasebtn.interactable = false;
            maxbtn.interactable = false;
            decreasebtn.interactable = true;
            minbtn.interactable = true;
        }
        else if (betValue == totalValue)
        {
            maxbtn.interactable = false;
        }
        else
        {
            increasebtn.interactable = true;
            decreasebtn.interactable = true;
            maxbtn.interactable = true;
            minbtn.interactable = true;
        }
    }
    public void getId(int id) {
        int loop = 0;
        beforeid = id;
        playbtn.interactable = true;
        if (clickNum < 10) {
            setClickAble(clickNum * 5, clickNum * 5+5,true);
        }
        for (int i = 0; i < pitArray.Length; i++)
        {
            if (pitArray[i] == id)
            {
                loop = loop + 1;
            }
        }
        if (loop > 0)
        {
            playbtnFlag = true;
            GameObject.Find("playbtn").transform.GetChild(0).GetComponent<TMP_Text>().text = "Bet";
            StartCoroutine(alert("Better luck next time!", "lose"));
            setClickAble(0, 50, true);
            for (int i = 0; i < pitArray.Length; i++)
            {
                string name = "card" + pitArray[i];
                GameObject.Find(name).GetComponent<Image>().sprite = pitImg;
            }
            GameObject.Find("card"+id).GetComponent<Image>().sprite = explodeImg;
            playflag = false;
            settingTrue();
        }
        else{
            for (int i = clickNum * pitNum-pitNum; i < clickNum * pitNum; i++) {
                string name = "card" + pitArray[i];
                GameObject.Find(name).GetComponent<Image>().sprite = pitImg;
            }
            GameObject.Find("card"+id).GetComponent<Image>().sprite = clickImg;
            playflag = true;
            if (clickNum == 10) {
                StartCoroutine(Server());
            }   
        }
    }
    private IEnumerator UpdateCoinsAmount(float preValue, float changeValue)
    {
        // Animation for increasing and decreasing of coins amount
        const float seconds = 0.2f;
        float elapsedTime = 0;
        while (elapsedTime < seconds)
        {
            totalPriceText.text = Mathf.Floor(Mathf.Lerp(preValue, changeValue, (elapsedTime / seconds))).ToString();
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        totalValue = changeValue;
        totalPriceText.text = totalValue.ToString();
    }
    private IEnumerator alert(string msg, string state)
    {
        alertPan.SetActive(true);
        if (state == "win"){
            AlertController.isWin = true;
        }else{
            AlertController.isLose = true;
        }
        alertText.text = msg;
        yield return new WaitForSeconds(2.5f);
    }
    private void betClear() {
        AlertController.isWin = false;
        AlertController.isLose = false;
        alertPan.SetActive(false);
        settingFalse();
        beforeid = 0;
        GameObject.Find("playbtn").transform.GetChild(0).GetComponent<TMP_Text>().text = "Collect";
        playbtn.interactable = false;
        setClickAble(5, 50, false);
        playbtnFlag = false;
        playflag = true;
        clickNum = 1;
        for (int i = 0; i < 50; i++)
        {
            string name = "card" + (i+1);
            GameObject.Find(name).GetComponent<Image>().sprite = first_card;
        }
    }
    private void collectClear() {
        settingTrue();
        if (clickNum < 10)
        {
            setClickAble(clickNum * 5-5, clickNum * 5, false);
        }
        GameObject.Find("playbtn").transform.GetChild(0).GetComponent<TMP_Text>().text = "Bet";
        playbtnFlag = true;
        playflag = false;
    }
    private void settingTrue() {
        increasebtn.interactable = true;
        decreasebtn.interactable = true;
        maxbtn.interactable = true;
        minbtn.interactable = true;
        pitPlusbtn.interactable = true;
        pitMinusbtn.interactable = true;
        inputPriceText.interactable = true;
        pitCount.interactable = true;
    }
    private void settingFalse()
    {
        increasebtn.interactable = false;
        decreasebtn.interactable = false;
        maxbtn.interactable = false;
        minbtn.interactable = false;
        pitPlusbtn.interactable = false;
        pitMinusbtn.interactable = false;
        inputPriceText.interactable = false;
        pitCount.interactable = false;
    }
}
public class BetPlayer
{
    public string username;
    public string token;
}