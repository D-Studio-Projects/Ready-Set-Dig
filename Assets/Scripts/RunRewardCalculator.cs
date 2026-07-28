using System;
using UnityEngine;

public class RunRewardCalculator : MonoBehaviour
{
    #region Fields

    [Header("Money")]
    [SerializeField]
    private long _moneyPerBlock = 1;

    [SerializeField]
    private float _moneyPerDepth = 2f;

    [Header("Score")]
    [SerializeField]
    private long _scorePerBlock = 10;

    [SerializeField]
    private float _scorePerDepth = 5f;

    #endregion

    #region Properties

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    public RunReward Calculate(RunResult _result)
    {
        long moneyFromBlocks = MultiplyInteger(_result.DugBlocks, _moneyPerBlock);
        long moneyFromDepth = MultiplyDepth(_result.Depth, _moneyPerDepth);
        long scoreFromBlocks = MultiplyInteger(_result.DugBlocks, _scorePerBlock);
        long scoreFromDepth = MultiplyDepth(_result.Depth, _scorePerDepth);

        return new RunReward(
            AddSaturated(moneyFromBlocks, moneyFromDepth),
            AddSaturated(scoreFromBlocks, scoreFromDepth)
        );
    }

    #endregion

    #region Private Methods

    private long MultiplyInteger(int _value, long _multiplier)
    {
        if (_value <= 0 || _multiplier <= 0)
            return 0;

        decimal result = (decimal)_value * _multiplier;
        return result >= long.MaxValue ? long.MaxValue : (long)result;
    }

    private long MultiplyDepth(float _depth, float _multiplier)
    {
        if (_depth <= 0f || _multiplier <= 0f ||
            float.IsNaN(_depth) || float.IsInfinity(_depth) ||
            float.IsNaN(_multiplier) || float.IsInfinity(_multiplier))
        {
            return 0;
        }

        decimal result = (decimal)_depth * (decimal)_multiplier;
        return result >= long.MaxValue ? long.MaxValue : (long)Math.Floor(result);
    }

    private long AddSaturated(long _left, long _right)
    {
        if (_right <= 0)
            return Math.Max(0, _left);

        if (long.MaxValue - _left < _right)
            return long.MaxValue;

        return _left + _right;
    }

    #endregion
}
