using UnityEngine;

public class DigEffectsController : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField]
    private Drill _drill;

    [SerializeField]
    private GameObjectPool _digParticlesPool;

    [SerializeField]
    private Transform _effectsRoot;

    [Header("Effect")]
    [SerializeField]
    private float _effectScale = 1f;

    #endregion

    #region Properties

    #endregion

    #region Events

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        if (_drill != null)
            _drill.DigCompleted += HandleDigCompleted;
    }

    private void OnDisable()
    {
        if (_drill != null)
            _drill.DigCompleted -= HandleDigCompleted;
    }

    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    private void HandleDigCompleted(DigResult _result)
    {
        if (!_result.HasChanges || _digParticlesPool == null)
            return;

        GameObject effect = _digParticlesPool.Get(
            _result.Position,
            Quaternion.identity,
            _effectsRoot
        );

        if (effect != null && _effectScale > 0f)
            effect.transform.localScale = Vector3.one * _effectScale;
    }

    #endregion
}
