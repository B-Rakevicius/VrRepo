using Items;
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
        
        // Interactable object
        private IXRSelectInteractable m_Interactable;
        
        // Input values
        private float m_ActivateVacuumValue;
        
        // Optimization booleans
        private bool m_IsHolding;

        private void Awake()
        {
            m_InteractionGroup = GetComponent<XRInteractionGroup>();
        }

        private void Update()
        {
            GatherInput();
        }

        private void GatherInput()
        {
            m_ActivateVacuumValue = m_ActivateVacuum.action.ReadValue<float>();
            
            // We are holding the button. Try to do the cleaning.
            if (m_ActivateVacuumValue > 0)
            {
                IsHoldingInteractable();
                if (m_IsHolding)
                {
                    ActivateVacuum();
                }
            }
        }

        private void IsHoldingInteractable()
        {
            IXRInteractor activeInteractor = m_InteractionGroup.activeInteractor;
            if (activeInteractor is IXRSelectInteractor selectInteractor)
            {
                // Check if we have an object selected
                if (selectInteractor.hasSelection)
                {
                    // Get the interactable GameObject
                    m_Interactable = selectInteractor.interactablesSelected[0];

                    Debug.Log($"Picked up: {m_Interactable.transform.name}");
                    
                    m_IsHolding = true;
                }
                else
                {
                    ResetInteractableRef();
                }
            }
            else
            {
                ResetInteractableRef();
            }
        }

        private void ResetInteractableRef()
        {
            Debug.Log("Not holding an interactable!");
            m_IsHolding = false;
            m_Interactable = null;
        }

        private void ActivateVacuum()
        {
            // If it's Vacuum Cleaner, activate it
            if (m_Interactable.transform.TryGetComponent(out VacuumCleaner vacuumCleaner))
            {
                vacuumCleaner.ActivateCleaner();
            }
            else // It's a regular item or something else
            {
                Debug.Log("Item is not a vacuum cleaner!");
            }
        }
    }
}
