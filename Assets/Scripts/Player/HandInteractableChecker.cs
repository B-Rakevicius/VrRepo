using Items;
using UI;
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
        [SerializeField] private InputActionReference m_InputAction_Use;
        [Tooltip("Reference to `Show item info UI` input action")]
        [SerializeField] private InputActionReference m_InputAction_ToggleUI;
        
        [Tooltip("Reference to left/right hand interactor")]
        [SerializeField] private XRInteractionGroup m_InteractionGroup;

        // Interactable object
        private IXRSelectInteractable m_Interactable;

        // Input values
        private float m_InputValue;
        
        // Optimization booleans
        private bool m_IsHolding;
        private bool m_stopCleaning = true;

        private float m_UIButtonLockedTill = 0f;


        private void Awake()
        {
            m_InteractionGroup = GetComponent<XRInteractionGroup>();
        }

        private void Start()
        {
            m_InputAction_ToggleUI.action.performed += InputAction_ToggleUI;
        }

        private void OnDestroy()
        {
            m_InputAction_ToggleUI.action.performed -= InputAction_ToggleUI;
        }

        /// <summary>
        /// Used to show item info panel when Show Item UI button is pressed
        /// </summary>
        /// <param name="obj"></param>
        private void InputAction_ToggleUI(InputAction.CallbackContext obj)
        {
            IsHoldingInteractable();
            if (!m_IsHolding) return;
            // Get ItemUI component and toggle UI
            if (m_Interactable.transform.TryGetComponent(out ItemUI itemUI) && Time.time > m_UIButtonLockedTill)
            {
                itemUI.ToggleUI();
                m_UIButtonLockedTill = Time.time + itemUI.AnimDuration;
            }
        }

        private void Update()
        {
            GatherInput();
        }

        private void GatherInput()
        {
            m_InputValue = m_InputAction_Use.action.ReadValue<float>();
            
            // We are holding the button. Try to activate currently held item
            if (m_InputValue > 0)
            {
                IsHoldingInteractable();
                if (!m_IsHolding) return;
                ActivateHeldItem();
            }
            else
            {
                if (m_Interactable == null) return;
                if (m_Interactable.transform.TryGetComponent(out ITool tool))
                {
                    tool.DeactivateTool();
                }
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
            // If it's a weapon (crossbow, vacuum cleaner), activate it.
            if (m_Interactable.transform.TryGetComponent(out IWeapon weapon))
            {
                weapon.UseWeapon();
            }
            else if(m_Interactable.transform.TryGetComponent(out ITool tool)) // It's a tool
            {
                tool.ActivateTool();
            }
        }
    }
}