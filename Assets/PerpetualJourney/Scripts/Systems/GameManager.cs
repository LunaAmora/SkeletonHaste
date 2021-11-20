using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using TMPro;

namespace PerpetualJourney
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]private Player _player;
        [SerializeField]private GameEvents _gameEvents;
        [SerializeField]private GameObject _gameOverUi;
        [SerializeField]private TextMeshProUGUI _scoreText;
        [SerializeField]private TextMeshProUGUI _gameOverScore;
        [SerializeField]private GameObject _highscoreIndicator;
        
        private ScoreObject _scoreObject = new ScoreObject();

        private const string DefaultNewScore = "New!";

        public void Initialize()
        {
            PersistentLoaderSystem.instance.GameIsLoaded += ActivatePlayer;
            PersistentLoaderSystem.instance.GameIsLoaded += PlayGameMusic;
            _gameEvents.OnObstacleCollided += OnGameOver;
            _gameEvents.OnObstacleCollided += StopGameMusic;
            _gameEvents.OnScoreChanged += UpdateTextScore;
            _player.Initialize();
        }

        public void SceneReset()
        {
            _player.SceneReset();
            _scoreText.gameObject.SetActive(true);
            _gameOverUi.gameObject.SetActive(false);
            _highscoreIndicator.SetActive(false);
            PlayGameMusic();
        }

        private void OnDisable()
        {
            PersistentLoaderSystem.instance.GameIsLoaded -= ActivatePlayer;
            PersistentLoaderSystem.instance.GameIsLoaded -= PlayGameMusic;
            _gameEvents.OnObstacleCollided -= OnGameOver;
            _gameEvents.OnObstacleCollided -= StopGameMusic;
            _gameEvents.OnScoreChanged -= UpdateTextScore;
        }

        private void ActivatePlayer()
        {
            _player.gameObject.SetActive(true);
        }

        private void PlayGameMusic()
        {
            SoundPlayer.instance.SetMusicGame();
        }

        private void StopGameMusic()
        {
            SoundPlayer.instance.StopMusic();
        }

        private void UpdateTextScore(int score)
        {
            _scoreObject.value = score;
            _scoreText.SetText("Score: {0}", score);
        }

        private void OnGameOver()
        {
            _scoreText.gameObject.SetActive(false);
            _gameOverUi.gameObject.SetActive(true);

            DisplayScores();
        }

        private void DisplayScores()
        {
            ScoreObject highscore = GetHighScore();
            string firstLine = _scoreObject.value.ToString();
            string lastLine = DefaultNewScore;
            
            if (highscore.value != 0)
            {
                lastLine = highscore.value.ToString();
            }

            if(_scoreObject.value > highscore.value)
            {
                SaveHighScore();
                _highscoreIndicator.SetActive(true);
                firstLine = "<color=green>" + firstLine + "</color>";
            }
            else if (highscore.value.Equals(0))
            {
                lastLine = "----";
            }
            
            _gameOverScore.SetText(firstLine + "\n" + lastLine);
        }

        private ScoreObject GetHighScore()
        {
            string scorePath = GetSystemSavePath();

            if(File.Exists(scorePath))
            {
                string json = File.ReadAllText(scorePath);
                ScoreObject scoreObject = JsonUtility.FromJson<ScoreObject>(json);
                return scoreObject;
            }

            return new ScoreObject();
        }

        private void SaveHighScore()
        {
            string json = JsonUtility.ToJson(_scoreObject);
            File.WriteAllText(GetSystemSavePath(), json);
        }

        private string GetSystemSavePath()
        {
            string folderPath;

            if(Application.platform == RuntimePlatform.Android)
            {
                folderPath = Application.persistentDataPath;
            }
            else
            {
                folderPath = Application.dataPath;
            }

            return folderPath + "/highscore.txt";
        }
    }
    
    class ScoreObject
    {
        public int value = 0;
    }
}

