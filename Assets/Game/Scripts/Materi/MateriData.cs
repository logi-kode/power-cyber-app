using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMateri", menuName = "Materi/Materi Data")]
public class MateriData : ScriptableObject
{
    [Header("Info Materi")]
    public string judulMateri;

    [TextArea(5, 20)]
    public string isiKonten;

    public Sprite ilustrasi;
}
