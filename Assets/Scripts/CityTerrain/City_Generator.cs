using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Flipit.Dialogue;
using Flipit.Combat;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// Top-level MonoBehaviour that orchestrates all 5 generation stages.
    /// Attached to a root GameObject in the CityExplorationScene.
    /// </summary>
    public class City_Generator : MonoBehaviour
    {
        [SerializeField] private City_Config _config;

        // Hierarchy parents (auto-created)
        private Transform _groundParent;
        private Transform _streetsParent;
        private Transform _sidewalksParent;
        private Transform _buildingsParent;
        private Transform _propsParent;
        private Transform _npcsParent;
        private Transform _encountersParent;

        // Internal state
        private Grid_Layout _grid;
        private System.Random _rng;
        private LandmarkPlacer.LandmarkResult _landmarks;
        private List<CellData> _path;

        /// <summary>
        /// Main entry point for city generation. Validates config, initializes seed,
        /// clears previous generation, and executes all 5 stages sequentially.
        /// </summary>
        public void ExecuteGeneration()
        {
            if (!PrepareGeneration())
                return;

            if (!ExecuteStage1_CoreLayout())
            {
                Debug.LogError("[City_Generator] Stage 1 (Core Layout) failed. Aborting generation.");
                return;
            }

            if (!ExecuteStage2_UrbanProps())
            {
                Debug.LogError("[City_Generator] Stage 2 (Urban Props) failed. Aborting generation.");
                return;
            }

            if (!ExecuteStage3_NPCPlacement())
            {
                Debug.LogError("[City_Generator] Stage 3 (NPC Placement) failed. Aborting generation.");
                return;
            }

            if (!ExecuteStage4_Transparency())
            {
                Debug.LogError("[City_Generator] Stage 4 (Transparency) failed. Aborting generation.");
                return;
            }

            if (!ExecuteStage5_Variation())
            {
                Debug.LogError("[City_Generator] Stage 5 (Variation) failed. Aborting generation.");
                return;
            }

            Debug.Log("[City_Generator] City generation complete.");
        }

        /// <summary>
        /// Validates the config, initializes the seed, and clears previous generation.
        /// Call this before executing stages individually from the Editor script.
        /// Returns true if ready for generation, false if validation failed.
        /// </summary>
        public bool PrepareGeneration()
        {
            if (_config == null)
            {
                Debug.LogError("[City_Generator] City_Config is not assigned. Aborting generation.");
                return false;
            }

            if (!_config.Validate())
            {
                Debug.LogError("[City_Generator] City_Config validation failed. Aborting generation.");
                return false;
            }

            // Initialize seed: if 0, use Environment.TickCount for a random seed
            int seed = _config.RandomSeed == 0 ? System.Environment.TickCount : _config.RandomSeed;
            _rng = new System.Random(seed);
            Debug.Log($"[City_Generator] Starting generation with seed: {seed}");

            ClearGenerated();
            return true;
        }

        /// <summary>
        /// Destroys all generated GameObjects and resets internal state.
        /// </summary>
        public void ClearGenerated()
        {
            // Destroy hierarchy parents
            DestroyChild("Ground");
            DestroyChild("Streets");
            DestroyChild("Sidewalks");
            DestroyChild("Buildings");
            DestroyChild("Props");
            DestroyChild("NPCs");
            DestroyChild("Encounters");

            // Destroy Player and Camera if they exist
            DestroyChild("Player");
            DestroyChild("Isometric_Camera");

            // Reset internal state
            _groundParent = null;
            _streetsParent = null;
            _sidewalksParent = null;
            _buildingsParent = null;
            _propsParent = null;
            _npcsParent = null;
            _encountersParent = null;
            _grid = null;
            _landmarks = default;
            _path = null;
        }

        private void DestroyChild(string childName)
        {
            Transform child = transform.Find(childName);
            if (child != null)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        /// <summary>
        /// Stage 1: Core Layout — creates hierarchy, grid, ground plane,
        /// streets, sidewalks, buildings, landmarks, player, and camera.
        /// </summary>
        public bool ExecuteStage1_CoreLayout()
        {
            var counts = new Dictionary<string, int>();

            // Create hierarchy parents as children of this GameObject
            _groundParent = CreateHierarchyParent("Ground");
            _streetsParent = CreateHierarchyParent("Streets");
            _sidewalksParent = CreateHierarchyParent("Sidewalks");
            _buildingsParent = CreateHierarchyParent("Buildings");
            _propsParent = CreateHierarchyParent("Props");
            _npcsParent = CreateHierarchyParent("NPCs");
            _encountersParent = CreateHierarchyParent("Encounters");

            // Initialize grid
            _grid = new Grid_Layout(_config.GridWidth, _config.GridDepth, _config.CellSize);
            GridGenerator.AssignCellTypes(_grid, _config.StreetInterval);

            // Create ground plane with fallback color if alpha is 0
            Color groundColor = _config.GroundPlaneColor;
            if (groundColor.a == 0f)
            {
                groundColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                Debug.LogWarning("[City_Generator] Ground plane color has alpha 0. Applying fallback gray (0.5, 0.5, 0.5, 1.0).");
            }

            float gridWorldWidth = _config.GridWidth * _config.CellSize;
            float gridWorldDepth = _config.GridDepth * _config.CellSize;
            GeometryFactory.CreateGroundPlane(gridWorldWidth, gridWorldDepth, groundColor, _groundParent);
            counts["Ground"] = 1;

            // Iterate all cells and create street/sidewalk/building GameObjects
            int streetCount = 0;
            int sidewalkCount = 0;
            int buildingCount = 0;

            // Get default building color from palette (first color)
            Color defaultBuildingColor = _config.BuildingColorPalette.Count > 0
                ? _config.BuildingColorPalette[0]
                : new Color(0.6f, 0.6f, 0.6f, 1f);

            float defaultBuildingHeight = 3f;

            for (int row = 0; row < _grid.Depth; row++)
            {
                for (int col = 0; col < _grid.Width; col++)
                {
                    CellData cell = _grid.GetCell(row, col);
                    Vector3 worldPos = cell.WorldPosition(_config.CellSize);

                    switch (cell.Type)
                    {
                        case CellType.Street:
                            GeometryFactory.CreateStreetCell(worldPos, _config.CellSize, _config.StreetColor, _streetsParent);
                            streetCount++;
                            break;

                        case CellType.Sidewalk:
                            GeometryFactory.CreateSidewalkCell(worldPos, _config.CellSize, _config.SidewalkColor, _sidewalksParent);
                            sidewalkCount++;
                            break;

                        case CellType.Building:
                            GeometryFactory.CreateBuilding(worldPos, _config.CellSize, defaultBuildingHeight, defaultBuildingColor, _buildingsParent);
                            buildingCount++;
                            break;
                    }
                }
            }

            counts["Streets"] = streetCount;
            counts["Sidewalks"] = sidewalkCount;
            counts["Buildings"] = buildingCount;

            // Place landmarks
            _landmarks = LandmarkPlacer.PlaceLandmarks(_grid);
            if (!_landmarks.Success)
            {
                Debug.LogError($"[City_Generator] Landmark placement failed: {_landmarks.ErrorMessage}");
                return false;
            }

            // Mark landmark cells as occupied
            _grid.MarkOccupied(_landmarks.SchoolCell.Row, _landmarks.SchoolCell.Col, "school");
            _grid.MarkOccupied(_landmarks.HouseCell.Row, _landmarks.HouseCell.Col, "house");
            _grid.MarkOccupied(_landmarks.SchoolSpawnCell.Row, _landmarks.SchoolSpawnCell.Col, "school_spawn");
            _grid.MarkOccupied(_landmarks.HouseTriggerCell.Row, _landmarks.HouseTriggerCell.Col, "house_trigger");

            // Create School building (1.5x scale)
            Vector3 schoolWorldPos = _landmarks.SchoolCell.WorldPosition(_config.CellSize);
            GameObject schoolGO = GeometryFactory.CreateBuilding(
                schoolWorldPos,
                _config.CellSize * 1.5f,
                defaultBuildingHeight,
                _config.SchoolColor,
                _buildingsParent);
            schoolGO.name = "School_Landmark";

            // Create spawn point as child of school
            Vector3 spawnWorldPos = _landmarks.SchoolSpawnCell.WorldPosition(_config.CellSize);
            var spawnGO = new GameObject("SpawnPoint");
            spawnGO.transform.position = spawnWorldPos;
            spawnGO.transform.SetParent(schoolGO.transform);

            // Create House building (1x scale)
            Vector3 houseWorldPos = _landmarks.HouseCell.WorldPosition(_config.CellSize);
            GameObject houseGO = GeometryFactory.CreateBuilding(
                houseWorldPos,
                _config.CellSize,
                defaultBuildingHeight,
                _config.HouseColor,
                _buildingsParent);
            houseGO.name = "House_Landmark";

            // Add trigger collider covering the adjacent Sidewalk_Cell (HouseTriggerCell)
            Vector3 triggerWorldPos = _landmarks.HouseTriggerCell.WorldPosition(_config.CellSize);
            Vector3 triggerOffset = triggerWorldPos - houseWorldPos;
            var houseTriggerCollider = houseGO.AddComponent<BoxCollider>();
            houseTriggerCollider.isTrigger = true;
            houseTriggerCollider.size = new Vector3(_config.CellSize, 1f, _config.CellSize);
            houseTriggerCollider.center = triggerOffset + new Vector3(0f, 0.5f, 0f);

            // Add House_Trigger component to fire level-completion event
            houseGO.AddComponent<House_Trigger>();

            counts["Landmarks"] = 2;

            // Find path between school spawn and house trigger for later stages
            _path = PathFinder.FindPath(_grid, _landmarks.SchoolSpawnCell, _landmarks.HouseTriggerCell);

            // Create Player
            Vector3 playerSpawnPos = spawnWorldPos + new Vector3(0f, 0f, 0f);
            var playerGO = new GameObject("Player");
            playerGO.transform.position = playerSpawnPos;
            playerGO.transform.SetParent(transform);

            var cc = playerGO.AddComponent<CharacterController>();
            cc.radius = 0.5f;
            cc.height = 2.0f;
            cc.center = new Vector3(0f, 1f, 0f);

            playerGO.AddComponent<PlayerInput>();
            var playerController = playerGO.AddComponent<Player_Controller>();

            // Set city boundary limits on the player controller
            float cityWidth = _config.GridWidth * _config.CellSize;
            float cityDepth = _config.GridDepth * _config.CellSize;
            playerController.BoundaryMin = new Vector2(0f, 0f);
            playerController.BoundaryMax = new Vector2(cityWidth, cityDepth);

            // Add visible capsule mesh so player is visible
            var playerMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerMesh.name = "PlayerMesh";
            playerMesh.transform.SetParent(playerGO.transform);
            playerMesh.transform.localPosition = new Vector3(0f, 1f, 0f);
            playerMesh.transform.localScale = Vector3.one;
            // Remove collider from mesh (CharacterController handles collision)
            var meshCollider = playerMesh.GetComponent<CapsuleCollider>();
            if (meshCollider != null) DestroyImmediate(meshCollider);
            // Set player color (cyan)
            var playerRenderer = playerMesh.GetComponent<MeshRenderer>();
            if (playerRenderer != null)
            {
                var playerMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                playerMat.color = new Color(0.2f, 0.8f, 1f, 1f);
                playerRenderer.sharedMaterial = playerMat;
            }

            // Add Player_Interactor for NPC interaction
            var interactor = playerGO.AddComponent<Flipit.Dialogue.Player_Interactor>();

            // Add CharacterSilhouette to PlayerMesh
            var playerSilhouette = playerMesh.AddComponent<CharacterSilhouette>();
            SetPrivateField(playerSilhouette, "_silhouetteColor", new Color(0f, 0.9f, 1f, 0.6f));

            // Create world-space label for Player
            var playerLabelGO = new GameObject("PlayerLabel");
            playerLabelGO.transform.SetParent(playerGO.transform);
            playerLabelGO.transform.localPosition = new Vector3(0f, 2.8f, 0f);
            playerLabelGO.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

            var playerLabelCanvas = playerLabelGO.AddComponent<Canvas>();
            playerLabelCanvas.renderMode = RenderMode.WorldSpace;
            var playerLabelRect = playerLabelGO.GetComponent<RectTransform>();
            playerLabelRect.sizeDelta = new Vector2(200f, 50f);

            var playerLabelTextGO = new GameObject("Text");
            playerLabelTextGO.transform.SetParent(playerLabelGO.transform);
            playerLabelTextGO.transform.localPosition = Vector3.zero;
            playerLabelTextGO.transform.localScale = Vector3.one;
            var playerLabelTMP = playerLabelTextGO.AddComponent<TextMeshProUGUI>();
            playerLabelTMP.text = "Player";
            playerLabelTMP.fontSize = 24;
            playerLabelTMP.color = Color.white;
            playerLabelTMP.alignment = TextAlignmentOptions.Center;
            playerLabelTMP.fontStyle = FontStyles.Bold;
            playerLabelTMP.enableWordWrapping = false;
            playerLabelTMP.overflowMode = TextOverflowModes.Overflow;
            var playerLabelTextRect = playerLabelTextGO.GetComponent<RectTransform>();
            playerLabelTextRect.sizeDelta = new Vector2(200f, 50f);
            playerLabelTextRect.anchoredPosition = Vector2.zero;

            // Add Billboard so label faces camera
            playerLabelGO.AddComponent<Billboard>();

            // Set default action map on PlayerInput
            var playerInput = playerGO.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.defaultActionMap = "Player";
            }

            counts["Player"] = 1;

            // Create Isometric Camera
            var cameraGO = new GameObject("Isometric_Camera");
            cameraGO.transform.SetParent(transform);
            cameraGO.transform.rotation = Quaternion.Euler(30f, 45f, 0f);

            // Position camera behind and above the player
            Vector3 cameraOffset = cameraGO.transform.rotation * new Vector3(0f, 0f, -_config.CameraFollowOffset);
            cameraGO.transform.position = playerSpawnPos + cameraOffset;

            var cam = cameraGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = _config.CameraOrthoSize;

            var isoCam = cameraGO.AddComponent<Isometric_Camera>();

            counts["Camera"] = 1;

            LogStageCompletion(1, counts);
            return true;
        }

        /// <summary>
        /// Stage 2: Urban Props — places trees, cars, lampposts, benches, and trash cans.
        /// </summary>
        public bool ExecuteStage2_UrbanProps()
        {
            var counts = new Dictionary<string, int>();

            // Place trees
            var treeResult = PropPlacer.PlaceTrees(_grid, _rng, _config.TreeDensitySidewalk, _config.TreeDensityPark);
            foreach (var cell in treeResult.PlacedCells)
            {
                Vector3 worldPos = cell.WorldPosition(_config.CellSize);
                GeometryFactory.CreateTree(worldPos, _config.TreeTrunkColor, _config.TreeCanopyColor, _propsParent);
            }
            counts["Trees"] = treeResult.ActualCount;

            // Place cars
            var carResult = PropPlacer.PlaceCars(_grid, _rng, _config.CarDensity);
            Color[] carPalette = _config.BuildingColorPalette.Count > 0
                ? _config.BuildingColorPalette.ToArray()
                : new Color[] { Color.red, Color.blue, Color.white, Color.black, Color.gray };

            foreach (var cell in carResult.PlacedCells)
            {
                Vector3 worldPos = cell.WorldPosition(_config.CellSize);

                // Determine rotation based on which street direction this cell belongs to
                bool isHorizontalStreet = cell.Row % _config.StreetInterval == 0;
                bool isVerticalStreet = cell.Col % _config.StreetInterval == 0;

                float yRotation;
                Vector3 offset = Vector3.zero;
                float laneOffset = _config.CellSize * 0.25f; // Offset car to one side of the street

                if (isHorizontalStreet && !isVerticalStreet)
                {
                    // Horizontal street - car faces along X (east-west)
                    yRotation = _rng.Next(2) == 0 ? 90f : 270f;
                    // Offset toward the sidewalk (north or south side)
                    offset = new Vector3(0f, 0f, _rng.Next(2) == 0 ? laneOffset : -laneOffset);
                }
                else if (isVerticalStreet && !isHorizontalStreet)
                {
                    // Vertical street - car faces along Z (north-south)
                    yRotation = _rng.Next(2) == 0 ? 0f : 180f;
                    // Offset toward the sidewalk (east or west side)
                    offset = new Vector3(_rng.Next(2) == 0 ? laneOffset : -laneOffset, 0f, 0f);
                }
                else
                {
                    // Intersection or edge case - skip (don't place cars at intersections)
                    continue;
                }

                Color carColor = carPalette[_rng.Next(carPalette.Length)];
                GeometryFactory.CreateCar(worldPos + offset, yRotation, carColor, _propsParent);
            }
            counts["Cars"] = carResult.ActualCount;

            // Place lampposts
            var lamppostResult = PropPlacer.PlaceLampposts(_grid, _config.StreetInterval);
            Color lamppostColor = new Color(1f, 0.95f, 0.8f, 1f); // white/yellow-ish
            foreach (var cell in lamppostResult.PlacedCells)
            {
                Vector3 worldPos = cell.WorldPosition(_config.CellSize);
                GeometryFactory.CreateLamppost(worldPos, lamppostColor, _propsParent);
            }
            counts["Lampposts"] = lamppostResult.ActualCount;

            // Place benches
            var benchResult = PropPlacer.PlaceBenches(_grid, _rng, _config.BenchDensity);
            Color benchColor = new Color(0.4f, 0.25f, 0.1f, 1f); // brown
            foreach (var cell in benchResult.PlacedCells)
            {
                Vector3 worldPos = cell.WorldPosition(_config.CellSize);

                // Determine rotation from adjacent Park cell direction
                float benchRotation = GetBenchRotationFromPark(cell);
                GeometryFactory.CreateBench(worldPos, benchRotation, benchColor, _propsParent);
            }
            counts["Benches"] = benchResult.ActualCount;

            // Place trash cans
            var trashCanResult = PropPlacer.PlaceTrashCans(_grid, _rng, _config.TrashCanDensity, 3);
            Color trashCanColor = new Color(0.2f, 0.2f, 0.2f, 1f); // dark gray
            foreach (var cell in trashCanResult.PlacedCells)
            {
                Vector3 worldPos = cell.WorldPosition(_config.CellSize);
                GeometryFactory.CreateTrashCan(worldPos, trashCanColor, _propsParent);
            }
            counts["TrashCans"] = trashCanResult.ActualCount;

            LogStageCompletion(2, counts);
            return true;
        }

        /// <summary>
        /// Determines bench Y-rotation based on the direction of the adjacent Park cell.
        /// The bench's long axis (1.2 scale) should run parallel to the Park edge.
        /// </summary>
        private float GetBenchRotationFromPark(CellData benchCell)
        {
            // Check each direction for an adjacent Park cell
            // If park is above or below (row offset), bench long axis along X → 0° rotation
            // If park is left or right (col offset), bench long axis along Z → 90° rotation
            if (benchCell.Row > 0 && _grid.Cells[benchCell.Row - 1, benchCell.Col].Type == CellType.Park)
                return 0f;
            if (benchCell.Row < _grid.Depth - 1 && _grid.Cells[benchCell.Row + 1, benchCell.Col].Type == CellType.Park)
                return 0f;
            if (benchCell.Col > 0 && _grid.Cells[benchCell.Row, benchCell.Col - 1].Type == CellType.Park)
                return 90f;
            if (benchCell.Col < _grid.Width - 1 && _grid.Cells[benchCell.Row, benchCell.Col + 1].Type == CellType.Park)
                return 90f;

            return 0f; // default
        }

        /// <summary>
        /// Stage 3: NPC Placement — places vendors, challengers, and surprise encounters.
        /// </summary>
        public bool ExecuteStage3_NPCPlacement()
        {
            var counts = new Dictionary<string, int>();

            // Place vendors
            var vendorResult = NPCPlacer.PlaceVendors(_grid, _rng, _config.VendorCount, _config.StreetInterval);
            for (int i = 0; i < vendorResult.PlacedCells.Count; i++)
            {
                CellData cell = vendorResult.PlacedCells[i];
                Vector3 worldPos = cell.WorldPosition(_config.CellSize);

                GameObject npcGO = GeometryFactory.CreateNPCCapsule(worldPos, _config.VendorColor, $"Vendor_{i}", _npcsParent);

                // Add NPC_Interactable component and assign dialogue data
                var interactable = npcGO.AddComponent<NPC_Interactable>();
                if (_config.VendorDialogues != null && _config.VendorDialogues.Length > 0)
                {
                    DialogueData dialogue = _config.VendorDialogues[i % _config.VendorDialogues.Length];
                    SetPrivateField(interactable, "dialogueData", dialogue);
                }

                // Add CharacterSilhouette to vendor capsule mesh
                var vendorMeshRenderer = npcGO.GetComponentInChildren<MeshRenderer>();
                if (vendorMeshRenderer != null)
                {
                    var vendorSilhouette = vendorMeshRenderer.gameObject.AddComponent<CharacterSilhouette>();
                    SetPrivateField(vendorSilhouette, "_silhouetteColor", new Color(1f, 0.9f, 0f, 0.6f));
                }

                // Create world-space label for vendor
                var vendorLabelGO = new GameObject("VendorLabel");
                vendorLabelGO.transform.SetParent(npcGO.transform);
                vendorLabelGO.transform.localPosition = new Vector3(0f, 1.8f, 0f);
                vendorLabelGO.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

                var vendorLabelCanvas = vendorLabelGO.AddComponent<Canvas>();
                vendorLabelCanvas.renderMode = RenderMode.WorldSpace;
                var vendorLabelRect = vendorLabelGO.GetComponent<RectTransform>();
                vendorLabelRect.sizeDelta = new Vector2(200f, 50f);

                var vendorLabelTextGO = new GameObject("Text");
                vendorLabelTextGO.transform.SetParent(vendorLabelGO.transform);
                vendorLabelTextGO.transform.localPosition = Vector3.zero;
                vendorLabelTextGO.transform.localScale = Vector3.one;
                var vendorLabelTMP = vendorLabelTextGO.AddComponent<TextMeshProUGUI>();
                vendorLabelTMP.text = "Vendedor";
                vendorLabelTMP.fontSize = 24;
                vendorLabelTMP.color = Color.yellow;
                vendorLabelTMP.alignment = TextAlignmentOptions.Center;
                vendorLabelTMP.fontStyle = FontStyles.Bold;
                vendorLabelTMP.enableWordWrapping = false;
                vendorLabelTMP.overflowMode = TextOverflowModes.Overflow;
                var vendorLabelTextRect = vendorLabelTextGO.GetComponent<RectTransform>();
                vendorLabelTextRect.sizeDelta = new Vector2(200f, 50f);
                vendorLabelTextRect.anchoredPosition = Vector2.zero;

                vendorLabelGO.AddComponent<Billboard>();
            }
            counts["Vendors"] = vendorResult.ActualCount;

            // Place challengers
            var challengerResult = NPCPlacer.PlaceChallengers(_grid, _rng, _config.ChallengerCount, _path);
            for (int i = 0; i < challengerResult.PlacedCells.Count; i++)
            {
                CellData cell = challengerResult.PlacedCells[i];
                Vector3 worldPos = cell.WorldPosition(_config.CellSize);

                GameObject npcGO = GeometryFactory.CreateNPCCapsule(worldPos, _config.ChallengerColor, $"Challenger_{i}", _npcsParent);

                // Add FlipCombat_NPC component and assign dialogue + encounter config
                var combatNPC = npcGO.AddComponent<FlipCombat_NPC>();

                if (_config.ChallengerDialogues != null && _config.ChallengerDialogues.Length > 0)
                {
                    DialogueData dialogue = _config.ChallengerDialogues[i % _config.ChallengerDialogues.Length];
                    SetPrivateField(combatNPC, "dialogueData", dialogue);
                }

                // Set combat scene name
                SetPrivateField(combatNPC, "_combatSceneName", _config.CombatSceneName);

                // FlipCombat_NPC doesn't have an encounterConfig field, but we note the config
                // is stored externally; the encounter system uses EncounterConfig at runtime.

                // Add CharacterSilhouette to challenger capsule mesh
                var challengerMeshRenderer = npcGO.GetComponentInChildren<MeshRenderer>();
                if (challengerMeshRenderer != null)
                {
                    var challengerSilhouette = challengerMeshRenderer.gameObject.AddComponent<CharacterSilhouette>();
                    SetPrivateField(challengerSilhouette, "_silhouetteColor", new Color(1f, 0.2f, 0.2f, 0.6f));
                }

                // Create world-space label for challenger
                var challengerLabelGO = new GameObject("ChallengerLabel");
                challengerLabelGO.transform.SetParent(npcGO.transform);
                challengerLabelGO.transform.localPosition = new Vector3(0f, 1.8f, 0f);
                challengerLabelGO.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

                var challengerLabelCanvas = challengerLabelGO.AddComponent<Canvas>();
                challengerLabelCanvas.renderMode = RenderMode.WorldSpace;
                var challengerLabelRect = challengerLabelGO.GetComponent<RectTransform>();
                challengerLabelRect.sizeDelta = new Vector2(200f, 50f);

                var challengerLabelTextGO = new GameObject("Text");
                challengerLabelTextGO.transform.SetParent(challengerLabelGO.transform);
                challengerLabelTextGO.transform.localPosition = Vector3.zero;
                challengerLabelTextGO.transform.localScale = Vector3.one;
                var challengerLabelTMP = challengerLabelTextGO.AddComponent<TextMeshProUGUI>();
                challengerLabelTMP.text = "Retador";
                challengerLabelTMP.fontSize = 24;
                challengerLabelTMP.color = Color.red;
                challengerLabelTMP.alignment = TextAlignmentOptions.Center;
                challengerLabelTMP.fontStyle = FontStyles.Bold;
                challengerLabelTMP.enableWordWrapping = false;
                challengerLabelTMP.overflowMode = TextOverflowModes.Overflow;
                var challengerLabelTextRect = challengerLabelTextGO.GetComponent<RectTransform>();
                challengerLabelTextRect.sizeDelta = new Vector2(200f, 50f);
                challengerLabelTextRect.anchoredPosition = Vector2.zero;

                challengerLabelGO.AddComponent<Billboard>();
            }
            counts["Challengers"] = challengerResult.ActualCount;

            // Place surprise encounters
            var encounterResult = EncounterPlacer.PlaceEncounters(
                _grid, _rng,
                _config.SurpriseEncounterCount,
                _path,
                _landmarks.SchoolCell,
                _landmarks.HouseCell);

            for (int i = 0; i < encounterResult.PlacedCells.Count; i++)
            {
                CellData cell = encounterResult.PlacedCells[i];
                Vector3 worldPos = cell.WorldPosition(_config.CellSize);

                // Create invisible GameObject with trigger BoxCollider
                var encounterGO = new GameObject($"Encounter_{i}");
                encounterGO.transform.position = worldPos;
                encounterGO.transform.SetParent(_encountersParent);

                // Add trigger BoxCollider covering full cell area (cellSize x 1 x cellSize)
                var boxCollider = encounterGO.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
                boxCollider.size = new Vector3(_config.CellSize, 1f, _config.CellSize);
                boxCollider.center = new Vector3(0f, 0.5f, 0f);

                // Add TriggerZone_Encounter component
                var triggerZone = encounterGO.AddComponent<TriggerZone_Encounter>();
                SetPrivateField(triggerZone, "_combatSceneName", _config.CombatSceneName);
                if (_config.ForcedEncounterDialogue != null)
                {
                    SetPrivateField(triggerZone, "_forcedDialogueData", _config.ForcedEncounterDialogue);
                }
            }
            counts["Encounters"] = encounterResult.ActualCount;

            LogStageCompletion(3, counts);
            return true;
        }

        /// <summary>
        /// Sets a private field value via reflection. Used to configure serialized
        /// component fields during Edit Mode generation.
        /// </summary>
        private static void SetPrivateField(object target, string fieldName, object value)
        {
            if (target == null) return;

            var type = target.GetType();
            FieldInfo field = null;

            // Walk up the hierarchy to find inherited private fields
            while (type != null && field == null)
            {
                field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                type = type.BaseType;
            }

            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"[City_Generator] Could not find field '{fieldName}' on {target.GetType().Name}");
            }
        }

        /// <summary>
        /// Stage 4: Transparency — attaches Building_Transparency_System to the camera.
        /// </summary>
        public bool ExecuteStage4_Transparency()
        {
            var counts = new Dictionary<string, int>();

            // Find the Isometric_Camera GameObject
            Transform cameraTransform = transform.Find("Isometric_Camera");
            if (cameraTransform == null)
            {
                Debug.LogError("[City_Generator] Stage 4: Could not find Isometric_Camera GameObject.");
                return false;
            }

            // Add Building_Transparency_System component
            var transparencySystem = cameraTransform.gameObject.AddComponent<Building_Transparency_System>();

            // Find the player transform for reference
            Transform playerTransform = transform.Find("Player");

            // Configure via reflection (fields are serialized/private)
            SetPrivateField(transparencySystem, "_player", playerTransform);
            SetPrivateField(transparencySystem, "_targetAlpha", _config.TransparencyAlpha);
            SetPrivateField(transparencySystem, "_fadeDuration", _config.FadeDuration);

            // Set the CityGeometry layer mask
            int cityGeometryLayer = LayerMask.NameToLayer("CityGeometry");
            if (cityGeometryLayer == -1)
            {
                Debug.LogWarning("[City_Generator] Stage 4: 'CityGeometry' layer not found. Using default layer mask.");
                cityGeometryLayer = 0;
            }
            LayerMask cityGeometryMask = 1 << cityGeometryLayer;
            SetPrivateField(transparencySystem, "_cityGeometryLayer", cityGeometryMask);

            counts["TransparencySystem"] = 1;

            LogStageCompletion(4, counts);
            return true;
        }

        /// <summary>
        /// Stage 5: Variation — applies random building heights/colors, converts empty cells,
        /// applies tree rotations, and applies prop scale variation.
        /// </summary>
        public bool ExecuteStage5_Variation()
        {
            var counts = new Dictionary<string, int>();
            int heightVariations = 0;
            int colorVariations = 0;
            int convertedCells = 0;
            int treeRotations = 0;
            int propScaleVariations = 0;

            // 1. Randomize building heights and colors (excluding landmarks)
            if (_buildingsParent != null)
            {
                foreach (Transform child in _buildingsParent)
                {
                    // Skip School and House landmarks
                    if (child.name == "School_Landmark" || child.name == "House_Landmark")
                        continue;

                    // Randomize Y-scale (height) between config min and max
                    float randomHeight = (float)(_config.BuildingHeightMin +
                        _rng.NextDouble() * (_config.BuildingHeightMax - _config.BuildingHeightMin));

                    Vector3 scale = child.localScale;
                    scale.y = randomHeight;
                    child.localScale = scale;

                    // Reposition so base stays on ground (center Y = height / 2)
                    Vector3 pos = child.position;
                    pos.y = randomHeight * 0.5f;
                    child.position = pos;

                    heightVariations++;

                    // Randomize color from palette
                    if (_config.BuildingColorPalette.Count > 0)
                    {
                        Color randomColor = _config.BuildingColorPalette[_rng.Next(_config.BuildingColorPalette.Count)];
                        var renderer = child.GetComponent<MeshRenderer>();
                        if (renderer != null && renderer.sharedMaterial != null)
                        {
                            // Create a new material instance to avoid shared material modification
                            var mat = new Material(renderer.sharedMaterial);
                            mat.color = randomColor;
                            renderer.sharedMaterial = mat;
                        }
                    }
                    colorVariations++;
                }
            }
            counts["HeightVariations"] = heightVariations;
            counts["ColorVariations"] = colorVariations;

            // 2. Convert empty cells adjacent to Building cells
            var emptyCells = _grid.GetCellsOfType(CellType.Empty);
            var eligibleForConversion = new List<CellData>();

            foreach (var cell in emptyCells)
            {
                // Check if adjacent to at least one Building cell
                var neighbors = _grid.GetAdjacentCells(cell.Row, cell.Col);
                bool adjacentToBuilding = false;
                foreach (var neighbor in neighbors)
                {
                    if (neighbor.Type == CellType.Building)
                    {
                        adjacentToBuilding = true;
                        break;
                    }
                }

                if (adjacentToBuilding && !cell.IsOccupied)
                {
                    eligibleForConversion.Add(cell);
                }
            }

            int convertTarget = Mathf.FloorToInt(eligibleForConversion.Count * _config.CellConversionPercent);
            // Shuffle for random selection
            for (int i = eligibleForConversion.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                CellData temp = eligibleForConversion[i];
                eligibleForConversion[i] = eligibleForConversion[j];
                eligibleForConversion[j] = temp;
            }

            for (int i = 0; i < convertTarget && i < eligibleForConversion.Count; i++)
            {
                CellData cell = eligibleForConversion[i];
                _grid.SetCellType(cell.Row, cell.Col, CellType.Building);

                // Create a building GameObject for the new building cell
                Vector3 worldPos = cell.WorldPosition(_config.CellSize);
                float height = (float)(_config.BuildingHeightMin +
                    _rng.NextDouble() * (_config.BuildingHeightMax - _config.BuildingHeightMin));
                Color color = _config.BuildingColorPalette.Count > 0
                    ? _config.BuildingColorPalette[_rng.Next(_config.BuildingColorPalette.Count)]
                    : new Color(0.6f, 0.6f, 0.6f, 1f);

                GeometryFactory.CreateBuilding(worldPos, _config.CellSize, height, color, _buildingsParent);
                convertedCells++;
            }
            counts["ConvertedCells"] = convertedCells;

            // 3. Apply random Y-rotation to tree canopies
            if (_propsParent != null)
            {
                foreach (Transform child in _propsParent)
                {
                    if (child.name.StartsWith("Tree_"))
                    {
                        // Find the Canopy child
                        Transform canopy = child.Find("Canopy");
                        if (canopy != null)
                        {
                            float[] rotations = { 0f, 90f, 180f, 270f };
                            float yRot = rotations[_rng.Next(rotations.Length)];
                            canopy.localRotation = Quaternion.Euler(0f, yRot, 0f);
                            treeRotations++;
                        }
                    }
                }
            }
            counts["TreeRotations"] = treeRotations;

            // 4. Apply random uniform scale variation (0.8 to 1.2) to all props
            if (_propsParent != null)
            {
                foreach (Transform child in _propsParent)
                {
                    float scaleFactor = 0.8f + (float)(_rng.NextDouble() * 0.4); // 0.8 to 1.2
                    child.localScale *= scaleFactor;
                    propScaleVariations++;
                }
            }
            counts["PropScaleVariations"] = propScaleVariations;

            LogStageCompletion(5, counts);
            return true;
        }

        private Transform CreateHierarchyParent(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            return go.transform;
        }

        private void LogStageCompletion(int stage, Dictionary<string, int> counts)
        {
            string countSummary = "";
            foreach (var kvp in counts)
            {
                if (countSummary.Length > 0)
                    countSummary += ", ";
                countSummary += $"{kvp.Key}: {kvp.Value}";
            }

            _lastStageSummary = countSummary;
            Debug.Log($"[City_Generator] Stage {stage} complete. {countSummary}");
        }

        /// <summary>
        /// Returns the summary string from the last completed stage.
        /// Used by the Editor script to display in confirmation dialogs.
        /// </summary>
        public string GetLastStageSummary()
        {
            return _lastStageSummary ?? "";
        }

        private string _lastStageSummary;
    }
}
