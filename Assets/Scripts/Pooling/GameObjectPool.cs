using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GameObjectPool : MonoBehaviour
{
    #region Fields

    [Header("Pool Configuration")]
    [SerializeField]
    private GameObject _prefab;

    [SerializeField]
    private int _initialCapacity = 4;

    [SerializeField]
    private int _maxSize = 10;

    [SerializeField]
    private bool _collectionCheck = true;

    [SerializeField]
    private Transform _inactiveContainer;

    private readonly Dictionary<GameObject, PooledObject> _pooledObjects = new Dictionary<GameObject, PooledObject>();
    private ObjectPool<GameObject> _pool;
    private bool _isInitialized;
    private bool _hasLoggedPoolLimit;

    #endregion

    #region Properties

    public int CountAll => _pool == null ? 0 : _pool.CountAll;

    public int CountInactive => _pool == null ? 0 : _pool.CountInactive;

    public int CountActive => _pool == null ? 0 : _pool.CountActive;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializePool();
    }

    private void OnDestroy()
    {
        if (_pool == null)
            return;

        _pool.Clear();
        _pool = null;
        _pooledObjects.Clear();
    }

    #endregion

    #region Public Methods

    public GameObject Get(
        Vector3 _position,
        Quaternion _rotation,
        Transform _parent = null)
    {
        if (!_isInitialized || !CanCreateObject())
            return null;

        GameObject instance = _pool.Get();

        if (instance == null || !_pooledObjects.TryGetValue(instance, out PooledObject pooledObject))
        {
            Debug.LogError("The pool instance is missing its cached PooledObject reference.", this);
            return null;
        }

        pooledObject.Spawn(
            _position,
            _rotation,
            _parent == null ? transform : _parent
        );

        return instance;
    }

    public bool Release(PooledObject _pooledObject)
    {
        if (!_isInitialized || _pooledObject == null)
            return false;

        if (_pooledObject.IsInPool)
        {
            if (_collectionCheck)
                Debug.LogWarning("A pooled object was returned more than once.", _pooledObject);

            return false;
        }

        _pool.Release(_pooledObject.gameObject);
        return true;
    }

    public void Clear()
    {
        if (_pool != null)
            _pool.Clear();
    }

    #endregion

    #region Private Methods

    private void InitializePool()
    {
        if (_prefab == null)
        {
            Debug.LogError("GameObjectPool needs a prefab reference.", this);
            return;
        }

        _initialCapacity = Mathf.Max(0, _initialCapacity);
        _maxSize = Mathf.Max(1, _maxSize);

        if (_initialCapacity > _maxSize)
            _initialCapacity = _maxSize;

        if (_inactiveContainer == null)
            _inactiveContainer = transform;

        _pool = new ObjectPool<GameObject>(
            CreateObject,
            OnTakeObject,
            OnReleaseObject,
            OnDestroyObject,
            _collectionCheck,
            _initialCapacity,
            _maxSize
        );

        _isInitialized = true;
        Prewarm();
    }

    private void Prewarm()
    {
        List<GameObject> prewarmedObjects = new List<GameObject>(_initialCapacity);

        for (int index = 0; index < _initialCapacity; index++)
        {
            GameObject instance = _pool.Get();

            if (instance != null)
                prewarmedObjects.Add(instance);
        }

        foreach (GameObject instance in prewarmedObjects)
            _pool.Release(instance);
    }

    private GameObject CreateObject()
    {
        GameObject instance = Instantiate(_prefab, _inactiveContainer);
        instance.SetActive(false);

        PooledObject pooledObject = instance.GetComponent<PooledObject>();

        if (pooledObject == null)
        {
            Debug.LogError("The pool prefab needs a PooledObject component.", this);
            Destroy(instance);
            return null;
        }

        pooledObject.SetOwnerPool(this);
        _pooledObjects.Add(instance, pooledObject);
        return instance;
    }

    private void OnTakeObject(GameObject _instance)
    {
        if (_instance != null && _pooledObjects.TryGetValue(_instance, out PooledObject pooledObject))
            pooledObject.MarkTakenFromPool();
    }

    private void OnReleaseObject(GameObject _instance)
    {
        if (_instance != null && _pooledObjects.TryGetValue(_instance, out PooledObject pooledObject))
            pooledObject.PrepareForReturn(_inactiveContainer);
    }

    private void OnDestroyObject(GameObject _instance)
    {
        if (_instance != null)
        {
            _pooledObjects.Remove(_instance);
            Destroy(_instance);
        }
    }

    private bool CanCreateObject()
    {
        if (_pool.CountInactive > 0 || _pool.CountAll < _maxSize)
            return true;

        if (!_hasLoggedPoolLimit)
        {
            Debug.LogWarning(
                "GameObjectPool reached its maximum size. Increase Max Size if this is unexpected.",
                this
            );
            _hasLoggedPoolLimit = true;
        }

        return false;
    }

    #endregion
}