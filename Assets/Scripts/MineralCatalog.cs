using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MineralCatalog",
    menuName = "Digging Madness/Minerals/Mineral Catalog"
)]
public class MineralCatalog : ScriptableObject
{
    #region Fields

    [SerializeField]
    private List<MineralData> _minerals = new List<MineralData>();

    #endregion

    #region Properties

    public int Count => _minerals == null ? 0 : _minerals.Count;

    public int ValidCount
    {
        get
        {
            if (_minerals == null)
                return 0;

            HashSet<string> ids = new HashSet<string>();
            int count = 0;

            foreach (MineralData mineral in _minerals)
            {
                if (mineral != null &&
                    mineral.HasValidConfiguration() &&
                    ids.Add(mineral.Id))
                {
                    count++;
                }
            }

            return count;
        }
    }

    #endregion

    #region Public Methods

    public MineralData GetAt(int _index)
    {
        if (_minerals == null || _index < 0 || _index >= _minerals.Count)
            return null;

        return _minerals[_index];
    }

    public bool TryGetById(string _id, out MineralData _mineral)
    {
        _mineral = null;

        if (_minerals == null || string.IsNullOrWhiteSpace(_id))
            return false;

        foreach (MineralData mineral in _minerals)
        {
            if (mineral != null && mineral.Id == _id)
            {
                _mineral = mineral;
                return true;
            }
        }

        return false;
    }

    public bool HasAnyValidMineral()
    {
        return ValidCount > 0;
    }

    public bool HasValidConfiguration()
    {
        if (_minerals == null || _minerals.Count == 0)
            return false;

        HashSet<string> ids = new HashSet<string>();

        foreach (MineralData mineral in _minerals)
        {
            if (mineral == null ||
                !mineral.HasValidConfiguration() ||
                !ids.Add(mineral.Id))
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}
