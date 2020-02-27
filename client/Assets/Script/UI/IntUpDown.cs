using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class IntUpDown : MonoBehaviour
{
    [FormerlySerializedAs("up")] [SerializeField] private Button _up;
    [FormerlySerializedAs("down")] [SerializeField] private Button _down;
    [FormerlySerializedAs("inputField")] [SerializeField] private InputField _inputField;

    [FormerlySerializedAs("min_")] [SerializeField]
    private int _min;

    [FormerlySerializedAs("max_")] [SerializeField]
    private int _max;

    public int min { get { return _min; } }
    public int max { get { return _max; } }

    [SerializeField] private int value_;
    public int value { get { return value_; } }

    public event Action<int> ValueChanged;

    public bool isButtonLocked = false;

    private void Awake()
    {
        _up.onClick.AddListener(Up);
        _down.onClick.AddListener(Down);
        _inputField.onEndEdit.AddListener(OnEndEdit);
        UpdateInputField();
    }

    public void Up()
    {
        if (isButtonLocked)
            return;
        ChangeValue(value + 1);
    }

    public void Down()
    {
        if (isButtonLocked)
            return;
        ChangeValue(value - 1);
    }

    private void UpdateInputField()
    {
        _inputField.text = value_.ToString();
    }

    public void ChangeMin(int num)
    {
        _min = num;
        if (_min > _max) _max = _min;
        if (_min > value_)
        {
            value_ = _min;
            ValueChanged?.Invoke(_min);
            UpdateInputField();
        }
    }

    public void ChangeMax(int num)
    {
        _max = num;
        if (_min > _max) _min = _max;
        if (_max < value_)
        {
            value_ = _max;
            ValueChanged?.Invoke(_max);
            UpdateInputField();
        }
    }

    public void ChangeValue(int num)
    {
        var prevValue = value_;

        if (_min > num)
        {
            value_ = _min;
        }
        else if (_max < num)
        {
            value_ = _max;
        }
        else
        {
            value_ = num;
        }

        if (value_ == prevValue) return;

        ValueChanged?.Invoke(value_);
        UpdateInputField();
    }

    private void OnEndEdit(string str)
    {
        if (isButtonLocked)
        {
            UpdateInputField();
            return;
        }

        if (int.TryParse(str, out var parsedNum))
        {
            ChangeValue(parsedNum);
            UpdateInputField();
            value_ = parsedNum;
        }
    }
}
