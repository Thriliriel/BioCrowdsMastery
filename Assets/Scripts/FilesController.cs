using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class FilesController {
    //all simulation files directory
    public string allSimulations;
    //config filename
    public string configFilename;
    //obstacles filename
    public string obstaclesFilename;
    //schedule filename
    public string scheduleFilename;
    //exit filename
    public string exitFilename;
    //signs filename
    public string signsFilename;
    //goals filename
    public string goalsFilename;
    //exit agents/goal filename
    public string agentsGoalFilename;
    //exit interactions filename
    public string interactionsFilename;
    //exit mean speed filename
    public string meanSpeedFilename;
    //exit mean angvar filename
    public string meanAngVarFilename;
    //exit mean distance filename
    public string meanDistaceFilename;

    //exit file
    private StreamWriter exitFile;
    //exit agents/goal file
    private StreamWriter agentsGoalFile;
    //exit interactions file
    private StreamWriter interactionsFile;
    //exit mean speeds file
    private StreamWriter meanSpeedFile;
    //exit mean angvar file
    private StreamWriter meanAngVarFile;
    //exit mean distance file
    private StreamWriter meanDistanceFile;
    //file for Rodolfo analisys
    private StreamWriter fullFDFile;

    //constructor
    public FilesController(string allSims, string configFN, string obstaclesFN, string scheduleFN, string exitFN, string signsFN, string goalsFN, string agentsGoalFN, 
        string interactionsFN, string meanSpeedFN, string meanAngVarFN, string meanDistanceFN) {
        //set default values for actual simulation
        allSimulations = allSims;
        configFilename = configFN;
        obstaclesFilename = obstaclesFN;
        scheduleFilename = scheduleFN;
        exitFilename = exitFN;
        signsFilename = signsFN;
        goalsFilename = goalsFN;
        agentsGoalFilename = agentsGoalFN;
        interactionsFilename = interactionsFN;
        meanSpeedFilename = meanSpeedFN;
        meanAngVarFilename = meanAngVarFN;
        meanDistaceFilename = meanDistanceFN;

        //open exit files
        exitFile = File.CreateText(Application.dataPath + "/" + exitFilename);
        agentsGoalFile = File.CreateText(Application.dataPath + "/" + agentsGoalFilename);
        interactionsFile = File.CreateText(Application.dataPath + "/" + interactionsFilename);
        meanSpeedFile = File.CreateText(Application.dataPath + "/" + meanSpeedFilename);
        meanAngVarFile = File.CreateText(Application.dataPath + "/" + meanAngVarFilename);
        meanDistanceFile = File.CreateText(Application.dataPath + "/" + meanDistaceFilename);
        fullFDFile = File.CreateText(Application.dataPath + "/RodolfoFile.txt");
    }

    //close the exit files
    public void Finish() {
        exitFile.Close();
        agentsGoalFile.Close();
        interactionsFile.Close();
        meanSpeedFile.Close();
        meanAngVarFile.Close();
        fullFDFile.Close();
    }

    //save a csv config file
    //files saved: Config.csv, goals.dat
    public void SaveConfigFile(float cellRadius, float auxinRadius, GameObject[] allObstacles)
    {
        //config file
        StreamWriter file = File.CreateText(Application.dataPath + "/" + configFilename);
        //goals file
        StreamWriter fileGoals = File.CreateText(Application.dataPath + "/" + goalsFilename);
        //obstacles file
        StreamWriter fileObstacles = File.CreateText(Application.dataPath + "/" + obstaclesFilename);

        //first, we save the terrain dimensions
        Terrain terrain = GameObject.Find("Terrain").GetComponent<Terrain>();
        file.WriteLine("terrainSize:" + terrain.terrainData.size.x + "," + terrain.terrainData.size.z);

        //then, camera position and height
        GameObject camera = GameObject.Find("Camera");
        file.WriteLine("camera:" + camera.transform.position.x + "," + camera.transform.position.y + "," +
            camera.transform.position.z + "," + camera.GetComponent<Camera>().orthographicSize);

        List<AuxinController> allAuxins = new List<AuxinController>();

        //get cells info
        GameObject[] allCells = GameObject.FindGameObjectsWithTag("Cell");
        if (allCells.Length > 0)
        {
            //each line: name, positionx, positiony, positionz, cell radius
            //separated with ;

            file.WriteLine("qntCells:" + allCells.Length);
            //for each auxin
            for (int i = 0; i < allCells.Length; i++)
            {
                file.WriteLine(allCells[i].name + ";" + allCells[i].transform.position.x + ";" + allCells[i].transform.position.y +
                    ";" + allCells[i].transform.position.z + ";" + cellRadius);

                //add all cell auxins to write later
                List<AuxinController> allCellAuxins = allCells[i].GetComponent<CellController>().GetAuxins();
                for (int j = 0; j < allCellAuxins.Count; j++)
                {
                    //Debug.Log(allCellAuxins[j].name+" -- "+ allCellAuxins[j].position);
                    allAuxins.Add(allCellAuxins[j]);
                }
            }
        }

        //get auxins info
        if (allAuxins.Count > 0)
        {
            //each line: name, positionx, positiony, positionz, auxinRadius, cell
            //separated with ;

            file.WriteLine("qntAuxins:" + allAuxins.Count);
            //for each auxin
            for (int i = 0; i < allAuxins.Count; i++)
            {
                file.WriteLine(allAuxins[i].name + ";" + allAuxins[i].position.x + ";" + allAuxins[i].position.y +
                    ";" + allAuxins[i].position.z + ";" + auxinRadius + ";" + allAuxins[i].GetCell().name);
            }
        }
        file.Close();

        //get goals info
        GameObject[] allGoals = GameObject.FindGameObjectsWithTag("Goal");
        if (allGoals.Length > 0)
        {
            //separated with " "
            fileGoals.WriteLine(allGoals.Length);
            //for each goal
            for (int i = 0; i < allGoals.Length; i++)
            {
                //new line for the goal name and position
                fileGoals.WriteLine(allGoals[i].name + " " + allGoals[i].transform.position.x + " " + allGoals[i].transform.position.z);
            }
        }
        fileGoals.Close();

        //get obstacles info
        if (allObstacles.Length > 0)
        {
            //separated with ";"
            fileObstacles.WriteLine("qntObstacles:" + allObstacles.Length);
            //for each obstacle
            for (int i = 0; i < allObstacles.Length; i++)
            {
                //new line for the obstacle name
                fileObstacles.WriteLine("\nObstacle");
                //new line for the qnt vertices
                //obstacle mesh
                MeshFilter obsMesh = allObstacles[i].GetComponent<MeshFilter>();
                fileObstacles.WriteLine("qntVertices:" + obsMesh.mesh.vertexCount);

                //for each vertice
                for (int j = 0; j < obsMesh.mesh.vertexCount; j++)
                {
                    fileObstacles.WriteLine(obsMesh.mesh.vertices[j].x + ";" + obsMesh.mesh.vertices[j].y + ";" + obsMesh.mesh.vertices[j].z);
                }

                //new line for the qnt triangles
                fileObstacles.WriteLine("qntTriangles:" + obsMesh.mesh.triangles.Length);

                //for each triangle
                for (int j = 0; j < obsMesh.mesh.triangles.Length; j++)
                {
                    fileObstacles.WriteLine(obsMesh.mesh.triangles[j]);
                }
            }
        }
        fileObstacles.Close();
    }

    //save a csv exit file, with positions of all agents in function of time
    public void SaveExitFile(int lastFrameCount)
    {
        //get agents info
        GameObject[] allAgents = GameObject.FindGameObjectsWithTag("Player");
        if (allAgents.Length > 0)
        {
            //each line: frame, agents name, positionx, positiony, positionz, goal object name, cell name
            //separated with ;
            //for each agent
            for (int i = 0; i < allAgents.Length; i++)
            {
                exitFile.WriteLine(Time.frameCount - lastFrameCount + ";" + allAgents[i].name + ";" + allAgents[i].transform.position.x + ";" +
                    allAgents[i].transform.position.y + ";" + allAgents[i].transform.position.z + ";" +
                    allAgents[i].GetComponent<AgentController>().go[0].name + ";" +
                    allAgents[i].GetComponent<AgentController>().GetCell().name);
            }
        }
    }

    //save final metric on exit file
    public void SaveMetrics(float simulationTime, int maxQntGroups)
    {
        //simulation time, max qnt of groups
        exitFile.WriteLine(simulationTime);
        exitFile.WriteLine(maxQntGroups);
    }

    public void SaveAgentsGoalFile(string agentName, string goalName, int lastFrameCount)
    {
        //we save: Agent name, Goal name, Time he arrived
        agentsGoalFile.WriteLine(agentName + ";" + goalName + ";" + (Time.frameCount - lastFrameCount));
    }

    public void SaveInteractionsFile(GameObject agent, int lastFrameCount, float deltaIntention, float newIntention, string goalName, 
        GameObject sign = null, GameObject otherAgent = null)
    {
        //we save: Time, Agent name, Sign name (may be null) or other agent name (may be null), Goal it points, DeltaIntention, New Intention
        string interactionPairName = "";
        if(sign != null)
        {
            interactionPairName = sign.name;
        }
        else if (otherAgent != null)
        {
            interactionPairName = otherAgent.name;
        }
        interactionsFile.WriteLine((Time.frameCount - lastFrameCount) + ";" + agent.name + ";" + interactionPairName + ";" + goalName + ";" + deltaIntention + ";" + newIntention);
    }

    public void SaveMeanSpeedFile(int lastFrameCount, float meanSpeed)
    {
        //we save: Time, mean speed
        meanSpeedFile.WriteLine((Time.frameCount - lastFrameCount) + ";" + meanSpeed);
    }

    public void SaveAngVarFile(int lastFrameCount, float meanAngVar)
    {
        //we save: Time, mean speed
        meanAngVarFile.WriteLine((Time.frameCount - lastFrameCount) + ";" + meanAngVar);
    }

    public void SaveDistanceFile(int lastFrameCount, float meanDist)
    {
        //we save: Time, mean speed
        meanDistanceFile.WriteLine((Time.frameCount - lastFrameCount) + ";" + meanDist);
    }

    public void SaveFullFDFile(int qntAgents)
    {
        //for each agent, get its file
        for (int i = 0; i < qntAgents; i++)
        {
            //open file
            StreamReader sr = new StreamReader(Application.dataPath + "/agent" + i + ".txt", System.Text.Encoding.Default);

            //copy this file to the full file
            fullFDFile.Write(sr.ReadToEnd());

            //blank line
            fullFDFile.WriteLine("");

            //close this file
            sr.Close();

            //delete this file
            File.Delete(Application.dataPath + "/agent" + i + ".txt");
            File.Delete(Application.dataPath + "/agent" + i + ".meta");
            File.Delete(Application.dataPath + "/agent" + i + ".txt.meta");
        }
    }
}
