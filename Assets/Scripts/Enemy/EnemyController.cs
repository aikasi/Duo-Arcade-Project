using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    // 등장 패턴
    public enum SpawnType
    {
        Top,Left,Right
    }
    public SpawnType currentSpawnType = SpawnType.Top;

    public string targetSpawnZoneName = "SpawnZone";

    // 구역 설정
    public BoxCollider2D SpawnArea;


    // 이동 설정
    public float moveSpeed = 3f;
    public float turnSpeed = 2.5f;
    public float turnAngle = 50f;

    // 내부 변수
    private float baseAngle = 180f;
    private float timeOffset;

    // 콜라이더 범위 설정
    private float minX, maxX, minY, maxY;


    private bool isDead = false;
    private bool isCaught = false;
    private Transform captorNet;

    public GameObject caughtNetObject; // 잡혔을 때 그물 오브젝트(자식)

    // 적 생성
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GameObject zoneObj = GameObject.Find(targetSpawnZoneName);

            Collider2D spawnArea = null;
            if (zoneObj != null)
            {
                spawnArea = zoneObj.GetComponent<Collider2D>();

                // 범위 값 계산
                    minX = spawnArea.bounds.min.x + 0.5f;
                    maxX = spawnArea.bounds.max.x - 0.5f;
                    minY = spawnArea.bounds.min.y + 0.5f;
                    maxY = spawnArea.bounds.max.y - 0.5f;

            }

            if (spawnArea == null)
            {
                if (IsServer) GetComponent<NetworkObject>().Despawn();
                return;
            }

            //등장 타입에 따라 위치와 각도 설정
            switch (currentSpawnType)
            {
                case SpawnType.Left: // 왼쪽 에서 등장 > 오른쪽으로 이동
                    transform.position = new Vector3(spawnArea.bounds.min.x, Random.Range(minY, maxY), 0f);
                    baseAngle = 270f; // 오른쪽 방향
                    break;

                case SpawnType.Right: // 오른쪽 벽에서 등장 > 왼쪽으로 이동
                    transform.position = new Vector3(spawnArea.bounds.max.x, Random.Range(minY, maxY), 0f);
                    baseAngle = 90f; // 왼쪽 방향
                    break;

                case SpawnType.Top: // 위에서 등장 > 아래로 이동
                default:
                    transform.position = new Vector3(Random.Range(minX, maxX), spawnArea.bounds.max.y, 0f);
                    baseAngle = 180f; // 아래쪽 방향
                    break;
            }

            // 랜덤 움직임 오프셋
            timeOffset = Random.Range(0f, 10f);
        }
    }

    private void Update()
    {
        if(!IsServer) return;
        if (isDead) return;

        // 해양 - 그물에 잡혔을 시
        if (isCaught)
        {
            if (captorNet != null)
            {
                transform.position = captorNet.position;
            }
            else
            {

                GetComponent<NetworkObject>().Despawn();
            }
            return;
        }

        // 벽 충돌 체크
        CheckBoundary();

        // S 움직임 로직
        float wave = Mathf.Sin((Time.time + timeOffset) * turnSpeed);
        float currentAngle = baseAngle + (wave * turnAngle);

        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
        transform.position += transform.up * moveSpeed * Time.deltaTime;

        if (CheckDespawnCondition())
        {
            GetComponent<NetworkObject>().Despawn();
        }

    }

    bool CheckDespawnCondition()
    {
        if (currentSpawnType == SpawnType.Top && transform.position.y < -10f) return true;

        if (currentSpawnType == SpawnType.Left && transform.position.x > 10f) return true; 

        if (currentSpawnType == SpawnType.Right && transform.position.x < -10f) return true;

        return false;
    }

    void CheckBoundary()
    {
        if (currentSpawnType == SpawnType.Top)
        {
            // 왼쪽 벽 충돌
            if (transform.position.x <= minX)
            {
                baseAngle = 225f; // 오른쪽 아래으로 회전
            }
            // 오른쪽 벽 충돌
            else if (transform.position.x >= maxX)
            {
                baseAngle = 135f; // 왼쪽 아래로 회전
            }
        }
        else
        {
            if (transform.position.y >= maxY)
            {
                baseAngle = (currentSpawnType == SpawnType.Left) ? 225f : 135f;
            }
            else if (transform.position.y <= minY)
            {
                baseAngle = (currentSpawnType == SpawnType.Left) ? 315f : 45f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (collision.CompareTag("Wall"))
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    // ----------------------------------------
    // 테마별 피격 

    // 1. 우주 - 폭팔 > Bullet 처리
    public void InstantDeath()
    {
        if(isDead) return;
        isDead = true;
        GetComponent<NetworkObject>().Despawn();
    }

    // 2. 해양 - 그물에 잡혀 끌려가기
    public void StartOceanCaptureDeath()
    {
        if (isDead) return;
        isDead = true;
        // 중복 피격 방지
        Collider2D col = GetComponent<Collider2D>();
        if(col != null)
        {
            col.enabled = false;
        }

        // 그물위치 수정
        transform.rotation = Quaternion.identity;

        if (caughtNetObject != null)
        {
            caughtNetObject.SetActive(true);
        }
        // 연출 시작
        StartCoroutine(OceanCaptureRoutine());
    }

    // 3. 인체 - 서서히 작아지다 삭제
    public void StartShrinkDeath()
    {
        if (isDead) return;
        isDead = true;

        // 작아지는 연출 실행
        StartCoroutine(ShrinkRoutine());

        // 삭제 대기
        StartCoroutine(ServerDestoryDelay(0.5f));

    }

    IEnumerator OceanCaptureRoutine()
    {
        // 1ch 동안 위로 떠오름
        float floatDuration = 1.0f;
        float elapsed = 0f;

        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            // 위로 이동
            transform.position += Vector3.up * Time.deltaTime;
            yield return null;
        }

        // 알파값 줄어들며 사라짐
        float fadeDuration = 0.5f;
        float fadeElapsed = 0f;

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);

            foreach (var r in renderers)
            {
                Color c = r.color;
                c.a = alpha;
                r.color = c;
            }

            // 페이드 중 올라가게
            transform.position += Vector3.up * 1f * Time.deltaTime;
            yield return null;
        }

        GetComponent<NetworkObject>().Despawn();
    }

    // 해양 연출
    IEnumerator ShrinkRoutine()
    {
        float duration = 0.5f;
            float elapsed = 0f;
        Vector3 initialScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
            yield return null;
        }
        transform.localScale = Vector3.zero;

    }

    IEnumerator ServerDestoryDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GetComponent<NetworkObject>().Despawn();
    }

}

