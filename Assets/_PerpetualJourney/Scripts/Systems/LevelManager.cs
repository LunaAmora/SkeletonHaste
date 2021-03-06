using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PerpetualJourney
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private Transform _levelGenPosition;
        [SerializeField] private List<LevelPart> _levelList;
        [SerializeField] private GameEvents _gameEvents;
        [SerializeField] private float _generationDistance = 150f;
        [SerializeField] private float _checkFrequency = 5f;

        private List<LevelPart> _instantiatedLevels = new List<LevelPart>();
        private Vector3 _playerPosition = Vector3.zero;
        private Vector3 _lastLevelPosition;
        
        private float _generationprogress;
        private bool _generationIsDone;

        public void Initialize()
        {
            _lastLevelPosition = _levelGenPosition.position;
            StartCoroutine(PlayerPositionCheckerAsync());
        }

        public void SceneReset()
        {
            _instantiatedLevels.ForEach((level) => level.SceneReset());
            
            StopAllCoroutines();
            Initialize();
        }

        private void InstantiateLevelPart()
        {
            LevelPart randomLevel = _levelList[Random.Range(0, _levelList.Count)];
            LevelPart instantiatedLevel = InstantiateLevelPart(randomLevel, _lastLevelPosition);

            _lastLevelPosition = instantiatedLevel.LevelEndPosition;
        }

        private LevelPart InstantiateLevelPart(LevelPart levelPart, Vector3 instancePosition)
        {
            LevelPart part = levelPart.RequestFromObjectPool();
            part.transform.SetParent(transform);
            part.transform.position = instancePosition;
            part.Initialize();

            if(!_instantiatedLevels.Contains(part))
            {
                _instantiatedLevels.Add(part);
            }
            
            return part;
        }
        
        private IEnumerator PlayerPositionCheckerAsync()
        {
            while(true)
            {
                _gameEvents.RequestPlayerPosition(ref _playerPosition);
                StartCoroutine(GenerateLevelAsync(_generationDistance));
                yield return new WaitForSeconds(_checkFrequency);
            }
        }

        private IEnumerator GenerateLevelAsync(float genDistance)
        {
            while (Vector3.Distance(_playerPosition, _lastLevelPosition) < genDistance)
            {
                InstantiateLevelPart();
                yield return null;

                if(!_generationIsDone)
                {
                    float distance = Vector3.Distance(_playerPosition, _lastLevelPosition);
                    float clampedDistance = Mathf.Clamp(distance, 0, genDistance);
                    float percent = clampedDistance / genDistance;
                    UpdateSceneLoadProgress(percent);
                }
            }

            if (!_generationIsDone)
            {
                UpdateSceneLoadProgress(1);
                yield return new WaitForSeconds(0.5f);
                _generationIsDone = true;
                PersistentLoaderSystem.Instance.LevelGenerationIsDone = true;
            }
        }

        private void UpdateSceneLoadProgress(float value)
        {
            _generationprogress = value;
            PersistentLoaderSystem.Instance.LevelGenerationProgress = _generationprogress;
        }
    }
}
