using UnityEngine;

public class PlayUISound : MonoBehaviour
{
    public static PlayUISound Instance { get; protected set; }
    public void Negative()
    {
        //AudioManager.Instance.PlaySound(AudioManager.ESound.Negative_UI);
    }
}
