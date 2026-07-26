using System.Collections.Generic;
using UnityEngine;
using Flipit.Dialogue;
using Flipit.Combat;

namespace Flipit.CityTerrain
{
    [CreateAssetMenu(fileName = "NewCityConfig", menuName = "Flipit/City Config")]
    public class City_Config : ScriptableObject
    {
        [Header("Grid Settings")]
        [SerializeField, Min(5)] private int _gridWidth = 10;
        [SerializeField, Min(5)] private int _gridDepth = 10;
        [SerializeField, Min(1f)] private float _cellSize = 4f;
        [SerializeField, Range(3, 10)] private int _streetInterval = 4;

        [Header("Seed")]
        [SerializeField] private int _randomSeed = 0;

        [Header("Player & Camera")]
        [SerializeField, Min(0.1f)] private float _playerMoveSpeed = 5f;
        [SerializeField, Min(1f)] private float _cameraOrthoSize = 8f;
        [SerializeField, Min(1f)] private float _cameraFollowOffset = 10f;
        [SerializeField, Min(0.1f)] private float _cameraSmoothSpeed = 5f;

        [Header("Building Variation")]
        [SerializeField, Min(0.5f)] private float _buildingHeightMin = 2f;
        [SerializeField, Min(0.5f)] private float _buildingHeightMax = 5f;
        [SerializeField] private List<Color> _buildingColorPalette = new();
        [SerializeField, Range(0f, 0.5f)] private float _cellConversionPercent = 0.2f;

        [Header("Prop Density")]
        [SerializeField, Range(0f, 1f)] private float _treeDensitySidewalk = 0.33f;
        [SerializeField, Range(0f, 1f)] private float _treeDensityPark = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _carDensity = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _benchDensity = 1f;
        [SerializeField, Range(0f, 1f)] private float _trashCanDensity = 0.125f;

        [Header("NPC Counts")]
        [SerializeField, Min(0)] private int _vendorCount = 3;
        [SerializeField, Range(1, 10)] private int _challengerCount = 4;
        [SerializeField, Range(1, 10)] private int _surpriseEncounterCount = 3;

        [Header("NPC References")]
        [SerializeField] private DialogueData[] _vendorDialogues;
        [SerializeField] private DialogueData[] _challengerDialogues;
        [SerializeField] private EncounterConfig[] _challengerEncounterConfigs;
        [SerializeField] private DialogueData _forcedEncounterDialogue;
        [SerializeField] private string _combatSceneName = "CombatScene";

        [Header("Transparency")]
        [SerializeField, Range(0f, 1f)] private float _transparencyAlpha = 0.3f;
        [SerializeField, Min(0.01f)] private float _fadeDuration = 0.2f;

        [Header("Colors")]
        [SerializeField] private Color _groundPlaneColor = new(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color _streetColor = new(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color _sidewalkColor = new(0.7f, 0.7f, 0.7f, 1f);
        [SerializeField] private Color _schoolColor = Color.blue;
        [SerializeField] private Color _houseColor = Color.green;
        [SerializeField] private Color _vendorColor = Color.yellow;
        [SerializeField] private Color _challengerColor = Color.red;
        [SerializeField] private Color _treeTrunkColor = new(0.4f, 0.25f, 0.1f, 1f);
        [SerializeField] private Color _treeCanopyColor = new(0.2f, 0.7f, 0.2f, 1f);

        // --- Public Properties with Clamping ---

        // Grid Settings
        public int GridWidth => Mathf.Max(_gridWidth, 5);
        public int GridDepth => Mathf.Max(_gridDepth, 5);
        public float CellSize => Mathf.Max(_cellSize, 1f);
        public int StreetInterval => Mathf.Clamp(_streetInterval, 3, 10);

        // Seed
        public int RandomSeed => _randomSeed;

        // Player & Camera
        public float PlayerMoveSpeed => Mathf.Max(_playerMoveSpeed, 0.1f);
        public float CameraOrthoSize => Mathf.Max(_cameraOrthoSize, 1f);
        public float CameraFollowOffset => Mathf.Max(_cameraFollowOffset, 1f);
        public float CameraSmoothSpeed => Mathf.Max(_cameraSmoothSpeed, 0.1f);

        // Building Variation
        public float BuildingHeightMin => Mathf.Max(_buildingHeightMin, 0.5f);
        public float BuildingHeightMax => Mathf.Max(Mathf.Max(_buildingHeightMax, 0.5f), BuildingHeightMin);
        public List<Color> BuildingColorPalette => _buildingColorPalette;
        public float CellConversionPercent => Mathf.Clamp(_cellConversionPercent, 0f, 0.5f);

        // Prop Density
        public float TreeDensitySidewalk => Mathf.Clamp(_treeDensitySidewalk, 0f, 1f);
        public float TreeDensityPark => Mathf.Clamp(_treeDensityPark, 0f, 1f);
        public float CarDensity => Mathf.Clamp(_carDensity, 0f, 1f);
        public float BenchDensity => Mathf.Clamp(_benchDensity, 0f, 1f);
        public float TrashCanDensity => Mathf.Clamp(_trashCanDensity, 0f, 1f);

        // NPC Counts
        public int VendorCount => Mathf.Max(_vendorCount, 0);
        public int ChallengerCount => Mathf.Clamp(_challengerCount, 1, 10);
        public int SurpriseEncounterCount => Mathf.Clamp(_surpriseEncounterCount, 1, 10);

        // NPC References
        public DialogueData[] VendorDialogues => _vendorDialogues;
        public DialogueData[] ChallengerDialogues => _challengerDialogues;
        public EncounterConfig[] ChallengerEncounterConfigs => _challengerEncounterConfigs;
        public DialogueData ForcedEncounterDialogue => _forcedEncounterDialogue;
        public string CombatSceneName => _combatSceneName;

        // Transparency
        public float TransparencyAlpha => Mathf.Clamp(_transparencyAlpha, 0f, 1f);
        public float FadeDuration => Mathf.Max(_fadeDuration, 0.01f);

        // Colors
        public Color GroundPlaneColor => _groundPlaneColor;
        public Color StreetColor => _streetColor;
        public Color SidewalkColor => _sidewalkColor;
        public Color SchoolColor => _schoolColor;
        public Color HouseColor => _houseColor;
        public Color VendorColor => _vendorColor;
        public Color ChallengerColor => _challengerColor;
        public Color TreeTrunkColor => _treeTrunkColor;
        public Color TreeCanopyColor => _treeCanopyColor;

        /// <summary>
        /// Validates the configuration at generation time.
        /// Logs warnings for clamped values and errors for invalid state.
        /// Returns true if generation can proceed, false if it must abort.
        /// </summary>
        public bool Validate()
        {
            bool valid = true;

            // Req 15.11: Clamp out-of-range values with warning
            if (_gridWidth < 5)
                Debug.LogWarning($"[City_Config] gridWidth ({_gridWidth}) is below minimum. Clamped to 5.");
            if (_gridDepth < 5)
                Debug.LogWarning($"[City_Config] gridDepth ({_gridDepth}) is below minimum. Clamped to 5.");
            if (_cellSize < 1f)
                Debug.LogWarning($"[City_Config] cellSize ({_cellSize}) is below minimum. Clamped to 1.0.");
            if (_streetInterval < 3 || _streetInterval > 10)
                Debug.LogWarning($"[City_Config] streetInterval ({_streetInterval}) is out of range. Clamped to {StreetInterval}.");
            if (_playerMoveSpeed < 0.1f)
                Debug.LogWarning($"[City_Config] playerMoveSpeed ({_playerMoveSpeed}) is below minimum. Clamped to 0.1.");
            if (_cameraOrthoSize < 1f)
                Debug.LogWarning($"[City_Config] cameraOrthoSize ({_cameraOrthoSize}) is below minimum. Clamped to 1.0.");
            if (_cameraFollowOffset < 1f)
                Debug.LogWarning($"[City_Config] cameraFollowOffset ({_cameraFollowOffset}) is below minimum. Clamped to 1.0.");
            if (_cameraSmoothSpeed < 0.1f)
                Debug.LogWarning($"[City_Config] cameraSmoothSpeed ({_cameraSmoothSpeed}) is below minimum. Clamped to 0.1.");
            if (_buildingHeightMin < 0.5f)
                Debug.LogWarning($"[City_Config] buildingHeightMin ({_buildingHeightMin}) is below minimum. Clamped to 0.5.");
            if (_buildingHeightMax < 0.5f)
                Debug.LogWarning($"[City_Config] buildingHeightMax ({_buildingHeightMax}) is below minimum. Clamped to 0.5.");
            if (_fadeDuration < 0.01f)
                Debug.LogWarning($"[City_Config] fadeDuration ({_fadeDuration}) is below minimum. Clamped to 0.01.");

            // Req 15.12: If height max < min, set max = min with warning
            if (_buildingHeightMax < _buildingHeightMin)
                Debug.LogWarning($"[City_Config] buildingHeightMax ({_buildingHeightMax}) is less than buildingHeightMin ({_buildingHeightMin}). Setting max = min ({BuildingHeightMin}).");

            // Req 15.13: If color palette has fewer than 5 entries, abort with error
            if (_buildingColorPalette == null || _buildingColorPalette.Count < 5)
            {
                Debug.LogError($"[City_Config] buildingColorPalette has {_buildingColorPalette?.Count ?? 0} entries. A minimum of 5 colors is required. Aborting generation.");
                valid = false;
            }

            return valid;
        }
    }
}
