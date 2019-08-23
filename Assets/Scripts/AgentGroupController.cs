using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AgentGroupController : MonoBehaviour {

    //agents of this groups
    public List<GameObject> agents;
    //cell
    public GameObject cell;
    //goals schedule
    public List<GameObject> go;
    //goals intentions
    public List<float> intentions;
    //goals desire
    public List<float> desire;
    //group cohesion
    public float cohesion;
    //group mean speed
    public float meanSpeed;
    //group mean speed deviation
    public float meanSpeedDeviation;
    //group angular variation
    public float angularVariation;
    //group leader
    public GameObject leader;
    //distance between agents of the group
    public float distanceBetweenAgents;

    //terrain limits
    private Vector3 terrainLimits;
    //exit file
    private StreamWriter exitAgentGroupFile;
    //game controller
    private GameController gameController;
    //last frame count (from game controller)
    private int lastFrameCount;

    //cultural stuff
    //Hofstede class
    public HofstedeClass hofstede;
    //Hall zones
    public HallClass hall;

    private void OnDestroy()
    {
        CloseFile();
    }

    // Use this for initialization
    void Awake () {
        agents = new List<GameObject>();
        hofstede = new HofstedeClass();
        cohesion = 0;
        meanSpeed = 1.4f;
        meanSpeedDeviation = (3 - cohesion) / 15;
        angularVariation = 0;
        hall = new HallClass();
        //terrainLimits = new Vector3(GameObject.Find("Terrain").GetComponent<Terrain>().terrainData.size.x, 0, GameObject.Find("Terrain").GetComponent<Terrain>().terrainData.size.z);
        terrainLimits = new Vector3(20, 0, 32);
        distanceBetweenAgents = 0;

        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        lastFrameCount = gameController.lastFrameCount;
    }

    private void Start()
    {
        //OpenFile();
    }

    private void Update()
    {
        //calculate the mean distance between all agents, in this frame
        if (agents.Count > 1)
        {
            float frameDistanceBetweenAgents = 0;
            float frameMeanSpeed = 0;
            float frameAngVar = 0;
            int qntDistances = 0;

            for (int i = 0; i < agents.Count; i++)
            {
                for (int j = i + 1; j < agents.Count; j++)
                {
                    //distance
                    frameDistanceBetweenAgents += Vector3.Distance(agents[i].transform.position, agents[j].transform.position);
                    qntDistances++;
                }

                //speed
                frameMeanSpeed += agents[i].GetComponent<AgentController>().speedModule;

                //angvar
                frameAngVar += Vector3.Angle(agents[i].GetComponent<AgentController>().goal - agents[i].transform.position, agents[i].GetComponent<AgentController>().GetM());
            }

            //average values
            frameDistanceBetweenAgents /= (float)qntDistances;
            frameMeanSpeed /= agents.Count;
            frameAngVar /= agents.Count;

            //update total distance
            distanceBetweenAgents = (distanceBetweenAgents + frameDistanceBetweenAgents) / 2;

            //update in the file
            //SaveAgentsExitFile(frameDistanceBetweenAgents, frameMeanSpeed, frameAngVar);
        }
        else
        {
            distanceBetweenAgents = 0;
        }
    }

    public void OpenFile()
    {
        //get the actual simulation path
        string fileName = gameController.exitFilename;
        //break it
        string[] path = fileName.Split('/');
        //remount it, changing last part
        fileName = "";
        for(int i = 0; i < path.Length - 1; i++)
        {
            fileName = fileName + path[i] + "/";
        }

        //open exit files
        exitAgentGroupFile = File.CreateText(Application.dataPath + "/" + fileName + name + ".csv");
        exitAgentGroupFile.WriteLine(name);
    }

    public void CloseFile()
    {
        //close exit file
        //exitAgentGroupFile.Close();
    }

    public void SaveAgentsExitFile(float meanDistanceAgents, float meanSpeed, float meanAngVar)
    {
        //we save 3 lines:
        //Line 1: frame count, mean distance between agents on this frame
        //Line 2: frame count, mean speed of the group on this frame
        //Line 3: frame count, mean angVar of the group on this frame
        //exitAgentGroupFile.WriteLine((Time.frameCount - lastFrameCount) + ";" + meanDistanceAgents);
        //exitAgentGroupFile.WriteLine((Time.frameCount - lastFrameCount) + ";" + meanSpeed);
        //exitAgentGroupFile.WriteLine((Time.frameCount - lastFrameCount) + ";" + meanAngVar);
    }

    //just to update group cell
    public void UpdateCell(float cellRadius) {
        //just to update group cell
        //find all neighbours cells
        int startX = (int)(cell.transform.position.x - (cellRadius * 2f));
        int startZ = (int)(cell.transform.position.z - (cellRadius * 2f));
        int endX = (int)(cell.transform.position.x + (cellRadius * 2f));
        int endZ = (int)(cell.transform.position.z + (cellRadius * 2f));

        //see if it is in some border
        if ((int)cell.transform.position.x == cellRadius)
        {
            startX = (int)cell.transform.position.x;
        }
        if ((int)cell.transform.position.z == cellRadius)
        {
            startZ = (int)cell.transform.position.z;
        }
        if ((int)cell.transform.position.x == (int)terrainLimits.x - cellRadius)
        //if ((int)cell.transform.position.x == 29)
        {
            endX = (int)cell.transform.position.x;
        }
        if ((int)cell.transform.position.z == (int)terrainLimits.z - cellRadius)
        //if ((int)cell.transform.position.z == 29)
        {
            endZ = (int)cell.transform.position.z;
        }

        //distance from agent to cell, to define agent new cell
        float distanceToCell = Vector3.Distance(transform.position, cell.transform.position);
        //Debug.Log(gameObject.name+" -- StartX: "+startX+" -- StartZ: "+startZ+" -- EndX: "+endX+" -- EndZ: "+endZ);
        //iterate to find the cells
        //2 in 2, since the radius of each cell is 1 = diameter 2
        for (float i = startX; i <= endX; i = i + (cellRadius * 2))
        {
            for (float j = startZ; j <= endZ; j = j + (cellRadius * 2))
            {
                float nameX = i - cellRadius;
                float nameZ = j - cellRadius;
                //find the cell
                //GameObject neighbourCell = GameObject.Find("cell" + nameX + "-" + nameZ);
                GameObject neighbourCell = CellController.GetCellByName("cell" + nameX + "-" + nameZ);

                //if it exists..
                if (neighbourCell)
                {
                    //see distance to this cell
                    //if it is lower, the agent is in another(neighbour) cell
                    float distanceToNeighbourCell = Vector3.Distance(transform.position, neighbourCell.transform.position);
                    if (distanceToNeighbourCell < distanceToCell)
                    {
                        distanceToCell = distanceToNeighbourCell;
                        cell = neighbourCell;
                    }
                }
            }
        }
    }

    //reorder goals/intentions
    public void ReorderGoals()
    {
        for (int i = 0; i < intentions.Count; i++)
        {
            for (int j = i + 1; j < intentions.Count; j++)
            {
                //if j element is bigger, change
                if (intentions[i] < intentions[j])
                {
                    //reorder intentions
                    float temp = intentions[j];
                    intentions[j] = intentions[i];
                    intentions[i] = temp;

                    //reorder desires
                    float tempD = desire[j];
                    desire[j] = desire[i];
                    desire[i] = tempD;

                    //reorder goals
                    GameObject tempG = go[j];
                    go[j] = go[i];
                    go[i] = tempG;
                }
            }
        }
    }

    //transform the cohesion value in a Hall distance value
    //Higher cohesion: until intimate zone
    //Lower cohesion: until personal zone
    public float CohesionToHall(float maxIntimate, float maxPersonal)
    {
        //just for testing all with the same split distance
        //return 3.6f;
        //get the difference between maxPersonal and maxIntimate
        float difference = maxPersonal - maxIntimate;
        //now, divide it by 3 to get the grain variation (since cohesion varies from 0 to 3)
        difference /= 3;

        //return the correspondent Hall value
        //how it works:
        //the maxPersonal value minus cohesion multiplied by the grain difference
        //so, the more cohesion, the less distance between agents
        return (float)(maxPersonal - (cohesion * difference));
        //NEW STUFF: the more cohesion, the more space the group has before split
        //return (float)(maxPersonal - ((3 - cohesion) * difference));
    }

    //getters and setters
    public float GetCohesion()
    {
        return cohesion;
    }
    public void SetCohesion(float value)
    {
        cohesion = value;
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
    public float GetAngularVariarion()
    {
        return angularVariation;
    }
    public void SetAngularVariarion(float value)
    {
        angularVariation = value;
    }
}
