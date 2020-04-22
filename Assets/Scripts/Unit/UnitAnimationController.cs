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
        private int _currentAnimLayer = 0;

        #endregion

        #region Public Fields

        public int currentAnimLayer { get { return _currentAnimLayer; } }
        public AnimatorStateInfo currentAnimStateInfo { get { return _animator.GetCurrentAnimatorStateInfo(_currentAnimLayer); } }

        #endregion

        public UnitAnimationController(Unit unit, Animator animator)
        {
            _unit = unit;
            _animator = animator;

            _animator.SetBool("IsRunMode", true);
            _animator.SetBool("IsCrouchMode", false);
            _animator.SetFloat("AttackAnimSpeed", 2.5f);

            _animator.SetLayerWeight(AnimationLayers.Crouch, 0);
            _animator.SetLayerWeight(AnimationLayers.CoverStanding, 0);
            _animator.SetLayerWeight(AnimationLayers.CoverCrouch, 0);
            _animator.SetLayerWeight(AnimationLayers.Throw, 0);
            _animator.SetLayerWeight(AnimationLayers.ThrowMove, 0);

            _currentAnimLayer = AnimationLayers.Standing;
        }

        #region Private Methods

        private void SetCurrentAnimLayer()
        {
            switch (_unit.curUnitPose)
            {
                case UnitPose.MOD_WALK:
                case UnitPose.MOD_RUN:
                case UnitPose.MOD_FALL:
                case UnitPose.MOD_ATTACK:
                    _currentAnimLayer = AnimationLayers.Standing;
                    break;
                case UnitPose.MOD_CROUCH:
                    _currentAnimLayer = AnimationLayers.Crouch;
                    break;
                case UnitPose.MOD_COVERCROUCH:
                    _currentAnimLayer = AnimationLayers.CoverCrouch;
                    break;
                case UnitPose.MOD_COVERSTAND:
                    _currentAnimLayer = AnimationLayers.CoverStanding;
                    break;
                case UnitPose.MOD_INSMOKE:
                    _currentAnimLayer = AnimationLayers.InSmoke;
                    break;
                case UnitPose.MOD_THROW:
                case UnitPose.MOD_THROWEND:
                    _currentAnimLayer = AnimationLayers.Throw;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Public Methods

        public void WalkPoseTONewPose(UnitPose newPose)
        {
            switch (newPose)
            {
                case UnitPose.MOD_ATTACK:
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

            SetCurrentAnimLayer();
        }

        public void RunPoseTONewPose(UnitPose newPose)
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

            SetCurrentAnimLayer();
        }

        public void CrouchPoseTONewPose(UnitPose newPose)
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
                    {
                        _animator.SetBool("IsCrouchMode", false);
                        _animator.SetBool("IsRunMode", true);
                    }
                    break;
                case UnitPose.MOD_WALK:
                    //_unit.curUnitPose 값은 SmoothStanding()에서 변경
                    if (_animator.GetLayerWeight(AnimationLayers.Crouch) == 1)
                    {
                        _animator.SetBool("IsCrouchMode", false);
                        _animator.SetBool("IsRunMode", false);
                    }
                    break;
                default:
                    break;
            }

            SetCurrentAnimLayer();
        }

        public void CoverStandingPoseTONewPose(UnitPose newPose)
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
                    //_unit.curUnitPose 값은 SmoothCrouching()에서 변경
                    if (_animator.GetLayerWeight(AnimationLayers.CoverCrouch) == 0)
                        _animator.SetBool("IsCrouchMode", true);
                    break;
                default:
                    break;
            }

            SetCurrentAnimLayer();
        }

        public void CoverCrouchPoseTONewPose(UnitPose newPose)
        {
            switch (newPose)
            {
                case UnitPose.MOD_CROUCH:
                    _animator.SetBool("IsCovering", false);
                    //벽에 붙이기
                    _unit.StartCoroutine(_unit.SetCoverPosition(false));
                    _animator.SetLayerWeight(1, 1);
                    _animator.SetLayerWeight(2, 0);
                    _animator.SetLayerWeight(3, 0);

                    _unit.curUnitPose = newPose;
                    break;
                case UnitPose.MOD_COVERSTAND:
                    //_unit.curUnitPose 값은 SmoothStanding()에서 변경
                    if (_animator.GetLayerWeight(AnimationLayers.CoverCrouch) == 1)
                        _animator.SetBool("IsCrouchMode", false);
                    break;
                default:
                    break;
            }

            SetCurrentAnimLayer();
        }

        /// <summary>
        /// Enemy가 연막탄에 당했을 때, Enemy 전용
        /// </summary>
        public void AnyPoseTOInSmokePose()
        {

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
                    player.SetBYCurUnitPose();
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
                collider.height = standColliderHeight;
                collider.center = new Vector3(0, standColliderHeight / 2, collider.center.z);

                layerWeight -= (Time.deltaTime * 3f);
                if (layerWeight <= 0)
                {
                    layerWeight = 0;
                    if (_animator.GetBool("IsCovering"))
                    {
                        _unit.curUnitPose = UnitPose.MOD_COVERSTAND;
                        _animator.SetBool("IsRunMode", false);
                    }
                    else
                    {
                        _unit.curUnitPose = UnitPose.MOD_RUN;
                        _animator.SetBool("IsRunMode", true);
                    }
                    player.SetBYCurUnitPose();
                }

                _animator.SetLayerWeight(AnimationLayers.Crouch, layerWeight);
                if (_animator.GetBool("IsCovering"))
                {
                    _animator.SetLayerWeight(AnimationLayers.CoverStanding, 1 - layerWeight);
                    _animator.SetLayerWeight(AnimationLayers.CoverCrouch, layerWeight);
                }
            }
        }

        //자연스러운 Crouch -> Standing, Enemy전용
        public void SmoothStanding(CapsuleCollider collider, float standColliderHeight)
        {
            float layerWeight = _animator.GetLayerWeight(AnimationLayers.Crouch);

            if (layerWeight > 0 && !_animator.GetBool("IsCrouchMode"))
            {
                collider.height = standColliderHeight;
                collider.center = new Vector3(0, standColliderHeight / 2, collider.center.z);

                layerWeight -= (Time.deltaTime * 3f);
                if (layerWeight <= 0)
                {
                    layerWeight = 0;
                    if (_animator.GetBool("IsCovering"))
                    {
                        _unit.curUnitPose = UnitPose.MOD_COVERSTAND;
                        _animator.SetBool("IsRunMode", false);
                    }
                    else
                    {
                        if(_animator.GetBool("IsRunMode"))
                            _unit.curUnitPose = UnitPose.MOD_RUN;
                        else
                            _unit.curUnitPose = UnitPose.MOD_WALK;
                    }
                }

                _animator.SetLayerWeight(AnimationLayers.Crouch, layerWeight);
                if (_animator.GetBool("IsCovering"))
                {
                    _animator.SetLayerWeight(AnimationLayers.CoverStanding, 1 - layerWeight);
                    _animator.SetLayerWeight(AnimationLayers.CoverCrouch, layerWeight);
                }
            }
        }

        //Throw, ThrowMove 애니메이션 레이어 제어
        public void ControlThrowLayer()
        {
            if (_animator.GetFloat("TurnRight") == 0 && _animator.GetFloat("MoveSpeed") == 0)
            {
                _animator.SetLayerWeight(AnimationLayers.Throw, 1);
                _animator.SetLayerWeight(AnimationLayers.ThrowMove, 0);
            }
            else
            {
                _animator.SetLayerWeight(AnimationLayers.Throw, 0);
                _animator.SetLayerWeight(AnimationLayers.ThrowMove, 1);
            }

            SetCurrentAnimLayer();
        }

        /// <summary>
        /// Standing Layer 빼고 모두 비활성화, MOD_RUN으로 변경
        /// </summary>
        public void TurnOffAllLayers()
        {
            _animator.SetLayerWeight(AnimationLayers.Crouch, 0);
            _animator.SetLayerWeight(AnimationLayers.CoverStanding, 0);
            _animator.SetLayerWeight(AnimationLayers.CoverCrouch, 0);
            _animator.SetLayerWeight(AnimationLayers.Throw, 0);
            _animator.SetLayerWeight(AnimationLayers.ThrowMove, 0);
            _animator.SetLayerWeight(AnimationLayers.InSmoke, 0);

            _animator.SetBool("IsRunMode", true);
            _animator.SetBool("IsCovering", false);
            _animator.SetBool("IsCrouchMode", false);
            _animator.SetBool("IsThrowMode", false);
            _animator.SetBool("ReadyAssassinateAnim", false);
            _animator.SetBool("ThrowItem", false);

            _unit.curUnitPose = UnitPose.MOD_RUN;
            SetCurrentAnimLayer();
        }

        public void PlayDeadAnim(int damage)
        {
            TurnOffAllLayers();
            //damage < 0이면 암살로 인한 사망, damage > 0이면 일반공격으로 사망
            _animator.SetInteger("DeadAnim", damage);
        }

        #endregion
    }
}
