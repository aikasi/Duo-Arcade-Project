using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerGunController : NetworkBehaviour
{
    // 셋팅
    public float rotateSpeed = 50f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    private bool isPositionSet = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            var rb = GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        if (IsServer && IsOwner)
        {
            // 1. 눈에 보이는 그림(Renderer)만 다 끕니다.
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }

            // 2. 충돌체(Collider)도 다 끕니다.
            foreach (var c in GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }

            // 3. (선택) 총 쏘는 기능도 막고 싶다면 enabled = false;
            this.enabled = false;
        }
    }

    void TrySetSpawnPosition()
    {
        var state = GetComponent<PlayerStateManager>();
        if (state != null)
        {
            string role = state.MyRole.Value.ToString();

            if (string.IsNullOrEmpty(role)) return;

            if (SpawnPointManager.Instance != null)
            {
                Transform targetTransform = SpawnPointManager.Instance.GetSpawnPoint(role);

                if (targetTransform != null)
                {
                    isPositionSet = true;

                    transform.rotation = Quaternion.identity;

                    StartCoroutine(IntroMoveRoutine(targetTransform.position));


                }
            }
        }
    }

    IEnumerator IntroMoveRoutine(Vector3 targetPos)
    {
        float duration = 1.0f;
        float elapsed = 0f;

        Vector3 startPos = targetPos + new Vector3(0, -5f, 0);

        transform.position = startPos;
        transform.rotation = Quaternion.identity;

        transform.position = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.identity;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = Quaternion.identity;
    }


    private void Update()
    {
        // 대기 상태 초기화
        var state = GetComponent<PlayerStateManager>();
        if (state != null && state.CurrentState.Value == GameState.StandBy)
        {
            isPositionSet = false;
        }

        if (!isPositionSet)
        {
            TrySetSpawnPosition();
        }
    }

    // 클라이언트
    public void RequestRotate(float direction)  // -1, 1 : 좌, 우
    {
        if (IsOwner)
        {
            RotateServerRpc(direction);
        }
    }

    public void RequestFire()
    {
        if(IsOwner)
        {
            FireServerRpc();
        }
    }

    // 서버
    [ServerRpc]
    void RotateServerRpc(float direction)
    {
        // 회전
        float rotationAmount = -direction * rotateSpeed * Time.deltaTime;
        transform.Rotate(0, 0, rotationAmount);

        // 각도 제한
        float z = transform.localEulerAngles.z;
        if (z > 180) z -= 360; // -180 ~ 180 범위로 변환
        z = Mathf.Clamp(z, -45f, 45f);
        transform.localRotation = Quaternion.Euler(0, 0, z);
    }

    [ServerRpc]
    void FireServerRpc()
    {
        // 총알대신 테마 총알 가져오기
        GameObject prefabToUse = bulletPrefab;

        var state = GetComponent<PlayerStateManager>();
        if (state != null && GameResourceManager.Instance != null)
        {
            int worldId = state.SelectedWorldId.Value;
            var theme = GameResourceManager.Instance.GetTheme(worldId);

            // 테마에 총알 있는 경우
            if (theme != null && theme.bulletPrefab != null)
            {
                prefabToUse = theme.bulletPrefab;
            }
        }
        if (prefabToUse == null) return;

        GameObject bullet = Instantiate(prefabToUse, firePoint.position, firePoint.rotation);

        // 총알 정보 설정
        var bulletScript = bullet.GetComponent<BulletController>();
        if (bulletScript != null)
        {
            bulletScript.shooterId = OwnerClientId;
        }

        var netObj = bullet.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }

    }
}
