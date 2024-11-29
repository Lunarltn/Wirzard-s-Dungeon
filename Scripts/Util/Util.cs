using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static T FindParent<T>(GameObject go)
    {
        Transform parent = go.transform;
        while (parent != null)
        {
            T component = parent.GetComponent<T>();
            if (component != null)
                return component;
            parent = parent.parent;
        }
        return default(T);
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;
        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>(true))
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }
        return null;
    }
    //라디안변환
    public static float RadianAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;
        return angle;
    }
    //각도 벡터 변환
    public static Vector2 AngleToVector2(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    public static Vector3 AngleToVector3(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians));
    }

    //벡터 각도 변환
    public static float Vector3ToAngle(Vector3 direction)
    {
        return Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
    }
    //벡터 각도 이동
    public static Vector2 MoveVector2ByAngle(Vector2 vector, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        float newX = (cos * vector.x) - (sin * vector.y);
        float newY = (sin * vector.x) + (cos * vector.y);
        return new Vector2(newX, newY);
    }
    //
    public static Vector3 RotateVectorAroundAxis(Vector3 vector, Vector3 axis, float angle)
    {
        Quaternion rotation = Quaternion.AngleAxis(angle, axis);
        return rotation * vector;
    }
    //아이템 문자열 스플릿
    public static int[] SplitItemString(string str)
    {
        if (str.Equals(string.Empty)) return null;
        string[] splitPlus = str.Split('+');
        int[] splitCommaInt = new int[splitPlus.Length * 2];
        for (int i = 0; i < splitPlus.Length; i++)
        {
            string[] splitCommaStr = splitPlus[i].Split(',');
            splitCommaInt[i * 2] = int.Parse(splitCommaStr[0]);
            splitCommaInt[i * 2 + 1] = int.Parse(splitCommaStr[1]);
        }

        return splitCommaInt;
    }
    //정규화
    public static float Normalize(float x, float xMin, float xMax, float yMin, float yMax)
    {
        return yMin + ((x - xMin) / (xMax - xMin)) * (yMax - yMin);
    }
}
