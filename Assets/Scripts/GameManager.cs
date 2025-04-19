using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool inBoardView, isTransitioning;
    void Awake()
    {
        instance = this;   
    }
}
