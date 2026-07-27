using UnityEngine;

[CreateAssetMenu(fileName = "ToolData", menuName = "Digging Madness/Tools/Tool Data")]
public class ToolData : ScriptableObject
{
    #region Fields

    [SerializeField]
    private string _toolName;

    [SerializeField]
    private Sprite _icon;

    [SerializeField]
    private float _digDamage = 1f;

    [SerializeField]
    private float _energyConsumption = 8f;

    [SerializeField]
    private float _digSpeed = 1f;

    #endregion

    #region Properties

    public string ToolName => _toolName;

    public Sprite Icon => _icon;

    public float DigDamage => _digDamage;

    public float EnergyConsumption => _energyConsumption;

    public float DigSpeed => _digSpeed;

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    #endregion
}
