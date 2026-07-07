using UnityEngine;
using UnityEngine.UI;

public class PlayerHud : MonoBehaviour
{
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Slider energyBar;
    [SerializeField] private Text speedText;
    [SerializeField] private string speedSuffix = " m/s";
    [SerializeField] private bool drawFallbackHud = true;

    private const float FallbackWidth = 220f;
    private const float FallbackHeight = 22f;

    private void Awake()
    {
        if (playerEnergy == null)
        {
            playerEnergy = FindFirstObjectByType<PlayerEnergy>();
        }

        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
        }
    }

    private void OnEnable()
    {
        if (playerEnergy != null)
        {
            playerEnergy.EnergyChanged += UpdateEnergyBar;
        }
    }

    private void OnDisable()
    {
        if (playerEnergy != null)
        {
            playerEnergy.EnergyChanged -= UpdateEnergyBar;
        }
    }

    private void Start()
    {
        if (playerEnergy != null)
        {
            UpdateEnergyBar(playerEnergy.CurrentEnergy, playerEnergy.MaxEnergy);
        }
    }

    private void Update()
    {
        if (speedText != null && playerMovement != null)
        {
            speedText.text = $"{playerMovement.CurrentSpeed:0.0}{speedSuffix}";
        }
    }

    private void UpdateEnergyBar(float currentEnergy, float maxEnergy)
    {
        if (energyBar == null)
            return;

        energyBar.maxValue = maxEnergy;
        energyBar.value = currentEnergy;
    }

    private void OnGUI()
    {
        if (!drawFallbackHud || playerEnergy == null || playerMovement == null)
            return;

        if (energyBar == null)
        {
            Rect background = new Rect(20f, 20f, FallbackWidth, FallbackHeight);
            Rect fill = new Rect(20f, 20f, FallbackWidth * playerEnergy.Normalized, FallbackHeight);

            GUI.Box(background, string.Empty);
            GUI.Box(fill, string.Empty);
            GUI.Label(new Rect(24f, 20f, FallbackWidth, FallbackHeight), $"Energy {playerEnergy.CurrentEnergy:0}/{playerEnergy.MaxEnergy:0}");
        }

        if (speedText == null)
        {
            GUI.Label(new Rect(20f, 48f, FallbackWidth, FallbackHeight), $"Speed {playerMovement.CurrentSpeed:0.0}{speedSuffix}");
        }
    }
}
