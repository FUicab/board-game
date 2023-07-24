using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCore : MonoBehaviour
{
    public int HP = 10;
    public Color color = new Color(1f,1f,1f);
    public bool stasis = true;
    public MeshRenderer Bubble;
    public Quaternion headRotation;
    public float rotationSpeed = 24f;
    public float RotMultiplierX = 0.5f;
    public float RotMultiplierY = 0.25f;
    public float RotMultiplierZ = 0.25f;
    public LayerMask Blocker;
    public Dice NumberDice = new Dice();
    public Dice ItemDice = new Dice();

    void Start()
    {
        if(Random.Range(0,2)==1){
            RotMultiplierX *= -1;
        }
        if(Random.Range(0,2)==1){
            RotMultiplierY *= -1;
        }
        if(Random.Range(0,2)==1){
            RotMultiplierZ *= -1;
        }
        RotMultiplierX *= Random.Range(0.51f,2.01f);
        RotMultiplierY *= Random.Range(0.51f,2.01f);
        RotMultiplierZ *= Random.Range(0.51f,2.01f);
        setStasisState();

        NumberDice.Sides = new List<DiceSide>();
        for(int i=0; i<6 ; i++){
            NumberDice.Sides.Add(new DiceSide());
            NumberDice.Sides[i].title = (i+1).ToString();
            NumberDice.Sides[i].value = (i+1).ToString();
            // Debug.Log(NumberDice.Sides[i].value);
        }
        ItemDice.Sides = new List<DiceSide>();
        for(int i=0; i<3 ; i++){
            ItemDice.Sides.Add(new DiceSide());
        }
        ItemDice.Sides[0].title = "Artifact";
        ItemDice.Sides[0].value = "artifact";
        ItemDice.Sides[1].title = "Weapon";
        ItemDice.Sides[1].value = "weapon";
        ItemDice.Sides[2].title = "Nothing";
        ItemDice.Sides[2].value = "nothing";
        ItemDice.KarmaDecreaseRate = 2;
        // Debug.Log(NumberDice.Sides[0].value);
    }

    // Update is called once per frame
    void Update()
    {
        if(stasis){
            if(!Bubble.enabled){
                transform.position += Vector3.up*1.5f;
            }
            // transform.rotation.x += rotationSpeed * Time.deltaTime;
            transform.Rotate(RotMultiplierX * rotationSpeed * Time.deltaTime,RotMultiplierY * rotationSpeed * Time.deltaTime,RotMultiplierZ * rotationSpeed * Time.deltaTime);
            Bubble.enabled = true;
        } else {
            Bubble.enabled = false;
        }
    }

    void setStasisState(){
        if(stasis){
            Vector3 x = new Vector3(Random.Range(-1.1f,1.1f),Random.Range(-1.1f,1.1f),Random.Range(-1.1f,1.1f));
            headRotation = Quaternion.LookRotation(x);
            transform.rotation = headRotation;
        }
    }

    public void BattleDefeat(int damage = 0){
        HP -= damage;
        if(HP<=0){
            // Destroy(gameObject);
            EventManager.OnPlayerDeath(gameObject);
        } else {
            int spawnX = (Random.Range(0,6)*2)-5;
            int spawnZ = (Random.Range(0,6)*2)-5;
            Vector3 newPos = new Vector3(spawnX, 1.5f ,spawnZ);
            while(Physics.Raycast(newPos, Vector3.down, 2, Blocker)){
                spawnX = (Random.Range(0,6)*2)-5;
                spawnZ = (Random.Range(0,6)*2)-5;
                newPos = new Vector3(spawnX, 1.5f ,spawnZ);
            }
            transform.position = newPos;
            stasis = true;
            setStasisState();
        }
    }
}

public class Dice{
    public string Name = "Unnamed Dice";
    public List<DiceSide> Sides;
    List<DiceSide> Pool;
    public int KarmaIncreaseRate = 1;
    public int KarmaDecreaseRate = 5;
    public int minWeight = 1;
    public int maxWeight = 24;

    public string Roll(){
        if(Sides.Count<=0){
            string result = "";
            Pool.Clear();
            foreach(DiceSide side in Sides){
                for(int i=0 ; i<side.weight ; i++){
                    Pool.Add(side);
                }
                side.weight += KarmaIncreaseRate;
                if(side.weight>=maxWeight){ side.weight = maxWeight; }
            }
            result = Pool[Random.Range(0,Pool.Count)].value;
            foreach(DiceSide side in Sides){
                if(side.value==result){
                    side.weight -= KarmaDecreaseRate;
                    if(side.weight<minWeight){ side.weight = minWeight; }
                }
            }
            return result;
        } else {
            return "No sides found";
        }
    }
}

[System.Serializable]
public class DiceSide{
    public string title = "Untitled Side";
    public string value = "";
    public int weight = 12;
}