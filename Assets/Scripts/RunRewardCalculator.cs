using System;
using UnityEngine;

public class RunRewardCalculator : MonoBehaviour
{
    #region Fields

    private const float DefaultBlocksPerMoney = 100f;
    private const float DefaultDepthPerMoney = 50f;
    private const float DefaultBlocksPerScore = 1000f;
    private const float DefaultScorePerDepth = 2f;

    [Header("Money")]
    [Tooltip("Amount of dug blocks required to earn one unit of money.")]
    [SerializeField]
    [Min(0.0001f)]
    private float _blocksPerMoney = DefaultBlocksPerMoney;

    [Tooltip("Amount of depth required to earn one unit of money.")]
    [SerializeField]
    [Min(0.0001f)]
    private float _depthPerMoney = DefaultDepthPerMoney;

    [Header("Score")]
    [Tooltip("Amount of dug blocks required to earn one score point.")]
    [SerializeField]
    [Min(0.0001f)]
    private float _blocksPerScore = DefaultBlocksPerScore;

    [Tooltip("Score points earned for each unit of depth reached.")]
    [SerializeField]
    [Min(0.0001f)]
    private float _scorePerDepthUnit = DefaultScorePerDepth;

    #endregion

    #region Properties

    #endregion

    #region Events

    #endregion

    #region Unity Methods

#if UNITY_EDITOR
    private void OnValidate()
    {
        _blocksPerMoney = ValidatePositiveFiniteValue(
            _blocksPerMoney,
            DefaultBlocksPerMoney
        );
        _depthPerMoney = ValidatePositiveFiniteValue(
            _depthPerMoney,
            DefaultDepthPerMoney
        );
        _blocksPerScore = ValidatePositiveFiniteValue(
            _blocksPerScore,
            DefaultBlocksPerScore
        );
        _scorePerDepthUnit = ValidatePositiveFiniteValue(
            _scorePerDepthUnit,
            DefaultScorePerDepth
        );
    }
#endif

    #endregion

    #region Public Methods

    public RunReward Calculate(RunResult _result)
    {
        return Calculate(_result, 1f);
    }

    public RunReward Calculate(RunResult _result, float _moneyMultiplier)
    {
        decimal baseMoney = CalculateBaseMoney(_result);

        return new RunReward(
            ApplyMoneyMultiplier(baseMoney, _moneyMultiplier),
            CalculateScore(_result.DugBlocks, _result.Depth)
        );
    }

    public long CalculateScore(int _dugBlocks, float _depth)
    {
        long scoreFromBlocks = DivideBlocksForScore(_dugBlocks);
        long scoreFromDepth = MultiplyDepthForScore(_depth);
        return AddSaturated(scoreFromBlocks, scoreFromDepth);
    }

    #endregion

    #region Private Methods

    private decimal CalculateBaseMoney(RunResult _result)
    {
        decimal blocksPerMoney = (decimal)ValidatePositiveFiniteValue(
            _blocksPerMoney,
            DefaultBlocksPerMoney
        );
        decimal depthPerMoney = (decimal)ValidatePositiveFiniteValue(
            _depthPerMoney,
            DefaultDepthPerMoney
        );

        decimal moneyFromBlocks = _result.DugBlocks <= 0
            ? 0m
            : (decimal)_result.DugBlocks / blocksPerMoney;

        decimal moneyFromDepth = !IsPositiveFinite(_result.Depth)
            ? 0m
            : (decimal)_result.Depth / depthPerMoney;

        decimal directMoney = Math.Max(0, _result.CollectedMoney);
        decimal mineralMoney = ToPositiveMoneyDecimal(
            _result.CollectedMineralValue
        );

        return AddMoneySaturated(
            AddMoneySaturated(
                AddMoneySaturated(moneyFromBlocks, moneyFromDepth),
                directMoney
            ),
            mineralMoney
        );
    }

    private long DivideBlocksForScore(int _dugBlocks)
    {
        if (_dugBlocks <= 0)
            return 0;

        decimal blocksPerScore = (decimal)ValidatePositiveFiniteValue(
            _blocksPerScore,
            DefaultBlocksPerScore
        );
        decimal result = (decimal)_dugBlocks / blocksPerScore;

        return result >= long.MaxValue
            ? long.MaxValue
            : (long)Math.Floor(result);
    }

    private long MultiplyDepthForScore(float _depth)
    {
        if (!IsPositiveFinite(_depth))
            return 0;

        decimal scorePerDepth = (decimal)ValidatePositiveFiniteValue(
            _scorePerDepthUnit,
            DefaultScorePerDepth
        );
        decimal result = (decimal)_depth * scorePerDepth;

        return result >= long.MaxValue
            ? long.MaxValue
            : (long)Math.Floor(result);
    }

    private long AddSaturated(long _left, long _right)
    {
        if (_right <= 0)
            return Math.Max(0, _left);

        if (long.MaxValue - _left < _right)
            return long.MaxValue;

        return _left + _right;
    }

    private decimal AddMoneySaturated(decimal _left, decimal _right)
    {
        decimal safeLeft = Math.Max(0m, _left);
        decimal safeRight = Math.Max(0m, _right);

        if (safeLeft >= long.MaxValue ||
            safeRight >= long.MaxValue ||
            safeLeft > long.MaxValue - safeRight)
        {
            return long.MaxValue;
        }

        return safeLeft + safeRight;
    }

    private long ApplyMoneyMultiplier(
        decimal _baseMoney,
        float _multiplier)
    {
        if (_baseMoney <= 0m)
            return 0;

        float safeMultiplier =
            float.IsNaN(_multiplier) || float.IsInfinity(_multiplier)
                ? 1f
                : Mathf.Max(1f, _multiplier);

        decimal maximumSafeMultiplier = long.MaxValue / _baseMoney;
        if (safeMultiplier >= (float)maximumSafeMultiplier)
            return long.MaxValue;

        decimal result = _baseMoney * (decimal)safeMultiplier;
        return result >= long.MaxValue
            ? long.MaxValue
            : (long)Math.Floor(result);
    }

    private decimal ToPositiveMoneyDecimal(float _value)
    {
        if (!IsPositiveFinite(_value))
            return 0m;

        if (_value >= long.MaxValue)
            return long.MaxValue;

        return (decimal)_value;
    }

    private bool IsPositiveFinite(float _value)
    {
        return _value > 0f &&
               !float.IsNaN(_value) &&
               !float.IsInfinity(_value);
    }

    private float ValidatePositiveFiniteValue(float _value, float _fallback)
    {
        return IsPositiveFinite(_value) ? _value : _fallback;
    }

    #endregion
}
