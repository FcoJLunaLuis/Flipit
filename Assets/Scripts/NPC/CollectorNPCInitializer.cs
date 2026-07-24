using System.Collections.Generic;
using UnityEngine;

namespace Flipit.NPC
{
    /// <summary>
    /// Initializes the Collector NPC system with mock dependencies for testing.
    /// Attach to the same GameObject as CollectorNPCManager.
    /// After merge with album branch, replace this with real inventory/catalog injection.
    /// </summary>
    public class CollectorNPCInitializer : MonoBehaviour
    {
        [SerializeField] private CollectorNPCManager npcManager;

        private MockChipInventory mockInventory;
        private MockChipCatalog mockCatalog;

        private void Start()
        {
            if (npcManager == null)
            {
                npcManager = GetComponent<CollectorNPCManager>();
            }

            if (npcManager == null)
            {
                Debug.LogError("[CollectorNPCInitializer] CollectorNPCManager not found!");
                return;
            }

            InitializeMocks();
            npcManager.Initialize(mockInventory, mockCatalog);

            Debug.Log("[CollectorNPCInitializer] NPC Coleccionista initialized with mock data.");
            Debug.Log($"  - Catalog: {mockCatalog.GetAllChipIds().Count} fichas");
            Debug.Log($"  - Inventory: {mockInventory.GetOwnedChipIds().Count} tipos de fichas");
            Debug.Log($"  - Trade offers: {npcManager.CurrentOffers.Length}");
        }

        private void InitializeMocks()
        {
            mockCatalog = new MockChipCatalog();

            mockInventory = new MockChipInventory(new Dictionary<string, int>
            {
                // Common chips (player has multiples)
                { "common_aguila", 8 },
                { "common_sol", 5 },
                { "common_luna", 6 },
                { "common_estrella", 4 },
                { "common_nube", 3 },
                { "common_flor", 2 },

                // Rare chips
                { "rare_dragon", 4 },
                { "rare_fenix", 3 },
                { "rare_kraken", 2 },

                // Ultra rare (few)
                { "ultra_cosmico", 2 }
            });
        }

        /// <summary>
        /// Gets the mock inventory for the tester UI.
        /// </summary>
        public IChipInventory GetInventory() => mockInventory;

        /// <summary>
        /// Gets the mock catalog for the tester UI.
        /// </summary>
        public IChipCatalog GetCatalog() => mockCatalog;
    }
}
