using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.XR;

namespace SceneAssets.Room.Scripts.Hand
{
    [Serializable]
    public class TriggerButtonEvent : UnityEvent<bool>{
    }

    [Serializable]
    public class primary2DAxisClick : UnityEvent<bool>{
    }

    public class HandInputWatcher : MonoBehaviour
    {
        InputFeatureUsage<bool> trigger_button_feature = CommonUsages.triggerButton;
        InputFeatureUsage<bool> touch_button_feature = CommonUsages.primary2DAxisClick;
        [SerializeField] private XRNode xrNode = XRNode.LeftHand;
    
        public TriggerButtonEvent triggerButtonPress;
        public primary2DAxisClick touchButtonPress;
        private bool lastTriggerButtonState;
        private bool lastTouchButtonState;
        private List<InputDevice> all_devices;
        List<InputDevice> trackedDevices;

        void Start(){
            if (triggerButtonPress == null){
                triggerButtonPress = new TriggerButtonEvent();
            }
        
            if (touchButtonPress == null){
                touchButtonPress = new primary2DAxisClick();
            }

            all_devices = new List<InputDevice>();
            trackedDevices = new List<InputDevice>();
            InputTracking.nodeAdded += InputTracking_nodeAdded;
        }

        // check for new input devices when new XRNode is added
        private void InputTracking_nodeAdded(XRNodeState obj){
            updateInputDevices();
        }

        void Update(){
            bool tempTriggerButtonState = false;
            bool tempTouchButtonState = false;
            bool invalidDeviceFound = false;
            foreach (var device in trackedDevices){
                tempTriggerButtonState = device.isValid // the device is still valid
                                         && device.TryGetFeatureValue(trigger_button_feature, out var buttonState) // did get a value
                                         && buttonState // the value we got
                                         || tempTriggerButtonState; // cumulative result from other controllers

                tempTouchButtonState = device.isValid // the device is still valid
                                       && device.TryGetFeatureValue(touch_button_feature, out var touchButtonState) // did get a value
                                       && touchButtonState // the value we got
                                       || tempTouchButtonState; // cumulative result from other controllers
            
            
                if (!device.isValid)
                    invalidDeviceFound = true;
            }

            if (tempTriggerButtonState != lastTriggerButtonState) { // Button state changed since last frame
                triggerButtonPress.Invoke(tempTriggerButtonState);
                lastTriggerButtonState = tempTriggerButtonState;
            }
        
            if (tempTouchButtonState != lastTouchButtonState) { // Button state changed since last frame
                touchButtonPress.Invoke(tempTouchButtonState);
                lastTouchButtonState = tempTouchButtonState;
            }

            if (invalidDeviceFound || trackedDevices.Count == 0) // refresh device lists
                updateInputDevices();
        }

        // find any devices supporting the desired feature usage
        void updateInputDevices(){
            trackedDevices.Clear();
            InputDevices.GetDevicesAtXRNode(xrNode, all_devices);
            bool discardedValue;

            foreach (var device in all_devices){
                if (device.TryGetFeatureValue(trigger_button_feature, out discardedValue)){
                    trackedDevices.Add(device); // Add any devices that have a trigger button.
                }else if (device.TryGetFeatureValue(touch_button_feature, out discardedValue)){
                    trackedDevices.Add(device);
                }
            }
        }
    }
}