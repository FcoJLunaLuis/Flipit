using UnityEngine;

namespace Flipit.DailyCycle
{
    /// <summary>
    /// Abstract base class for all daily events.
    /// Create concrete event assets via the Unity Editor: Assets → Create → Flipit → Daily Event.
    /// Implementations MUST NOT destroy/create GameObjects, remove NPCs, or regenerate the city.
    /// </summary>
    [CreateAssetMenu(fileName = "New_Daily_Event", menuName = "Flipit/Daily Event")]
    public abstract class Daily_Event : ScriptableObject
    {
        [SerializeField] private string _title;
        [SerializeField] private string _description;
        [SerializeField] private Daily_Event_Category _category;
        [SerializeField] private float _probability = 1.0f;

        /// <summary>Display title shown on the event card UI.</summary>
        public string Title => _title;

        /// <summary>Description shown on the event card UI.</summary>
        public string Description => _description;

        /// <summary>Category used for weighted random selection.</summary>
        public Daily_Event_Category Category => _category;

        /// <summary>Relative probability weight within this event's category.</summary>
        public float Probability => _probability;

        /// <summary>
        /// Executes the event effect. Returns true if applied successfully.
        /// Implementations MUST NOT destroy/create GameObjects, remove NPCs,
        /// or regenerate the city.
        /// </summary>
        /// <param name="context">Read-only game state context for the event.</param>
        /// <returns>True if the event was applied successfully; false otherwise.</returns>
        public abstract bool Execute(IDailyEventContext context);
    }
}