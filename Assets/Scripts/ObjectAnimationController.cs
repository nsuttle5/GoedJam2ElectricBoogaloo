using System.Collections.Generic;
using UnityEngine;

public class ObjectAnimationController : MonoBehaviour
{
    public CutsceneManager2 CM;
    public Animator animator;
    public List<int> shotNumbers;

    private int index = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CM = GameObject.Find("CinematicsCenter").GetComponent<CutsceneManager2>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetInteger("ShotNumber", CM.shotNumber);
    }
}
