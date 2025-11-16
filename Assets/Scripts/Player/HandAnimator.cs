using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Player
{
    public class HandAnimator : MonoBehaviour
    {
        [Tooltip("Which characteristics to use when detecting a device?")]
        [SerializeField] private InputDeviceCharacteristics _deviceCharacteristics;

        [Tooltip("Reference to hand animator. Can be left unassigned.")]
        [SerializeField] private Animator _animator;

        private InputDevice _targetDevice; // Current device
        private Vector2 m_input;
        
        private void Start()
        {
            Init();
        }
        
        private void Init()
        {
            List<InputDevice> devices = new List<InputDevice>();

            InputDevices.GetDevicesWithCharacteristics(_deviceCharacteristics, devices);
            
            if (devices.Count > 0)
            {
                _targetDevice = devices[0];
            }
            
            // Find the animator if not assigned
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }

        private void Update()
        {
            // Animate hand
            AnimateHand();
        }

        private void FixedUpdate()
        {
            // Read the input
            GatherInput();
        }
        
        private void GatherInput()
        {
            _targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerFloatValue);
            _targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripFloatValue);
            m_input = new Vector2(triggerFloatValue, gripFloatValue);
        }

        /// <summary>
        /// Animates hand. m_input.x is trigger, m_input.y is grip.
        /// </summary>
        private void AnimateHand()
        {
            // Animate Trigger
            if (m_input.x > 0)
            {
                _animator.SetFloat(HandAnimatorParameters.Trigger, m_input.x);
            }
            else
            {
                _animator.SetFloat(HandAnimatorParameters.Trigger, 0);
            }
            
            // Animate Grip
            if (m_input.y > 0)
            {
                _animator.SetFloat(HandAnimatorParameters.Grip, m_input.y);
            }
            else
            {
                _animator.SetFloat(HandAnimatorParameters.Grip, 0);
            }
        }
    }

    public static class HandAnimatorParameters
    {
        public static readonly int Grip = Animator.StringToHash("Grip");
        public static readonly int Trigger = Animator.StringToHash("Trigger");
    }
}
