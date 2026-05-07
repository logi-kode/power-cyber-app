using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class QuizManager : MonoBehaviour
{
    [SerializeField] private List<QuizQuestion> daftarPertanyaan = new List<QuizQuestion>();

    [SerializeField] private int kesempatanAwal = 3;
    [SerializeField] private float waktuPerSoal = 300f; 
    [SerializeField] private int skorPerJawaban = 10;
    [SerializeField] private bool acakPertanyaan = true;

    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject hasilPanel;
    [SerializeField] private TextMeshProUGUI skorText;
    [SerializeField] private TextMeshProUGUI kesempatanText;
    [SerializeField] private TextMeshProUGUI waktuText;
    [SerializeField] private TextMeshProUGUI pertanyaanText;
    [SerializeField] private TextMeshProUGUI nomorSoalText;
    [SerializeField] private TextMeshProUGUI teksA;
    [SerializeField] private TextMeshProUGUI teksB;
    [SerializeField] private TextMeshProUGUI teksC;
    [SerializeField] private TextMeshProUGUI teksD;
    [SerializeField] private TextMeshProUGUI judulHasilText;
    [SerializeField] private TextMeshProUGUI skorAkhirText;
    [SerializeField] private TextMeshProUGUI totalBenarText;

    [SerializeField] private Button tombolA;
    [SerializeField] private Button tombolB;
    [SerializeField] private Button tombolC;
    [SerializeField] private Button tombolD;
    [SerializeField] private Button tombolUlangi;
    [SerializeField] private Button tombolMenuUtama;

    [SerializeField] private string menuUtamaScene = "MainMenu";
    [SerializeField] private string quizScene = "QuizScene";

    [SerializeField] private Color warnaBenar = new Color(0.2f, 0.8f, 0.3f);
    [SerializeField] private Color warnaSalah = new Color(0.9f, 0.2f, 0.2f);
    [SerializeField] private Color warnaNormal = Color.white;
    [SerializeField] private float feedbackDuration = 0.8f;

    private List<QuizQuestion> _pertanyaanAktif = new List<QuizQuestion>();
    private int _indexSoal = 0;
    private int _skorSaatIni = 0;
    private int _kesempatan;
    private float _waktuSisaDet;
    private bool _quizBerjalan = false;
    private bool _sedangFeedback = false;
    private int _totalBenar = 0;

    private void Start()
    {
        if (hasilPanel) hasilPanel.SetActive(false);
        if (quizPanel) quizPanel.SetActive(true);

        if (tombolA) tombolA.onClick.AddListener(() => PilihJawaban("A"));
        if (tombolB) tombolB.onClick.AddListener(() => PilihJawaban("B"));
        if (tombolC) tombolC.onClick.AddListener(() => PilihJawaban("C"));
        if (tombolD) tombolD.onClick.AddListener(() => PilihJawaban("D"));

        if (tombolUlangi) tombolUlangi.onClick.AddListener(UlangiQuiz);
        if (tombolMenuUtama) tombolMenuUtama.onClick.AddListener(KeMenuUtama);

        MulaiQuiz();
    }

    private void Update()
    {
        if (!_quizBerjalan) return;

        _waktuSisaDet -= Time.deltaTime;
        if (_waktuSisaDet <= 0f)
        {
            _waktuSisaDet = 0f;
            UpdateWaktuUI();
            SelesaiQuiz("Waktu Habis!");
            return;
        }

        UpdateWaktuUI();
    }

    private void MulaiQuiz()
    {
        // Reset state
        _skorSaatIni = 0;
        _kesempatan = kesempatanAwal;
        _waktuSisaDet = waktuPerSoal;
        _indexSoal = 0;
        _totalBenar = 0;
        _quizBerjalan = true;

        // Siapkan daftar pertanyaan
        _pertanyaanAktif = new List<QuizQuestion>(daftarPertanyaan);
        if (acakPertanyaan) AcakPertanyaan();

        UpdateSkorUI();
        UpdateKesempatanUI();
        TampilkanSoal();
    }

    private void AcakPertanyaan()
    {
        for (int i = _pertanyaanAktif.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = _pertanyaanAktif[i];
            _pertanyaanAktif[i] = _pertanyaanAktif[j];
            _pertanyaanAktif[j] = temp;
        }
    }

    private void TampilkanSoal()
    {
        if (_indexSoal >= _pertanyaanAktif.Count)
        {
            SelesaiQuiz("Quiz Selesai!");
            return;
        }

        var soal = _pertanyaanAktif[_indexSoal];

        // Update nomor soal
        if (nomorSoalText)
            nomorSoalText.text = $"Pertanyaan {_indexSoal + 1} / {_pertanyaanAktif.Count}";

        // Update teks pertanyaan
        if (pertanyaanText) pertanyaanText.text = soal.pertanyaan;

        // Update teks pilihan jawaban
        if (teksA) teksA.text = soal.jawabanA;
        if (teksB) teksB.text = soal.jawabanB;
        if (teksC) teksC.text = soal.jawabanC;
        if (teksD) teksD.text = soal.jawabanD;

        // Reset warna tombol
        ResetWarnaTombol();
        SetTombolInteractable(true);
    }

    public void PilihJawaban(string jawaban)
    {
        if (!_quizBerjalan || _sedangFeedback) return;

        var soal = _pertanyaanAktif[_indexSoal];
        bool benar = jawaban.ToUpper() == soal.jawabanBenar.ToUpper();

        // Tampilkan feedback warna
        StartCoroutine(FeedbackJawaban(jawaban, soal.jawabanBenar, benar));

        if (benar)
        {
            _skorSaatIni += skorPerJawaban;
            _totalBenar++;
            UpdateSkorUI();
        }
        else
        {
            _kesempatan--;
            UpdateKesempatanUI();

            if (_kesempatan <= 0)
            {
                StartCoroutine(SelesaiSetelahFeedback("Kesempatan Habis!"));
                return;
            }
        }
    }

    private IEnumerator FeedbackJawaban(string dipilih, string benar, bool isBenar)
    {
        _sedangFeedback = true;
        SetTombolInteractable(false);

        var tombolDipilih = GetTombol(dipilih);
        var tombolBenar = GetTombol(benar);

        if (tombolDipilih != null)
            tombolDipilih.GetComponent<Image>().color = isBenar ? warnaBenar : warnaSalah;

        if (!isBenar && tombolBenar != null)
            tombolBenar.GetComponent<Image>().color = warnaBenar;

        yield return new WaitForSeconds(feedbackDuration);

        ResetWarnaTombol();
        _sedangFeedback = false;

        _indexSoal++;
        TampilkanSoal();
    }

    private IEnumerator SelesaiSetelahFeedback(string pesan)
    {
        yield return new WaitForSeconds(feedbackDuration);
        SelesaiQuiz(pesan);
    }

    private void SelesaiQuiz(string pesan)
    {
        _quizBerjalan = false;

        if (quizPanel) quizPanel.SetActive(false);
        if (hasilPanel) hasilPanel.SetActive(true);

        if (judulHasilText) judulHasilText.text = pesan;
        if (skorAkhirText) skorAkhirText.text = $"Skor: {_skorSaatIni}";
        if (totalBenarText) totalBenarText.text = $"Benar: {_totalBenar} / {_pertanyaanAktif.Count}";

        // Simpan skor ke PlayerPrefs
        PlayerPrefs.SetInt("QuizScore", _skorSaatIni);
        PlayerPrefs.SetInt("QuizBenar", _totalBenar);
        PlayerPrefs.Save();

        Debug.Log($"[QuizManager] Quiz selesai. Skor: {_skorSaatIni}, Benar: {_totalBenar}/{_pertanyaanAktif.Count}");
    }

    private void UlangiQuiz()
    {
        SceneManager.LoadScene(quizScene);
    }

    private void KeMenuUtama()
    {
        SceneManager.LoadScene(menuUtamaScene);
    }


    private void UpdateSkorUI()
    {
        if (skorText) skorText.text = $"Skor: {_skorSaatIni}";
    }

    private void UpdateKesempatanUI()
    {
        if (kesempatanText) kesempatanText.text = $"Kesempatan: {_kesempatan}";
    }

    private void UpdateWaktuUI()
    {
        int menit = Mathf.FloorToInt(_waktuSisaDet / 60f);
        int detik = Mathf.FloorToInt(_waktuSisaDet % 60f);
        if (waktuText) waktuText.text = $"Waktu: {menit:00}:{detik:00}";
    }

    private void ResetWarnaTombol()
    {
        if (tombolA) tombolA.GetComponent<Image>().color = warnaNormal;
        if (tombolB) tombolB.GetComponent<Image>().color = warnaNormal;
        if (tombolC) tombolC.GetComponent<Image>().color = warnaNormal;
        if (tombolD) tombolD.GetComponent<Image>().color = warnaNormal;
    }

    private void SetTombolInteractable(bool aktif)
    {
        if (tombolA) tombolA.interactable = aktif;
        if (tombolB) tombolB.interactable = aktif;
        if (tombolC) tombolC.interactable = aktif;
        if (tombolD) tombolD.interactable = aktif;
    }

    private Button GetTombol(string jawaban) => jawaban.ToUpper() switch
    {
        "A" => tombolA,
        "B" => tombolB,
        "C" => tombolC,
        "D" => tombolD,
        _ => null
    };
}
