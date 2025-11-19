using Player;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Items
{
    public class MeleeWeapon : MonoBehaviour
    {
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
