using UnityEngine;

public class BelligerentControl : MonoBehaviour
{
    [SerializeField] private GameObject[] belligerentImages;

    void Start()
    {
        InputManager.Instance.BelligerentPressedInt += ChangeBelligerent;
        ChangeBelligerent((int)CreatureManager.Instance.CurrentAllyBelligerent);
    }

    public void ChangeBelligerent(int belligerentIndex)
    {
        for(int i = 0; i < belligerentImages.Length; i++)
        {
            if(i == belligerentIndex)
                belligerentImages[i].SetActive(true);
            else
                belligerentImages[i].SetActive(false);
        }
    }
}
