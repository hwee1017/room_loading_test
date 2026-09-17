using UnityEngine;

public class CorridorTrigger : MonoBehaviour
{
    [SerializeField]
    private RoomStreamManager roomManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        roomManager.EnterCorridor();
    }
}