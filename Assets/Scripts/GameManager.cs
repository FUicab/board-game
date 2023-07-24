using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /* General variables */
    public static GameManager Instance;
    public GameState State;
    public GameObject[] Players;
    public int currentTurn = 1;
    public GameObject ActivePlayer;
    public LayerMask PlayerLayer;

    /* Treasure variables */
    public GameObject TreasurePrefab;
    public int maxTreasureCount = 1;
    public GameObject[] Treasures;
    int gridXOffset = 1;
    int gridZOffset = 1;
    int gridXSize = 2;
    int gridZSize = 2;
    int maxTileSize = 6;

    /* Dices */
    public int DiceA = 0;
    public int DiceB = 0;
    public string ItemDice = "nothing";
    // public enum ItemDice{
    //     weapon,
    //     artifact,
    //     nothing
    // }
    /* Combat variables */
    public GameObject[] Duelers;
    public int AttackerDice = 1;
    public int DefenderDice = 1;

    //Light props
    public Light MainLight;
    public Light Spotlight;


    void Awake(){
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        GetPlayers();
        ActivePlayer = Players[currentTurn];
        // Debug.Log(Players.Length+" players found.");
        setPlayerTurn(currentTurn);
        MainLight.enabled = true;
        Spotlight.enabled = false;
    }

    void OnEnable(){
        EventManager.TreasurePickedUp += HandleTreasurePickUp;
        EventManager.BattleStart += HandleBattleStart;
        EventManager.BattleEnd += HandleBattleEnd;
        EventManager.BattleAccept += HandleBattleAccept;
        EventManager.BattleReject += HandleBattleReject;
        EventManager.PlayerDeath += HandlePlayerDeath;
        EventManager.PlayerCollide += HandlePlayerCollision;
    }

    void OnDisable(){
        EventManager.TreasurePickedUp -= HandleTreasurePickUp;
        EventManager.BattleStart -= HandleBattleStart;
        EventManager.BattleEnd -= HandleBattleEnd;
        EventManager.BattleAccept -= HandleBattleAccept;
        EventManager.BattleReject -= HandleBattleReject;
        EventManager.PlayerDeath -= HandlePlayerDeath;
        EventManager.PlayerCollide -= HandlePlayerCollision;
    }

    void GetPlayers(){
        Players = GameObject.FindGameObjectsWithTag("Player");
    }

    public void nextPlayerTurn(){
        currentTurn ++;
        if(currentTurn>=Players.Length){
            currentTurn = 0;
        }
        setPlayerTurn(currentTurn);

        /* Handle Treasure spawning */
        Vector3 newLocation = new Vector3(
            Random.Range(0,maxTileSize) * maxTileSize - (gridXSize * maxTileSize) - gridXOffset * 3,
            0,
            Random.Range(0,maxTileSize) * maxTileSize - (gridZSize * maxTileSize) - gridZOffset * 3
        );
        while( Physics.Raycast(newLocation + (Vector3.up * 2), Vector3.down, 3, PlayerLayer) ){
            newLocation = new Vector3(
                Random.Range(0,maxTileSize) * maxTileSize - (gridXSize * maxTileSize) - gridXOffset * 3,
                0,
                Random.Range(0,maxTileSize) * maxTileSize - (gridZSize * maxTileSize) - gridZOffset * 3
            );
        };
        Treasures = GameObject.FindGameObjectsWithTag("Treasure");
        if(Treasures.Length<maxTreasureCount){
            Instantiate(TreasurePrefab, newLocation, Quaternion.identity);
        }
    }

    public void setPlayerTurn(int index){
        currentTurn = index;
        foreach(GameObject player in Players){
            player.GetComponent<Movement>().active = false;
            EventManager.EndTurn -= nextPlayerTurn;
        }
        ActivePlayer = Players[index];
        EventManager.EndTurn += nextPlayerTurn;
        ActivePlayer.GetComponent<Movement>().active = true;
        ActivePlayer.GetComponent<Movement>().Enemy = null;
        rollTheDice();
        ActivePlayer.GetComponent<Movement>().moves = DiceA + DiceB;
        switch(ItemDice){
            case "weapon":
                ActivePlayer.GetComponent<Inventory>().AddToBag("random_weapon");
                break;
            case "artifact":
                ActivePlayer.GetComponent<Inventory>().AddToBag("random_artifact");
                break;
            case "nothing":
                /* Well... you know... just nothing... */
                break;
        };
    }

    public void rollTheDice(){
        DiceA = Random.Range(1,7);
        DiceB = Random.Range(1,7);
        switch(Random.Range(0,3)){
            case 0:
                ItemDice = "weapon";
                break;
            case 1:
                ItemDice = "artifact";
                break;
            case 2:
                ItemDice = "nothing";
                break;
        };
    }

    public void UpdateGameState(GameState newState){
        State = newState;
    }

    public void HandleTreasurePickUp(GameObject player){
        player.GetComponent<Inventory>().AddToBag("treasure");
        // Vector3 newLocation = new Vector3(
        //     Random.Range(0,maxTileSize) * maxTileSize - (gridXSize * maxTileSize) - gridXOffset * 3,
        //     0,
        //     Random.Range(0,maxTileSize) * maxTileSize - (gridZSize * maxTileSize) - gridZOffset * 3
        // );
        // Instantiate(TreasurePrefab, newLocation, Quaternion.identity);
    }

    public void HandleBattleStart(GameObject[] duelers){
        // Deciding the winner...
        int winnerIndex = Random.Range(0,duelers.Length);
        AttackerDice = Random.Range(1,7);
        DefenderDice = Random.Range(1,7);
        int difference = 1; // The difference between the winner and the loser
        // Deueler indexes: Attacker is 0 and Defender is 1
        if(AttackerDice==DefenderDice){ // It's a TIE
            foreach(GameObject dueler in duelers){
                dueler.GetComponent<PlayerCore>().BattleDefeat(difference);
            }
        } else if(AttackerDice>DefenderDice){ // Attacker WINS
            difference = AttackerDice - DefenderDice;
            duelers[0].GetComponent<Movement>().BattleParticles.Play();
            duelers[1].GetComponent<PlayerCore>().BattleDefeat(difference);
        } else { // Attacker LOSES
            difference = DefenderDice - AttackerDice;
            duelers[1].GetComponent<Movement>().BattleParticles.Play();
            duelers[0].GetComponent<PlayerCore>().BattleDefeat(difference);
        }
        // EventManager.OnBattleEnd();
        // Debug.Log("Att: "+(AttackerDice.ToString())+" | Def: "+(DefenderDice.ToString())+" ---> Diff: "+(difference.ToString()));

    }

    public void HandleBattleEnd(){
        nextPlayerTurn();
        MainLight.intensity = 1.0f;
        Spotlight.enabled = false;
    }

    public void HandlePlayerCollision(GameObject[] players){
        MainLight.intensity = 0.8f;
        Spotlight.enabled = true;
        Spotlight.gameObject.transform.position = new Vector3(players[1].transform.position.x, 9.0f, players[1].transform.position.z);
        Duelers = players;

    }

    public void HandleBattleAccept(){
        Debug.Log("Battle accepted");
        EventManager.OnBattleStart(Duelers);
    }

    public void HandleBattleReject(){
        Debug.Log("Battle rejected");
        MainLight.intensity = 1.0f;
        Spotlight.enabled = false;
        ActivePlayer.GetComponent<Movement>().active = true;
    }

    public void HandlePlayerDeath(GameObject player){
        player.tag = "Untagged";
        Destroy(player);
        GetPlayers();
        nextPlayerTurn();
    }

    public void TriggerBattleAccept(){
        EventManager.OnBattleAccept();
    }

    public void TriggerBattleReject(){
        EventManager.OnBattleReject();
    }

}

public enum GameState{
    A,
    B,
    C
}