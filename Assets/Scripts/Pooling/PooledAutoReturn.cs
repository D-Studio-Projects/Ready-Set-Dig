using System.Collections;
using UnityEngine;

public class PooledAutoReturn : MonoBehaviour, IPoolable
{
    #region Fields

    [SerializeField]
    private float _lifetime = 1f;

    [SerializeField]
    private bool _useUnscaledTime;

    private PooledObject _pooledObject;
    private Coroutine _returnRoutine;

    #endregion

    #region Properties

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void Awake()
    {
        _pooledObject = GetComponentInParent<PooledObject>();
    }

    #endregion

    #region Public Methods

    public void OnSpawnFromPool()
    {
        if (_pooledObject == null || _lifetime <= 0f)
            return;

        int spawnVersion = _pooledObject.SpawnVersion;
        _returnRoutine = StartCoroutine(ReturnAfterLifetime(spawnVersion));
    }

    public void OnReturnToPool()
    {
        if (_returnRoutine != null)
        {
            StopCoroutine(_returnRoutine);
            _returnRoutine = null;
        }
    }

    #endregion

    #region Private Methods

    private IEnumerator ReturnAfterLifetime(int _spawnVersion)
    {
        if (_useUnscaledTime)
            yield return new WaitForSecondsRealtime(_lifetime);
        else
            yield return new WaitForSeconds(_lifetime);

        _returnRoutine = null;

        if (_pooledObject != null &&
            !_pooledObject.IsInPool &&
            _pooledObject.SpawnVersion == _spawnVersion)
        {
            _pooledObject.ReturnToPool();
        }
    }

    #endregion
}
