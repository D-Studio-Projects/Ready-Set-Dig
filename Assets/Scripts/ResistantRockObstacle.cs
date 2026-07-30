using System;
using UnityEngine;

public class ResistantRockObstacle : TerrainObstacle
{
    #region Fields

    [Header("Optional Scene Override")]
    [SerializeField]
    private ResistantRockQteController _qteController;

    private bool _hasLoggedMissingController;

    #endregion

    #region Events

    public event Action<ResistantRockObstacle> DashHit;
    public event Action<ResistantRockObstacle> QteRequested;

    #endregion

    #region Unity Methods

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (IsResolved || !TryGetPlayerMovement(_other, out PlayerMovement playerMovement))
            return;

        if (playerMovement.IsDashing)
        {
            DashHit?.Invoke(this);
            Resolve(ObstacleResolution.Dash);
            return;
        }

        ResistantRockQteController controller = ResolveQteController();

        if (controller == null)
        {
            if (!_hasLoggedMissingController)
            {
                Debug.LogWarning(
                    "ResistantRockObstacle could not find a ResistantRockQteController.",
                    this
                );
                _hasLoggedMissingController = true;
            }

            return;
        }

        if (controller.TryStartQte(this, playerMovement))
            QteRequested?.Invoke(this);
    }

    #endregion

    #region Private Methods

    private ResistantRockQteController ResolveQteController()
    {
        if (_qteController == null)
            _qteController = ResistantRockQteController.Instance;

        if (_qteController == null)
            _qteController = FindFirstObjectByType<ResistantRockQteController>();

        return _qteController;
    }

    private bool TryGetPlayerMovement(
        Collider2D _other,
        out PlayerMovement _playerMovement)
    {
        _playerMovement = null;

        if (_other == null)
            return false;

        _playerMovement = _other.GetComponentInParent<PlayerMovement>();

        if (_playerMovement == null && _other.attachedRigidbody != null)
        {
            _playerMovement =
                _other.attachedRigidbody.GetComponent<PlayerMovement>();
        }

        return _playerMovement != null;
    }

    #endregion
}
