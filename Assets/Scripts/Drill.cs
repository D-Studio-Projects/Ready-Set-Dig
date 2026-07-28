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

    private ToolData _activeToolData;
    private float _digTimer;

    #endregion

    #region Properties

    public ToolData Data => _activeToolData == null ? _toolData : _activeToolData;

    #endregion

    #region Events

    public event Action<DigResult> DigCompleted;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        ResetEquipment();
    }

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
            Data.EnergyConsumption,
            elapsedSinceDig
        );
    }

    #endregion

    #region Public Methods

    public void ApplyEquipment(ToolData _equipmentData)
    {
        _activeToolData = _equipmentData == null ? _toolData : _equipmentData;
        _digTimer = 0f;
    }

    public void ResetEquipment()
    {
        _activeToolData = _toolData;
        _digTimer = 0f;
    }

    public bool CanUse()
    {
        return _runManager != null &&
               _runManager.IsRunning &&
               _terrain != null &&
               _playerEnergy != null &&
               _playerEnergy.HasEnergy &&
               Data != null;
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
        return _baseRadius * Mathf.Max(0f, Data.DigDamage);
    }

    private float GetDigInterval()
    {
        float digSpeed = Mathf.Max(.01f, Data.DigSpeed);
        return 1f / digSpeed;
    }

    #endregion
}
