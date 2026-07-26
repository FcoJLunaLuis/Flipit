using UnityEngine;
using Flipit.Combat;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// Trigger component attached to House_Landmark. When the Player_Controller
    /// enters the trigger collider (covering the adjacent Sidewalk_Cell),
    /// fires a level-completion event via Combat_Event_Bus.
    /// </summary>
    public class House_Trigger : MonoBehaviour
    {
        [SerializeField] private string _eventId = "level_complete";
        [SerializeField] private bool _fireOnce = true;

        private bool _hasFired;

        private void OnTriggerEnter(Collider other)
        {
            if (_fireOnce && _hasFired)
                return;

            // Check if the entering collider belongs to a Player_Controller
            if (other.GetComponent<Player_Controller>() != null)
            {
                _hasFired = true;
                Combat_Event_Bus.RaiseEvent(_eventId);
                Debug.Log($"[House_Trigger] Level-completion event fired: {_eventId}");
            }
        }
    }
}
