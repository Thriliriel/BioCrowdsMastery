using UnityEngine;
using System.Collections;

public class SignController : MonoBehaviour {

    //sign appeal
    public float appeal;
    //goal it directs
    public GameObject goal;

    //GETs and SETs
    public float GetAppeal() {
        return this.appeal;
    }
    public void SetAppeal(float appeal) {
        this.appeal = appeal;
    }
    public GameObject GetGoal()
    {
        return this.goal;
    }
    public void SetGoal(GameObject goal)
    {
        this.goal = goal;
    }
}
