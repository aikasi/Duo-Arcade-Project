using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    // 어디서든 접근 가능
    public static SpawnPointManager Instance;

    // 에디터에서 스폰직접 연결
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(Instance);
    }


    // 역할에 맞는 Transform을 반환
    public Transform GetSpawnPoint(string role)
    {
        if (role == "Left") return leftSpawnPoint;
        else if (role == "Right") return rightSpawnPoint;
        return null;
    }
}
