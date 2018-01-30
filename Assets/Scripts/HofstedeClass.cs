using UnityEngine;

public class HofstedeClass {
    //constructors
    public HofstedeClass()
    {
	    meanAngVar = meanCohesion = meanDist = meanSpeed = 0;
	    numGroups = 1;
        commonHumanSpeed = GameObject.Find("GameController").GetComponent<GameController>().defaultMeanSpeed;
    }

    public HofstedeClass(int newNumGroups)
    {
	    meanAngVar = meanCohesion = meanDist = meanSpeed = 0;
	    numGroups = newNumGroups;
        commonHumanSpeed = GameObject.Find("GameController").GetComponent<GameController>().defaultMeanSpeed;
    }

    //number of groups
    public int numGroups;
    // Common human speed m/s
    private float commonHumanSpeed;
    //Hofstede values
    private float meanDist, meanCohesion, meanAngVar, meanSpeed;
    //mean speed deviation
    private float meanSpeedDeviation;

    //calculate the hofstede values
    public void CalculateHofstede(int pdi, int mas, int lto, int ing, int idv)
    {
        meanDist = ((100.0f - pdi) * (1.2f) / 100) * numGroups;

        meanCohesion = (((100.0f - mas) * 3.0f) / 100) * numGroups;
        //meanCohesion = (((100.0f - mas) - (0.5f * lto)) / 0.5f) / 100 * 3;
        //meanCohesion = (((100.0f - mas) - (0.5f * lto)) * 0.5f) / 100 * 3;
        //meanCohesion = ((mas - (0.5f * lto)) / 0.5f);
        /*meanCohesion = ((mas - (0.5f * lto)));
        //if value is below zero, invert it
        if(meanCohesion < 0)
        {
            meanCohesion = 100.0f + meanCohesion;
        }
        //since it is a percentage...
        meanCohesion *= 0.03f;
        //abs
        //meanCohesion = Mathf.Abs(meanCohesion);*/

        meanAngVar = ((100.0f - lto) / 100) * numGroups;

        meanSpeed = (ing * commonHumanSpeed / 100) * numGroups;
        //meanSpeed = ((ing) - (0.5f * (100.0f - idv))) / 0.5f;
        /*meanSpeed = ((ing) - (0.5f * (100.0f - idv)));
        //if value is below zero, invert it
        if(meanSpeed < 0)
        {
            meanSpeed = 100.0f + meanSpeed;
        }
        //since it is a percentage
        meanSpeed *= (commonHumanSpeed / 100.0f);*/
        //abs
        //meanSpeed = Mathf.Abs(meanSpeed);
    }

    //Getters and Setters
    public float GetMeanDist()
    {
        return meanDist;
    }
    public void SetMeanDist(float value)
    {
        meanDist = value;
    }
    public float GetMeanCohesion()
    {
        return meanCohesion;
    }
    public void SetMeanCohesion(float value)
    {
        meanCohesion = value;
    }
    public float GetMeanAngVar()
    {
        return meanAngVar;
    }
    public void SetMeanAngVar(float value)
    {
        meanAngVar = value;
    }
    public float GetMeanSpeed()
    {
        return meanSpeed;
    }
    public void SetMeanSpeed(float value)
    {
        meanSpeed = value;
    }
    public float GetMeanSpeedDeviation()
    {
        return meanSpeedDeviation;
    }
    public void SetMeanSpeedDeviation(float value)
    {
        meanSpeedDeviation = value;
    }
}
