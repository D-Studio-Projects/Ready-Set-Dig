using System;
using UnityEngine;

public class Drill : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private Terrain _terrain;

    [SerializeField]
    private PlayerEnergy _playerEnergy;

    [Header("Dig")]
    [SerializeField]
    private float _radius = .4f;

    #endregion

    #region Properties

    #endregion

    #region Events

    public event Action<int> BlocksDug;

    #endregion

    #region Unity Methods

    private void Update()
    {
        if (!CanDig())
            return;

        int dugCells = _terrain.Dig(transform.position, _radius);

        if (dugCells <= 0)
            return;

        BlocksDug?.Invoke(dugCells);
        _playerEnergy.ConsumeDigging(Time.deltaTime);
    }

    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    private bool CanDig()
    {
        return _runManager != null &&
               _runManager.IsRunning &&
               _terrain != null &&
               _playerEnergy != null &&
               _playerEnergy.HasEnergy;
    }

    #endregion
}
