using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.MyCompany.MyGame.Collections;

namespace Com.MyCompany.MyGame
{
    public class UnitAnimationController
    {
        #region Private Fields

        private Unit _unit;
        private Animator _animator;

        #endregion

        #region Public Fields


        #endregion

        public UnitAnimationController(Unit unit, Animator animator)
        {
            _unit = unit;
            _animator = animator;

            _animator.SetBool("IsRunMode", true);
            _animator.SetBool("IsCrouchMode", false);

            _animator.SetLayerWeight(AnimationLayers.Crouch, 0);
            _animator.SetLayerWeight(AnimationLayers.CoverStanding, 0);
            _animator.SetLayerWeight(AnimationLayers.CoverCrouch, 0);
            _animator.SetLayerWeight(AnimationLayers.Throw, 0);
            _animator.SetLayerWeight(AnimationLayers.ThrowMove, 0);
        }

        #region Public Methods

        public void WalkPoseToNewPose(UnitPose newPose)
        {
            switch (newPose)
            {
                case UnitPose.MOD_RUN:
                    _animator.SetBool("IsRunMode", true);
                    _unit.curUnitPose = newPose;
                    break;

                case UnitPose.MOD_COVERSTAND:
                    _animator.SetBool("IsCovering", true);
                    //벽에 붙이기
                    _unit.StartCoroutine(_unit.SetCoverPosition(true));
                    _animator.SetLayerWeight(AnimationLayers.CoverStanding, 1);
                    _animator.SetLayerWeight(AnimationLayers.CoverCrouch, 0);

                    _unit.curUnitPose = newPose;
                    break;

                case UnitPose.MOD_CROUCH:
                    //_unit.curUnitPose 값은 SmoothCrouching()에서 변경
                    if (_animator.GetLayerWeight(AnimationLayers.Crouch) == 0)
                        _animator.SetBool("IsCrouchMode", true);
                    break;
                default:
                    break;
            }
        }

        public void RunPoseToNewPose(UnitPose newPose)
        {
            switch (newPose)
            {
                case UnitPose.MOD_WALK:
                    _animator.SetBool("IsRunMode", false);
                    _unit.curUnitPose = newPose;
                    break;

                case UnitPose.MOD_COVERSTAND:
                    _animator.SetBool("IsCovering", true);
                    //벽에 붙이기
                    _unit.StartCoroutine(_unit.SetCoverPosition(true));
                    _animator.SetLayerWeight(AnimationLayers.CoverStanding, 1);
                    _animator.SetLayerWeight(AnimationLayers.CoverCrouch, 0);

                    _unit.curUnitPose = newPose;
                    break;

                case UnitPose.MOD_CROUCH:
                    //_unit.curUnitPose 값은 SmoothCrouching()에서 변경
                    if (_animator.GetLayerWeight(AnimationLayers.Crouch) == 0)
                        _animator.SetBool("IsCrouchMode", true);
                    break;
                default:
                    break;
            }
        }

        public void CrouchPoseToNewPose(UnitPose newPose)
        {
            switch (newPose)
            {
                case UnitPose.MOD_COVERCROUCH:
                    _animator.SetBool("IsCovering", true);
                    //벽에 붙이기
                    _unit.StartCoroutine(_unit.SetCoverPosition(true));
                    _animator.SetLayerWeight(AnimationLayers.CoverStanding, 0);
                    _animator.SetLayerWeight(AnimationLayers.CoverCrouch, 1);

                    _unit.curUnitPose = newPose;
                    break;
                case UnitPose.MOD_RUN:
                    //_unit.curUnitPose 값은 SmoothStanding()에서 변경
                    if (_animator.GetLayerWeight(AnimationLayers.Crouch) == 1)
                        _animator.SetBool("IsCrouchMode", false);
                    break;
                default:
                    break;
            }
        }

        public void CoverStandingPoseToNewPose(UnitPose newPose)
        {
            switch (newPose)
            {
                case UnitPose.MOD_RUN:
                    _animator.SetBool("IsCovering", false);
                    //벽에 붙이기
                    _unit.StartCoroutine(_unit.SetCoverPosition(false));
                    _animator.SetLayerWeight(AnimationLayers.CoverStanding, 0);

                    _unit.curUnitPose = newPose;
                    break;
                case UnitPose.MOD_COVERCROUCH:
                    if(_animator.GetLayerWeight(AnimationLayers.CoverCrouch) == 0)
                        _animator.SetBool("IsCrouchMode", true);
                    break;
                default:
                    break;
            }
        }

        public void CoverCrouchPoseToNewPose(UnitPose newPose)
        {
            switch (newPose)
            {
                case UnitPose.MOD_RUN:
                    break;
                case UnitPose.MOD_COVERSTAND:
                    break;
                default:
                    break;
            }
        }
        
        //자연스러운 Standing -> Crouch, Player 전용
        public void SmoothCrouching(CapsuleCollider collider, float crouchColliderHeight,PlayerController player)
        {
            float layerWeight = _animator.GetLayerWeight(AnimationLayers.Crouch);
            
            if (layerWeight < 1 && _animator.GetBool("IsCrouchMode"))
            {
                _animator.SetBool("IsRunMode", false);

                collider.height = crouchColliderHeight;
                collider.center = new Vector3(0, crouchColliderHeight / 2, collider.center.z);

                layerWeight += (Time.deltaTime * 3f);
                if (layerWeight >= 1)
                {
                    layerWeight = 1;
                    if (_animator.GetBool("IsCovering"))
                        _unit.curUnitPose = UnitPose.MOD_COVERCROUCH;
                    else
                        _unit.curUnitPose = UnitPose.MOD_CROUCH;
                    player.SetAggro();
                }

                _animator.SetLayerWeight(AnimationLayers.Crouch, layerWeight);
                if (_animator.GetBool("IsCovering"))
                {
                    _animator.SetLayerWeight(AnimationLayers.CoverStanding, 1 - layerWeight);
                    _animator.SetLayerWeight(AnimationLayers.CoverCrouch, layerWeight);
                }
            }
        }

        //자연스러운 Standing -> Crouch, Enemy 전용
        public void SmoothCrouching(CapsuleCollider collider, float crouchColliderHeight)
        {
            float layerWeight = _animator.GetLayerWeight(AnimationLayers.Crouch);

            if (layerWeight < 1 && _animator.GetBool("IsCrouchMode"))
            {
                _animator.SetBool("IsRunMode", false);

                collider.height = crouchColliderHeight;
                collider.center = new Vector3(0, crouchColliderHeight / 2, collider.center.z);

                layerWeight += (Time.deltaTime * 3f);
                if (layerWeight >= 1)
                {
                    layerWeight = 1;
                    if (_animator.GetBool("IsCovering"))
                        _unit.curUnitPose = UnitPose.MOD_COVERCROUCH;
                    else
                        _unit.curUnitPose = UnitPose.MOD_CROUCH;
                }

                _animator.SetLayerWeight(AnimationLayers.Crouch, layerWeight);
                if (_animator.GetBool("IsCovering"))
                {
                    _animator.SetLayerWeight(AnimationLayers.CoverStanding, 1 - layerWeight);
                    _animator.SetLayerWeight(AnimationLayers.CoverCrouch, layerWeight);
                }
            }
        }

        //자연스러운 Crouch -> Standing, Player전용
        public void SmoothStanding(CapsuleCollider collider, float standColliderHeight, PlayerController player)
        {
            float layerWeight = _animator.GetLayerWeight(AnimationLayers.Crouch);

            if (layerWeight > 0 && !_animator.GetBool("IsCrouchMode"))
            {
                _animator.SetBool("IsRunMode", true);

                collider.height = standColliderHeight;
                collider.center = new Vector3(0, standColliderHeight / 2, collider.center.z);

                layerWeight -= (Time.deltaTime * 3f);
                if (layerWeight <= 0)
                {
                    layerWeight = 0;
                    _unit.curUnitPose = UnitPose.MOD_RUN;
                    player.SetAggro();
                }

                _animator.SetLayerWeight(AnimationLayers.Crouch, layerWeight);
            }
        }

        //자연스러운 Crouch -> Standing, Enemy전용
        public void SmoothStanding(CapsuleCollider collider, float standColliderHeight)
        {
            float layerWeight = _animator.GetLayerWeight(AnimationLayers.Crouch);

            if (layerWeight > 0 && !_animator.GetBool("IsCrouchMode"))
            {
                _animator.SetBool("IsRunMode", true);

                collider.height = standColliderHeight;
                collider.center = new Vector3(0, standColliderHeight / 2, collider.center.z);

                layerWeight -= (Time.deltaTime * 3f);
                if (layerWeight <= 0)
                {
                    layerWeight = 0;
                    _unit.curUnitPose = UnitPose.MOD_RUN;
                }

                _animator.SetLayerWeight(AnimationLayers.Crouch, layerWeight);
            }
        }


        #endregion
    }
}
