using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Items
{
    /// <summary>
    /// Used to tell which hand is attached to which point
    /// </summary>
    enum AttachPointType
    {
        None,
        Main,
        Secondary
    }
    
    public class CrossBuh : MonoBehaviour, IWeapon
    {
        [Header("Crossbow Settings")]
        [Tooltip("Crossbow's shooting point")]
        [SerializeField] private Transform shootPoint;
        
        [Tooltip("Crossbow's arrow insert point")]
        [SerializeField] private Transform arrowInsertPoint;
        
        [Tooltip("How fast should the arrow travel when shot?")]
        [SerializeField] private float shootForce = 20f;
        
        [SerializeField] private float shootRange = 10f;
        
        [Tooltip("Crossbow's fire rate")]
        [SerializeField] private float fireRate = 1f;
        
        [Tooltip("Interactable arrow's prefab")]
        [SerializeField] private GameObject arrowPrefab;
        
        [Tooltip("Visuals for the arrow prefab. Will be used when instantiating arrow on the crossbow")]
        [SerializeField] private GameObject arrowPrefabVisuals;
        [SerializeField] private int maxArrows = 10;
        
        [Tooltip("How many arrows does one arrow reload")]
        [SerializeField] private int reloadAmount = 5;
        
        [Header("Cone Settings")]
        [SerializeField] private float coneAngle = 15f;
        [SerializeField] private float startRadius = 0.05f;
        private float m_nextFireTime;

        [Header("Hand Pose Settings")] 
        [SerializeField] private int triggerPoseID = 1;
        [SerializeField] private int handlePoseID = 2;
        
        // Count of currently loaded arrows.
        private int m_currentArrows;
        public bool IsCrossbowLoaded { get; private set; } = false;
        private GameObject m_currentArrowVisuals;

        // Grab interactable component to subscribe to grab events
        [SerializeField] private XRGrabInteractable interactable;
        
        // Attach points
        [SerializeField] private Transform leftHandTriggerAttach;
        [SerializeField] private Transform rightHandTriggerAttach;
        [SerializeField] private Transform leftHandHandleAttach;
        [SerializeField] private Transform rightHandHandleAttach;
        
        // Identify hand grab points between main and secondary
        private AttachPointType _leftHandAttachPointType = AttachPointType.None;
        private AttachPointType _rightHandAttachPointType = AttachPointType.None;
        
        // Interactors to keep track of which are active for this object, a.k.a which hands are holding the object
        private IXRSelectInteractor _leftHandInteractor;
        private IXRSelectInteractor _rightHandInteractor;

        private void Start()
        {
            if (interactable == null)
            {
                interactable = GetComponent<XRGrabInteractable>();
            }
            interactable.selectEntered.AddListener(OnGrab);
            interactable.selectExited.AddListener(OnRelease);
            interactable.hoverEntered.AddListener(OnHover);
        }

        /// <summary>
        /// Sets attach points prior to grabbing actual object.
        /// </summary>
        /// <param name="arg0"></param>
        private void OnHover(HoverEnterEventArgs arg0)
        {
            var interactor = arg0.interactorObject;
            var handedness = interactor.handedness;

            if (handedness == InteractorHandedness.Right)
            {
                interactable.attachTransform = rightHandTriggerAttach;
                interactable.secondaryAttachTransform = leftHandHandleAttach;
            }
            else if (handedness == InteractorHandedness.Left)
            {
                interactable.attachTransform = leftHandTriggerAttach;
                interactable.secondaryAttachTransform = rightHandHandleAttach;
            }
        }

        /// <summary>
        /// Gets called then the object is released. Checks which hand was holding it to properly clear variables, change poses.
        /// </summary>
        /// <param name="arg0"></param>
        private void OnRelease(SelectExitEventArgs arg0)
        {
            var interactor = arg0.interactorObject;
            var handedness = interactor.handedness;
            HandAnimator handAnimator = interactor.transform.GetComponentInParent<HandInteractableChecker>().GetHandAnimator();
            
            // Check if released hand was holding the handle.
            if (handedness == InteractorHandedness.Right)
            {
                // Right hand was holding the trigger. Change left hand's pose to hold the trigger.
                if (_rightHandAttachPointType == AttachPointType.Main)
                {
                    HandAnimator animator = _leftHandInteractor.transform.GetComponentInParent<HandInteractableChecker>().GetHandAnimator();
                    animator.SetHandPose(triggerPoseID);
                    _leftHandAttachPointType = AttachPointType.Main;
                }
                handAnimator.ClearHandPose();
                _rightHandAttachPointType = AttachPointType.None;
                _rightHandInteractor = null;
            }
            else if (handedness == InteractorHandedness.Left)
            {
                // Left hand was holding the trigger. Change right hand's pose to hold the trigger.
                if (_leftHandAttachPointType == AttachPointType.Main)
                {
                    HandAnimator animator = _rightHandInteractor.transform.GetComponentInParent<HandInteractableChecker>().GetHandAnimator();
                    animator.SetHandPose(triggerPoseID);
                    _rightHandAttachPointType = AttachPointType.Main;
                }
                handAnimator.ClearHandPose();
                _leftHandAttachPointType = AttachPointType.None;
                _leftHandInteractor = null;
            }
        }

        /// <summary>
        /// Sets hand pose depending on which hand picked up the object.
        /// </summary>
        /// <param name="arg0"></param>
        private void OnGrab(SelectEnterEventArgs arg0)
        {
            var interactor = arg0.interactorObject;
            var handedness = interactor.handedness;
            HandAnimator handAnimator = interactor.transform.GetComponentInParent<HandInteractableChecker>().GetHandAnimator();

            Debug.Log("Grabbing object...");
            if (handedness == InteractorHandedness.Right)
            {
                // Left hand is not holding it. Grab by trigger
                if (_leftHandInteractor == null)
                {
                    handAnimator.SetHandPose(triggerPoseID);
                    _rightHandAttachPointType = AttachPointType.Main;
                }
                else // Left hand is holding it. Grab by handle
                {
                    handAnimator.SetHandPose(handlePoseID);
                    _rightHandAttachPointType = AttachPointType.Secondary;
                }
                _rightHandInteractor = interactor;
            }
            else if (handedness == InteractorHandedness.Left)
            {
                // Right hand is not holding it. Grab by trigger
                if (_rightHandInteractor == null)
                {
                    handAnimator.SetHandPose(triggerPoseID);
                    _leftHandAttachPointType = AttachPointType.Main;
                }
                else // Right hand is holding it. Grab by handle
                {
                    handAnimator.SetHandPose(handlePoseID);
                    _leftHandAttachPointType = AttachPointType.Secondary;
                }
                _leftHandInteractor = interactor;
            }
        }

        public void UseWeapon()
        {
            if (!IsCrossbowLoaded) { return; }
            
            // Check for shooting cooldown
            if (Time.time < m_nextFireTime) return;
            m_nextFireTime = Time.time + 1f / fireRate;
            
            // Calculate random direction within cone
            Vector3 shootDirection = shootPoint.forward;
            
            // Instantiate arrow with colliders, rigidbody
            GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.LookRotation(shootDirection));

            arrow.GetComponent<Arrow>().Shoot(shootDirection, shootForce);

            m_currentArrows--;

            // If we ran out of arrows, destroy arrow visual and mark crossbow as unloaded.
            if (m_currentArrows <= 0)
            {
                Destroy(m_currentArrowVisuals);
                
                IsCrossbowLoaded = false;
            }
        }

        public bool TryReload()
        {
            // Check if crossbow is already reloaded
            if(IsCrossbowLoaded) { return false; }
            
            // Instantiate viewmodel arrow (only visuals) at insertion point
            m_currentArrowVisuals = Instantiate(arrowPrefabVisuals, arrowInsertPoint);

            m_currentArrows = reloadAmount;
            
            IsCrossbowLoaded = true;
            
            return true;
        }
        
        private Vector3 GetRandomDirectionInCone()
        {
            // Start with forward direction
            Vector3 direction = shootPoint.forward;
            // Apply random rotation within cone angle
            float randomAngle = Random.Range(0f, coneAngle * 0.5f);
            Vector3 randomAxis = Random.onUnitSphere;
            // Ensure the random axis is perpendicular to forward direction for more natural spread
            randomAxis = Vector3.Cross(randomAxis, direction).normalized;
            if (randomAxis.magnitude < 0.1f)
                randomAxis = Vector3.Cross(Vector3.up, direction).normalized;

            // Rotate the direction by random angle
            direction = Quaternion.AngleAxis(randomAngle, randomAxis) * direction;

            return direction.normalized;
        }
        private void OnDrawGizmosSelected()
        {
            if (shootPoint)
            {
                // Draw cone
                Gizmos.color = Color.red;
                float endRadius = Mathf.Tan(coneAngle * 0.5f * Mathf.Deg2Rad) * shootRange;
                Vector3 startPoint = shootPoint.position;
                Vector3 endPoint = shootPoint.position + shootPoint.forward * shootRange;
                // Draw start circle
                DrawGizmoCircle(startPoint, shootPoint.rotation, startRadius, 16);
                // Draw end circle
                DrawGizmoCircle(endPoint, shootPoint.rotation, endRadius, 16);
                // Draw connecting lines between circles
                Vector3[] startCirclePoints = GetCirclePoints(startPoint, shootPoint.rotation, startRadius, 8);
                Vector3[] endCirclePoints = GetCirclePoints(endPoint, shootPoint.rotation, endRadius, 8);
                for (int i = 0; i < 8; i++)
                {
                    Gizmos.DrawLine(startCirclePoints[i], endCirclePoints[i]);
                }
                // Draw wireframe box representing the cone volume
                Gizmos.color = new Color(1, 0, 0, 0.3f);
                DrawConeWireframe();
                Gizmos.color = Color.yellow;
                for (int i = 0; i < 10; i++)
                {
                    Vector3 sampleDir = GetRandomDirectionInCone();
                    Gizmos.DrawRay(shootPoint.position, sampleDir * shootRange);
                }
            }
        }
        private Vector3[] GetCirclePoints(Vector3 center, Quaternion rotation, float radius, int segments)
        {
            Vector3[] points = new Vector3[segments];
            Vector3 up = rotation * Vector3.up;
            Vector3 right = rotation * Vector3.right;
            float angleIncrement = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleIncrement * Mathf.Deg2Rad;
                points[i] = center + (up * Mathf.Sin(angle) + right * Mathf.Cos(angle)) * radius;
            }

            return points;
        }
        private void DrawGizmoCircle(Vector3 center, Quaternion rotation, float radius, int segments)
        {
            Vector3[] points = GetCirclePoints(center, rotation, radius, segments);

            for (int i = 0; i < segments; i++)
            {
                int nextIndex = (i + 1) % segments;
                Gizmos.DrawLine(points[i], points[nextIndex]);
            }
        }
        private void DrawConeWireframe()
        {
            float endRadius = Mathf.Tan(coneAngle * 0.5f * Mathf.Deg2Rad) * shootRange;
            Vector3 startPoint = shootPoint.position;
            Vector3 endPoint = shootPoint.position + shootPoint.forward * shootRange;
            Vector3 startUp = shootPoint.up * startRadius;
            Vector3 startRight = shootPoint.right * startRadius;
            Vector3 endUp = shootPoint.up * endRadius;
            Vector3 endRight = shootPoint.right * endRadius;
            Vector3[] startCorners = new Vector3[4];
            Vector3[] endCorners = new Vector3[4];
            startCorners[0] = startPoint + startUp + startRight;
            startCorners[1] = startPoint + startUp - startRight;
            startCorners[2] = startPoint - startUp - startRight;
            startCorners[3] = startPoint - startUp + startRight;
            endCorners[0] = endPoint + endUp + endRight;
            endCorners[1] = endPoint + endUp - endRight;
            endCorners[2] = endPoint - endUp - endRight;
            endCorners[3] = endPoint - endUp + endRight;
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(startCorners[i], endCorners[i]);
            }
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(startCorners[i], startCorners[(i + 1) % 4]);
            }
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(endCorners[i], endCorners[(i + 1) % 4]);
            }
        }
    }
}