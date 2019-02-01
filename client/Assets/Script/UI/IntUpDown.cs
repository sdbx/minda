using System;
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

    [SerializeField]
    private int min_;
    [SerializeField]
    private int max_;

    public int min{get{return min_;}}
    public int max{get{return max_;}}
    
    [SerializeField]
    private int value_;
    public int value{get{return value_;}}

    public event Action<int> ValueChanged;
    
    public bool isButtonLocked = false;

    private void Awake() 
    {
        up.onClick.AddListener(Up);
        down.onClick.AddListener(Down);
        inputField.onEndEdit.AddListener(OnEndEdit);
        UpdateInputField();
    }

    public void Up()
    {
        if(isButtonLocked)
            return;
        ChangeValue(value+1);
    }

    public void Down()
    {
        if(isButtonLocked)
            return;
        ChangeValue(value-1);
    }

    private void UpdateInputField()
    {
        inputField.text = value_.ToString();
    }

    public void ChangeMin(int num)
    {
        min_ = num;
        if(min_>max_) max_ = min_;
        if(min_>value_)
        {
           value_ = min_;
           ValueChanged?.Invoke(min_);
           UpdateInputField();
        } 
    }

    public void ChangeMax(int num)
    {
        max_ = num;
        if(min_>max_) min_ = max_;
        if(max_<value_) 
        {
            value_ = max_;
            ValueChanged?.Invoke(max_);
            UpdateInputField();
        }
    }

    public void ChangeValue(int num)
    {
        if (min_ > num)
        {
            value_ = min_;
        }
        else if (max_ < num)
        {
            value_ = max_;
        }
        else
        {
            value_ = num;
        }
        ValueChanged?.Invoke(value_);
        UpdateInputField();
    }

    private void OnEndEdit(string str)
    {
        if (int.TryParse(str, out int parsedNum))
        {
            ChangeValue(parsedNum);
            UpdateInputField();
            value_ = parsedNum;
        }
    }

}