using System;
using UnityEngine;

public class FavarettoClass
{
    public FavarettoClass()
    {
        commonHumanSpeed = GameObject.Find("GameController").GetComponent<GameController>().defaultMeanSpeed;
        cNormalizationFactor = 0.0525f;
        speedNomalizationFactor = 6.8125f;
    }

    //Favaretto parameters
    private float angularVariation, speed, speedFactor, collectivity;

    // Common human speed m/s
    private float commonHumanSpeed;

    // Value used to normalize the original Conscientiousness by Favaretto between [0..100]
    private float cNormalizationFactor;

    // Value used to normalize the speed between [0..1]
    private float speedNomalizationFactor;
    
    public void CalculateFavaretto(float O, float C, float E, float A, float N)
    {
        angularVariation = 1 - O;

        speedFactor = (1 / (4 * angularVariation)) + cNormalizationFactor * (C * 100);
        speedFactor /= speedNomalizationFactor;
        speed = speedFactor * commonHumanSpeed * 2;

        collectivity = A;
        
        Debug.Log(String.Format(" Agente [{0},{1},{2},{3},{4}]  _speed:{5} - deviation:{6} _collectivity{7} ", O, C, E, A, N, speed, speedFactor, collectivity));
        // _coesion will be the mean values between _collectivity from all  group participant
        //cohesion is calculated for the group later
    }

    public float GetCollectivity()
    {
        return collectivity;
    }

    public float GetAngularVariation()
    {
        return angularVariation;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public float GetSpeedFactor()
    {
        return speedFactor;
    }
}