using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Exercice 4 : relie bouton, slider et InputField à un texte de feedback.
/// À placer sur le Canvas.
public class UIDemo : MonoBehaviour
{
    public Button button;
    public Slider slider;
    public TMP_InputField inputField;
    public TMP_Text feedback;

    int clicks;

    void Start()
    {
        button.onClick.AddListener(() => Show($"Bouton cliqué {++clicks} fois"));
        slider.onValueChanged.AddListener(v => Show($"Slider : {v:0.00}"));
        inputField.onEndEdit.AddListener(t => Show($"Texte saisi : {t}"));
    }

    void Show(string msg)
    {
        feedback.text = msg;
        Debug.Log(msg);
    }
}
