using System;
using System.IO;
using UnityEngine;

public class PlayerProgressService : MonoBehaviour
{
    #region Fields

    private const int CurrentSaveVersion = 1;
    private const string SaveFileName = "player_progress.json";
    private const string SaveTemporarySuffix = ".tmp";
    private const string SaveBackupSuffix = ".backup.json";

    [Header("References")]
    [SerializeField]
    private RunManager _runManager;

    [SerializeField]
    private RunRewardCalculator _rewardCalculator;

    private PlayerProgressData _data;
    private int _lastAppliedRunId;
    private bool _hasLoaded;
    private bool _hasLoggedRewardCalculatorError;

    #endregion

    #region Properties

    public long TotalMoney => _data == null ? 0 : _data.TotalMoney;

    public float BestDepth => _data == null ? 0f : _data.BestDepth;

    public long BestScore => _data == null ? 0 : _data.BestScore;

    public int TotalRuns => _data == null ? 0 : _data.TotalRuns;

    public long TotalDugBlocks => _data == null ? 0 : _data.TotalDugBlocks;

    public string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public string SaveBackupPath => SaveFilePath + SaveBackupSuffix;

    #endregion

    #region Events

    public event Action ProgressChanged;
    public event Action<ProgressUpdate> RunSettled;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        Load();
    }

    private void OnEnable()
    {
        if (_runManager != null)
            _runManager.RunFinished += HandleRunFinished;
    }

    private void OnDisable()
    {
        if (_runManager != null)
            _runManager.RunFinished -= HandleRunFinished;
    }

    #endregion

    #region Public Methods

    public bool ApplyRunResult(RunResult _result)
    {
        if (!CanApplyRunResult(_result))
            return false;

        RunReward reward = _rewardCalculator.Calculate(_result);
        _data.ApplyRunResult(
            _result.Depth,
            reward.Score,
            reward.EarnedMoney,
            _result.DugBlocks,
            out long previousMoney,
            out bool isNewBestDepth,
            out bool isNewBestScore
        );

        _lastAppliedRunId = _result.RunId;
        Save();
        ProgressChanged?.Invoke();

        ProgressUpdate update = new ProgressUpdate(
            _result,
            reward.Score,
            reward.EarnedMoney,
            previousMoney,
            TotalMoney,
            isNewBestDepth,
            isNewBestScore
        );
        RunSettled?.Invoke(update);
        return true;
    }

    public bool Save()
    {
        if (!_hasLoaded)
            Load();

        if (_data == null)
            return false;

        string temporaryPath = SaveFilePath + SaveTemporarySuffix;

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            string json = JsonUtility.ToJson(_data, true);
            File.WriteAllText(temporaryPath, json);

            if (!File.Exists(temporaryPath))
                throw new IOException("The progress temporary file was not created.");

            if (File.Exists(SaveFilePath))
                File.Copy(SaveFilePath, SaveBackupPath, true);

            File.Copy(temporaryPath, SaveFilePath, true);
            File.Delete(temporaryPath);
            return true;
        }
        catch (IOException _exception)
        {
            LogSaveError(_exception);
            return false;
        }
        catch (UnauthorizedAccessException _exception)
        {
            LogSaveError(_exception);
            return false;
        }
        catch (Exception _exception)
        {
            LogSaveError(_exception);
            return false;
        }
    }

    public void DeleteProgress()
    {
        DeleteFileIfExists(SaveFilePath);
        DeleteFileIfExists(SaveFilePath + SaveTemporarySuffix);
        DeleteFileIfExists(SaveBackupPath);
        DeleteInvalidSaveFiles();

        _data = CreateDefaultData();
        _lastAppliedRunId = 0;
        _hasLoaded = true;
        Save();
        ProgressChanged?.Invoke();
    }

    #endregion

    #region Private Methods

    private void HandleRunFinished(RunResult _result)
    {
        ApplyRunResult(_result);
    }

    private void Load()
    {
        _data = CreateDefaultData();
        _hasLoaded = true;

        if (!File.Exists(SaveFilePath))
        {
            Save();
            ProgressChanged?.Invoke();
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                HandleInvalidSave("The progress file is empty.");
                return;
            }

            PlayerProgressData loadedData = JsonUtility.FromJson<PlayerProgressData>(json);

            if (loadedData == null || loadedData.SaveVersion != CurrentSaveVersion)
            {
                HandleInvalidSave("The progress file has an unsupported save version.");
                return;
            }

            _data = loadedData;

            if (_data.Normalize())
                Save();

            ProgressChanged?.Invoke();
        }
        catch (ArgumentException _exception)
        {
            HandleInvalidSave("The progress file contains invalid JSON.", _exception);
        }
        catch (IOException _exception)
        {
            LogLoadError(_exception);
            ProgressChanged?.Invoke();
        }
        catch (UnauthorizedAccessException _exception)
        {
            LogLoadError(_exception);
            ProgressChanged?.Invoke();
        }
        catch (Exception _exception)
        {
            LogLoadError(_exception);
            ProgressChanged?.Invoke();
        }
    }

    private bool CanApplyRunResult(RunResult _result)
    {
        if (!_hasLoaded)
            Load();

        if (_data == null || _rewardCalculator == null)
        {
            if (!_hasLoggedRewardCalculatorError)
            {
                Debug.LogError("PlayerProgressService needs a RunRewardCalculator reference.", this);
                _hasLoggedRewardCalculatorError = true;
            }

            return false;
        }

        if (_result.EndReason != RunEndReason.EnergyDepleted)
            return false;

        if (_result.RunId <= 0 || _lastAppliedRunId >= _result.RunId)
            return false;

        if (_result.Time < 0f || float.IsNaN(_result.Time) || float.IsInfinity(_result.Time))
            return false;

        if (_result.Depth < 0f || float.IsNaN(_result.Depth) || float.IsInfinity(_result.Depth))
            return false;

        return _result.DugBlocks >= 0;
    }

    private PlayerProgressData CreateDefaultData()
    {
        return new PlayerProgressData();
    }

    private void HandleInvalidSave(string _message)
    {
        HandleInvalidSave(_message, null);
    }

    private void HandleInvalidSave(string _message, Exception _exception)
    {
        PreserveInvalidSave();

        if (_exception == null)
            Debug.LogWarning($"{_message} A new progress file will be created.", this);
        else
            Debug.LogWarning($"{_message} A new progress file will be created. {_exception.Message}", this);

        _data = CreateDefaultData();
        Save();
        ProgressChanged?.Invoke();
    }

    private void PreserveInvalidSave()
    {
        if (!File.Exists(SaveFilePath))
            return;

        string invalidPath = SaveFilePath + ".invalid." + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + ".json";

        try
        {
            File.Move(SaveFilePath, invalidPath);
        }
        catch (Exception _exception)
        {
            LogLoadError(_exception);
        }
    }

    private void DeleteInvalidSaveFiles()
    {
        try
        {
            string directory = Application.persistentDataPath;
            string searchPattern = SaveFileName + ".invalid.*.json";

            foreach (string path in Directory.GetFiles(directory, searchPattern))
                DeleteFileIfExists(path);
        }
        catch (Exception _exception)
        {
            LogLoadError(_exception);
        }
    }

    private void DeleteFileIfExists(string _path)
    {
        try
        {
            if (File.Exists(_path))
                File.Delete(_path);
        }
        catch (Exception _exception)
        {
            LogSaveError(_exception);
        }
    }

    private void LogSaveError(Exception _exception)
    {
        Debug.LogError($"Player progress could not be saved: {_exception.Message}", this);
    }

    private void LogLoadError(Exception _exception)
    {
        Debug.LogError($"Player progress could not be loaded: {_exception.Message}", this);
    }

    #endregion
}
