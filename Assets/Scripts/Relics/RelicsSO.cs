using UnityEngine;

public enum ExtractionType
{
    Dig = 0,
    Chop = 1,
    RemoveFoliage = 2,
    Stumble = 3
}

[CreateAssetMenu(fileName = "NewRelic", menuName = "SO/Relic")]
public class RelicsSO : ScriptableObject
{
    public GameObject pref;
    public AudioClip onPickupSound;
    public float spawnChance = 1;
    public bool isUnick;
    public ExtractionType type;
}
