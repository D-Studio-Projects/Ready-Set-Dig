using System;
using UnityEngine;

public class TerrainObstacle : MonoBehaviour
{
    #region Fields

    private TerrainChunk _ownerChunk;
    private bool _isResolved;

    #endregion

    #region Properties

    public TerrainChunk OwnerChunk => _ownerChunk;

    public bool IsResolved => _isResolved;

    #endregion

    #region Events

    public event Action<TerrainObstacle, ObstacleResolution> Resolved;

    #endregion

    #region Public Methods

    public void InitializeOwner(TerrainChunk _chunk)
    {
        _ownerChunk = _chunk;
        _isResolved = false;
    }

    public bool Resolve(ObstacleResolution _resolution)
    {
        if (_isResolved)
            return false;

        _isResolved = true;
        OnResolved(_resolution);
        Resolved?.Invoke(this, _resolution);

        if (_ownerChunk != null)
            _ownerChunk.NotifyObstacleRemoved(this);

        gameObject.SetActive(false);
        Destroy(gameObject);
        return true;
    }

    #endregion

    #region Protected Methods

    protected virtual void OnResolved(ObstacleResolution _resolution)
    {
    }

    #endregion
}
