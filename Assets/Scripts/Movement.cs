using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

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
    [SerializeField]
    Transform characterBody;

    GameManager gameManager;

    float rayLength = 2.25f;
    float rayOffsetX = 0f;
    // float rayOffsetY = 0f;
    float rayOffsetZ = 0f;

    public LayerMask Blocker;
    public bool active = false;

    public int moves = 0;

    Vector3 targetPosition;
    Vector3 startPosition = Vector3.zero;
    Vector3 characterBodyWigglePosition;
    public Vector3 currentDirection;

    bool moving;
    float moveSpeed = 12f;
    float gridSize = 2f;
    // float rotationSpeed = 12f;
    Quaternion headRotation = Quaternion.LookRotation(Vector3.forward);
    public PlayerCore playerCore;

    /* AI Related variables */
    //public bool useAI = false;
    public Vector3 target;
    public List<Tracker> trackers; // The initial trackers
    public List<Vector3> endPositions; // The available end positions
    public List<Vector3> pathLocations; // All the walkable paths on the board
    public List<Vector3> stepsToTarget;
    public bool pathToTargetFound = false;
    public Vector3 trackedTarget; // The position of the player's main target
    /* -------------------- */


    // Start is called before the first frame update
    void Start()
    {
        playerCore = transform.GetComponent<PlayerCore>();
        gameManager = FindAnyObjectByType<GameManager>();
        if (startPosition == Vector3.zero) {
            startPosition = new Vector3(transform.position.x, 0.25f, transform.position.z);
        }
        //if (playerCore.useAI) { StartTracking(); }
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

        if (moving){
            gameObject.GetComponent<PlayerCore>().stasis = false;
            if(Vector3.Distance(startPosition, transform.position) > gridSize){
                transform.position = targetPosition;
                moving = false;
                return;
            }

            transform.position += (targetPosition - startPosition) * moveSpeed * Time.deltaTime;
            characterBody.position += (Vector3.zero - characterBodyWigglePosition) * (moveSpeed/15) * Time.deltaTime;
            return;
        } else if(moves<=0 && active){
            EventManager.OnTurnEnd();
        }

        if(playerCore.useAI){
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
        characterBodyWigglePosition = new Vector3(0 + Random.Range(0, 0.25f), 0, 0 + Random.Range(0, 0.25f));
        startPosition = transform.position;
        moving = true;
        moves --;
        headRotation = Quaternion.LookRotation(currentDirection);
        transform.rotation = headRotation;
        // Quaternion rotation = Quaternion.LookRotation(currentDirection);
        // Debug.Log("Moves: "+moves);
    }
    

    /* Tracking functions ------------------------------------------------------------------------------------------- */
    public void TrackerEndCallback(Vector3 endPosition)
    {
        endPositions.Add(endPosition);
        Debug.DrawRay(endPosition - ((Vector3.left + Vector3.forward) * 0.5f), ((Vector3.left + Vector3.forward) * 1f), Color.blue, 1f);
        Debug.DrawRay(endPosition - ((Vector3.right + Vector3.forward) * 0.5f), ((Vector3.right + Vector3.forward) * 1f), Color.blue, 1f);
    }

    public bool RegisterPath(Vector3 pathLocation)
    {
        bool hasBeenSavedAlready = false;
        pathLocations.ForEach(path =>
        {
            if (pathLocation.x == path.x && pathLocation.z == path.z) {
                hasBeenSavedAlready = true;
            }
        });

        if (!hasBeenSavedAlready) {
            pathLocations.Add(pathLocation);
        }
        return hasBeenSavedAlready;
    }

    public void StartTracking()
    {
        trackers.Clear();
        endPositions.Clear();
        pathLocations.Clear();
        stepsToTarget.Clear();
        pathToTargetFound = false;

        Vector3 eyesPosition = new Vector3(transform.position.x, 0.25f, transform.position.z);
        if (CanMove(Vector3.forward)) { trackers.Add(new Tracker(eyesPosition, Vector3.forward, moves, this)); }
        if (CanMove(Vector3.back)) { trackers.Add(new Tracker(eyesPosition, Vector3.back, moves, this)); }
        if (CanMove(Vector3.left)) { trackers.Add(new Tracker(eyesPosition, Vector3.left, moves, this)); }
        if (CanMove(Vector3.right)) { trackers.Add(new Tracker(eyesPosition, Vector3.right, moves, this)); }

        int maxSteps = 36;
        for (int i = 0; i < maxSteps; i++)
        {
            trackers.ForEach(tracker => tracker.LookAround(TrackerEndCallback, RegisterPath));
        }
    }

    /* ------------------------------------------------------------------------------------------- */

    void doAIMovement(){
        // Debug.Log(Mathf.Round(Time.realtimeSinceStartup * 100f) / 100f);

        Vector3 AIDirection;

        /* The AI's priority is now the treasure */
        if (pathToTargetFound && stepsToTarget.Count > 0) /* If the tracker found the treasure then we follow the path towards it */
        {
            AIDirection = stepsToTarget[0];
            Debug.DrawRay(transform.position + Vector3.up * 0.33f, AIDirection, Color.darkSalmon, 0.5f);
            stepsToTarget.RemoveAt(0);
        } else /* Otherwise, we use the old AI pathfinding (may remove it in the future) */
        {
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
        }
        

        /* Draw a ray to debug the direction */
        Debug.DrawRay(transform.position, AIDirection*4, Color.red, 0.1f, false);

        /* Check if there's a blocker in our direct path towards the target
           UNFINISHED!!! Let's finish this sometime in the future
         */
        bool pathBlocked = false;
        //if(Vector3.Equals(AIDirection,Vector3.forward)||Vector3.Equals(AIDirection,Vector3.back)){
        //    if(Physics.Raycast(transform.position, AIDirection, Vector3.Distance(transform.position, new Vector3(target.x,transform.position.y,transform.position.z)), Blocker)){
        //        pathBlocked = true;
        //    }
        //} else if(Vector3.Equals(AIDirection,Vector3.left)||Vector3.Equals(AIDirection,Vector3.right)){
        //    if(Physics.Raycast(transform.position, AIDirection, Vector3.Distance(transform.position, new Vector3(transform.position.x,transform.position.y,target.z)), Blocker)){
        //        pathBlocked = true;
        //    }
        //}

        if(!pathBlocked && CanMove(AIDirection) && !Vector3.Equals(AIDirection,Vector3.zero)
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


    bool CanMove(Vector3 direction)
    {

        /* Any movement other than the 4 allowed directions will be invalid */
        if (
            direction != Vector3.forward &&
            direction != Vector3.back &&
            direction != Vector3.right &&
            direction != Vector3.left
            )
        {
            return false;
        }

        /* Prevents moving backwards unless there's no other way out or we're under stasis effect. */
        if (Vector3.Equals(currentDirection, direction * -1) && !gameObject.GetComponent<PlayerCore>().stasis)
        {
            if ((!Vector3.Equals(currentDirection * -1, Vector3.forward) == Physics.Raycast(transform.position, Vector3.forward, rayLength, Blocker))
            && (!Vector3.Equals(currentDirection * -1, Vector3.back) == Physics.Raycast(transform.position, Vector3.back, rayLength, Blocker))
            && (!Vector3.Equals(currentDirection * -1, Vector3.left) == Physics.Raycast(transform.position, Vector3.left, rayLength, Blocker))
            && (!Vector3.Equals(currentDirection * -1, Vector3.right) == Physics.Raycast(transform.position, Vector3.right, rayLength, Blocker)))
            {

            }
            else
            {
                return false;
            }
        }

        /* Look for blockers. If there's a blocker in the direction we want to move, then we just can't move. */
        if (Vector3.Equals(Vector3.forward, direction) || Vector3.Equals(Vector3.back, direction))
        {
            if (Physics.Raycast(transform.position + Vector3.right * rayOffsetX, direction, rayLength, Blocker)) return false;
            if (Physics.Raycast(transform.position - Vector3.right * rayOffsetX, direction, rayLength, Blocker)) return false;
        }
        else if (Vector3.Equals(Vector3.left, direction) || Vector3.Equals(Vector3.right, direction))
        {
            if (Physics.Raycast(transform.position + Vector3.forward * rayOffsetZ, direction, rayLength, Blocker)) return false;
            if (Physics.Raycast(transform.position - Vector3.forward * rayOffsetZ, direction, rayLength, Blocker)) return false;
        }

        /* Looking for blockers from the stasis bubble. */
        if (Physics.Raycast(transform.position, direction + Vector3.down * 1.5f, rayLength * 2, Blocker)) return false;

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

[Serializable]
public class Tracker
{
    Vector3 trackerPosition;
    Vector3 incomingDirection;
    int stepsLeft = 0;
    LayerMask Blocker = LayerMask.NameToLayer("Blocker");
    public delegate void TrackEndCallback(Vector3 trackEnd);
    List<Tracker> subtrackers = new();
    List<TrackerFootPrint> footPrints = new();
    Tracker parent;
    public delegate bool PathRegisterCallback(Vector3 path);
    Movement movement;
    bool priorityPath = false; //TRUE if this path leads towards the target
    Vector3 targetLocation;

    public Tracker(Vector3 initialPosition, Vector3 incomingDirection, int stepsLeft, Movement movement, Tracker parent = null)
    {
        this.incomingDirection = incomingDirection;
        this.trackerPosition = initialPosition + (incomingDirection * 2f);
        //this.trackerPosition = initialPosition;
        this.stepsLeft= stepsLeft;
        this.Blocker = movement.Blocker;
        this.movement = movement;
        //this.LookAround(movement.TrackerEndCallback, movement.RegisterPath);
        this.parent= parent;
        this.targetLocation = movement.target;
        //this.footPrints.Add(new TrackerFootPrint(initialPosition, incomingDirection));
    }

    public void LookAround(TrackEndCallback endCallback, PathRegisterCallback pathCallback)
    {
        //this.validDirections.Clear();
        this.stepsLeft--;

        if(this.stepsLeft > 0)
        {
            //Debug.DrawRay(trackerPosition, incomingDirection * -2, Color.yellow, 0.5f);
            bool isDuplicate = pathCallback(trackerPosition);
            if (!isDuplicate)
            {
                CheckAndBranch();
            } else
            {
                DrawDuplicationCross();
            }
        } else if(this.stepsLeft <= 0)
        {
            if(this.stepsLeft == 0)
            {
                //Debug.DrawRay(trackerPosition, incomingDirection * -2, Color.yellow, 0.5f);
                endCallback(trackerPosition);
            }
            bool isDuplicate = pathCallback(trackerPosition);
            if (!isDuplicate) {
                if(this.stepsLeft < 0)
                {
                    //Debug.DrawRay(trackerPosition, incomingDirection * -2, Color.white, 0.5f);
                }
                CheckAndBranch();
            } else
            {
                DrawDuplicationCross();
            }
        }
        subtrackers.ForEach(subtracker => subtracker.LookAround(endCallback, pathCallback));
    }

    public void MoveTrack(Vector3 direction)
    {
        //Debug.DrawRay(trackerPosition, incomingDirection * -2, Color.white, 0.5f);
        this.trackerPosition = trackerPosition + (direction * 2f);
        this.incomingDirection = direction;

        //if(trackerPosition.x == targetLocation.x && trackerPosition.z == targetLocation.z)
        //{
        //    TargetLocationFound();
        //}

        if (!movement.pathToTargetFound)
        {
            RaycastHit hit;
            if (Physics.Raycast(this.trackerPosition + Vector3.up, Vector3.down, out hit, 1f))
            {
                if (hit.transform.CompareTag("Treasure"))
                {
                    this.TargetLocationFound();
                }
            }
        }
    }

    public bool IsDirectionBlocked(Vector3 direction)
    {
        return (Physics.Raycast(this.trackerPosition, direction, 2f, this.Blocker) || Vector3.Equals(direction,this.incomingDirection * -1f));
    }

    public void CheckAndBranch()
    {
        bool shouldMove = false;
        Vector3 moveDirection = Vector3.zero;
        List<Vector3> validDirections = new();
        if (!IsDirectionBlocked(Vector3.forward)) { validDirections.Add(Vector3.forward); }
        if (!IsDirectionBlocked(Vector3.back)) { validDirections.Add(Vector3.back); }
        if (!IsDirectionBlocked(Vector3.left)) { validDirections.Add(Vector3.left); }
        if (!IsDirectionBlocked(Vector3.right)) { validDirections.Add(Vector3.right); }

        if (validDirections.Count > 0)
        {
            footPrints.Add(new TrackerFootPrint(trackerPosition, incomingDirection));
            foreach (Vector3 direction in validDirections)
            {
                if (direction == incomingDirection || validDirections.Count == 1)
                {
                    //Debug.DrawRay(trackerPosition, direction * 1.75f, Color.white, 0.5f);
                    //DrawArrow(direction);
                    //footPrints.Add(new TrackerFootPrint(trackerPosition, incomingDirection));
                    moveDirection = direction;
                    shouldMove = true;
                } else
                {
                    //Debug.DrawRay(trackerPosition, direction * 1.75f, Color.orange, 0.5f);
                    //DrawArrow(direction);
                    Tracker newTracker = new Tracker(trackerPosition, direction, stepsLeft, movement, this);
                    //footPrints.Add(new TrackerFootPrint(trackerPosition, incomingDirection));
                    newTracker.footPrints.AddRange(footPrints);
                    subtrackers.Add(newTracker);
                }
            }
            if (shouldMove)
            {
                    MoveTrack(moveDirection);
            }
        }
        else if (validDirections.Count == 0)
        {
            if (stepsLeft > 0)
            {
                MoveTrack(incomingDirection * -1f);
            }
            //subtrackers.Add(new Tracker(trackerPosition, incomingDirection * -1, stepsLeft, Blocker, endCallback, pathCallback, targetLocation, this));
        }
    }

    public void TargetLocationFound()
    {
        this.priorityPath = true;
        Debug.DrawRay(trackerPosition + Vector3.up*0.05f, incomingDirection * -1.75f, Color.cyan, 0.5f);
        footPrints.ForEach(footprint => {
            footprint.color = Color.cyan;
            footprint.DrawLine();
            movement.stepsToTarget.Add(footprint.incomingDirection);
        });
        movement.pathToTargetFound = true;
        //if (this.parent != null) {
        //    this.parent.TargetLocationFound();
        //    this.parent.footPrints.ForEach((footprint) => {
        //        footprint.color = Color.cyan; 
        //        footprint.DrawLine();
        //    });
        //}
    }
    public void DrawArrow(Vector3 direction)
    {
        if (direction == Vector3.forward)
        {
            Debug.DrawRay(trackerPosition + (Vector3.forward * 1.75f) + (Vector3.back + Vector3.left) * 0.25f, (Vector3.forward + Vector3.right) * 0.25f, Color.white, 0.5f);
            Debug.DrawRay(trackerPosition + (Vector3.forward * 1.75f) + (Vector3.back + Vector3.right) * 0.25f, (Vector3.forward + Vector3.left) * 0.25f, Color.white, 0.5f);
        }
        if (direction == Vector3.back)
        {
            Debug.DrawRay(trackerPosition + (Vector3.back * 1.75f) + (Vector3.forward + Vector3.left) * 0.25f, (Vector3.back + Vector3.right) * 0.25f, Color.white, 0.5f);
            Debug.DrawRay(trackerPosition + (Vector3.back * 1.75f) + (Vector3.forward + Vector3.right) * 0.25f, (Vector3.back + Vector3.left) * 0.25f, Color.white, 0.5f);
        }
        if (direction == Vector3.right)
        {
            Debug.DrawRay(trackerPosition + (Vector3.right * 1.75f) + (Vector3.left + Vector3.back) * 0.25f, (Vector3.right + Vector3.forward) * 0.25f, Color.white, 0.5f);
            Debug.DrawRay(trackerPosition + (Vector3.right * 1.75f) + (Vector3.left + Vector3.forward) * 0.25f, (Vector3.right + Vector3.back) * 0.25f, Color.white, 0.5f);
        }
        if (direction == Vector3.left)
        {
            Debug.DrawRay(trackerPosition + (Vector3.left * 1.75f) + (Vector3.right + Vector3.back) * 0.25f, (Vector3.left + Vector3.forward) * 0.25f, Color.white, 0.5f);
            Debug.DrawRay(trackerPosition + (Vector3.left * 1.75f) + (Vector3.right + Vector3.forward) * 0.25f, (Vector3.left + Vector3.back) * 0.25f, Color.white, 0.5f);
        }
    }

    public void DrawDuplicationCross()
    {
        Debug.DrawRay(trackerPosition + (Vector3.back + Vector3.left) * 0.125f, (Vector3.forward + Vector3.right) * 0.25f, Color.red, 0.5f);
        Debug.DrawRay(trackerPosition + (Vector3.back + Vector3.right) * 0.125f, (Vector3.forward + Vector3.left) * 0.25f, Color.red, 0.5f);
    }
}

public class TrackerFootPrint
{
    public Vector3 position;
    public Vector3 incomingDirection;
    public Color color = Color.white;
    public TrackerFootPrint(Vector3 position, Vector3 incomingDirecion){
        this.position = position;
        this.incomingDirection = incomingDirecion;
    }

    public void DrawLine()
    {
        Debug.DrawRay(position + Vector3.up * 0.05f, incomingDirection * -1.75f, color, 0.5f);
    }
}

//static bool CanMove(Vector3 currentDirection, Vector3 direction, Vector3 playerPosition, PlayerCore playerCore = null)
//{
//    float rayLength = 2.25f;
//    float rayOffsetX = 0f;
//    float rayOffsetZ = 0f;
//    LayerMask Blocker = 7;

//    if (
//            direction != Vector3.forward &&
//            direction != Vector3.back &&
//            direction != Vector3.right &&
//            direction != Vector3.left
//            )
//    {
//        return false;
//    }

//    /* Prevents moving backwards unless there's no other way out or we're under stasis effect. */
//    if (Vector3.Equals(currentDirection, direction * -1) && (playerCore && !playerCore.stasis))
//    {
//        if ((!Vector3.Equals(currentDirection * -1, Vector3.forward) == Physics.Raycast(playerPosition, Vector3.forward, rayLength, Blocker))
//        && (!Vector3.Equals(currentDirection * -1, Vector3.back) == Physics.Raycast(playerPosition, Vector3.back, rayLength, Blocker))
//        && (!Vector3.Equals(currentDirection * -1, Vector3.left) == Physics.Raycast(playerPosition, Vector3.left, rayLength, Blocker))
//        && (!Vector3.Equals(currentDirection * -1, Vector3.right) == Physics.Raycast(playerPosition, Vector3.right, rayLength, Blocker)))
//        {

//        }
//        else
//        {
//            return false;
//        }
//    }

//    /* Look for blockers. If there's a blocker in the direction we want to move, then we just can't move. */
//    if (Vector3.Equals(Vector3.forward, direction) || Vector3.Equals(Vector3.back, direction))
//    {
//        if (Physics.Raycast(playerPosition + Vector3.right * rayOffsetX, direction, rayLength, Blocker)) return false;
//        if (Physics.Raycast(playerPosition - Vector3.right * rayOffsetX, direction, rayLength, Blocker)) return false;
//    }
//    else if (Vector3.Equals(Vector3.left, direction) || Vector3.Equals(Vector3.right, direction))
//    {
//        if (Physics.Raycast(playerPosition + Vector3.forward * rayOffsetZ, direction, rayLength, Blocker)) return false;
//        if (Physics.Raycast(playerPosition - Vector3.forward * rayOffsetZ, direction, rayLength, Blocker)) return false;
//    }

//    /* Looking for blockers from the stasis bubble. */
//    if (Physics.Raycast(playerPosition, direction + Vector3.down * 1.5f, rayLength * 2, Blocker)) return false;

//    return true;
//}