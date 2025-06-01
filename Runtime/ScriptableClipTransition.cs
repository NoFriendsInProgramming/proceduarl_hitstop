using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralHitstop
{
    [CreateAssetMenu(fileName = "ClipTransition", menuName = "AnimancerUtilities/ScriptableClipTransition", order = 100)]
    public class ScriptableClipTransition : ScriptableObject
    {
        [SerializeField] ClipTransition transition;

        public AnimationClip clip => transition.Clip;
        public ClipTransition ClipTransition => transition;

    }
}
