using Animancer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralHitstop
{
    [RequireComponent(typeof (HitStopAnimation))]
    public class ExampleCharacter : MonoBehaviour
    {
        [System.Serializable]
        struct AnimationWithHitstop
        {
            public float hitstopTime;
            public ScriptableClipTransition transition;
            public HitStopAnimation.IKTipRootPair hitstopPair;
            public HitStopAnimation.HitstopParameters hitstopParameters;

            public AnimationWithHitstop(float histopTime, ScriptableClipTransition transition, HitStopAnimation.IKTipRootPair hitstopPair, HitStopAnimation.HitstopParameters hitstopParameters)
            {
                this.hitstopTime = histopTime;
                this.transition = transition;
                this.hitstopPair = hitstopPair;
                this.hitstopParameters = hitstopParameters;
            }
        }
        [Header("Testing Variables")]
        [SerializeField] bool isUsingHitstop = true;
        [Range(0f, 1f)]
        [SerializeField] float currentSpeed = 1;

        [Header("Put all the animations you want to try here:")]
        [SerializeField] AnimationWithHitstop[] animationsWithHitstop;

        HitStopAnimation _hitstopAnimation;
        HitStopAnimation hitstopAnimation => _hitstopAnimation ??= GetComponent<HitStopAnimation>();

        // Start is called before the first frame update
        void Start()
        {
            hitstopAnimation.Initialize(animationsWithHitstop.Select(x=> x.hitstopPair).ToArray());

            // We make it so that the character randomly uses the animations provided and uses their corresponding hitstop parameters
            foreach (var info in animationsWithHitstop)
            {
                info.transition.ClipTransition.Events.OnEnd += PlayAnimation;
            }
            PlayAnimation();

        }

        private void Update()
        {
            Time.timeScale = currentSpeed;
        }

        void PlayAnimation()
        {
            int index = Random.Range(0, animationsWithHitstop.Length);
            var info = animationsWithHitstop[index];
            hitstopAnimation.mainAnimancer.Play(info.transition.ClipTransition);
            if(isUsingHitstop) hitstopAnimation.IncurHitStop(info.hitstopPair.tip, info.hitstopParameters, info.hitstopTime);
        }


    }
}
