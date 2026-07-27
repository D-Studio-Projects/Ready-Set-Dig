public interface ITool
{
    #region Properties

    ToolData Data { get; }

    #endregion

    #region Methods

    bool CanUse();

    int Use();

    #endregion
}
