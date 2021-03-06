using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace PerpetualJourney
{
    public class MenuSystem : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Slider _loadingProgress;
        [SerializeField] private GameObject _menuUi;
        [SerializeField] private GameObject _loadingScreen;

        private TextMeshProUGUI _loadingText;

        private void Awake()
        {
            InitializeSystem();
        }

        private void OnDisable()
        {
            RemoveEventSubscriptions();
        }

        private void InitializeSystem()
        {
            _loadingText = _loadingProgress.GetComponentInChildren<TextMeshProUGUI>();
            
            PersistentLoaderSystem.Instance.OnProgressUpdated += OnLoading;
            _playButton.onClick.AddListener(PlayGame);
            _exitButton.onClick.AddListener(ExitGame);

            SoundPlayer.Instance.SetMusicMenu();
        }

        private void RemoveEventSubscriptions()
        {
            PersistentLoaderSystem.Instance.OnProgressUpdated -= OnLoading;
        }

        private void PlayGame()
        {
            PlayClickSound();
            
            _menuUi.SetActive(false);
            _loadingScreen.SetActive(true);

            PersistentLoaderSystem.Instance.LoadGame();
        }

        private void ExitGame()
        {
            PlayClickSound();
            Application.Quit();
        }

        private void OnLoading(float progress)
        {
            _loadingProgress.value = progress;
            _loadingText.SetText("Loading...{0}%", (int)(progress * 100f));
        }
        
        private void PlayClickSound()
        {
            SoundPlayer.Instance.PlayAudio(SoundPlayer.AudioEnum.Click);
        }
    }
}
