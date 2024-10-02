using Cysharp.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class QuizRoom : BaseRoom
{
    [SerializeField]
    Transform[] _bridges = new Transform[2];
    [SerializeField]
    Transform[] _altars = new Transform[6];
    public Transform CameraFollow;
    Vector3[] _points = new Vector3[9];
    //0 2 4
    //1 3 5
    const string BALL_PATH = "Object/Effect/AltarFlameBall";
    const int GRAPH_SIZE = 6;

    AltarFlameBall[] _altarFlameBalls = new AltarFlameBall[9];
    Graph _graph;
    int _currentRound;
    Vector3 _cameraFollowInitLocalPostion;
    readonly float[,] _mask = new float[9, 2]
        {                {0, -0.5f},
        {0.5f, -0.25f},  {0, -0.25f}, {-0.5f, -0.25f},
                         {0, 0},
        {0.5f, 0.25f},   {0, 0.25f},  {-0.5f, 0.25f},
                         {0, 0.5f}};

    class Graph
    {
        int n;
        int[,] matrix;
        public Graph(int vertices)
        {
            n = vertices;
            matrix = new int[vertices, vertices];
            for (int i = 0; i < vertices; i++)
                for (int j = 0; j < vertices; j++)
                    matrix[i, j] = -1;
        }

        public void InsertEdge(int start, int end, int pointIndex)
        {
            if (n <= start || n <= end)
                return;
            matrix[start, end] = pointIndex;
            matrix[end, start] = pointIndex;
        }

        public List<int> GetIndexList(int max)
        {
            if (max >= n) max = n - 1;
            List<int> list = new List<int>();
            bool[] visit = new bool[n];
            int now = 0;

            const int max_search_count = 5;
            for (int i = 0; i < max_search_count; i++)
            {

                while (true)
                {
                    visit[now] = true;
                    List<int> node = new List<int>();
                    for (int j = 0; j < n; j++)
                    {
                        if (matrix[now, j] == -1 || visit[j])
                            continue;
                        node.Add(j);
                    }
                    if (node.Count == 0)
                        break;
                    int index = UnityEngine.Random.Range(0, node.Count);
                    list.Add(matrix[now, node[index]]);
                    now = node[index];
                }


                if (list.Count >= max)
                    break;

                list = new List<int>();
                visit = new bool[n];
                now = 0;
            }

            while (list.Count > max)
            {
                list.RemoveAt(UnityEngine.Random.Range(0, list.Count));
            }

            return list;
        }
    }

    void Start()
    {
        //룸 포지션, 사이즈 계산
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
        for (int i = 0; i < _altars.Length; i++)
        {
            if (_altars[i].position.x > max.x) max.x = _altars[i].position.x;
            if (_altars[i].position.z > max.y) max.y = _altars[i].position.z;
            if (_altars[i].position.x < min.x) min.x = _altars[i].position.x;
            if (_altars[i].position.z < min.y) min.y = _altars[i].position.z;
        }
        roomSize = new Vector2(max.x - min.x, max.y - min.y);
        roomPosition = new Vector3(min.x + roomSize.x / 2, 2.6f, min.y + roomSize.y / 2);

        //포인트 배치
        for (int i = 0; i < _points.Length; i++)
        {
            _points[i] = new Vector3(roomPosition.x + roomSize.x * _mask[i, 1], roomPosition.y,
                roomPosition.z + roomSize.y * _mask[i, 0]);
        }

        _graph = new Graph(GRAPH_SIZE);
        _graph.InsertEdge(0, 1, 0);
        _graph.InsertEdge(0, 2, 1);
        _graph.InsertEdge(0, 3, 2);
        _graph.InsertEdge(1, 2, 2);
        _graph.InsertEdge(1, 3, 3);
        _graph.InsertEdge(2, 3, 4);
        _graph.InsertEdge(2, 4, 5);
        _graph.InsertEdge(2, 5, 6);
        _graph.InsertEdge(3, 4, 6);
        _graph.InsertEdge(3, 5, 7);
        _graph.InsertEdge(4, 5, 8);

        NextRound().Forget();

        _cameraFollowInitLocalPostion = CameraFollow.localPosition;

        Managers.Test.GetAction(1, () => CompleteQuiz().Forget());
    }

    public async UniTask NextRound(float delay = 0)
    {
        switch (_currentRound)
        {
            case 0:
                RefillFlameBalls(3);
                break;
            case 1:
                ResetQuiz();
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
                RefillFlameBalls(4);
                break;
            case 2:
                ResetQuiz();
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
                RefillFlameBalls(5);
                break;
            case 3:
                ResetQuiz();
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
                CompleteQuiz().Forget();
                break;
        }
        _currentRound++;
    }

    async UniTask CompleteQuiz()
    {
        var bridgePosition = (_bridges[0].position + _bridges[1].position) / 2;
        await DOTween.Sequence()
            .AppendInterval(0.5f)
            .Append(CameraFollow.DOLocalMoveZ(bridgePosition.z, 1f))
            .AppendInterval(1)
            .Append(_bridges[0].DOLocalMoveY(-3.7f, 1))
            .Join(_bridges[1].DOLocalMoveY(-3.7f, 1))
            .AppendInterval(1)
            .Append(CameraFollow.DOLocalMoveZ(_cameraFollowInitLocalPostion.z, 1f));
        _bridges[0].GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 0;
        _bridges[1].GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 0;
    }

    void ResetQuiz()
    {
        for (int i = 0; i < _altarFlameBalls.Length; i++)
        {
            if (_altarFlameBalls[i] != null)
            {
                _altarFlameBalls[i].Disable();
                _altarFlameBalls[i] = null;
            }
        }
    }

    void RefillFlameBalls(int count)
    {
        var indexList = _graph.GetIndexList(count);
        foreach (var index in indexList)
        {
            if (_altarFlameBalls[index] != null)
                _altarFlameBalls[index].Init(2);
            else
            {
                var go = Managers.Resource.Instantiate(BALL_PATH);
                go.transform.position = _points[index];
                _altarFlameBalls[index] = go.GetComponent<AltarFlameBall>();
                _altarFlameBalls[index].Init(1);
            }
        }
    }

    public bool IsCompletedQuiz()
    {
        bool result = true;
        bool isNull = true;
        for (int i = 0; i < _altarFlameBalls.Length; i++)
        {
            if (_altarFlameBalls[i] != null)
            {
                result = result && _altarFlameBalls[i].IsConnectedAll;
                isNull = false;
            }
        }

        return result ^ isNull;
    }
}
