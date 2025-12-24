using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance;

    [Serializable]
    public class RankData
    {
        public string name;
        public int score;
    }

    // 메모리에 저장
    public List<RankData> rankList = new List<RankData>();

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 점수 추가 요청 (공통)
    public void AddScore(string playerName, int score)
    {

        rankList.Add(new RankData { name = playerName, score = score });
        
        // 높은 순서로 정렬후 7명 남김
        rankList = rankList.OrderByDescending(x => x.score).Take(7).ToList();
    }

    // 랭킹 리스트 요청
    public List<RankData> GetRankings()
    {
        return rankList;
    }
}
