using System;
using UnityEngine;

public class MineralPickup : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private Collider2D _triggerCollider;

    [Header("Collection")]
    [SerializeField]
    private string _playerTag = "Player";

    private RunStatistics _runStatistics;
    private Transform _playerRoot;
    private MineralData _data;
    private Sprite _fallbackSprite;
    private bool _isAvailable;
    private bool _hasInvalidPlayerTag;

    #endregion

    #region Properties

    public MineralData Data => _data;

    public bool IsAvailable => _isAvailable;

    #endregion

    #region Events

    public event Action<MineralPickup> Collected;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        CacheReferences();
        CacheFallbackSprite();
        ConfigureCollider();
    }

    private void OnEnable()
    {
        CacheReferences();
        CacheFallbackSprite();
        ConfigureCollider();
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        TryCollect(_other == null ? null : _other.gameObject);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CacheReferences();
        ConfigureCollider();
    }
#endif

    #endregion

    #region Public Methods

    public void Initialize(RunStatistics _statistics)
    {
        Initialize(_statistics, null);
    }

    public void Initialize(RunStatistics _statistics, Transform _player)
    {
        _runStatistics = _statistics;
        _playerRoot = _player;
        _hasInvalidPlayerTag = false;
        CacheReferences();
        CacheFallbackSprite();
        ConfigureCollider();
    }

    public void Activate(MineralData _mineral, Vector3 _position)
    {
        if (_mineral == null || !_mineral.HasValidConfiguration())
        {
            Deactivate();
            return;
        }

        _data = _mineral;
        transform.position = _position;
        _isAvailable = true;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        CacheReferences();
        CacheFallbackSprite();
        ConfigureCollider();
        ApplyVisual();
    }

    public void Deactivate()
    {
        _isAvailable = false;
        _data = null;

        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    public bool TryCollect(GameObject _collector)
    {
        if (!_isAvailable || _data == null || !IsPlayer(_collector))
            return false;

        MineralData collectedData = _data;
        _isAvailable = false;

        if (_runStatistics != null)
            _runStatistics.AddMineralValue(collectedData.MoneyValue);

        gameObject.SetActive(false);
        Collected?.Invoke(this);
        return true;
    }

    #endregion

    #region Private Methods

    private void CacheReferences()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (_triggerCollider == null)
            _triggerCollider = GetComponent<Collider2D>();

        if (_triggerCollider == null)
            _triggerCollider = GetComponentInChildren<Collider2D>(true);
    }

    private void CacheFallbackSprite()
    {
        if (_fallbackSprite == null && _spriteRenderer != null)
            _fallbackSprite = _spriteRenderer.sprite;
    }

    private void ConfigureCollider()
    {
        if (_triggerCollider != null)
            _triggerCollider.isTrigger = true;
    }

    private void ApplyVisual()
    {
        if (_spriteRenderer == null || _data == null)
            return;

        _spriteRenderer.sprite = _data.Sprite == null
            ? _fallbackSprite
            : _data.Sprite;
        _spriteRenderer.color = _data.Color;
        _spriteRenderer.enabled = true;
    }

    private bool IsPlayer(GameObject _collector)
    {
        if (_collector == null)
            return false;

        Transform collectorTransform = _collector.transform;

        if (_playerRoot != null &&
            (collectorTransform == _playerRoot || collectorTransform.IsChildOf(_playerRoot)))
        {
            return true;
        }

        if (_hasInvalidPlayerTag || string.IsNullOrWhiteSpace(_playerTag))
            return false;

        Transform current = collectorTransform;

        while (current != null)
        {
            try
            {
                if (current.CompareTag(_playerTag))
                    return true;
            }
            catch (UnityException)
            {
                _hasInvalidPlayerTag = true;
                return false;
            }

            current = current.parent;
        }

        return false;
    }

    #endregion
}
