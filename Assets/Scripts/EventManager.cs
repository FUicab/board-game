using UnityEngine;
using UnityEngine.Events;

public static class EventManager
{
    public static event UnityAction EndTurn;
    public static void OnTurnEnd() => EndTurn?.Invoke();

    public static event UnityAction<GameObject> TreasurePickedUp;
    public static void OnTreasurePickedUp(GameObject player) => TreasurePickedUp?.Invoke(player);

    public static event UnityAction<GameObject> PlayerDeath;
    public static void OnPlayerDeath(GameObject player) => PlayerDeath?.Invoke(player);

    // An array is called. Player in [0] is expected to be the attacker and [1] to be the defender
    public static event UnityAction<GameObject[]> PlayerCollide;
    public static void OnPlayerCollide(GameObject[] players) => PlayerCollide?.Invoke(players);

    // Triggered when the battle has been accepted
    public static event UnityAction BattleAccept;
    public static void OnBattleAccept() => BattleAccept?.Invoke();

    // Triggered when the battle has been rejected
    public static event UnityAction BattleReject;
    public static void OnBattleReject() => BattleReject?.Invoke();

    // Triggered when the battle starts
    public static event UnityAction<GameObject[]> BattleStart;
    public static void OnBattleStart(GameObject[] duelers) => BattleStart?.Invoke(duelers);

    // Triggered when the battle ends
    public static event UnityAction BattleEnd;
    public static void OnBattleEnd() => BattleEnd?.Invoke();
}
