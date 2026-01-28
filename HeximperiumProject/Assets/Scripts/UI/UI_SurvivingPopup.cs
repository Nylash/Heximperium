using UnityEngine;

public class UI_SurvivingPopup : MonoBehaviour
{
    public float survivingDuration;
    public bool isSurviving;
    public float survivingTime;

    private void Update()
    {
        if (isSurviving)
        {
            survivingTime += Time.deltaTime;
            if (survivingTime > survivingDuration)
            {
                ClosePopup();
            }
        }
    }

    public void ClosePopup()
    {
        isSurviving = false;
        GetComponent<Animator>().SetTrigger("Close");
        if (gameObject == JuiceManager.Instance.PopUpVisualizingCombo)
        {
            JuiceManager.Instance.KillAllComboVFX();
        }
        this.enabled = false;
    }
}
