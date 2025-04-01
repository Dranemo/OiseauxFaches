using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasMain : MonoBehaviour
{
    [SerializeField] private Button RestartButton;
    [SerializeField] GameObject Cancel;
    [SerializeField] GameObject Power;

    string defaultPowerText;



    static private CanvasMain instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    static public CanvasMain GetInstance()
    {
        return instance;
    }


    // Start is called before the first frame update
    void Start()
    {
        defaultPowerText = Power.GetComponentInChildren<TextMeshProUGUI>().text;


        DisplayCancel(false);
        DisplayPower(false);

        RestartButton.onClick.AddListener(Reset);
    }


    public void DisplayCancel(bool _bool)
    {
        if(Cancel.GetComponent<ContinuousMovementUI>() != null)
            Cancel.GetComponent<ContinuousMovementUI>().SetVisualsActive(_bool);
        else 
            Cancel.SetActive(_bool);
    }

    public void DisplayPower(bool _bool, string _powerName = "*")
    {
        if (Power.GetComponent<ContinuousMovementUI>() != null)
            Power.GetComponent<ContinuousMovementUI>().SetVisualsActive(_bool);
        else
            Power.SetActive(_bool);


        if (_bool)
        {
            Power.GetComponentInChildren<TextMeshProUGUI>().text = Power.GetComponentInChildren<TextMeshProUGUI>().text.Replace("*", _powerName);
        }
        else 
            Power.GetComponentInChildren<TextMeshProUGUI>().text = defaultPowerText;
    }



    private void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
