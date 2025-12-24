using System.Collections.Generic;
using UnityEngine;

public class GameResourceManager : MonoBehaviour
{
    public static GameResourceManager Instance;

    [Header("월드 테마 데이터 (ID 0, 1, 2 순서대로 연결)")]
    public List<WorldThemeSO> worldThemes;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public WorldThemeSO GetTheme(int worldId)
    {
        if (worldId >= 0 && worldId < worldThemes.Count)
            return worldThemes[worldId];
        return null;
    }
}