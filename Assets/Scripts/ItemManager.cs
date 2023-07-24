using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;
    public TextAsset cardsJSON;
    public List<Item> ItemCards;
    /* It contains the same as the ItemCards list except that
       it uses keys to access them instead of index numbers */
    public Hashtable CardLibrary = new Hashtable();
    public CardList cardDB;

    void Awake(){
        Instance = this;
    }

    void Start()
    {
        cardDB = JsonUtility.FromJson<CardList>(cardsJSON.text);
        foreach(Item card in cardDB.list){
            CardLibrary.Add(card.id, card);
            ItemCards.Add(card);
        }
    }

    public Item getRandom(string useType=""){
        int i = UnityEngine.Random.Range(0, ItemCards.Count);
        Debug.Log(i);
        if(useType=="" && ItemCards.Count>=i){
            return ItemCards[i];
        }

        List<Item> CardChunk = new List<Item>();
        foreach(Item card in ItemCards){
            if (card.useType==useType){
                CardChunk.Add(card);
            }
        }
        i = UnityEngine.Random.Range(0, CardChunk.Count);
        switch(useType){
            case "weapon":
                return CardChunk[i];
            case "personal":
                return CardChunk[i];
        }
        i = UnityEngine.Random.Range(0, ItemCards.Count);
        if(i<ItemCards.Count){
            return ItemCards[i];
        } else {
            return null;
        }
    }

    void Update()
    {
        
    }
}

[Serializable]
public class Item{
    public string title = "...";
    public string id = "";
    public string icon = "...";
    public bool isTreasure = false;

    /* The item will be activated automatically when conditions are met */
    public bool auto = false;

    public string useType;
    // public enum useType{
    //     weapon, // It is a weapon
    //     summon, // It summons something
    //     personal, // Is does something to the player
    // }

    public string usePhase;
    // public enum usePhase{
    //     duringCombat, // Used during combat
    //     combatAsDefender, // Upon joining combat as defender
    //     beforeMoving, // Before rolling the dice to move
    //     afterRolling, // After rolling a dice
    //     trapEncounter, // After encountering a trap
    // }

    /* Those values are for weapons. Non-weapon cards have no use for those. */
    public int dmg = 0;
    public int monsterDmg = 0;
    public int defenderDmg = 0;
}

[Serializable]
public class CardList{
    public List<Item> list;
}