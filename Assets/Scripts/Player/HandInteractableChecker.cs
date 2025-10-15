using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Player
{
    public class HandInteractableChecker : MonoBehaviour
    {
        // Reference to input action
        [SerializeField] private InputActionReference m_ActivateVacuum;

        // Reference to left hand interactor
        [SerializeField] private XRInteractionGroup m_InteractionGroup;

        private void Awake()
        {
            m_InteractionGroup = GetComponent<XRInteractionGroup>();
        }
        
        private void Start()
        {
            // Subscribe to input events
            m_ActivateVacuum.action.started += Input_ActivateVacuum;
        }

        private void OnDestroy()
        {
            m_ActivateVacuum.action.started -= Input_ActivateVacuum;
        }
        
        // Is called when Activate button is pressed on Left Controller
        private void Input_ActivateVacuum(InputAction.CallbackContext obj)
        {
            IXRInteractor activeInteractor = m_InteractionGroup.activeInteractor;
            if (activeInteractor is IXRSelectInteractor selectInteractor)
            {
                // Check if we have an object selected
                if (selectInteractor.hasSelection)
                {
                    // Get the interactable GameObject
                    IXRSelectInteractable interactable = selectInteractor.interactablesSelected[0];
                    
                    Debug.Log($"Picked up: {interactable.transform.name}");
                }
            }
        }
    }
}
