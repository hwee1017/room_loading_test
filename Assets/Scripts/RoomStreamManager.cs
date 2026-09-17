using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomStreamManager : MonoBehaviour
{
    [Header("사용할 방 Scene 이름")]
    [SerializeField]
    private string[] roomScenes =
    {
        "Room_A",
        "Room_B",
        "Room_C"
    };

    [Header("방 연결 위치")]
    [SerializeField] private Transform room1DoorTarget;
    [SerializeField] private Transform room2DoorTarget;

    [Header("다음 방이 로딩될 때까지 막는 벽")]
    [SerializeField] private GameObject loadingGate;

    private string currentRoom;
    private string nextRoom;

    private bool transitioning = false;
    private bool nextRoomReady = false;

    public bool NextRoomReady => nextRoomReady;

    private IEnumerator Start()
    {
        // 처음 방 랜덤 선택
        currentRoom = GetRandomRoom("");

        // Room 1 위치에 방 생성
        yield return LoadRoomAt(currentRoom, room1DoorTarget);

        Debug.Log("첫 번째 방 로딩 완료: " + currentRoom);
    }

    public void EnterCorridor()
    {
        if (transitioning)
            return;

        StartCoroutine(TransitionToNextRoom());
    }

    private IEnumerator TransitionToNextRoom()
    {
        transitioning = true;
        nextRoomReady = false;

        // 통로 끝 막기
        if (loadingGate != null)
            loadingGate.SetActive(true);

        Debug.Log("통로 진입");

        // 기존 방 제거
        if (!string.IsNullOrEmpty(currentRoom))
        {
            AsyncOperation unload =
                SceneManager.UnloadSceneAsync(currentRoom);

            if (unload != null)
                yield return unload;

            Debug.Log("기존 방 제거 완료");
        }

        // 다음 방 랜덤 선택
        nextRoom = GetRandomRoom(currentRoom);

        Debug.Log("다음 방: " + nextRoom);

        // 다음 방 로딩
        yield return LoadRoomAt(nextRoom, room2DoorTarget);

        nextRoomReady = true;

        // 로딩 끝 → 통로 끝 개방
        if (loadingGate != null)
            loadingGate.SetActive(false);

        Debug.Log("다음 방 로딩 완료");

        currentRoom = nextRoom;
        transitioning = false;
    }

    private IEnumerator LoadRoomAt(
        string sceneName,
        Transform targetDoor)
    {
        AsyncOperation loadOperation =
            SceneManager.LoadSceneAsync(
                sceneName,
                LoadSceneMode.Additive
            );

        yield return loadOperation;

        Scene loadedScene =
            SceneManager.GetSceneByName(sceneName);

        RoomDefinition room = null;

        GameObject[] roots =
            loadedScene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            room = root.GetComponentInChildren<RoomDefinition>();

            if (room != null)
                break;
        }

        if (room == null)
        {
            Debug.LogError(
                sceneName +
                "에 RoomDefinition이 없습니다."
            );

            yield break;
        }

        if (room.doorAnchor == null)
        {
            Debug.LogError(
                sceneName +
                "의 DoorAnchor가 지정되지 않았습니다."
            );

            yield break;
        }

        // DoorAnchor 방향을
        // 통로 DoorTarget 방향에 맞춘다.
        Quaternion rotationDelta =
            targetDoor.rotation *
            Quaternion.Inverse(room.doorAnchor.rotation);

        room.transform.rotation =
            rotationDelta * room.transform.rotation;

        // 회전 후 위치를 다시 맞춘다.
        Vector3 positionDelta =
            targetDoor.position -
            room.doorAnchor.position;

        room.transform.position += positionDelta;
    }

    private string GetRandomRoom(string exceptRoom)
    {
        if (roomScenes.Length == 0)
            return "";

        if (roomScenes.Length == 1)
            return roomScenes[0];

        string result;

        do
        {
            int index =
                Random.Range(0, roomScenes.Length);

            result = roomScenes[index];

        } while (result == exceptRoom);

        return result;
    }
}