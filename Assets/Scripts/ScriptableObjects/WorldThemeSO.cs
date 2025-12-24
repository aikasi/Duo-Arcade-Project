using UnityEngine;

[CreateAssetMenu(fileName = "NewWorldTheme", menuName = "Game/World Theme Data")]
public class WorldThemeSO : ScriptableObject
{
    public string worldName;

    [Header("적 프리팹 (통째로 교체)")]
    public GameObject[] enemyPrefabs;      // 일반 적
    public GameObject[] trapEnemyPrefabs;  // 함정 적

    [Header("총알 프리팹 (통째로 교체)")]
    public GameObject bulletPrefab;     // 총알
}