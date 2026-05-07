using UnityEngine;


[CreateAssetMenu(fileName = "NewQuestion", menuName = "Quiz/Quiz Question")]
public class QuizQuestion : ScriptableObject
{
    public string pertanyaan;

    public string jawabanA;
    public string jawabanB;
    public string jawabanC;
    public string jawabanD;

    public string jawabanBenar = "A";
}
