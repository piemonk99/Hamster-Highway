using UnityEngine;

public class SublevelDifficulty : MonoBehaviour
{
    public int difficultyValue; // 1 = Easy, 3 = Medium, 7 = Hard
    public bool isCoreLevel; // True if this is the easiest/only version of this sublevel - used for picking from unique sublevels, rather than all.
}
