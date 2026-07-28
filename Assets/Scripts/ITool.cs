public interface ITool
{
    #region Properties

    ToolData Data { get; }

    #endregion

    #region Public Methods

    bool CanUse();

    int Use();

    #endregion
}