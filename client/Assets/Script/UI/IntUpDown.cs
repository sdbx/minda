using UnityEngine;
using UnityEngine.UI;

public class IntUpDown : MonoBehaviour 
{
    [SerializeField]
    private Button up;
    [SerializeField]
    private Button down;
    [SerializeField]
    private InputField inputField;

    public int number;

    private void Awake() 
    {
        up.onClick.AddListener(Up);
        down.onClick.AddListener(Down);
        inputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void Up()
    {
        number++;
        RefreshInputField();
    }

    private void Down()
    {
        number--;
        RefreshInputField();
    }

    private void RefreshInputField()
    {
        inputField.text = number.ToString();
    }

    private void OnEndEdit(string str)
    {
        if(int.TryParse(str, out int num))
        {
            number = num;
        }
    }

}