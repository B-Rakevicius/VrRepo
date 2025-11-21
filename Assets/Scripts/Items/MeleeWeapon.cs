using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Items
{
    public class MeleeWeapon : MonoBehaviour
    {
        [Header("Weapon Settings")] 
        [Tooltip("How much damage should this melee weapon inflict?")]
        [SerializeField] private float damage = 5f;
        [Tooltip("How much knockback should this melee weapon inflict?")]
        [SerializeField] private float knockback = 5f;
        [Tooltip("Minimum speed from which hit detection activates")]
        [SerializeField] private float minVelocityThreshold = 1f;
        [Tooltip("Reference to weapon's part, that will do damage")] 
        [SerializeField] private Transform damagePart;
        
        private Vector3 m_LastPos;
        private float m_Velocity;
        private Vector3 m_Direction;
        
        [Header("Hand Pose Settings")]
        [Tooltip("Defines which hands animation blend tree to use.")]
        [SerializeField] private int handlePoseID = 4;
        
        [Header("Grab Points")]
        [SerializeField] private Transform leftHandGrabPointLower;
        [SerializeField] private Transform leftHandGrabPointUpper;
        [SerializeField] private Transform rightHandGrabPointLower;
        [SerializeField] private Transform rightHandGrabPointUpper;
        
        // Grab interactable component to subscribe to grab events
        private XRGrabInteractable _interactable;
        
        // Identify hand grab points between main and secondary
        private AttachPointType _leftHandAttachPointType = AttachPointType.None;
        private AttachPointType _rightHandAttachPointType = AttachPointType.None;
        
        private void Start()
        {
            _interactable = GetComponent<XRGrabInteractable>();
            
            // Subscribe to grab events
            _interactable.selectEntered.AddListener(OnGrab);
            _interactable.selectExited.AddListener(OnRelease);
            _interactable.hoverEntered.AddListener(OnHover);

            m_LastPos = damagePart.position;
        }


        private void OnDisable()
        {
            _interactable.selectEntered.RemoveListener(OnGrab);
            _interactable.selectExited.RemoveListener(OnRelease);
            _interactable.hoverEntered.RemoveListener(OnHover);
        }
        
        private void Update()
        {
            CalculateWeaponVelocity();
        }

        /// <summary>
        /// Returns a list of controllers that meet provided characteristics.
        /// </summary>
        /// <param name="characteristics">Controller characteristics</param>
        /// <param name="devices">A list of controllers</param>
        /// <returns>List of controllers that meet provided characteristics.</returns>
        private bool GetDevicesWithCharacteristics(InputDeviceCharacteristics characteristics,
            out List<InputDevice> devices)
        {
            devices = new List<InputDevice>();
            
            InputDevices.GetDevicesWithCharacteristics(characteristics, devices);

            return devices.Count > 0;
        }
        
        /// <summary>
        /// Gets provided hand's velocity.
        /// </summary>
        /// <param name="device">Controller, which velocity should be gotten.</param>
        /// <returns>Hand's velocity</returns>
        private float GetHandVelocity(InputDevice device)
        {
            Vector3 velocity = Vector3.zero;

            device.TryGetFeatureValue(CommonUsages.deviceVelocity, out velocity);

            return velocity.magnitude;
        }

        /// <summary>
        /// Gets called each frame to calculate weapons velocity.
        /// </summary>
        private void CalculateWeaponVelocity()
        {
            InputDeviceCharacteristics characteristics;
            
            DetectHandCharacteristics(out characteristics);

            Vector3 currentPos = damagePart.position;
            m_Direction = (currentPos - m_LastPos).normalized;
            float distance = m_Direction.magnitude;

            // Object is in hands - calculate velocity based of controller
            if (characteristics != InputDeviceCharacteristics.None)
            {
                if (GetDevicesWithCharacteristics(characteristics, out List<InputDevice> devices))
                {
                    m_Velocity = GetHandVelocity(devices[0]);
                }
            }
            // Object is not in hands - calculate velocity based of the object itself
            else
            {
                // Basic physics speed formula: speed = distance / time
                m_Velocity = distance / Time.deltaTime;
            }
                        
            m_LastPos = currentPos;
        }
        
        /// <summary>
        /// Uses weapon's velocity to determine if collider can receive damage.
        /// </summary>
        /// <param name="other">Collider to interact with</param>
        public void DetectHits(Collider other)
        {
            // Check if velocity is not too low
            if (m_Velocity > minVelocityThreshold)
            {
                if (other.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(damage, m_Direction, knockback);
                }
            }
        }

        /// <summary>
        /// Finds which controller should be considered main for calculating velocity.
        /// </summary>
        /// <param name="characteristics">Controller characteristics.</param>
        private void DetectHandCharacteristics(out InputDeviceCharacteristics characteristics)
        {
            // Only left hand is holding the weapon
            if (_leftHandAttachPointType == AttachPointType.Main && _rightHandAttachPointType == AttachPointType.None)
            {
                // Construct characteristics
                characteristics = InputDeviceCharacteristics.Left;
            }
            // Left hand is holding lower handle, and right hand - upper handle
            else if (_leftHandAttachPointType == AttachPointType.Main &&
                     _rightHandAttachPointType == AttachPointType.Secondary)
            {
                // Construct characteristics
                characteristics = InputDeviceCharacteristics.Right;
            }
            // Only right hand is holding
            else if (_rightHandAttachPointType == AttachPointType.Main &&
                     _leftHandAttachPointType == AttachPointType.None)
            {
                // Construct characteristics
                characteristics = InputDeviceCharacteristics.Right;
            }
            // Right hand is holding lower handle, and left hand - upper handle
            else if (_rightHandAttachPointType == AttachPointType.Main &&
                     _leftHandAttachPointType == AttachPointType.Secondary)
            {
                // Construct characteristics
                characteristics = InputDeviceCharacteristics.Left;
            }
            else
            {
                characteristics = InputDeviceCharacteristics.None;
            }
        }

        /// <summary>
        /// Sets attach points prior to grabbing actual object.
        /// </summary>
        /// <param name="arg0"></param>
        private void OnHover(HoverEnterEventArgs arg0)
        {
            // It's possible to hover while holding the object in other hand. Check this before changing attach transforms
            if (_rightHandAttachPointType != AttachPointType.None || _leftHandAttachPointType != AttachPointType.None)
            { return; }
            
            NearFarInteractor interactor = arg0.interactorObject as NearFarInteractor;
            InteractorHandedness handedness = interactor.handedness;

            if (handedness == InteractorHandedness.Right)
            {
                SwapAttachTransformsRightSided();
            }
            else if (handedness == InteractorHandedness.Left)
            {
                SwapAttachTransformsLeftSided();
            }
        }

        /// <summary>
        /// Gets called then the object is released. Checks which hand was holding it to properly clear variables, change poses.
        /// </summary>
        /// <param name="arg0"></param>
        private void OnRelease(SelectExitEventArgs arg0)
        {
            NearFarInteractor interactor = arg0.interactorObject as NearFarInteractor;
            InteractorHandedness handedness = interactor.handedness;
            HandAnimator handAnimator = interactor.transform.GetComponentInParent<HandInteractableChecker>().GetHandAnimator();

            if (handedness == InteractorHandedness.Right)
            {
                // Right hand is attached to main grab point. Reset right hand, change left hand's pose.
                if (_rightHandAttachPointType == AttachPointType.Main && _leftHandAttachPointType == AttachPointType.Secondary)
                {
                    SwapAttachTransformsLeftSided();
                    _leftHandAttachPointType = AttachPointType.Main;
                }
                
                _rightHandAttachPointType = AttachPointType.None;
            }
            else if (handedness == InteractorHandedness.Left)
            {
                // Left hand is attached to main grab point. Reset left hand, change right hand's pose.
                if (_leftHandAttachPointType == AttachPointType.Main && _rightHandAttachPointType == AttachPointType.Secondary)
                {
                    SwapAttachTransformsRightSided();
                    _rightHandAttachPointType = AttachPointType.Main;
                }
                
                _leftHandAttachPointType = AttachPointType.None;
            }
            handAnimator.ClearHandPose();
        }

        /// <summary>
        /// Sets hand pose depending on which hand picked up the object.
        /// </summary>
        /// <param name="arg0"></param>
        private void OnGrab(SelectEnterEventArgs arg0)
        {
            NearFarInteractor interactor = arg0.interactorObject as NearFarInteractor;
            InteractorHandedness handedness = interactor.handedness;
            HandAnimator handAnimator = interactor.transform.GetComponentInParent<HandInteractableChecker>().GetHandAnimator();

            if (handedness == InteractorHandedness.Right)
            {
                _rightHandAttachPointType = _leftHandAttachPointType != AttachPointType.Main ? AttachPointType.Main : AttachPointType.Secondary;
            }
            else if (handedness == InteractorHandedness.Left)
            {
                _leftHandAttachPointType = _rightHandAttachPointType != AttachPointType.Main ? AttachPointType.Main : AttachPointType.Secondary;
            }
            handAnimator.SetHandPose(handlePoseID);
        }

        /// <summary>
        /// Swaps grab point transforms, so that left hand holds lower handle point, and right hand - upper grab point.
        /// </summary>
        private void SwapAttachTransformsLeftSided()
        {
            _interactable.attachTransform = leftHandGrabPointLower;
            _interactable.secondaryAttachTransform = rightHandGrabPointUpper;
        }

        /// <summary>
        /// Swaps grab point transforms, so that right hand holds lower handle point, and left hand - upper grab point.
        /// </summary>
        private void SwapAttachTransformsRightSided()
        {
            _interactable.attachTransform = rightHandGrabPointLower;
            _interactable.secondaryAttachTransform = leftHandGrabPointUpper;
        }
    }
}
