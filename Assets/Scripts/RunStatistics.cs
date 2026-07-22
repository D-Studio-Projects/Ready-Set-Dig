using UnityEngine;

public class RunStatistics : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private RunManager _runManager;

    private float _time;
    private float _distance;
    private int _money;
    private int _dugBlocks;
    private int _dashCount;

    #endregion

    #region Properties

    public float Time => _time;

    public float Distance => _distance;

    public int Money => _money;

    public int DugBlocks => _dugBlocks;

    public int DashCount => _dashCount;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void Update()
    {
        if (_runManager != null && _runManager.IsRunning)
        {
            _time += UnityEngine.Time.deltaTime;
        }
    }

    #endregion

    #region Public Methods

    public void AddDistance(float amount)
    {
        if (amount <= 0f)
            return;

        _distance += amount;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        _money += amount;
    }

    public void AddDiggedBlocks(int amount)
    {
        if (amount <= 0)
            return;

        _dugBlocks += amount;
    }

    public void IncrementDashCount()
    {
        _dashCount++;
    }

    public void Reset()
    {
        _time = 0f;
        _distance = 0f;
        _money = 0;
        _dugBlocks = 0;
        _dashCount = 0;
    }

    #endregion

    #region Private Methods

    #endregion
}
