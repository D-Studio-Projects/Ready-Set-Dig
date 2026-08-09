using NUnit.Framework;
using UnityEngine;

public class MineralRewardTests
{
    [Test]
    public void Calculate_PreservesMineralDecimalsUntilFinalFloor()
    {
        GameObject calculatorObject = new GameObject("RewardCalculatorTest");
        RunRewardCalculator calculator =
            calculatorObject.AddComponent<RunRewardCalculator>();
        RunResult result = new RunResult(
            1,
            1f,
            2.5f,
            10,
            0,
            4.9f,
            RunEndReason.EnergyDepleted
        );

        RunReward reward = calculator.Calculate(result);

        Assert.That(reward.EarnedMoney, Is.EqualTo(5));
        Assert.That(reward.Score, Is.EqualTo(5));
        Object.DestroyImmediate(calculatorObject);
    }

    [Test]
    public void RunStatistics_AddMineralValueAccumulatesConfiguredValues()
    {
        GameObject statisticsObject = new GameObject("RunStatisticsTest");
        RunStatistics statistics = statisticsObject.AddComponent<RunStatistics>();

        statistics.AddMineralValue(1.3f);
        statistics.AddMineralValue(1.6f);
        statistics.AddMineralValue(2f);

        Assert.That(statistics.CollectedMineralValue, Is.EqualTo(4.9f).Within(.001f));
        Assert.That(statistics.Money, Is.EqualTo(4));
        RunResult result = statistics.CreateResult(
            1,
            RunEndReason.EnergyDepleted
        );
        Assert.That(result.CollectedMineralValue, Is.EqualTo(4.9f).Within(.001f));

        statistics.Reset();

        Assert.That(statistics.CollectedMineralValue, Is.Zero);
        Object.DestroyImmediate(statisticsObject);
    }
}
