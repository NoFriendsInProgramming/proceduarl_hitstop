// Copyright (c) 2025 Alex Ruiz Suarez
// Licensed under CC BY-NC-ND 4.0
// See LICENSE file for details

using Animancer;
using UnityEngine;

namespace ProceduralHitstop
{
    public class HaltableRig : MonoBehaviour
    {
        [HideInInspector] public Transform[] hitstopPoints;

        AnimancerComponent _animancer;
        AnimancerState currentState => animancer.States.Current;
        public AnimancerComponent animancer => _animancer ??= GetComponent<AnimancerComponent>();

        public Animator animator => animancer.Animator;

        public void MatchOtherHaltableRig(HaltableRig other)
        {
            var currentState = animancer.Play(other.currentState.Clip);
            currentState.Speed = other.currentState.Speed;
            currentState.NormalizedTime = other.currentState.NormalizedTime;

        }

        public void MatchOtherHaltableRig(HaltableRig other, float normalizedTime)
        {
            var currentState = animancer.Play(other.currentState.Clip);
            currentState.Speed = other.currentState.Speed;
            currentState.NormalizedTime = normalizedTime;

        }
        public float CurrentAnimationTime() => currentState.Time;
        public void HaltAnimation() => currentState.Speed = 0;
        public void SetCurrentAnimationSpeed(float speed = 1) => currentState.Speed = speed;

        public void MoveAnimationTime(float deltaTime) => currentState.Time += deltaTime;
        public void SetAnimationTime(float time) => currentState.Time = time;



    }
}
