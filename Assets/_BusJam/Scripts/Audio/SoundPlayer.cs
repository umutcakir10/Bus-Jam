using UnityEngine;

namespace BusJam.Audio
{
    public enum Sfx { Tap, Blocked, Seat, BusLeave, Click, Win, Lose }

    [RequireComponent(typeof(AudioSource))]
    public sealed class SoundPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip tap;
        [SerializeField] private AudioClip blocked;
        [SerializeField] private AudioClip seat;
        [SerializeField] private AudioClip busLeave;
        [SerializeField] private AudioClip click;
        [SerializeField] private AudioClip win;
        [SerializeField] private AudioClip lose;
        [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

        // Sesi her yerden çalabilmek için oyundaki tek oynatıcıyı burada tutuyoruz
        private static SoundPlayer _instance;
        private AudioSource _source;

        private void Awake()
        {
            // Game sahnesi her yeniden yüklendiğinde yeni bir kopya geliyor ilki yaşıyorsa yenisini siliyoruz
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _source = GetComponent<AudioSource>();

            // Sahne değişince yok olmasın yoksa Next ya da Retrya basınca tık sesi yarıda kesilir
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // Oynatıcı yoksa sessizce hiçbir şey yapmıyor hiçbir sınıf sese bağımlı değil
        public static void Play(Sfx sfx)
        {
            if (_instance != null) _instance.PlayClip(sfx);
        }

        private void PlayClip(Sfx sfx)
        {
            AudioClip clip = sfx switch
            {
                Sfx.Tap => tap,
                Sfx.Blocked => blocked,
                Sfx.Seat => seat,
                Sfx.BusLeave => busLeave,
                Sfx.Click => click,
                Sfx.Win => win,
                _ => lose,
            };

            // PlayOneShot sesleri üst üste çalabiliyor ve bellek ayırmıyor
            if (clip != null) _source.PlayOneShot(clip, volume);
        }
    }
}