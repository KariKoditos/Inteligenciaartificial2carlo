using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlUI : MonoBehaviour
{
    public TMP_Text generationText;    
    public TMP_Text bestFitnessText;   
    public TMP_Text aliveText;         
    public TMP_Text timerText;         
    public TMP_Text timeScaleText;     

    public void SetGeneration(int gen)
    {
        if (generationText != null)
            generationText.text = "Generación: " + gen.ToString();
    }

    public void SetBestFitness(float value)
    {
        if (bestFitnessText != null)
            bestFitnessText.text = "Mejor Fitness: " + value.ToString("F2");
    }

    public void SetAliveCount(int alive)
    {
        if (aliveText != null)
            aliveText.text = "Vivos: " + alive.ToString();
    }

    public void SetTimer(float remaining)
    {
        if (timerText != null)
            timerText.text = "Tiempo: " + Mathf.CeilToInt(remaining).ToString();
    }

    public void SetTimeScale(float ts)
    {
        if (timeScaleText != null)
            timeScaleText.text = "Velocidad: " + ts.ToString("F1") + "x";
    }
}
