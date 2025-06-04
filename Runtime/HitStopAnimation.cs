// Copyright (c) 2025 Alex Ruiz Suarez
// Licensed under CC BY-NC-ND 4.0
// See LICENSE file for details

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Animancer;
using System.Linq;
namespace ProceduralHitstop
{
    public class HitStopAnimation : MonoBehaviour
    {
        [System.Serializable]
        public struct IKTipRootPair
        {
            public Transform tip;
            public Transform root;
            public IKTipRootPair(Transform tip, Transform root)
            {
                this.tip = tip;
                this.root = root;
            }
        }
        [System.Serializable]
        public struct HitstopParameters
        {
            public float hitstopDuration;
            public float hitstopSpeedMultiplier;
            public float hitstopReturnDuration;
            public AnimationCurve returnCurve;

            public HitstopParameters(float hitstopDuration, float hitstopSpeedMultiplier, float hitstopReturnDuration, AnimationCurve returnCurve)
            {
                this.hitstopDuration = hitstopDuration;
                this.hitstopSpeedMultiplier = hitstopSpeedMultiplier;
                this.hitstopReturnDuration = hitstopReturnDuration;
                this.returnCurve = returnCurve;
            }
        }
        struct IkController
        {
            public ChainIKConstraint constraint;
            public Transform impactPointReference;

            public IkController(ChainIKConstraint constraint, Transform impactPointReference)
            {
                this.constraint = constraint;
                this.impactPointReference = impactPointReference;
            }
        }

        bool hasBeenInialized = false;

        Transform target;
        Dictionary<Transform, IkController> ikControllers;
        Rig rig;
        RigBuilder builder;

        [SerializeField] IKTipRootPair[] manuallyPlacedHitstopPairs;
        [SerializeField] HitstopParameters defaultHitstopParameters = new HitstopParameters(0.1f, 0.2f, 0.3f, AnimationCurve.EaseInOut(0, 0, 1, 1));

        HaltableRig _referenceRig;
        HaltableRig haltableRig;
        HaltableRig referenceRig => _referenceRig ??= CreateReferenceRig();

        public AnimancerComponent mainAnimancer => haltableRig.animancer;
        HaltableRig CreateReferenceRig()
        {
            var rig = Instantiate(haltableRig);
            Destroy(rig.GetComponentInChildren<Rig>()?.gameObject);
            rig.transform.SetParent(transform, false);
            rig.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        
            foreach (var renderer in rig.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                renderer.enabled = false;
            }
        
            return rig;
        }

        private void Awake()
        {
            if (manuallyPlacedHitstopPairs != null && manuallyPlacedHitstopPairs.Length > 0)
            {
                Initialize(manuallyPlacedHitstopPairs);
            }
        }

        public void Initialize(IKTipRootPair[] hitstopPairInfo)
        {
            if (!hasBeenInialized)
            {
                var animator = GetComponentInChildren<Animator>();
                haltableRig = animator.GetComponent<HaltableRig>();
                if (haltableRig == null)
                {
                    haltableRig = animator.gameObject.AddComponent<HaltableRig>();
                }

                hasBeenInialized = true;
                builder = haltableRig.gameObject.AddComponent<RigBuilder>();

                rig = new GameObject("Rig").AddComponent<Rig>();
                rig.transform.SetParent(haltableRig.transform);
                rig.weight = 1;
                builder.layers.Add(new RigLayer(rig, true));

                target = new GameObject("Target").transform;

                haltableRig.hitstopPoints = hitstopPairInfo.Select(x=>x.tip).ToArray();

                ikControllers = new Dictionary<Transform, IkController>();
                for (int i = 0; i < hitstopPairInfo.Length; i++)
                {
                    CreateChainIkConstraint(i, hitstopPairInfo[i].root);
                }

            
                Component[] components = referenceRig.GetComponentInChildren<SkinnedMeshRenderer>().rootBone.GetComponentsInChildren<Component>();

                foreach (Component comp in components)
                {
                    // We remove any unnecessary scripts that may be in the skeleton, since it is a duplicate and could have unintended consequences
                    if (!(comp is Transform))
                    {
                        Destroy(comp);
                    }
                }
            

                builder.Build();

            }
        }

        void CreateChainIkConstraint(int hitstopPointIndex, Transform ikRoot)
        {
            Transform hitstopPoint = haltableRig.hitstopPoints[hitstopPointIndex];
            if (!ikControllers.ContainsKey(hitstopPoint))
            {
                ChainIKConstraint ikConstraint;
                ikControllers.Add(hitstopPoint,
                                  new IkController(ikConstraint = (new GameObject("IK" + hitstopPoint.name)).AddComponent<ChainIKConstraint>(),
                                  referenceRig.hitstopPoints[hitstopPointIndex]));
                ikConstraint.Reset();
                ikConstraint.transform.SetParent(rig.transform);
                ikConstraint.data.target = target;
                ikConstraint.data.root = ikRoot;
                ikConstraint.data.tip = hitstopPoint;
                ikConstraint.weight = 0;
            }
        }

        void MatchIKTargetWithBone(Transform bone)
        {
            target.SetParent(bone);
            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.Euler(Vector3.zero);
        }

        public void IncurHitStop(Transform impactPoint, float delay = 0.55f)
        {
            StartCoroutine(HitStop(impactPoint, defaultHitstopParameters, delay));
        }

        public void IncurHitStop(Transform impactPoint, HitstopParameters parameters, float delay = 0.55f)
        {
            StartCoroutine(HitStop(impactPoint, parameters, delay));
        }

        IEnumerator HitStop(Transform impactPoint, HitstopParameters parameters, float delay = 0)
        {

            referenceRig.MatchOtherHaltableRig(haltableRig);
            yield return delay == 0 ? null : new WaitForSeconds(delay);

            if (ikControllers.ContainsKey(impactPoint))
            {
                ChainIKConstraint ikConstraint = ikControllers[impactPoint].constraint;
            
                ikConstraint.weight = 1;
                float animationTimeBeforeHitStop = referenceRig.CurrentAnimationTime();
                MatchIKTargetWithBone(ikControllers[impactPoint].impactPointReference);

                referenceRig.SetCurrentAnimationSpeed(parameters.hitstopSpeedMultiplier);

                yield return new WaitForSeconds(parameters.hitstopDuration);

                float postImpactTime = referenceRig.CurrentAnimationTime();
                var durationToCatchUpOn = (1 - parameters.hitstopSpeedMultiplier) * parameters.hitstopDuration + parameters.hitstopReturnDuration;
                var normalizeMultiplier = 1 / parameters.hitstopReturnDuration;

                for (float i = 0; i <= parameters.hitstopReturnDuration; i += Time.deltaTime)
                {
                    referenceRig.SetAnimationTime(postImpactTime + (parameters.returnCurve.Evaluate(i * normalizeMultiplier) * durationToCatchUpOn));
                    yield return null;
                }

                referenceRig.animancer.Stop();

                ikConstraint.weight = 0;
            }
            else
            {
                Debug.LogError("Point of impact inputted for the hitstop mechanism not found in the original list of usable transforms."
                    + " If you want to use this transform to create a hitstop effect make sure to pass it to the Initialize function or in the manuallyPlacedHitstopPoints array variable");
            }


        }

    }
}
