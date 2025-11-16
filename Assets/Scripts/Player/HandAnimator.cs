using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Player
{
    enum ControllerSide { Left, Right }
    
    public class HandAnimator : MonoBehaviour
    {
        private InputDevice _targetDevice;
        private InputDeviceCharacteristics _deviceCharacteristics;
        
        [Tooltip("Which controller side to use (Left or Right)?")]
        [SerializeField] private ControllerSide _controllerSide;

        private void Start()
        {
            Init();
        }
        
        private void Init()
        {
            List<InputDevice> devices = new List<InputDevice>();

            // Get left or right controller, depending on provided value _controllerSide
            switch (_controllerSide)
            {
                case ControllerSide.Left:
                    _deviceCharacteristics =
                        InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
                    break;
                case ControllerSide.Right:
                    _deviceCharacteristics =
                        InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
                    break;
                default:
                    Debug.LogError("Invalid controller side. Please assign it on HandAnimator component.");
                    break;
            }

            InputDevices.GetDevicesWithCharacteristics(_deviceCharacteristics, devices);
            
            if (devices.Count > 0)
            {
                _targetDevice = devices[0];
            }
        }

        private void Update()
        {
            // Read the input
            _targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float primaryButtonValue);
        }
    }
}
