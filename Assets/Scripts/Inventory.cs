using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemManager;

public class Inventory : MonoBehaviour
{

    public List<Item> Bag;
    public int maxInvSize = 6;
    // public List<Item> ItemCards;
    /* It contains the same as the ItemCards list except that
       it uses keys to access them instead of index numbers */
    // public Hashtable CardList = new Hashtable();
    // public CardList cardDB;

    public void AddToBag(string itemCode){
        var item = new Item();
        switch(itemCode){
            case "random_weapon":
                // item.useType = "weapon";
                item = ItemManager.Instance.getRandom("weapon");
                break;
            case "random_artifact":
                // item.useType = "personal";
                item = ItemManager.Instance.getRandom("personal");
                break;
            // case "treasure":
            //     item.isTreasure = true;
            //     item.icon = "*";
            //     break;
        }
        if(Bag.Count>=maxInvSize){
            Bag.RemoveAt(Bag.Count-1);
            Bag.Insert(0,item);
        } else {
            Bag.Add(item);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        // cardDB = JsonUtility.FromJson<CardList>(@"Assets\Scripts\CardDatabase");
        // Debug.Log(cardDB);

        // string[] keys = new string[] {"treasure","w_sabre","w_axe"};
        // foreach(string key in keys){
        //     CardList.Add( key, new Item() );
        // }
        // Debug.Log(CardList.Count);
    }
}