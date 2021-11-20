using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PerpetualJourney
{
    public class LaneController : MonoBehaviour
    {
        [SerializeField]private float _maxVelocity = 15;
        [SerializeField]private float _acceleration = 0.2f;
        [SerializeField]private float _jumpVelocity = 7;
        [SerializeField]private float _laneInputDelay = 0.25f;
        [SerializeField]private float _laneChangeAngle = 40;

        private InputReader _inputReader;
        private GameEvents _gameEvents;
        
        private Rigidbody _rigidbody;
        private float _laneSize;
        
        private bool _hasInputActive = false;
        private bool _hasGroundContact = false;
        private bool _isChangingLane = false;
        private int _currentLane = 0;

        public void Initialize(InputReader inputReader, GameEvents gameEvents)
        {
            _inputReader = inputReader;
            _gameEvents = gameEvents;

            _rigidbody = GetComponent<Rigidbody>();
            _laneSize = GameSystem.instance.LaneSize;

            Initialize();
        }

        public void Initialize()
        {
            _inputReader.OnJumpEvent += OnJump;
            _inputReader.OnMoveEvent += OnMove;
            _inputReader.OnSwipeEvent += OnSwipeMove;
            _gameEvents.OnPlayerPositionRequest += GetCurrentPosition;
            // InvokeRepeating("PlaySoundStep", 0.1f, 0.3f);
        }

        public void SceneReset()
        {
            gameObject.SetActive(true);
            _rigidbody.velocity = Vector3.zero;
            _hasInputActive = false;
            _hasGroundContact = false;
            _isChangingLane = false;
            _currentLane = 0;

            Initialize();
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnJumpEvent -= OnJump;
                _inputReader.OnMoveEvent -= OnMove;
                _inputReader.OnSwipeEvent -= OnSwipeMove;
            }

            _gameEvents.OnPlayerPositionRequest -= GetCurrentPosition;
            LeanTween.cancel(gameObject);
            // CancelInvoke();
        }

        private void OnJump()
        {
            if (_hasInputActive && !_isChangingLane && _hasGroundContact)
            {
                _hasGroundContact = false;
                _rigidbody.AddForce(Vector3.up * _jumpVelocity, ForceMode.VelocityChange);
                
                PlayJumpSound();
            }
        }

        private void OnMove(int laneValue)
        {
            if(_hasInputActive && !_isChangingLane && _hasGroundContact)
            {
                int targetLane = _currentLane + laneValue;
                if (targetLane <= 1 && targetLane >= -1)
                {
                    _currentLane = targetLane;
                    _hasGroundContact = false;
                    _isChangingLane = true;
                    JumpToLanePosition(_laneChangeAngle);
                    
                    PlayJumpSound();
                }
            }
        }

        private void OnSwipeMove(Vector2 swipeDir)
        {
            if (swipeDir.y > 0)
            {
                OnJump();
                return;
            }

            OnMove((int)swipeDir.x);
        }

        private Vector3 GetCurrentPosition()
        {
            return transform.position;
        }

        private void FixedUpdate() 
        {
            if (_hasGroundContact || _isChangingLane)
            {
                Vector3 velocity = _rigidbody.velocity;

                if (Mathf.Abs(velocity.x) < _maxVelocity)
                {
                    _rigidbody.AddForce(Vector3.left * _acceleration, ForceMode.VelocityChange);
                }
            }
        }

        private void JumpToLanePosition(float jumpAngle)
        {
            Vector3 relativePosition = Vector3.zero;
            relativePosition.z = _currentLane * _laneSize - transform.position.z;
            
            JumpToRelativePosition(relativePosition, jumpAngle);
        }

        private void JumpToRelativePosition(Vector3 relativePosition, float jumpAngle)
        {
            float gravity = Physics.gravity.magnitude;
            float angle = jumpAngle * Mathf.Deg2Rad;
            float yOffset = relativePosition.y;

            Vector3 origin = new Vector3();
            float distance = Vector3.Distance(origin, relativePosition);

            float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));
            Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));
            float angleBetweenObjects = Vector3.Angle(Vector3.forward, relativePosition) * Mathf.Sign(relativePosition.x);
            Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

            _rigidbody.AddForce(finalVelocity * _rigidbody.mass, ForceMode.Impulse);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!_hasGroundContact)
            {
                _hasGroundContact = true;
                PlayLandSound();

                if(_isChangingLane && !gameObject.LeanIsTweening())
                {
                    LeanTween.delayedCall(gameObject, _laneInputDelay, () => 
                    {
                        _isChangingLane = false;
                    });
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if(!_hasInputActive && other.GetComponent<LevelPart>() != null)
            {
                _hasInputActive = true;
            }
        }
        
        private void PlayJumpSound()
        {
            SoundPlayer.instance.PlayAudio(SoundPlayer.AudioEnum.Jump);
        }

        // Is implemented but doesn't sounds good
        private void PlaySoundStep()
        {
            if(_hasGroundContact)
            {
                // SoundPlayer.instance.PlayRandomStep();
            }
        }

        // Is implemented but doesn't sounds good
        private void PlayLandSound()
        {
            // SoundPlayer.instance.PlayAudio(SoundPlayer.AudioEnum.land);
        }
    }
}
