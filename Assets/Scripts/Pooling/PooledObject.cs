using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    #region Fields

    private readonly List<IPoolable> _poolableComponents = new List<IPoolable>();
    private readonly List<MonoBehaviour> _behaviours = new List<MonoBehaviour>();
    private ParticleSystem[] _particleSystems;
    private TrailRenderer[] _trailRenderers;
    private GameObjectPool _ownerPool;
    private bool _isInPool = true;
    private int _spawnVersion;

    #endregion

    #region Properties

    public bool IsInPool => _isInPool;

    public int SpawnVersion => _spawnVersion;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void Awake()
    {
        CacheComponents();
    }

    #endregion

    #region Public Methods

    public void SetOwnerPool(GameObjectPool _pool)
    {
        _ownerPool = _pool;
    }

    public void MarkTakenFromPool()
    {
        _isInPool = false;
    }

    public void Spawn(
        Vector3 _position,
        Quaternion _rotation,
        Transform _parent)
    {
        _spawnVersion++;
        transform.SetParent(_parent, false);
        transform.SetPositionAndRotation(_position, _rotation);
        gameObject.SetActive(true);
        RestartVisuals();
        NotifySpawned();
    }

    public bool ReturnToPool()
    {
        if (_ownerPool == null)
        {
            Debug.LogWarning("PooledObject has no owner pool.", this);
            return false;
        }

        return _ownerPool.Release(this);
    }

    public void PrepareForReturn(Transform _inactiveContainer)
    {
        if (_isInPool)
            return;

        _isInPool = true;
        StopBehaviours();
        StopVisuals();
        NotifyReturned();
        gameObject.SetActive(false);
        transform.SetParent(_inactiveContainer, false);
    }

    #endregion

    #region Private Methods

    private void CacheComponents()
    {
        MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
        _behaviours.AddRange(behaviours);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IPoolable poolable)
                _poolableComponents.Add(poolable);
        }

        _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        _trailRenderers = GetComponentsInChildren<TrailRenderer>(true);
    }

    private void RestartVisuals()
    {
        if (_particleSystems != null)
        {
            foreach (ParticleSystem particleSystem in _particleSystems)
            {
                if (particleSystem == null)
                    continue;

                particleSystem.Simulate(0f, true, true);
                particleSystem.Play(true);
            }
        }

        if (_trailRenderers != null)
        {
            foreach (TrailRenderer trailRenderer in _trailRenderers)
            {
                if (trailRenderer != null)
                    trailRenderer.Clear();
            }
        }
    }

    private void StopBehaviours()
    {
        foreach (MonoBehaviour behaviour in _behaviours)
        {
            if (behaviour != null)
                behaviour.StopAllCoroutines();
        }
    }

    private void StopVisuals()
    {
        if (_particleSystems != null)
        {
            foreach (ParticleSystem particleSystem in _particleSystems)
            {
                if (particleSystem != null)
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (_trailRenderers != null)
        {
            foreach (TrailRenderer trailRenderer in _trailRenderers)
            {
                if (trailRenderer != null)
                    trailRenderer.Clear();
            }
        }
    }

    private void NotifySpawned()
    {
        foreach (IPoolable poolable in _poolableComponents)
        {
            if (poolable != null)
                poolable.OnSpawnFromPool();
        }
    }

    private void NotifyReturned()
    {
        foreach (IPoolable poolable in _poolableComponents)
        {
            if (poolable != null)
                poolable.OnReturnToPool();
        }
    }

    #endregion
}
