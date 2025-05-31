using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralHitstop
{
    public class HaltableRig : MonoBehaviour
    {
        [HideInInspector] public Transform[] hitstopPoints;

        AnimancerComponent _animancer;
        AnimancerState _currentState;
        AnimancerState currentState => _currentState ??= animancer.States.Current;
        AnimancerComponent animancer => _animancer ??= GetComponent<AnimancerComponent>();

        public Animator animator => animancer.Animator;


        //public void PlayAnimation(ClipTransition transition) => _currentState = animancer.Play(lastTransitionPlayed = transition);
        public void PlayAnimation(ITransition transition) => _currentState = animancer.Play(transition);
        public void MatchOtherHaltableRig(HaltableRig other)
        {
            var currentState = animancer.Play(other.currentState.Clip);
            currentState.Speed = other.currentState.Speed;
            currentState.NormalizedTime = other.currentState.NormalizedTime;

        }
        public float CurrentAnimationTime() => currentState.Time;
        public void HaltAnimation() => currentState.Speed = 0;
        public void SetCurrentAnimationSpeed(float speed = 1) => currentState.Speed = speed;

        public void MoveAnimationTime(float deltaTime) => currentState.Time += deltaTime;
        public void SetAnimationTime(float time) => currentState.Time = time;



    }
}
