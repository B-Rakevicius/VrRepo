using Items;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Player
{
    public class HandInteractableChecker : MonoBehaviour
    {
        [Tooltip("Reference to input action")]
        [SerializeField] private InputActionReference m_InputAction;
        
        [Tooltip("Reference to left/right hand interactor")]
        [SerializeField] private XRInteractionGroup m_InteractionGroup;

        // Interactable object
        private IXRSelectInteractable m_Interactable;

        // Input values
        private float m_InputValue;
        
        // Optimization booleans
        private bool m_IsHolding;
        private bool m_stopCleaning = true;


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
            m_InputValue = m_InputAction.action.ReadValue<float>();
            
            // We are holding the button. Try to activate currently held item
            if (m_InputValue > 0)
            {
                IsHoldingInteractable();
                if (!m_IsHolding) return;
                ActivateHeldItem();
            }
            else
            {
                if (m_stopCleaning) return;
                if (m_Interactable == null) return;
                if (!m_Interactable.transform.TryGetComponent(out VacuumCleaner2 vacuumCleaner)) return;
                vacuumCleaner.StopCleaner();
                m_stopCleaning = true;
            }
        }

        private void IsHoldingInteractable()
        {
            IXRInteractor activeInteractor = m_InteractionGroup.activeInteractor;
            if (activeInteractor is not IXRSelectInteractor selectInteractor)
            {
                ResetInteractableRef();
            }
            else
            {
                // Check if we have an object selected
                if (!selectInteractor.hasSelection)
                {
                    ResetInteractableRef();
                }
                else
                {
                    // Get the interactable GameObject
                    m_Interactable = selectInteractor.interactablesSelected[0];
                    m_IsHolding = true;
                }
            }
        }

        private void ResetInteractableRef()
        {
            m_IsHolding = false;
            m_Interactable = null;
        }

        private void ActivateHeldItem()
        {
            // If it's Vacuum Cleaner, activate it
            if (m_Interactable.transform.TryGetComponent(out VacuumCleaner2 vacuumCleaner))
            {
                vacuumCleaner.VacuumOrbs();
                m_stopCleaning = false;
            }
            // If it's Crossbow, shoot it
            else if (m_Interactable.transform.TryGetComponent(out CrossBuh crossbow))
            {
                crossbow.Shoot();
            }
            else // It's a regular item or something else
            {
                Debug.Log("Item is not a vacuum cleaner or crossbow!");
            }
        }
    }
}