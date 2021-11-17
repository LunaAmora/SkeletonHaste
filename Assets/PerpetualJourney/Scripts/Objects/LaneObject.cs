using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PerpetualJourney
{
    public abstract class LaneObject : PoolableObject
    {
        [SerializeField]private GameEvents _gameEvents;
        
        private float _laneSize;
        private int _lane;

        protected GameEvents GameEvent => _gameEvents;

        public virtual void Initialize(int lane)
        {
            _lane = lane;
            _laneSize = GameSystem.Current.LaneSize;
            SetLanePosition();
        }

        protected abstract void CollidedWithPlayer();

        private void SetLanePosition()
        {
            Vector3 lanePos = transform.position;
            lanePos.z = _lane * _laneSize;
            transform.position = lanePos;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<Player>() != null)
            {
                CollidedWithPlayer();
            }
        }
    }
}
