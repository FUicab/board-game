using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static ItemManager;

public class UIManager : MonoBehaviour
{

    // General UI
    public Text PlayerLabel;
    public Text DiceAText;
    public Text DiceBText;
    public TextMeshProUGUI ItemDiceDisplay;
    public Text TotalMoves;
    public Text ScoreBoard;
    public Image[] InvSlots;
    public Image InvSlot1;
    public Image InvSlot2;
    public Image InvSlot3;
    public Image InvSlot4;
    public Image InvSlot5;
    public Image InvSlot6;

    // Utility
    public GameObject ActivePlayer;

    // Combat UI
    public GameObject BattleButtonsUI;
    public Button BattleAccept;
    public Button BattleReject;
    public GameObject BattleUI;
    public Text AttackerLabel;
    public Text DefenderLabel;
    public Text AttackerDice;
    public Text DefenderDice;

    // Start is called before the first frame update
    void Start()
    {   
        // PlayerLabel.text = ActivePlayer?.name;
        BattleButtonsUI.SetActive(false);
        BattleUI.SetActive(false);
    }

    void OnEnable(){
        EventManager.PlayerCollide += HandlePlayerCollision;
        EventManager.BattleStart += HandleBattleStart;
        EventManager.BattleEnd += HandleBattleEnd;
        EventManager.BattleReject += HandleBattleReject;
    }

    void OnDisable(){
        EventManager.PlayerCollide -= HandlePlayerCollision;
        EventManager.BattleStart -= HandleBattleStart;
        EventManager.BattleEnd -= HandleBattleEnd;
        EventManager.BattleReject -= HandleBattleReject;
    }

    // Update is called once per frame
    void Update()
    {
        ActivePlayer = GameManager.Instance.ActivePlayer;
        if(ActivePlayer!=null){
            PlayerLabel.text = ActivePlayer?.name;
            if(ActivePlayer.GetComponent<Movement>().useAI){
                PlayerLabel.text += " (Bot)";
            }
            PlayerLabel.color = ActivePlayer.GetComponent<PlayerCore>().color;
            DiceAText.text = GameManager.Instance.DiceA.ToString();
            DiceBText.text = GameManager.Instance.DiceB.ToString();
            TotalMoves.text = ActivePlayer?.GetComponent<Movement>().moves.ToString();
            ScoreBoard.text = "";
            foreach(GameObject player in GameManager.Instance.Players){
                if(player!=null)
                ScoreBoard.text += "♥ "+player.GetComponent<PlayerCore>().HP.ToString()+": "+player.name+"\n";
            }
            // ScoreBoard.text = "Scoreboard is due to have some changes applied soon...";
            // foreach(GameObject player in GameManager.Instance.Players){
            //     ScoreBoard.text += player.GetComponent<Inventory>().Bag?.Count.ToString()+": "+player.name+"\n";
            // }
            switch(GameManager.Instance.ItemDice){
                case "weapon":
                    ItemDiceDisplay.text = "▲";
                    break;
                case "artifact":
                    ItemDiceDisplay.text = "●";
                    break;
                case "nothing":
                    ItemDiceDisplay.text = "□";
                    break;
            }
            for(int i = 0; i < InvSlots.Length; i++){
                TextMeshProUGUI textObject = InvSlots[i].GetComponentInChildren (typeof(TextMeshProUGUI)) as TextMeshProUGUI;
                List<Item> Bag = ActivePlayer?.GetComponent<Inventory>().Bag;
                if(Bag.Count >= i+1){
                    InvSlots[i].GetComponent<Image>().color = new Color32(255,255,225,255);
                    textObject.text = Bag[i].icon;
                } else {
                    InvSlots[i].GetComponent<Image>().color = new Color32(255,255,225,64);
                    textObject.text = "";
                }
            }
            // TextMeshProUGUI text1 = InvSlot1.GetComponentInChildren (typeof(TextMeshProUGUI)) as TextMeshProUGUI;
            // if(ActivePlayer?.GetComponent<Inventory>().Bag?.Count>=1){
            //     InvSlot1.GetComponent<Image>().color = new Color32(255,255,225,255);
            //     text1.text = ActivePlayer?.GetComponent<Inventory>().Bag[0].icon;
            // } else {
            //     InvSlot1.GetComponent<Image>().color = new Color32(255,255,225,64);
            //     text1.text = "";
            // }
            // TextMeshProUGUI text2 = InvSlot2.GetComponentInChildren (typeof(TextMeshProUGUI)) as TextMeshProUGUI;
            // if(ActivePlayer?.GetComponent<Inventory>().Bag?.Count>=2){
            //     InvSlot2.GetComponent<Image>().color = new Color32(255,255,225,255);
            //     text2.text = ActivePlayer?.GetComponent<Inventory>().Bag[1].icon;
            // } else {
            //     InvSlot2.GetComponent<Image>().color = new Color32(255,255,225,64);
            //     text2.text = "";
            // }
            // TextMeshProUGUI text3 = InvSlot3.GetComponentInChildren (typeof(TextMeshProUGUI)) as TextMeshProUGUI;
            // if(ActivePlayer?.GetComponent<Inventory>().Bag?.Count>=3){
            //     InvSlot3.GetComponent<Image>().color = new Color32(255,255,225,255);
            //     text3.text = ActivePlayer?.GetComponent<Inventory>().Bag[2].icon;
            // } else {
            //     InvSlot3.GetComponent<Image>().color = new Color32(255,255,225,64);
            //     text3.text = "";
            // }
            // TextMeshProUGUI text4 = InvSlot4.GetComponentInChildren (typeof(TextMeshProUGUI)) as TextMeshProUGUI;
            // if(ActivePlayer?.GetComponent<Inventory>().Bag?.Count>=4){
            //     InvSlot4.GetComponent<Image>().color = new Color32(255,255,225,255);
            //     text4.text = ActivePlayer?.GetComponent<Inventory>().Bag[3].icon;
            // } else {
            //     InvSlot4.GetComponent<Image>().color = new Color32(255,255,225,64);
            //     text4.text = "";
            // }
            // TextMeshProUGUI text5 = InvSlot5.GetComponentInChildren (typeof(TextMeshProUGUI)) as TextMeshProUGUI;
            // if(ActivePlayer?.GetComponent<Inventory>().Bag?.Count>=5){
            //     InvSlot5.GetComponent<Image>().color = new Color32(255,255,225,255);
            //     text5.text = ActivePlayer?.GetComponent<Inventory>().Bag[4].icon;
            // } else {
            //     InvSlot5.GetComponent<Image>().color = new Color32(255,255,225,64);
            //     text5.text = "";
            // }
            // TextMeshProUGUI text6 = InvSlot6.GetComponentInChildren (typeof(TextMeshProUGUI)) as TextMeshProUGUI;
            // if(ActivePlayer?.GetComponent<Inventory>().Bag?.Count>=6){
            //     InvSlot6.GetComponent<Image>().color = new Color32(255,255,225,255);
            //     text6.text = ActivePlayer?.GetComponent<Inventory>().Bag[5].icon;
            // } else {
            //     InvSlot6.GetComponent<Image>().color = new Color32(255,255,225,64);
            //     text6.text = "";
            // }
        }

    }

    public void HandlePlayerCollision(GameObject[] Players){
        BattleAccept.GetComponentInChildren<Text>().text = "Figth "+Players[1].name;
        BattleButtonsUI.SetActive(true);
        if(ActivePlayer.GetComponent<Movement>().useAI){
            BattleAccept.interactable = false;
            BattleReject.interactable = false;
            StopCoroutine(SimulateAIWaitTime());
            StartCoroutine(SimulateAIWaitTime());
        } else {
            BattleAccept.interactable = true;
            BattleReject.interactable = true;
        }
        // if(ActivePlayer.GetComponent<Movement>().useAI){
        //     if(Random.Range(0,2)==1){
        //         // Player wants to fight!
        //         BattleAccept.onClick.Invoke();
        //     } else {
        //         // Player has chosen peace for now...
        //         BattleReject.onClick.Invoke();
        //     }
        // }
    }

    public void HandleBattleStart(GameObject[] duelers){
        BattleUI.SetActive(true);
        AttackerLabel.text = duelers[0].name;
        DefenderLabel.text = duelers[1].name;
        AttackerLabel.color = duelers[0].GetComponent<PlayerCore>().color;
        DefenderLabel.color = duelers[1].GetComponent<PlayerCore>().color;
        AttackerDice.text = GameManager.Instance.AttackerDice.ToString();
        DefenderDice.text = GameManager.Instance.DefenderDice.ToString();
        StartCoroutine(WaitBeforeClosingBattle());
    }

    public void HandleBattleEnd(){
        BattleButtonsUI.SetActive(false);
        BattleUI.SetActive(false);
    }

    public void HandleBattleReject(){
        BattleButtonsUI.SetActive(false);
    }

    public void TriggerBattleEnd(){
        EventManager.OnBattleEnd();
    }

    IEnumerator SimulateAIWaitTime(){
        yield return new WaitForSeconds(0.5f);
        if(ActivePlayer.GetComponent<Movement>().useAI){
            if(Random.Range(0,2)==1){
                // Player wants to fight!
                BattleAccept.onClick.Invoke();
            } else {
                // Player has chosen peace for now...
                BattleReject.onClick.Invoke();
            }
        }
    }

    IEnumerator WaitBeforeClosingBattle(){
        yield return new WaitForSeconds(1.5f);
        EventManager.OnBattleEnd();
    }
}
