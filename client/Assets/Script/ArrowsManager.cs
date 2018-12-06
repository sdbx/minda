using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowsManager : MonoBehaviour
{
    public GameObject boardObject;
    public GameObject arrowPrefab;
    public float arrowDistance;
    public CubeCoord cubeCoord = new CubeCoord(0,0);
    private DirectionSelectingArrow[] _arrows = new DirectionSelectingArrow[6];
    private BoardManager _boardManger;
    public int pushingArrow = -1;
    public bool selected = false;
    public bool working = true;

    void Start()
    {
        _boardManger = boardObject.GetComponent<BoardManager>();

        for (int i = 0; i < 6; i++)
        {
            DirectionSelectingArrow current;
            current = Instantiate(arrowPrefab, new Vector3(), new Quaternion(0, 0, 0, 0), gameObject.transform).GetComponent<DirectionSelectingArrow>();
            current.SetDirection(i);
            current.originDistance = arrowDistance;
            _arrows[i] = current;
        }
    }
    
    void Update()
    {
         if(!working)
        {
            for (int i = 0; i < 6; i++)
            {
                _arrows[i].gameObject.SetActive(false);
            }
        }

        gameObject.transform.localPosition = cubeCoord.GetPixelPoint(_boardManger.holeDistance);

        if (pushingArrow != -1)
        {
            for (int i = 0; i < 6; i++)
            {
                if(i == pushingArrow)
                    continue;
                _arrows[i].gameObject.SetActive(false);
            }

            if(selected)
            {
                working = false;
            }
        }
        else 
        {
            for (int i = 0; i < 6; i++)
            {
                _arrows[i].gameObject.SetActive(true);
            }
        }
    }

}