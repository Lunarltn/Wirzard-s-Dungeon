using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//오브젝트 풀링 관리
public class PoolManager
{
    #region Pool
    class Pool
    {
        public GameObject Original { get; private set; }//실제 오브젝트
        public Transform Root { get; private set; }//상위 폴더

        Stack<Poolable> _poolStack = new Stack<Poolable>();

        public Poolable Init(GameObject original)
        {
            Original = original;
            Poolable poolable = original.GetComponent<Poolable>();
            if (poolable.IsNotParent == false)
            {
                Root = new GameObject().transform;
                Root.name = $"{original.name}_Root";
            }
            return poolable;
        }

        Poolable Create()
        {
            GameObject go = Object.Instantiate<GameObject>(Original);
            go.name = Original.name;
            return go.GetOrAddComponent<Poolable>();
        }

        public void Push(Poolable poolable)
        {
            if (poolable == null)
                return;

            if (poolable.IsNotParent == false)
                poolable.transform.SetParent(Root);
            poolable.gameObject.SetActive(false);

            _poolStack.Push(poolable);
        }

        public Poolable Pop(Transform parent)
        {
            Poolable poolable;

            if (_poolStack.Count > 0)
                poolable = _poolStack.Pop();
            else
                poolable = Create();

            if (poolable.IsNotParent == false)
            {
                if (parent == null)
                    poolable.transform.parent = Managers.Scene.CurrentScene.transform;
                poolable.transform.SetParent(parent);
            }
            poolable.gameObject.SetActive(true);

            return poolable;
        }
    }
    #endregion

    Dictionary<string, Pool> _pool = new Dictionary<string, Pool>();
    Transform _root;

    public void Init()
    {
        if (_root == null)
        {
            _root = new GameObject { name = "@Pool_Root" }.transform;
            Object.DontDestroyOnLoad(_root);
        }
    }

    public void CreatePool(GameObject original)
    {
        Pool pool = new Pool();
        Poolable poolable = pool.Init(original);
        if (poolable.IsNotParent == false)
            pool.Root.parent = _root;

        _pool.Add(original.name, pool);
    }

    public void Push(Poolable poolable)
    {
        string name = poolable.gameObject.name;
        if (_pool.ContainsKey(name) == false)
        {
            GameObject.Destroy(poolable.gameObject);
            return;
        }

        _pool[name].Push(poolable);
    }

    public Poolable Pop(GameObject original, Transform parent = null)
    {
        if (_pool.ContainsKey(original.name) == false)
            CreatePool(original);
        return _pool[original.name].Pop(parent);
    }

    public GameObject GetOriginal(string name)
    {
        if (_pool.ContainsKey(name) == false)
            return null;
        return _pool[name].Original;
    }

    public void Clear()
    {
        foreach (Transform child in _root)
            GameObject.Destroy(child.gameObject);
        _pool.Clear();
    }
}
