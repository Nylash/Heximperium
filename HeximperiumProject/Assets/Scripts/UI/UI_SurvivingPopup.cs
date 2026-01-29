using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SurvivingPopup : MonoBehaviour
{
    private float _survivingDuration;
    private float _survivingTime;
    private bool _isSurviving;
    private Transform _timerRoot;
    private List<Image> _timerImages = new List<Image>();

    public float SurvivingDuration { get => _survivingDuration; set => _survivingDuration = value; }
    public bool IsSurviving
    {
        get => _isSurviving;
        set
        {
            _isSurviving = value;
            _survivingTime = 0f;
            foreach (Image item in _timerImages)
            {
                item.fillAmount = 1f;
            }
        }
    }
    public Transform TimerRoot { get => _timerRoot; set => _timerRoot = value; }

    private void Start()
    {
        foreach (Transform child in _timerRoot)
        {
            Image img = child.GetComponent<Image>();
            if (img != null && img.type == Image.Type.Filled)
            {
                _timerImages.Add(img);
            }
        }
    }

    private void Update()
    {
        if (_isSurviving)
        {
            _survivingTime += Time.deltaTime;
            foreach (Image item in _timerImages)
            {
                item.fillAmount = 1 - (_survivingTime / _survivingDuration);
            }
            if (_survivingTime > _survivingDuration)
            {
                ClosePopup();
            }
        }
    }

    public void ClosePopup()
    {
        if (this == null) // Safety check
            return;

        _isSurviving = false;
        GetComponent<Animator>().SetTrigger("Close");
        if (gameObject == JuiceManager.Instance.PopUpVisualizingCombo)
        {
            JuiceManager.Instance.KillAllComboVFX();
        }
        this.enabled = false;
    }
}
