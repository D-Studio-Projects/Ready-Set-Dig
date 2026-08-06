using NUnit.Framework;

namespace DiggingMadness.Tests.EditMode
{
    public class GlobalUpgradeDefinitionTests
    {
        [Test]
        public void DashCountUpgradeAddsOneChargePerLevel()
        {
            GlobalUpgradeDefinition definition = new GlobalUpgradeDefinition(
                GlobalUpgradeType.DashCount,
                "Quantidade de dash",
                5,
                100,
                10,
                1f
            );

            Assert.AreEqual(0, definition.GetFlatBonus(0));
            Assert.AreEqual(1, definition.GetFlatBonus(1));
            Assert.AreEqual(5, definition.GetFlatBonus(5));
        }
    }
}
