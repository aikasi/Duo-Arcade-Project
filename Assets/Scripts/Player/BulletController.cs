using Unity.Netcode;
using UnityEngine;

public class BulletController : NetworkBehaviour
{
    // 행동 타입
    public enum BulletBehavior
    {
        SpaceExplosion,
        OceanNet,
        BodyShrink
    }

    public BulletBehavior behaviorType;
    public float speed = 20f;
    public float lifeTime = 3f; // 3f 뒤 삭제

    // 총알을 쏜 플레이어 ID
    public ulong shooterId;

    // 리소스 
    public GameObject explosionPrefab;

    private bool isNetFull = false;

    public override void OnNetworkSpawn()
    {
        // 총알 생성
        if(IsServer)
        {
         GetComponent<Rigidbody2D>().linearVelocity = transform.up * speed;
                Destroy(gameObject, lifeTime);
        }
    }

    private void  OnTriggerEnter2D(Collider2D other)
    {
        // 충돌 처리 (예: 적 맞추기)
        if (!IsServer) return;

        // 이미 잡은 상태면 추가 충돌 무시
        if (behaviorType == BulletBehavior.OceanNet && isNetFull) return;

        bool hit = false;
        int scoreToAdd = 0;

        if (other.CompareTag("Target"))
        {
            Debug.Log("타겟 명중! +2");
            scoreToAdd = 2;
            hit = true;

        }
        else if (other.CompareTag("Trap"))
        {
            Debug.Log("함정 명중! -1");
            scoreToAdd = -1;
            hit = true;
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            GetComponent<NetworkObject>().Despawn();
        }

        if (hit)
        {
            EnemyController enemy = other.GetComponent<EnemyController>();

            if (enemy != null)
            {
                switch (behaviorType)
                {
                    case BulletBehavior.BodyShrink:
                        enemy.StartShrinkDeath();
                        DespawnBullet();
                        break;

                    case BulletBehavior.OceanNet:
                        enemy.StartOceanCaptureDeath();
                        DespawnBullet(); // 총알 즉시 삭제
                        break;
                    case BulletBehavior.SpaceExplosion:
                    default:
                        SpawnExplosionEffect(transform.position);
                        enemy.InstantDeath();
                        DespawnBullet();
                        break;
                }
            }
        AddScoreToShooter(scoreToAdd);
        }
        
    }

    private void DespawnBullet()
    {
        GetComponent<NetworkObject>().Despawn();
    }



    void SpawnExplosionEffect(Vector3 pos)
    {
        if (explosionPrefab != null)
        {
            // 이펙트는 단순 파티클 - NetworkObject 없어도 됨
            Instantiate(explosionPrefab, pos, Quaternion.identity);
        }
    }

    void AddScoreToShooter(int score)
    {
        // 점수 적용 로직
        if (score != 0 && NetworkManager.Singleton.ConnectedClients.TryGetValue(shooterId, out var client))
        {
            var playerState = client.PlayerObject.GetComponent<PlayerStateManager>();
            if (playerState != null) playerState.AddScore(score);
        }
    }
}
