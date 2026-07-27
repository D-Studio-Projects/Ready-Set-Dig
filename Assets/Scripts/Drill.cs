using System;
using UnityEngine;

public class Drill : MonoBehaviour, ITool
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private Terrain _terrain;

    [SerializeField]
    private PlayerEnergy _playerEnergy;

    [SerializeField]
    private ToolData _toolData;

    [Header("Dig")]
    [SerializeField]
    private float _baseRadius = .4f;

    private float _digTimer;

    #endregion

    #region Properties

    public ToolData Data => _toolData;

    #endregion

    #region Events

    public event Action<int> BlocksDug;

    #endregion

    #region Unity Methods

    private void Update()
    {
        if (!CanUse())
            return;

        _digTimer += Time.deltaTime;

        if (_digTimer < GetDigInterval())
            return;

        _digTimer = 0f;
        Use();
    }

    #endregion

    #region Public Methods

    public bool CanUse()
    {
        return _runManager != null &&
               _runManager.IsRunning &&
               _terrain != null &&
               _playerEnergy != null &&
               _playerEnergy.HasEnergy &&
               _toolData != null;
    }

    public int Use()
    {
        int dugCells = _terrain.Dig(transform.position, GetDigRadius());

        if (dugCells <= 0)
            return 0;

        BlocksDug?.Invoke(dugCells);
        //_playerEnergy.ConsumeToolEnergy(_toolData.EnergyConsumption, Time.deltaTime);

        return dugCells;
    }

    #endregion

    #region Private Methods

    private float GetDigRadius()
    {
        return _baseRadius * Mathf.Max(0f, _toolData.DigDamage);
    }

    private float GetDigInterval()
    {
        float digSpeed = Mathf.Max(0.01f, _toolData.DigSpeed);
        return 1f / digSpeed;
    }

    #endregion
}
