using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    // 설정
    public GameObject enemyPrefab;
    public GameObject trapEnemyPrefab;
    public float spawnInterval = 2f;

    [Header("구역 이름 (Object의 이름과 같아야 함)")]
    public string leftZoneName = "SpawnZone_Left";
    public string rightZoneName = "SpawnZone_Right";
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(SpawnRoutine("Left", leftZoneName));
            StartCoroutine(SpawnRoutine("Right", rightZoneName));

            // 몬스터 청소
            StartCoroutine(CleanupRoutine());
        }
    }

    IEnumerator SpawnRoutine(string targetRole, string zoneName)
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            PlayerStateManager targetPlayer = null;
            foreach (var player in FindObjectsByType<PlayerStateManager>(FindObjectsSortMode.None))
            {
                if (player.MyRole.Value.ToString() == targetRole)
                {
                    targetPlayer = player;
                    break;
                }
            }

            if (targetPlayer == null || targetPlayer.CurrentState.Value != GameState.Playing)
            {
                continue;
            }

            // 테마에 따라 설정된 프리펩 사용
            int worldId = targetPlayer.SelectedWorldId.Value;
            WorldThemeSO theme = GameResourceManager.Instance.GetTheme(worldId);

            //테마 X 기본 사용
            GameObject prefabToSpawn = enemyPrefab;
            GameObject trapToSpawn = trapEnemyPrefab;

            if (theme != null)
            {

                if (theme.enemyPrefabs != null && theme.enemyPrefabs.Length > 0)
                {
                    int randIdx = Random.Range(0, theme.enemyPrefabs.Length);
                    prefabToSpawn = theme.enemyPrefabs[randIdx];
                }
                if (theme.trapEnemyPrefabs != null && theme.trapEnemyPrefabs.Length > 0)
                {
                    int randIdx = Random.Range(0, theme.trapEnemyPrefabs.Length);
                    trapToSpawn = theme.trapEnemyPrefabs[randIdx];
                }
            }
            
            int i = Random.Range(0, 2);
            GameObject enemyObj = null;

            // 적 생성
            if( i == 0) enemyObj = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);
            else enemyObj = Instantiate(trapToSpawn, Vector3.zero, Quaternion.identity);

            if (enemyObj != null)
            {
                var controller = enemyObj.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.targetSpawnZoneName = zoneName;
                

                    int directionRoll = Random.Range(0, 100);
                    if (directionRoll < 60)
                    {
                        controller.currentSpawnType = EnemyController.SpawnType.Top;
                    }
                    else if (directionRoll < 80)
                    {
                        controller.currentSpawnType = EnemyController.SpawnType.Left;
                    }
                    else
                    {
                        controller.currentSpawnType = EnemyController.SpawnType.Right;
                    }
                }
                enemyObj.GetComponent<NetworkObject>().Spawn();
            }
        }
    }


    IEnumerator CleanupRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            // 모든 플레이어의 상태 확인
            bool anyonePlaying = false;

            foreach (var player in FindObjectsByType<PlayerStateManager>(FindObjectsSortMode.None))
            {
                if (player.CurrentState.Value == GameState.Playing)
                {
                    anyonePlaying = true;
                    break;
                }
            }

            // 아무도 게임중 아니면
            if (!anyonePlaying)
            {
                // 씬에 모든 적 삭제
                var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
                foreach (var enemy in enemies)
                {
                    if (enemy != null && enemy.GetComponent<NetworkObject>().IsSpawned)
                    {
                        enemy.GetComponent<NetworkObject>().Despawn();
                    }
                }
            }
        }
    }

}
