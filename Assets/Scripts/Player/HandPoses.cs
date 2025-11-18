using System;
using UnityEngine;

namespace Player
{
    [Serializable]
    public class HandPoses : MonoBehaviour
    {
        [Header("Hand poses")]
        [Tooltip("Left hand poses. One pose when item is held with left hand, other when with right, but left is used as support.")]
        public AnimationClip leftPoseMain;
        public AnimationClip leftPoseSupport;
        
        [Tooltip("Right hand poses. One pose when item is held with right hand, other when with left, but right is used as support.")]
        public AnimationClip rightPoseMain;
        public AnimationClip rightPoseSupport;

    }
}
