using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DiggingMadness.Tests.PlayMode
{
    public class ShopUIInteractionTests
    {
        private const BindingFlags FieldFlags =
            BindingFlags.Instance | BindingFlags.NonPublic;

        [UnityTest]
        public IEnumerator EquipmentUpgradeAttemptKeepsNewRunAvailable()
        {
            GameObject shopObject = new GameObject("ShopUITest");
            shopObject.SetActive(false);
            ShopUI shop = shopObject.AddComponent<ShopUI>();
            Button upgradeButton = CreateButton("Upgrade", shopObject.transform);
            Button startRunButton = CreateButton("StartRun", shopObject.transform);
            GameObject panelRoot = new GameObject("Panel");
            panelRoot.transform.SetParent(shopObject.transform);
            panelRoot.SetActive(false);

            SetField(shop, "_panelRoot", panelRoot);
            SetField(shop, "_upgradeButton", upgradeButton);
            SetField(shop, "_startRunButton", startRunButton);
            SetField(shop, "_selectedItemId", "missing-test-item");

            bool newRunRequested = false;
            shop.NewRunRequested += () => newRunRequested = true;

            shopObject.SetActive(true);
            shop.Open();

            upgradeButton.onClick.Invoke();
            yield return null;

            Assert.IsTrue(startRunButton.interactable);

            startRunButton.onClick.Invoke();
            Assert.IsTrue(newRunRequested);

            Object.Destroy(shopObject);
            yield return null;
        }

        private static Button CreateButton(string _name, Transform _parent)
        {
            GameObject buttonObject = new GameObject(
                _name,
                typeof(RectTransform),
                typeof(Button)
            );
            buttonObject.transform.SetParent(_parent);
            return buttonObject.GetComponent<Button>();
        }

        private static void SetField<T>(ShopUI _shop, string _fieldName, T _value)
        {
            FieldInfo field = typeof(ShopUI).GetField(_fieldName, FieldFlags);
            Assert.IsNotNull(field, $"ShopUI field '{_fieldName}' was not found.");
            field.SetValue(_shop, _value);
        }
    }
}
