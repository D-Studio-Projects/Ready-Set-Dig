using UnityEngine;

public class EnergyObstacle : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private float _energyDrainMultiplier = 2f;

    #endregion

    #region Properties

    public float EnergyDrainMultiplier => Mathf.Max(1f, _energyDrainMultiplier);

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
