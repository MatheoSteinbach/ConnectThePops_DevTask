using DG.Tweening;
using System.Collections;
using UnityEngine;

public class UIAnims : MonoBehaviour
{
    [SerializeField] private RectTransform MenuPanel;
    [SerializeField] private RectTransform TapToPlay;

    private void Start()
    {
        StartCoroutine(Punch());
    }
    
    public void SlideMenuOff()
    {
        MenuPanel.transform.DOLocalMoveX(2160, 0.8f);
    }
    public void SlideMenuBack()
    {
        MenuPanel.transform.DOLocalMoveX(0, 0.8f);
    }

    public void StartPunchAnim()
    {
        StartCoroutine (Punch());
    }
    IEnumerator Punch()
    {
        TapToPlay.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 1.5f);
        yield return new WaitForSeconds(1.6f);
        TapToPlay.transform.DOScale(new Vector3(1, 1, 1), 1.5f);
        yield return new WaitForSeconds(1.6f);
        if(TapToPlay.gameObject.activeSelf)
        {
            StartCoroutine(Punch());
        }
    }
}
