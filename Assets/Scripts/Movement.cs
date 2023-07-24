using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    KeyCode MoveForwardButton = KeyCode.W;
    [SerializeField]
    KeyCode MoveLeftButton = KeyCode.A;
    [SerializeField]
    KeyCode MoveBackButton = KeyCode.S;
    [SerializeField]
    KeyCode MoveRightButton = KeyCode.D;

    float rayLength = 2.25f;
    float rayOffsetX = 0f;
    // float rayOffsetY = 0f;
    float rayOffsetZ = 0f;

    public LayerMask Blocker;
    public bool active = false;

    public int moves = 0;

    Vector3 targetPosition;
    Vector3 startPosition;
    public Vector3 currentDirection;

    bool moving;
    float moveSpeed = 12f;
    float gridSize = 2f;
    // float rotationSpeed = 12f;
    Quaternion headRotation = Quaternion.LookRotation(Vector3.forward);

    /* AI Related variables */
    public bool useAI = false;
    public Vector3 target;
    /* -------------------- */


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        triggerMovement();
        // if (Input.GetKeyDown(KeyCode.E) && active){
        //     rollTheDice();
        // }
    }

    // public void rollTheDice(){
    //     moves = 0;
    //     moves += Random.Range(1,7);
    //     moves += Random.Range(1,7);
    //     Debug.Log("Moves: "+moves);
    // }

    void triggerMovement(){
        if(moving){
            gameObject.GetComponent<PlayerCore>().stasis = false;
            if(Vector3.Distance(startPosition, transform.position) > gridSize){
                transform.position = targetPosition;
                moving = false;
                return;
            }

            transform.position += (targetPosition - startPosition) * moveSpeed * Time.deltaTime;
            return;
        } else if(moves<=0 && active){
            EventManager.OnTurnEnd();
        }

        if(useAI){
            if(moves>0 && active){
                doAIMovement();
            }
        } else {
            if(moves>0 && active){
                if (Input.GetKeyDown(MoveForwardButton)){
                    if(CanMove(Vector3.forward)){
                        Move(Vector3.forward);
                    }
                } else if (Input.GetKeyDown(MoveBackButton)){
                    if(CanMove(Vector3.back)){
                        Move(Vector3.back);
                    }
                } else if (Input.GetKeyDown(MoveLeftButton)){
                    if(CanMove(Vector3.left)){
                        Move(Vector3.left);
                    }
                } else if(Input.GetKeyDown(MoveRightButton)){
                    if(CanMove(Vector3.right)){
                        Move(Vector3.right);
                    }
                }
            }
        }
    }

    void Move(Vector3 direction){
        currentDirection = direction;
        targetPosition = transform.position + direction * gridSize;
        targetPosition.y = 0f;
        startPosition = transform.position;
        moving = true;
        moves --;
        headRotation = Quaternion.LookRotation(currentDirection);
        transform.rotation = headRotation;
        // Quaternion rotation = Quaternion.LookRotation(currentDirection);
        // Debug.Log("Moves: "+moves);
    }

    void doAIMovement(){
        // Debug.Log(Mathf.Round(Time.realtimeSinceStartup * 100f) / 100f);

        Vector3 AIDirection;

        /* The AI's priority is now the treasure */
        target = Vector3.zero;
        foreach(GameObject treasureItem in GameObject.FindGameObjectsWithTag("Treasure")){
            if(target==null || target==Vector3.zero){
                target = treasureItem.transform.position;
            } else if(Vector3.Distance(transform.position,target) > Vector3.Distance(transform.position,treasureItem.transform.position)){
                target = treasureItem.transform.position;
            }
        }
        
        /* The direction towards the target is transformed into a Vector3 direction */
        AIDirection = (target - transform.position).normalized;

        /* Draw a ray to debug the direction
           It is needed even if the player doesn't move towards the target
         */
        Debug.DrawLine(transform.position, target, Color.magenta, 0.1f, false);

        /* The direction towards the target is transformed into a Vector3 direction
           If the diagonal towards the target is leaning more to a given direction it will go that way
        */
        if(Mathf.Abs(AIDirection.x) >= Mathf.Abs(AIDirection.z)){
            /* If the path is blocked or illegal we go the other way */
            if(!Physics.Raycast(transform.position, new Vector3(Mathf.Sign(AIDirection.x)*Mathf.Ceil(Mathf.Abs(AIDirection.x)),0,0), 6, Blocker)
                &&(new Vector3(Mathf.Sign(AIDirection.x)*Mathf.Ceil(Mathf.Abs(AIDirection.x)),0,0))!=(currentDirection*-1)){
                AIDirection = new Vector3(Mathf.Sign(AIDirection.x)*Mathf.Ceil(Mathf.Abs(AIDirection.x)),0,0);
            } else {
                AIDirection = new Vector3(0,0,Mathf.Sign(AIDirection.z)*Mathf.Ceil(Mathf.Abs(AIDirection.z)));
            }
        } else {
            if(!Physics.Raycast(transform.position, new Vector3(0,0,Mathf.Sign(AIDirection.z)*Mathf.Ceil(Mathf.Abs(AIDirection.z))), 6, Blocker)
                &&(new Vector3(0,0,Mathf.Sign(AIDirection.z)*Mathf.Ceil(Mathf.Abs(AIDirection.z))))!=(currentDirection*-1)){
                AIDirection = new Vector3(0,0,Mathf.Sign(AIDirection.z)*Mathf.Ceil(Mathf.Abs(AIDirection.z)));
            } else {
                AIDirection = new Vector3(Mathf.Sign(AIDirection.x)*Mathf.Ceil(Mathf.Abs(AIDirection.x)),0,0);
            }
        }
        
        // Debug.DrawRay(transform.position, AIDirection * 4, Color.red);
        if(Vector3.Equals(AIDirection,Vector3.zero)){
            // Debug.DrawRay(transform.position, (target - transform.position).normalized*Vector3.Distance(transform.position, target), Color.white, 5f , false);
            // Debug.Log(transform.position.ToString()+" | "+target.ToString());
            // Debug.Log("FOUND A ZERO MATCH!!!");
        }

        /* Draw a ray to debug the direction */
        Debug.DrawRay(transform.position, AIDirection*4, Color.red, 0.1f, false);

        /* Check if there's a blocker in our direct path towards the target
           UNFINISHED!!! Let's finish this sometime in the future
         */
        if(Vector3.Equals(AIDirection,Vector3.forward)||Vector3.Equals(AIDirection,Vector3.back)){
            if(Physics.Raycast(transform.position, AIDirection, Vector3.Distance(transform.position, new Vector3(target.x,transform.position.y,transform.position.z)), Blocker)){
            }
        } else if(Vector3.Equals(AIDirection,Vector3.left)||Vector3.Equals(AIDirection,Vector3.right)){
            if(Physics.Raycast(transform.position, AIDirection, Vector3.Distance(transform.position, new Vector3(transform.position.x,transform.position.y,target.z)), Blocker)){
            }
        }

        if(CanMove(AIDirection) && !Vector3.Equals(AIDirection,Vector3.zero)
            ){
            Move(AIDirection);
        } else {
            /* If we can't move to the desired direction then we just move randomly */
            switch(Random.Range(0,4)){
                case 0:
                    AIDirection = Vector3.forward;
                    break;
                case 1:
                    AIDirection = Vector3.back;
                    break;
                case 2:
                    AIDirection = Vector3.left;
                    break;
                case 3:
                    AIDirection = Vector3.right;
                    break;
            }
            if(CanMove(AIDirection)){
                Move(AIDirection);
            }
        }
        // if((Mathf.Round(Time.realtimeSinceStartup * 10f) / 10f)%0.5f == 0f){
        // }
    }

    bool CanMove(Vector3 direction){

        /* Any movement other than the 4 allowed directions will be invalid */
        if(
            direction != Vector3.forward &&
            direction != Vector3.back &&
            direction != Vector3.right &&
            direction != Vector3.left
            ){
            return false;
        }

        /* Prevents moving backwards unless there's no other way out or we're under stasis effect. */
        if(Vector3.Equals(currentDirection, direction*-1) && !gameObject.GetComponent<PlayerCore>().stasis){
            if( ( !Vector3.Equals(currentDirection*-1, Vector3.forward) == Physics.Raycast(transform.position, Vector3.forward, rayLength, Blocker) ) 
            &&  ( !Vector3.Equals(currentDirection*-1, Vector3.back) == Physics.Raycast(transform.position, Vector3.back, rayLength, Blocker) ) 
            &&  ( !Vector3.Equals(currentDirection*-1, Vector3.left) == Physics.Raycast(transform.position, Vector3.left, rayLength, Blocker) ) 
            &&  ( !Vector3.Equals(currentDirection*-1, Vector3.right) == Physics.Raycast(transform.position, Vector3.right, rayLength, Blocker) ) ){

            } else {
                return false;
            }
        }

        /* Look for blockers. If there's a blocker in the direction we want to move, then we just can't move. */
        if (Vector3.Equals(Vector3.forward, direction) || Vector3.Equals(Vector3.back, direction)){
            if(Physics.Raycast(transform.position + Vector3.right * rayOffsetX, direction, rayLength, Blocker)) return false;
            if(Physics.Raycast(transform.position - Vector3.right * rayOffsetX, direction, rayLength, Blocker)) return false;
        } else if (Vector3.Equals(Vector3.left, direction) || Vector3.Equals(Vector3.right, direction)){
            if(Physics.Raycast(transform.position + Vector3.forward * rayOffsetZ, direction, rayLength, Blocker)) return false;
            if(Physics.Raycast(transform.position - Vector3.forward * rayOffsetZ, direction, rayLength, Blocker)) return false;
        }

        /* Looking for blockers from the stasis bubble. */
        if(Physics.Raycast(transform.position, direction + Vector3.down * 1.5f, rayLength*2, Blocker)) return false;

        return true;
    }

    public ParticleSystem BattleParticles;
    public List<GameObject> Duelers;
    public GameObject Enemy;
    void OnTriggerEnter(Collider other){

        if(other.gameObject.tag=="Player" && Enemy==null){

            if(other.gameObject.GetComponent<PlayerCore>()!=null){

                if(active && !other.gameObject.GetComponent<PlayerCore>().stasis){

                    //This is the other player. A potential enemy.
                    Enemy = other.gameObject;
                    Duelers.Clear();
                    Duelers.Add(gameObject);
                    Duelers.Add(Enemy);
                    EventManager.OnPlayerCollide(Duelers.ToArray());
                    active = false;
                    
                    // if(Random.Range(0,2)==1){
                    //     // Player wants to fight!
                    //     // Enemy = other.gameObject;
                    //     moving = false;
                    //     transform.position = targetPosition;
                    //     // Duelers.Clear();
                    //     // Duelers.Add(gameObject);
                    //     // Duelers.Add(Enemy);
                    //     moves = 0;
                    //     // BattleParticles.Play();
                    //     // EventManager.OnBattleTrigger(Duelers.ToArray());
                    // } else {
                    //     // Player has chosen peace for now...
                    // }
                    
                }
            }
        }

    }

    void OnTriggerExit(Collider other){
        Enemy = null;
    }
}
