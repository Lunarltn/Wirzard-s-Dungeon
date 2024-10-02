using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolDestinations : MonoBehaviour
{
    List<Transform> _destinations = new List<Transform>();
    List<bool> _visited = new List<bool>();
    bool _init;

    void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            _destinations.Add(child);
            _visited.Add(false);
        }
    }

    public Vector3 GetPositionNearestDestination(Vector3 position)
    {
        float min = float.MaxValue;
        int minIndex = 0;
        int count = 0;
        for (int i = 0; i < _destinations.Count; i++)
        {
            Vector3 dest = _destinations[i].position;
            if (_init == false && Vector3.Distance(position, dest) < 1)
            {
                _init = true;
                _visited[i] = true;
            }

            if (min > Vector3.Distance(position, dest) && _visited[i] == false)
            {
                min = Vector3.Distance(position, dest);
                minIndex = i;
            }
            if (_visited[i]) count++;
        }

        if (_visited.Count - 1 <= count)
            for (int i = 0; i < _destinations.Count; i++)
                _visited[i] = false;

        _visited[minIndex] = true;

        return _destinations[minIndex].position;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            if (_visited.Count > 0 && _visited[i])
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.GetChild(i).position, 0.5f);
        }
    }
}
