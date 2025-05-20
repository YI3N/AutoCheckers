using AutoCheckers;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class LockButton : MonoBehaviour
{
    [SerializeField]
    private Button lockButton;
    [SerializeField]
    private RawImage image;
    [SerializeField]
    private Texture lockedImage;
    [SerializeField]
    private Texture unlockedImage;



    void Start()
    {
        lockButton.onClick.AddListener(() => Shop.instance.LockShop(GameTag.Human));
    }

    public void ChangeButton()
    {
        lockButton.image.color = Shop.instance.HumanLocked ? new Color(51f / 255f, 51f / 255f, 51f / 255f) : new Color(128f / 255f, 128f / 255f, 128f / 255f);
        image.texture = Shop.instance.HumanLocked ? lockedImage : unlockedImage;
    }
}
