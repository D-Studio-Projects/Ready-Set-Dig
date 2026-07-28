using UnityEngine;

public readonly struct DigResult
{
    #region Fields

    private readonly Vector2 _position;
    private readonly float _radius;
    private readonly int _dirtCells;
    private readonly int _stoneCells;
    private readonly int _ironCells;
    private readonly int _goldCells;

    #endregion

    #region Properties

    public Vector2 Position => _position;

    public float Radius => _radius;

    public int DirtCells => _dirtCells;

    public int StoneCells => _stoneCells;

    public int IronCells => _ironCells;

    public int GoldCells => _goldCells;

    public int TotalCells => _dirtCells + _stoneCells + _ironCells + _goldCells;

    public bool HasChanges => TotalCells > 0;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public DigResult(
        Vector2 _position,
        float _radius,
        int _dirtCells,
        int _stoneCells,
        int _ironCells,
        int _goldCells)
    {
        this._position = _position;
        this._radius = _radius;
        this._dirtCells = Mathf.Max(0, _dirtCells);
        this._stoneCells = Mathf.Max(0, _stoneCells);
        this._ironCells = Mathf.Max(0, _ironCells);
        this._goldCells = Mathf.Max(0, _goldCells);
    }

    public DigResult Add(DigResult _other)
    {
        return new DigResult(
            _position,
            _radius,
            _dirtCells + _other.DirtCells,
            _stoneCells + _other.StoneCells,
            _ironCells + _other.IronCells,
            _goldCells + _other.GoldCells
        );
    }

    #endregion

    #region Private Methods

    #endregion
}
