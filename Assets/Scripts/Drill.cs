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
    private Collider2D _playerCollider;

    [SerializeField]
    private ToolData _toolData;

    [Header("Dig")]
    [SerializeField]
    private float _baseRadius = .4f;

    [SerializeField]
    private float _tunnelPadding = .1f;

    private ToolData _activeToolData;
    private DrillStats _activeDrillStats;
    private bool _hasActiveStats;
    private bool _hasLastDigPosition;
    private Vector2 _lastDigPosition;
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
        if (_playerCollider == null)
            _playerCollider = GetComponentInChildren<Collider2D>();

        ResetEquipment();
    }

    private void Update()
    {
        if (!CanUse())
        {
            _digTimer = 0f;
            _hasLastDigPosition = false;
            return;
        }

        if (!_hasLastDigPosition)
        {
            _lastDigPosition = transform.position;
            _hasLastDigPosition = true;
        }

        _digTimer += Time.deltaTime;
        float digInterval = GetDigInterval();

        if (_digTimer < digInterval)
            return;

        float elapsedSinceDig = _digTimer;
        _digTimer = 0f;
        Vector2 currentPosition = transform.position;
        DigResult result = PerformDigPath(_lastDigPosition, currentPosition);
        _lastDigPosition = currentPosition;

        if (!result.HasChanges)
            return;

        DigCompleted?.Invoke(result);
        _playerEnergy.ConsumeToolEnergy(
            _activeDrillStats.EnergyConsumption,
            elapsedSinceDig
        );
    }

    #endregion

    #region Public Methods

    public void ApplyEquipment(ToolData _equipmentData)
    {
        _activeToolData = _equipmentData == null ? _toolData : _equipmentData;
        _activeDrillStats = DrillStats.FromToolData(_activeToolData);
        _hasActiveStats = _activeDrillStats.IsValid();
        _digTimer = 0f;
        _hasLastDigPosition = false;
    }

    public void ApplyEquipment(DrillStats _stats)
    {
        if (!_stats.IsValid())
        {
            ResetEquipment();
            return;
        }

        _activeToolData = _toolData;
        _activeDrillStats = _stats;
        _hasActiveStats = true;
        _digTimer = 0f;
        _hasLastDigPosition = false;
    }

    public void ResetEquipment()
    {
        _activeToolData = _toolData;
        _activeDrillStats = DrillStats.FromToolData(_toolData);
        _hasActiveStats = _activeDrillStats.IsValid();
        _digTimer = 0f;
        _hasLastDigPosition = false;
    }

    public bool CanUse()
    {
        return _runManager != null &&
               _runManager.IsRunning &&
               _terrain != null &&
               _playerEnergy != null &&
               _playerEnergy.HasEnergy &&
               _hasActiveStats &&
               _activeDrillStats.IsValid();
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
        return _terrain.Dig(transform.position, GetTunnelRadius());
    }

    private DigResult PerformDigPath(Vector2 _startPosition, Vector2 _endPosition)
    {
        return _terrain.DigPath(_startPosition, _endPosition, GetTunnelRadius());
    }

    private float GetDigRadius()
    {
        return _baseRadius * Mathf.Max(0f, _activeDrillStats.DigDamage);
    }

    private float GetTunnelRadius()
    {
        float playerRadius = _playerCollider == null
            ? 0f
            : Mathf.Max(
                _playerCollider.bounds.extents.x,
                _playerCollider.bounds.extents.y
            ) + Mathf.Max(0f, _tunnelPadding);

        return Mathf.Max(GetDigRadius(), playerRadius);
    }

    private float GetDigInterval()
    {
        float digSpeed = Mathf.Max(.01f, _activeDrillStats.DigSpeed);
        return 1f / digSpeed;
    }

    #endregion
}
