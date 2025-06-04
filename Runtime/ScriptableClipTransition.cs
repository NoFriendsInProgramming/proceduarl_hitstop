// Copyright (c) 2025 Alex Ruiz Suarez
// Licensed under CC BY-NC-ND 4.0
// See LICENSE file for details

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
