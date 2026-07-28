using System;
using UnityEngine;

public class Drill : MonoBehaviour, ITool
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private TerrainChunkManager _terrain;

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

    public event Action<DigResult> DigCompleted;

    #endregion

    #region Unity Methods

    private void Update()
    {
        if (!CanUse())
        {
            _digTimer = 0f;
            return;
        }

        _digTimer += Time.deltaTime;
        float digInterval = GetDigInterval();

        if (_digTimer < digInterval)
            return;

        float elapsedSinceDig = _digTimer;
        _digTimer = 0f;
        DigResult result = PerformDig();

        if (!result.HasChanges)
            return;

        DigCompleted?.Invoke(result);
        _playerEnergy.ConsumeToolEnergy(
            _toolData.EnergyConsumption,
            elapsedSinceDig
        );
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
        DigResult result = PerformDig();

        if (result.HasChanges)
            DigCompleted?.Invoke(result);

        return result.TotalCells;
    }

    #endregion

    #region Private Methods

    private DigResult PerformDig()
    {
        return _terrain.Dig(transform.position, GetDigRadius());
    }

    private float GetDigRadius()
    {
        return _baseRadius * Mathf.Max(0f, _toolData.DigDamage);
    }

    private float GetDigInterval()
    {
        float digSpeed = Mathf.Max(.01f, _toolData.DigSpeed);
        return 1f / digSpeed;
    }

    #endregion
}