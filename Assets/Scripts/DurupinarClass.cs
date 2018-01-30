public class DurupinarClass
{
    //constructors
    public DurupinarClass()
    {
        leadership = panic = impatience = rightPreference = personalSpace = waitingRadius = walkingSpeed = gesturing = exploreEnvironment = waitingTimer = 0;
        trained = communication = pushing = false;
    }

    //Durupinar values
    private float leadership, panic, impatience, rightPreference, personalSpace, waitingRadius, walkingSpeed, gesturing, exploreEnvironment;
    private int waitingTimer;
    private bool trained, communication, pushing;

    //calculate the durupinar values according OCEAN informed
    public void CalculateDurupinar(float O, float C, float E, float A, float N)
    {
        //leadership
        float weight = 0.5f;
        leadership = (weight * E) + ((1 - weight) * (1 - N));
        if (leadership > 1) leadership = 1;

        //trained
        if (O >= 0.5f)
        {
            trained = true;
        }
        else trained = false;

        //communication
        if (E >= 0.5f)
        {
            communication = true;
        }
        else communication = false;

        //panic
        weight = 0.5f;
        float cFuntion = 0;
        if (C >= 0.5f)
        {
            cFuntion = (-2 * C) + 2;
        }
        panic = (weight * N) + (weight * cFuntion);
        if (panic > 1) panic = 1;

        //impatience
        float weightE = 0.1f;
        float weightAC = 0.45f;
        float eFunction = 0;
        if (E >= 0.5f)
        {
            eFunction = (2 * E) - 1;
        }
        impatience = (weightE * eFunction) + (weightAC * (1 - A)) + (weightAC * (1 - C));
        if (impatience > 1) impatience = 1;

        //pushing
        weight = 0.5f;
        if ((weight * E) + (weight * (1 - A)) > 0.5f)
        {
            pushing = true;
        }
        else pushing = false;

        //right preference
        weight = 0.5f;
        rightPreference = 0;
        float rFunction = (weight * A) + (weight * C);
        if (A < 0.5f || C < 0.5f)
        {
            rFunction = 0.5f;
        }
        if (rFunction >= 0.5f)
        {
            rightPreference = 1;
        }
        if (rightPreference > 1) rightPreference = 1;

        //personal space
        //already using the hall personal space. Plus, the Durupinar personal space is not fixed, since it depends if agent is behind or in front of another agent.

        //waiting radius
        waitingRadius = 0.25f;
        if (A >= 0.333f && A <= 0.666f)
        {
            waitingRadius = 0.45f;
        }
        else if (A > 0.666f && A <= 1)
        {
            waitingRadius = 0.65f;
        }

        //waiting timer
        waitingTimer = 1;
        if (A >= 1 / 3 && A <= 2 / 3)
        {
            waitingTimer = 5;
        }
        else if (A > 2 / 3 && A <= 1)
        {
            waitingTimer = 50;
        }

        //exploring the environment
        exploreEnvironment = 10 * O;

        //walking speed
        walkingSpeed = 1 + E;

        //gesturing
        gesturing = E * 10;
    }

    //Getters and Setters
    public float GetLeadership()
    {
        return leadership;
    }
    public void SetLeadership(float value)
    {
        leadership = value;
    }
    public bool GetTrained()
    {
        return trained;
    }
    public void SetTrained(bool value)
    {
        trained = value;
    }
    public bool GetCommunication()
    {
        return communication;
    }
    public void SetCommunication(bool value)
    {
        communication = value;
    }
    public float GetPanic()
    {
        return panic;
    }
    public void SetPanic(float value)
    {
        panic = value;
    }
    public float GetImpatience()
    {
        return impatience;
    }
    public void SetImpatience(float value)
    {
        impatience = value;
    }
    public bool GetPushing()
    {
        return pushing;
    }
    public void SetPushing(bool value)
    {
        pushing = value;
    }
    public float GetRightPreference()
    {
        return rightPreference;
    }
    public void SetRightPreference(float value)
    {
        rightPreference = value;
    }
    public float GetWaitingRadius()
    {
        return waitingRadius;
    }
    public void SetWaitingRadius(float value)
    {
        waitingRadius = value;
    }
    public int GetWaitingTimer()
    {
        return waitingTimer;
    }
    public void SetWaitingTimer(int value)
    {
        waitingTimer = value;
    }
    public float GetWalkingSpeed()
    {
        return walkingSpeed;
    }
    public void SetWalkingSpeed(float value)
    {
        walkingSpeed = value;
    }
    public float GetExploreEnvironment()
    {
        return exploreEnvironment;
    }
    public void SetExploreEnvironment(float value)
    {
        exploreEnvironment = value;
    }
    public float GetGesturing()
    {
        return gesturing;
    }
    public void SetGesturing(float value)
    {
        gesturing = value;
    }
}