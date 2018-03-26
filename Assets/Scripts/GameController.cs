using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    //scenario X
    public float scenarioSizeX;
    //scenario Z
    public float scenarioSizeZ;
    //agent prefab
    public GameObject agent;
    //agent radius
    public float agentRadius;
    //cell radius
    public float cellRadius;
    //cell prefab
    public GameObject cell;
    //sign prefab
    public GameObject sign;
    //goal prefab
    public GameObject goalP;
    //agent group prefab
    public GameObject agentGroup;
    //qnt of agents in the scene
    public int qntAgents;
    //qnt of signs in the scene
    public int qntSigns;
    //qnt of goals in the scene
    public int qntGoals;
    //radius for auxin collide
    public float auxinRadius;
    //save config file?
    public bool saveConfigFile;
    //load config file?
    public bool loadConfigFile;
    //all simulation files directory
    public string allSimulations;
    //master filename
    public string masterFilename;
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
    //Hofstede filename
    public string hofstedeFilename;
    //Durupinar filename
    public string durupinarFilename;
    //Favaretto filename
    public string favarettoFilename;
    //goals filename
    public string goalsFilename;
    //exit agents/goal filename
    public string agentsGoalFilename;
    //exit interactions filename
    public string interactionsFilename;
    //mean speed filename
    public string meanSpeedFilename;
    //mean angvar filename
    public string meanAngVarFilename;
    //canvas text
    public Transform canvasText;
    //files controller
    public FilesController filesController;
    //last frame count
    public int lastFrameCount = 0;
    //obstacle displacement (since the obstacle is not at Unity 0,0,0)
    public float obstacleDisplacement;
    //qnt groups (0 value means no groups, where each agent will be alone inside a group)
    public int qntGroups;
    //use Hofstede?
    public bool useHofstede;
    //use Durupinar?
    public bool useDurupinar;
    //use Favaretto
    public bool useFavaretto;
    //paint heatMaps?
    public bool paintHeatMap;
    //using exploratory behavior?
    public bool exploratoryBehavior;
    //using evacuation scenario?
    public bool evacuationBehavior;
    //using group behavior? (split and join agents)
    public bool groupBehavior;
    //using thermal comfort?
    public bool thermalComfort;
    //using density comfort?
    public bool densityComfort;
    //qnt frames
    public int qntFrames;
    //seconds taken
    public float secondsTaken;
    //type of place
    //example: restaurant, library, square..
    public enum roomTypes
    {
        square, restaurant, theater, shop, corridor
    }

    //default group values
    //default cohesion
    public float defaultCohesion = 0;
    //default mean speed
    public float defaultMeanSpeed = 1.4f;
    //default mean speed deviation
    public float defaultMeanSpeedDeviation = 0.2f;
    //default mean speed deviation
    public float defaultAngularVariation = 0;
    //finished default group values

    //fixed frame step for calculus (in seconds)
    public float fixedStep;
    //FD file to fix
    public string fDFileToFix;
    //qnt frame to fix
    public int startingFrameToFix;

    //mean speed of all agents through the simulation
    public float meanAgentsSpeed;
    //mean angular variation of all agents through the simulation
    public float meanAgentsAngVar;
    //max qnt of groups through the simulation
    public int maxQntGroups;
    //store all cells
    public GameObject[] allCells;

    //all agents
    private GameObject[] allAgents;
    //all Groups
    private GameObject[] allGroups;
    //all rooms
    private GameObject[] allRooms;
    //auxins density
    private float PORC_QTD_Marcacoes = 0.65f;
    //private float PORC_QTD_Marcacoes = 1f;
    //qnt of auxins on the ground
    private int qntAuxins;
    //all config directories
    private string[] allDirs;
    //simulation index
    private int simulationIndex = 0;
    //stop all sims
    private bool gameOver = false;
    //terrain
    private Terrain terrain;
    //intention threshold
    private float intentionThreshold = 0.8f;
    //signs instantiated positions
    private List<Vector3> positionsSigns;
    //all obstacles
    private GameObject[] allObstacles;
    //text element
    private Text text;
    //the time it shows in the screen
    private int showTime;
    //junt to control que qnt of agents instantiated
    private int controlQntAgents = 0;
    //static looking for, for tests
    private Vector3 staticLookingFor;
    //control the group names
    private int groupsNameCounter;

    //testing D*
    //PathPlanningDClass dStar;
    //public List<NodeClass> pathD;

    //on destroy application, close the exit files
    void OnDestroy()
    {
        //just to be sure, close agents files
        GameObject[] allAgents = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject ag in allAgents)
        {
            ag.GetComponent<AgentController>().CloseFile();
        }

        //save the Rodolfo file
        filesController.SaveFullFDFile(qntAgents);

        filesController.Finish();
    }

    // Use this for initialization
    void Awake()
    {
        //default useless value
        staticLookingFor = Vector3.zero;

        //get rooms
        allRooms = GameObject.FindGameObjectsWithTag("Room");

        meanAgentsSpeed = meanAgentsAngVar = maxQntGroups = groupsNameCounter = 0;

        if (qntGroups == 0 || qntGroups > qntAgents)
        {
            qntGroups = qntAgents;
        }

        //get the text element
        text = canvasText.GetComponent<Text>();

        showTime = 0;

        qntFrames = 0;
        secondsTaken = 0;

        //start the positionSigns list
        positionsSigns = new List<Vector3>();

        //get all subdirectories within the defined config directory
        allDirs = Directory.GetDirectories(Application.dataPath + "/" + allSimulations);

        //reorder to make the numbers right (0 1 2 3, instead 0 1 10 11)
        System.Array.Sort(allDirs, delegate (string a, string b)
        {
            string[] breakA = a.Split('_');
            string[] breakB = b.Split('_');
            int indA = System.Int32.Parse(breakA[breakA.Length - 1]);
            int indB = System.Int32.Parse(breakB[breakB.Length - 1]);
            return (indA).CompareTo(indB);
        });
        /*foreach (string dir in allDirs) {
            Debug.Log(dir);
        }*/

        //camera height and center position
        ConfigCamera();

        //find the Terrain gameObject and his terrain component
        terrain = GameObject.Find("Terrain").GetComponent<Terrain>();
        //change terrain size according informed
        terrain.terrainData.size = new Vector3(scenarioSizeX, terrain.terrainData.size.y, scenarioSizeZ);

        //get all obstacles (which should already be on the scene, along cells, goals, signs and markers)
        allObstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        //get all cells
        allCells = GameObject.FindGameObjectsWithTag("Cell");

        //set cells names and stuff
        foreach(GameObject cell in allCells)
        {
            cell.GetComponent<CellController>().cellName = cell.name;
            cell.GetComponent<CellController>().qntAgents = qntAgents;
            cell.GetComponent<CellController>().paintHeatMap = paintHeatMap;
        }

        //load master file
        LoadMasterFile();

        //if loadConfigFile is checked, we do not generate random agents and signs. We load it from the agents.dat and signs.dat
        if (loadConfigFile)
        {
            //change the heat maps, since its value may be diferent at the master file
            foreach (GameObject cell in allCells)
            {
                cell.GetComponent<CellController>().paintHeatMap = paintHeatMap;
            }

            //load the chain simulation, according iterator saved
            LoadChainSimulation();
        }//else, generate anew
        else
        {
            GenerateAnew();
        }

        //if using durupinar, set the group information
        if (useDurupinar)
        {
            GameObject[] theGroups = GameObject.FindGameObjectsWithTag("AgentGroup");
            foreach (GameObject agp in theGroups)
            {
                //if the group has a strong leader, the group values are calculated with its values
                float leaderValue = 0;
                int leaderIndex = 0;
                float meanLeadership = 0;
                float meanWalkingSpeed = 0;
                //float meanGesturing = 0;
                float meanExploringEnvironment = 0;
                //float meanWaitingRadius = 0;
                //int meanCommunication = 0;
                float meanImpatience = 0;
                for (int i = 0; i < agp.GetComponent<AgentGroupController>().agents.Count; i++)
                {
                    //update for mean values
                    meanLeadership += agp.GetComponent<AgentGroupController>().agents[i].GetComponent<AgentController>().durupinar.GetLeadership();
                    meanWalkingSpeed += agp.GetComponent<AgentGroupController>().agents[i].GetComponent<AgentController>().durupinar.GetWalkingSpeed();
                    //meanGesturing += agp.GetComponent<AgentGroupController>().agents[i].GetComponent<AgentController>().durupinar.GetGesturing();
                    meanExploringEnvironment += agp.GetComponent<AgentGroupController>().agents[i].GetComponent<AgentController>().durupinar.GetExploreEnvironment();
                    meanImpatience += agp.GetComponent<AgentGroupController>().agents[i].GetComponent<AgentController>().durupinar.GetImpatience();
                    //meanWaitingRadius += agp.GetComponent<AgentGroupController>().agents[i].GetComponent<AgentController>().durupinar.GetWaitingRadius();
                    /*if (agp.GetComponent<AgentGroupController>().agents[i].GetComponent<AgentController>().durupinar.GetCommunication())
                    {
                        meanCommunication++;
                    }*/

                    if (agp.GetComponent<AgentGroupController>().agents[i].GetComponent<AgentController>().durupinar.GetLeadership() > leaderValue)
                    {
                        leaderValue = agp.GetComponent<AgentGroupController>().agents[i].GetComponent<AgentController>().durupinar.GetLeadership();
                        leaderIndex = i;
                    }
                }
                if (leaderValue > 0.9f)
                {
                    //set mean speed. PROPOSAL: use the same calculated by Duru
                    /*agp.GetComponent<AgentGroupController>().meanSpeed =
                        agp.GetComponent<AgentGroupController>().agents[leaderIndex].GetComponent<AgentController>().durupinar.GetWalkingSpeed();*/
                    //NEW PROPOSAL: get the calculated duru as a percentage of the max speed, and use this to calculate the speed
                    float duruSpeed = agp.GetComponent<AgentGroupController>().agents[leaderIndex].GetComponent<AgentController>().durupinar.GetWalkingSpeed();
                    //since it is between 1 and 2, we can normalize the value just subtracting 1
                    duruSpeed -= 1;
                    //now, calculate the real value
                    agp.GetComponent<AgentGroupController>().meanSpeed = defaultMeanSpeed * duruSpeed;

                    //set cohesion. PROPOSAL: function of communication, leadership and gesturing
                    //since the values varies from 0 to 3, each of them is 1
                    /*agp.GetComponent<AgentGroupController>().cohesion =
                        (agp.GetComponent<AgentGroupController>().agents[leaderIndex].GetComponent<AgentController>().durupinar.GetLeadership()) +
                        (agp.GetComponent<AgentGroupController>().agents[leaderIndex].GetComponent<AgentController>().durupinar.GetGesturing() / 10);
                    if (agp.GetComponent<AgentGroupController>().agents[leaderIndex].GetComponent<AgentController>().durupinar.GetCommunication())
                    {
                        agp.GetComponent<AgentGroupController>().cohesion += 1;
                    }*/
                    //NEW PROPOSAL: function of waiting radius
                    /*float waitingRadius = agp.GetComponent<AgentGroupController>().agents[leaderIndex].GetComponent<AgentController>().durupinar.GetWaitingRadius();
                    float cohesionFromWR = (waitingRadius - 0.25f) * 7.5f;
                    //cohesion
                    agp.GetComponent<AgentGroupController>().cohesion = cohesionFromWR;*/
                    //NEEEWWWW PROPOSAL: function of impatience
                    //cohesion
                    agp.GetComponent<AgentGroupController>().cohesion = 
                        (1 - agp.GetComponent<AgentGroupController>().agents[leaderIndex].GetComponent<AgentController>().durupinar.GetImpatience()) * 3;

                    //set speed deviation. PROPOSAL: keep it as function of Cohesion, just like Hof
                    agp.GetComponent<AgentGroupController>().meanSpeedDeviation =
                        (3 - agp.GetComponent<AgentGroupController>().cohesion) / 15;

                    //set the angular variation. PROPOSAL: function of explore enviroment
                    //we set a percentage value between 0 and 90 degrees
                    //inversed, since the more O value, the less variation.
                    float percAV = 1 - (agp.GetComponent<AgentGroupController>().agents[leaderIndex].GetComponent<AgentController>().durupinar.GetExploreEnvironment() / 10);

                    agp.GetComponent<AgentGroupController>().angularVariation = 90 * percAV;

                    //set the leader
                    agp.GetComponent<AgentGroupController>().leader = agp.GetComponent<AgentGroupController>().agents[leaderIndex];
                }//else, uses the average values of all agents
                else
                {
                    meanLeadership /= agp.GetComponent<AgentGroupController>().agents.Count;
                    meanWalkingSpeed /= agp.GetComponent<AgentGroupController>().agents.Count;
                    //meanGesturing /= agp.GetComponent<AgentGroupController>().agents.Count;
                    meanExploringEnvironment /= agp.GetComponent<AgentGroupController>().agents.Count;
                    meanImpatience /= agp.GetComponent<AgentGroupController>().agents.Count;
                    //meanWaitingRadius /= agp.GetComponent<AgentGroupController>().agents.Count;
                    /*bool comu = false;
                    if (meanCommunication > agp.GetComponent<AgentGroupController>().agents.Count / 2)
                    {
                        comu = true;
                    }*/

                    //set mean speed. PROPOSAL: use the same calculated by Duru
                    //agp.GetComponent<AgentGroupController>().meanSpeed = meanWalkingSpeed;
                    //NEW PROPOSAL: get the calculated duru as a percentage of the max speed, and use this to calculate the speed
                    float duruSpeed = meanWalkingSpeed;
                    //since it is between 1 and 2, we can normalize the value just subtracting 1
                    duruSpeed -= 1;
                    //now, calculate the real value
                    agp.GetComponent<AgentGroupController>().meanSpeed = defaultMeanSpeed * duruSpeed;

                    //set cohesion. PROPOSAL: function of communication, leadership and gesturing
                    //since the values varies from 0 to 3, each of them is 1
                    /*agp.GetComponent<AgentGroupController>().cohesion = (meanLeadership) + (meanGesturing / 10);
                    if (comu)
                    {
                        agp.GetComponent<AgentGroupController>().cohesion += 1;
                    }*/
                    //NEW PROPOSAL: function of waiting radius
                    /*float waitingRadius = meanWaitingRadius;
                    float cohesionFromWR = (waitingRadius - 0.25f) * 7.5f;
                    //cohesion
                    agp.GetComponent<AgentGroupController>().cohesion = cohesionFromWR;*/
                    //NEEEWWWW PROPOSAL: function of impatience
                    //cohesion
                    agp.GetComponent<AgentGroupController>().cohesion = (1 - meanImpatience) * 3;

                    //set speed deviation. PROPOSAL: keep it as function of Cohesion, just like Hof
                    agp.GetComponent<AgentGroupController>().meanSpeedDeviation =
                        (3 - agp.GetComponent<AgentGroupController>().cohesion) / 15;

                    //set the angular variation. PROPOSAL: function of explore enviroment
                    //we set a percentage value between 0 and 90 degrees
                    //inversed, since the more O value, the less variation.
                    float percAV = 1 - (meanExploringEnvironment / 10);

                    agp.GetComponent<AgentGroupController>().angularVariation = 90 * percAV;
                }

                //once we have the group info, set the agents info
                foreach (GameObject ag in agp.GetComponent<AgentGroupController>().agents)
                {
                    //the agent maxSpeed will not be the defined maxSpeed, but the calculated meanSpeed of the group with a variation in the meanSpeedDeviation, defined by the group cohesion
                    float newSpeed = -1;
                    //while speed is invalid
                    while (newSpeed <= 0)//newSpeed > agent.GetComponent<AgentController>().maxSpeed || 
                    {
                        newSpeed = Random.Range(agp.GetComponent<AgentGroupController>().GetMeanSpeed() - agp.GetComponent<AgentGroupController>().GetMeanSpeedDeviation(),
                            agp.GetComponent<AgentGroupController>().GetMeanSpeed() + agp.GetComponent<AgentGroupController>().GetMeanSpeedDeviation());
                    }
                    //set agent speed
                    ag.GetComponent<AgentController>().maxSpeed = newSpeed;
                }
            }
        }//if using Favaretto, set the information
        else if (useFavaretto)
        {
            GameObject[] groups = GameObject.FindGameObjectsWithTag("AgentGroup");
            foreach (GameObject group in groups)
            {
                float sumCollectivity = 0f, sumSpeed = 0f, sumSpeedDeviation = 0f, sumAngularVar = 0f;
                AgentGroupController groupController = group.GetComponent<AgentGroupController>();
                int groupSize = groupController.agents.Count;

                //sets the first agent as leader to be used as comparison in the algorithm
                groupController.leader = groupController.agents[0];
                foreach (GameObject agent in groupController.agents)
                {
                    AgentController controller = agent.GetComponent<AgentController>();

                    controller.maxSpeed = controller.favaretto.GetSpeed();
                    sumCollectivity += controller.favaretto.GetCollectivity();
                    sumSpeed += controller.favaretto.GetSpeed();
                    sumSpeedDeviation += controller.favaretto.GetSpeedFactor();
                    sumAngularVar += controller.favaretto.GetAngularVariation();
                    if (controller.favaretto.GetCollectivity() > groupController.leader.GetComponent<AgentController>().favaretto.GetCollectivity())
                    {
                        groupController.leader = agent;
                    }
                }

                //Collectivity is between [0..1] and Cohesion is between [0..3]
                groupController.cohesion = (sumCollectivity / groupSize) * 3;
                groupController.meanSpeed = sumSpeed / groupSize;
                //groupController.meanSpeedDeviation = System.Math.Abs(0.5f - (sumSpeedDeviation / groupSize));
                groupController.meanSpeedDeviation = (3 - groupController.cohesion) / 15;
                groupController.angularVariation = 90 * (sumAngularVar / groupSize);
            }
        }
        //set the text
        text.text = "Simulation " + simulationIndex + " Started!";

        //calculate paths
        //CalculateAllPaths();

        //testing D*
        //dStar = new PathPlanningDClass(allCells.Length * 10);
        //pathD = new List<NodeClass>();
    }

    void Start()
    {
        //all ready to go. If saveConfigFile is checked, save this config in a csv file
        if (saveConfigFile)
        {
            filesController.SaveConfigFile(cellRadius, auxinRadius, allObstacles);
        }

        //change timeScale (make things faster)
        //seems to not be so good, since it calculates just once for a great amount of time
        //Time.timeScale = 5f;

        //testing D*
        //pathD = dStar.FindPath(allCells[0], allCells[100]);
    }

    // Update is called once per frame
    void Update()
    {
        //testing D*
        //List<GameObject> pathD = dStar.FindPath(allCells[0], allCells[100]);
        /*for (int i = 0; i < pathD.Count - 1; i++)
        {
            Debug.DrawLine(pathD[i].cell.transform.position, pathD[i + 1].cell.transform.position, Color.red);
        }*/

        //if simulation should be running yet
        if (!gameOver)
        {
            //Debug.Log(Time.deltaTime);

            //update agents list
            allAgents = GameObject.FindGameObjectsWithTag("Player");
            
            //reset auxins
            //for each agent, we reset their auxins
            foreach (GameObject ag in allAgents)
            {
                //reset its speed
                ag.GetComponent<AgentController>().speed = Vector3.zero;

                List<AuxinController> axAge = ag.GetComponent<AgentController>().GetAuxins();
                for (int j = 0; j < axAge.Count; j++)
                {
                    axAge[j].ResetAuxin();
                }
            }

            //find nearest auxins for each agent
            foreach (GameObject ag in allAgents)
            {
                //find all auxins near him (Voronoi Diagram)
                ag.GetComponent<AgentController>().FindNearAuxins(cellRadius);
            }

            //get all groups
            allGroups = GameObject.FindGameObjectsWithTag("AgentGroup");

            //defined threshold to separate
            float threshold = 3.6f;

            /*to find where the agent must move, we need to get the vectors from the agent to each auxin he has, and compare with 
            the vector from agent to goal, generating a angle which must lie between 0 (best case) and 180 (worst case)
            The calculation formula was taken from the Bicho´s mastery tesis and from Paravisi algorithm, all included
            in AgentController.
            */

            /*for each agent, we:
            1 - verify if he is in the scene. If he is...
            2 - find him 
            3 - for each auxin near him, find the distance vector between it and the agent
            4 - calculate the movement vector (CalculateMotionVector())
            5 - calculate speed vector (CalculateSpeed())
            6 - walk (Walk())
            7 - verify if the agent has reached the goal. If so, destroy it
            */
            foreach (GameObject agentI in allAgents)
            {
                
                //find the agent
                AgentController agentIController = agentI.GetComponent<AgentController>();
                GameObject goal = agentIController.go[0];
                List<AuxinController> agentAuxins = agentIController.GetAuxins();
                AgentGroupController agentGroupI = agentIController.group.GetComponent<AgentGroupController>();

                //just calculate the motion vector if agent is not marked as idle
                if (!agentIController.isIdle)
                {
                    threshold = agentGroupI.CohesionToHall(agentGroupI.hall.GetPersonalZone().y, agentGroupI.hall.GetSocialZone().y);
                    //Debug.Log(threshold);
                    //vector for each auxin
                    for (int j = 0; j < agentAuxins.Count; j++)
                    {
                        //add the distance vector between it and the agent
                        agentIController.vetorDistRelacaoMarcacao.Add(agentAuxins[j].position - agentI.transform.position);

                        //just draw the little spider legs xD
                        if (agentAuxins[j].ignoreAuxin)
                        {
                            Debug.DrawLine(agentAuxins[j].position, agentI.transform.position, Color.black);
                        }
                        else
                        {
                            Debug.DrawLine(agentAuxins[j].position, agentI.transform.position);
                        }
                    }

                    //calculate the movement vector
                    agentIController.CalculateMotionVector();
                    //calculate speed vector
                    agentIController.CalculateSpeed();

                    //if group behavior, check splits
                    if (groupBehavior)
                    {
                        //check if agent is too far away of the group, so we need to create a new group for him
                        bool tooFarAway = true;

                        //if size = 1, there are no other agents on the group. So, tooFarAway = false
                        if (agentGroupI.agents.Count == 1)
                        {
                            tooFarAway = false;
                        }//else, we check its distance with every other agent in the group
                        else
                        {
                            //NEW: check the distance between the agent and the center of its group
                            float distance = (Vector3.Distance(agentI.transform.position, agentIController.group.transform.position)) - (auxinRadius * 2); //- (2 * agentRadius)

                            //if it is still inside the threshold distance, it is ok
                            if (distance <= threshold)
                            {
                                //reset the agent farAwayTimer
                                agentIController.farAwayTimer = 0;
                                tooFarAway = false;
                            }//else, we need to update the counter and see if it is still inside the max timer
                            else
                            {
                                tooFarAway = true;
                                //Debug.Log(agentI.name + " with " + thisAgent.name + " and threshold " + threshold + " - Distance: " + distance);
                            }
                        }

                        //now, if tooFarAway, need to create a new group for the agent
                        if (tooFarAway)
                        {
                            //update timer
                            agentIController.farAwayTimer++;

                            //test the timer. Agent is just far away if the max timer is passed
                            if (agentIController.farAwayTimer >= agentIController.maxFarAwayTimer)
                            {
                                SplitAgent(agentI);
                            }
                        }//else, he is still inside the group. Check interactions between them IF inside the cohesion value
                        else
                        {
                            foreach (GameObject otherAgentInGroup in agentGroupI.agents)
                            {
                                if (agentI.name != otherAgentInGroup.name)
                                {
                                    float distance = Vector3.Distance(agentI.transform.position, otherAgentInGroup.transform.position);
                                    if (distance < threshold)
                                    {
                                        agentIController.InteractionBetweenAgents(otherAgentInGroup, distance, agentGroupI.GetCohesion());
                                    }
                                }
                            }
                        }
                    }

                    //just to be sure
                    agentGroupI = agentIController.group.GetComponent<AgentGroupController>();
                    //Debug.Log(agentIController.group.name);

                    //now, we check if agent is stuck with another agent
                    //if so, change places
                    if (agentIController.speed.Equals(Vector3.zero))
                    {
                        Collider[] lockHit = Physics.OverlapSphere(agentI.transform.position, agentRadius);
                        foreach (Collider loki in lockHit)
                        {
                            //if it is the Player tag (agent) and it is not the agent itself and he can change position (to avoid forever changing)
                            if (loki.gameObject.tag == "Player" && loki.gameObject.name != agentI.gameObject.name && agentIController.changePosition)
                            {
                                //the other agent will not change position in this frame
                                loki.GetComponent<AgentController>().changePosition = false;
                                Debug.Log(agentI.gameObject.name + " -- " + loki.gameObject.name);
                                //exchange!!!
                                Vector3 positionA = agentI.transform.position;
                                agentI.transform.position = loki.gameObject.transform.position;
                                loki.gameObject.transform.position = positionA;
                            }
                        }
                    }
                }
            }

            //just to be sure
            allGroups = GameObject.FindGameObjectsWithTag("AgentGroup");

            //reset mean speed
            //meanAgentsSpeed = 0;
            float frameMeanAgentsSpeed = 0;
            float frameMeanAgentsAngVar = 0;

            foreach (GameObject agentI in allAgents)
            {
                //find the agent
                AgentController agentIController = agentI.GetComponent<AgentController>();
                GameObject goal = agentIController.go[0];
                List<AuxinController> agentAuxins = agentIController.GetAuxins();
                AgentGroupController agentGroupI = agentIController.group.GetComponent<AgentGroupController>();

                //just walk and stuff if agent is not marked as idle
                if (!agentIController.isIdle)
                {
                    threshold = agentGroupI.CohesionToHall(agentGroupI.hall.GetPersonalZone().y, agentGroupI.hall.GetSocialZone().y);

                    //update mean speed
                    frameMeanAgentsSpeed += agentIController.speedModule;

                    //update angle
                    frameMeanAgentsAngVar += Vector3.Angle(agentIController.goal - agentI.transform.position, agentIController.GetM());

                    //walk
                    agentIController.Walk(fixedStep);

                    //write the Rodolfo exit file
                    agentIController.SaveAgentsExitFile(lastFrameCount);

                    //verify agent position, in relation to the goal.
                    //if the distance between them is less than 1 (arbitrary, maybe authors have a better solution), he arrived. Destroy it so
                    float dist = Vector3.Distance(goal.transform.position, agentI.transform.position);
                    if (dist < agentIController.agentRadius)
                    {
                        //if we are already at the last agent goal, he arrived
                        //if he has 2 goals yet, but the second one is the Looking For, he arrived too
                        if (agentIController.go.Count == 1 ||
                            (agentIController.go.Count == 2 && agentIController.go[1].gameObject.tag == "LookingFor"))
                        {
                            //save on the file
                            filesController.SaveAgentsGoalFile(agentI.name, goal.name, lastFrameCount);
                            //destroy it
                            agentGroupI.agents.Remove(agentI);
                            Destroy(agentI);
                        }//else, he must go to the next goal. Remove this actual goal and this intention
                        else
                        {
                            //before we remove his actual go, we check if it is the looking for state.
                            //if it is, we remove it, but add a new one, because he doesnt know where to go yet
                            bool newLookingFor = false;

                            if (agentIController.go[0].gameObject.tag == "LookingFor")
                            {
                                newLookingFor = true;
                            }//else, it is a goal. Save on the file
                            else
                            {
                                //save on the file
                                filesController.SaveAgentsGoalFile(agentI.name, goal.name, lastFrameCount);
                            }

                            //remove it from the lists, for each agent of this group
                            foreach (GameObject agRemove in agentGroupI.agents)
                            {
                                //reset the agent path
                                agRemove.GetComponent<AgentController>().cornerPath.Clear();

                                //just remove if it is not a LF state
                                if (!newLookingFor)
                                {
                                    RemoveGoalFromList(agRemove.GetComponent<AgentController>());
                                }
                            }

                            if (newLookingFor)
                            {
                                //change the Looking For state, with a random position
                                GameObject lookingFor = GenerateLookingFor();

                                foreach (GameObject agRemove in agentGroupI.agents)
                                {
                                    //foreach goal of this agent
                                    foreach (GameObject thisLF in agRemove.GetComponent<AgentController>().go)
                                    {
                                        if (thisLF.tag == "LookingFor")
                                        {
                                            thisLF.transform.position = lookingFor.transform.position;
                                            break;
                                        }
                                    }

                                    //since we have a new one, reorder
                                    //agRemove.GetComponent<AgentController>().ReorderGoals();
                                }

                                //update in group too
                                foreach (GameObject groupLF in agentGroupI.GetComponent<AgentGroupController>().go)
                                {
                                    if (groupLF.tag == "LookingFor")
                                    {
                                        groupLF.transform.position = lookingFor.transform.position;
                                        break;
                                    }
                                }

                                //delete this temp LF
                                Destroy(lookingFor);
                            }
                        }
                    }
                }

                //update the distance between this agent and its group center
                agentIController.distanceToGroupCenter = Vector3.Distance(agentI.transform.position, agentIController.group.transform.position);
            }

            //if group behavior, check joining agents
            if (groupBehavior)
            {
                //now, we check if agents can be brought together again (agent joining an existent group)
                if (allGroups.Length > 1)
                {
                    //for each pair of groups
                    for (int group1 = 0; group1 < allGroups.Length; group1++)
                    {
                        AgentGroupController group1Controller = allGroups[group1].GetComponent<AgentGroupController>();
                        for (int group2 = 0; group2 < allGroups.Length; group2++)
                        //for (int group2 = group1+1; group2 < allGroups.Length; group2++)
                        {
                            AgentGroupController group2Controller = allGroups[group2].GetComponent<AgentGroupController>();
                            //if they are diferent groups and group2 has more or equal number of agents
                            if (group1 != group2 && group2Controller.agents.Count >= group1Controller.agents.Count)
                            {
                                //for each pair of agents in each group
                                for (int agent1 = 0; agent1 < group1Controller.agents.Count; agent1++)
                                {
                                    AgentController agent1Controller = group1Controller.agents[agent1].GetComponent<AgentController>();

                                    float dist = Vector3.Distance(group1Controller.agents[agent1].transform.position, allGroups[group2].transform.position) - (2 * auxinRadius);
                                    //if the distance is lower or equal than the larger group Hall AND lower than the distance to its actual group
                                    if (dist <= threshold && dist < agent1Controller.distanceToGroupCenter)
                                    {
                                        //if distance test is satisfied, need to see now if they are both with the same goals
                                        if (allGroups[group2].GetComponent<AgentGroupController>().agents.Count > 0)
                                        {
                                            if (agent1Controller.CheckGoals(allGroups[group2].GetComponent<AgentGroupController>().agents[0].GetComponent<AgentController>()))
                                            {
                                                //need to do this for all agents in the group1
                                                /*for(int z = 0; z < group1Controller.agents.Count; z++)
                                                {
                                                    ChangeGroupAgent(group1Controller.agents[z], allGroups[group2]);
                                                }*/
                                                /*foreach (GameObject ag in group1Controller.agents)
                                                {
                                                    ChangeGroupAgent(ag, allGroups[group2]);
                                                }*/
                                                ChangeGroupAgent(group1Controller.agents[agent1], allGroups[group2]);

                                                //update the distance between this agent and its new group center
                                                agent1Controller.distanceToGroupCenter = dist;

                                                //since it already changes for all agents in the group, break;
                                                //break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //update agents mean speed for frame...
            if (allAgents.Length > 0)
            {
                frameMeanAgentsSpeed /= (float)allAgents.Length;
                //... and the total
                meanAgentsSpeed = (meanAgentsSpeed + frameMeanAgentsSpeed) / 2;

                //angvar
                frameMeanAgentsAngVar /= (float)allAgents.Length;
                //... and the total
                meanAgentsAngVar = (meanAgentsAngVar + frameMeanAgentsAngVar) / 2;

                //update in the files too
                filesController.SaveMeanSpeedFile(lastFrameCount, frameMeanAgentsSpeed);
                filesController.SaveAngVarFile(lastFrameCount, frameMeanAgentsAngVar);
            }

            allGroups = GameObject.FindGameObjectsWithTag("AgentGroup");
            //update maxqnt of groups
            if(allGroups.Length > maxQntGroups)
            {
                maxQntGroups = allGroups.Length;
            }

            //after all agents walked, find the centroid of all agents to update center position of each group
            foreach (GameObject ag in allGroups)
            {
                //just if there are agents in the group yet
                if (ag.GetComponent<AgentGroupController>().agents.Count > 0)
                {
                    Vector3 center = Vector3.zero;
                    for (int j = 0; j < ag.GetComponent<AgentGroupController>().agents.Count; j++)
                    {
                        center = center + ag.GetComponent<AgentGroupController>().agents[j].transform.position;
                    }
                    Vector3 newPosition = center / (float)ag.GetComponent<AgentGroupController>().agents.Count;
                    ag.transform.position = newPosition;

                    //update its center
                    ag.GetComponent<AgentGroupController>().UpdateCell(cellRadius);
                }//else, destroy it
                else
                {
                    //destroy the LF too
                    foreach(GameObject lf in ag.GetComponent<AgentGroupController>().go)
                    {
                        if(lf.tag == "LookingFor")
                        {
                            Destroy(lf);
                            break;
                        }
                    }
                    Destroy(ag);
                }
            }

            //write the exit file
            //filesController.SaveExitFile(lastFrameCount);

            //update time
            qntFrames++;
            if(fixedStep == 0)
            {
                secondsTaken += Time.deltaTime;
            }
            else
            {
                secondsTaken += fixedStep;
            }

            //End simulation?
            if (loadConfigFile)
            {
                EndSimulation();
            }
            else
            {
                if (allAgents.Length == 0)
                {
                    gameOver = true;

                    filesController.SaveMetrics(Time.time, maxQntGroups);

                    Debug.Log("Simulation time (seconds): " + Time.time);
                    Debug.Log(secondsTaken + " seconds taken");
                    Debug.Log("Qnt Frames: " + qntFrames);
                    Debug.Log("Max qnt of groups: " + maxQntGroups);

                    //get a print (for the heatmap)
                    ScreenCapture.CaptureScreenshot(Application.dataPath + "/" + allSimulations + "/" + "HeatMap.png");
                }
            }
        }
    }

    //check if the raised cell is present in any path
    public void CheckRaisedCell(GameObject cellToCheck)
    {
        //update, just to be sure
        allAgents = GameObject.FindGameObjectsWithTag("Player");

        //for each agent
        foreach(GameObject ag in allAgents)
        {
            //agent controller
            AgentController ac = ag.GetComponent<AgentController>();

            //if has the cell, get the index
            int nodeIndex = -1;
            for (int i = 0; i < ac.fullPath.Count; i++)
            {
                if (ac.fullPath[i].cell.name == cellToCheck.name)
                {
                    nodeIndex = i;
                    break;
                }
            }

            //if found, check if it is close enough of the agent to affect the path
            bool closeEnough = false;
            if(nodeIndex > -1)
            {
                float distanceCellAg = Vector3.Distance(ag.transform.position, ac.fullPath[nodeIndex].cell.transform.position);
                if(distanceCellAg <= ac.fieldOfView * 2)
                {
                    closeEnough = true;
                }
            }
            
            //if found and it is close enough, check back and forward to find unchanged nodes
            if (nodeIndex > -1 && closeEnough)
            {
                NodeClass nodeBefore = new NodeClass();
                NodeClass nodeAfter = new NodeClass();

                if (nodeIndex > 0)
                {
                    for (int i = nodeIndex - 1; i >= 0; i--)
                    {
                        if (ac.fullPath[i].cell.name != "LookingFor")
                        {
                            //if higher is false, this one can be used
                            if (!ac.fullPath[i].cell.GetComponent<CellController>().higher)
                            {
                                nodeBefore = ac.fullPath[i];
                                break;
                            }
                        }
                    }
                }

                if (nodeIndex < ac.fullPath.Count - 1)
                {
                    for (int i = nodeIndex + 1; i < ac.fullPath.Count; i++)
                    {
                        //if higher is false, this one can be used
                        if (ac.fullPath[i].cell.name != "LookingFor")
                        {
                            if (!ac.fullPath[i].cell.GetComponent<CellController>().higher)
                            {
                                nodeAfter = ac.fullPath[i];
                                break;
                            }
                        }
                    }
                }

                //if found nodes
                if (nodeBefore.cell != null && nodeAfter.cell != null)
                {
                    //now, calculate the sub-path between nodeBefore and nodeAfter
                    List<List<NodeClass>> subPath = ac.paths.FindPath(nodeBefore.cell, nodeAfter.cell);
                    bool substitute = false;
                    //index for subpath
                    int j = 0;

                    //now, recreate the path including the new subPath
                    List<NodeClass> newPathD = new List<NodeClass>();
                    for (int i = 0; i < ac.fullPath.Count; i++)
                    {
                        if (substitute && j < subPath[1].Count)
                        {
                            newPathD.Add(subPath[1][j]);
                            i++; j++;

                            //if passed the size, break;
                            if (i >= ac.fullPath.Count) break;
                        }
                        else
                        {
                            newPathD.Add(ac.fullPath[i]);
                        }

                        //if it is the node before, need to mark to use the subpath
                        if (ac.fullPath[i].cell.name == nodeBefore.cell.name)
                        {
                            substitute = true;
                        }//else, if it is the node after, need to unmark to use subpath
                        else if (ac.fullPath[i].cell.name == nodeAfter.cell.name)
                        {
                            //since the last does not come back from path planning, add it
                            newPathD.Add(ac.fullPath[i]);

                            substitute = false;
                        }
                    }

                    //update the path
                    ac.fullPath = newPathD;

                    //reset the higher and lower values
                    foreach (NodeClass nd in ac.fullPath)
                    {
                        if (nd.cell.name != "LookingFor")
                        {
                            nd.cell.GetComponent<CellController>().higher = false;
                            nd.cell.GetComponent<CellController>().lower = false;
                        }
                    }

                    //update path corners
                    ac.cornerPath = ac.paths.FindPathCorners(ac.fullPath);

                    //update agent goal
                    ac.goal = ac.cornerPath[0].cell.transform.position;
                }
            }
        }
    }

    //check if the lowered cell is present in any original path
    public void CheckLoweredCell(GameObject cellToCheck)
    {
        //update, just to be sure
        allAgents = GameObject.FindGameObjectsWithTag("Player");

        //for each agent
        foreach (GameObject ag in allAgents)
        {
            //agent controller
            AgentController ac = ag.GetComponent<AgentController>();

            //if has the cell, get the index
            //the test should be against the original path, since the agent actual path may be different already
            int nodeIndex = -1;
            for (int i = 0; i < ac.originalPath.Count; i++)
            {
                if (ac.originalPath[i].cell.name == cellToCheck.name)
                {
                    nodeIndex = i;
                    break;
                }
            }

            //if found, check if it is close enough of the agent to affect the path
            bool closeEnough = false;
            if (nodeIndex > -1)
            {
                float distanceCellAg = Vector3.Distance(ag.transform.position, ac.originalPath[nodeIndex].cell.transform.position);
                if (distanceCellAg <= ac.fieldOfView * 2)
                {
                    closeEnough = true;
                }
            }

            //if found and close enough, check back and forward to find unchanged nodes
            if (nodeIndex > -1 && closeEnough)
            {
                NodeClass nodeBefore = new NodeClass();
                NodeClass nodeAfter = new NodeClass();

                if (nodeIndex > 0)
                {
                    for (int i = nodeIndex - 1; i >= 0; i--)
                    {
                        if (ac.originalPath[i].cell.name != "LookingFor")
                        {
                            //if lower is false, this one can be used
                            if (!ac.originalPath[i].cell.GetComponent<CellController>().lower)
                            {
                                nodeBefore = ac.originalPath[i];
                                break;
                            }
                        }
                    }
                }

                if (nodeIndex < ac.originalPath.Count - 1)
                {
                    for (int i = nodeIndex + 1; i < ac.originalPath.Count; i++)
                    {
                        //if lower is false, this one can be used
                        if (ac.originalPath[i].cell.name != "LookingFor")
                        {
                            if (!ac.originalPath[i].cell.GetComponent<CellController>().lower)
                            {
                                nodeAfter = ac.originalPath[i];
                                break;
                            }
                        }
                    }
                }

                //if found nodes
                if (nodeBefore.cell != null && nodeAfter.cell != null)
                {
                    //now, calculate the sub-path between nodeBefore and nodeAfter
                    List<List<NodeClass>> subPath = ac.paths.FindPath(nodeBefore.cell, nodeAfter.cell);
                    bool substitute = false;
                    //index for subpath
                    int j = 0;

                    //now, recreate the path including the new subPath
                    List<NodeClass> newPathD = new List<NodeClass>();
                    for (int i = 0; i < ac.originalPath.Count; i++)
                    {
                        if (substitute && j < subPath[1].Count)
                        {
                            newPathD.Add(subPath[1][j]);
                            i++; j++;
                        }
                        else
                        {
                            newPathD.Add(ac.originalPath[i]);
                        }

                        //if it is the node before, need to mark to use the subpath
                        if (ac.originalPath[i].cell.name == nodeBefore.cell.name)
                        {
                            substitute = true;
                        }//else, if it is the node after, need to unmark to use subpath
                        else if (ac.originalPath[i].cell.name == nodeAfter.cell.name)
                        {
                            //since the last does not come back from path planning, add it
                            newPathD.Add(ac.originalPath[i]);

                            substitute = false;
                        }
                    }

                    //update the path
                    ac.fullPath = newPathD;

                    //reset the higher and lower values
                    foreach (NodeClass nd in ac.fullPath)
                    {
                        if (nd.cell.name != "LookingFor")
                        {
                            nd.cell.GetComponent<CellController>().higher = false;
                            nd.cell.GetComponent<CellController>().lower = false;
                        }
                    }

                    //update path corners
                    ac.cornerPath = ac.paths.FindPathCorners(ac.fullPath);

                    //update agent goal
                    ac.goal = ac.cornerPath[0].cell.transform.position;
                }
            }
        }
    }

    //split agent
    public void SplitAgent(GameObject agentToSplit)
    {
        AgentController agentIController = agentToSplit.GetComponent<AgentController>();

        Debug.Log(agentToSplit.name + " left " + agentIController.group.name);
        //Debug.Break();
        agentIController.farAwayTimer = 0;

        //instantiate new group for this agent
        GameObject newAgentGroup = Instantiate(agentGroup, agentToSplit.transform.position, Quaternion.identity) as GameObject;
        AgentGroupController newAgentGroupController = newAgentGroup.GetComponent<AgentGroupController>();
        //Debug.Break();
        //change its name
        groupsNameCounter++;
        newAgentGroup.name = "agentGroup" + groupsNameCounter;
        //set its cell
        newAgentGroupController.cell = agentIController.cell;

        //set the same hofstede (if not using it, just get the zeroed one, so, no problem)
        newAgentGroupController.hofstede = agentIController.group.GetComponent<AgentGroupController>().hofstede;
        newAgentGroupController.SetCohesion(agentIController.group.GetComponent<AgentGroupController>().GetCohesion());
        newAgentGroupController.SetMeanSpeed(agentIController.group.GetComponent<AgentGroupController>().GetMeanSpeed());
        newAgentGroupController.SetMeanSpeedDeviation(agentIController.group.GetComponent<AgentGroupController>().GetMeanSpeedDeviation());
        newAgentGroupController.SetAngularVariarion(agentIController.group.GetComponent<AgentGroupController>().GetAngularVariarion());
        //Debug.Break();
        //set the same hall
        newAgentGroupController.hall = agentIController.group.GetComponent<AgentGroupController>().hall;

        //same agent group goals
        newAgentGroupController.go.AddRange(agentIController.go);
        newAgentGroupController.intentions.AddRange(agentIController.intentions);
        newAgentGroupController.desire.AddRange(agentIController.desire);
        //Debug.Break();
        //create a new looking for for this group
        for (int l = 0; l < newAgentGroupController.go.Count; l++)
        {
            if (newAgentGroupController.go[l].tag == "LookingFor")
            {
                //add the Looking For state, with a random position
                GameObject lookingFor = GenerateLookingFor();

                //keep the position
                lookingFor.transform.position = newAgentGroupController.go[l].transform.position;

                newAgentGroupController.go[l] = lookingFor;
                break;
            }
        }
        //Debug.Break();
        //change agent looking for too
        agentIController.go.Clear();
        agentIController.go.AddRange(newAgentGroupController.go);
        //Debug.Break();
        //set a new speed for it (plus 1 to be invalid and enter the while)
        float newSpeed = -1;

        //while speed is invalid
        while (newSpeed <= 0)//newSpeed > agent.GetComponent<AgentController>().maxSpeed || 
        {
            newSpeed = Random.Range(newAgentGroupController.GetMeanSpeed() - newAgentGroupController.GetMeanSpeedDeviation(),
                newAgentGroupController.GetMeanSpeed() + newAgentGroupController.GetMeanSpeedDeviation());
        }
        //set agent speed
        agentIController.maxSpeed = newSpeed;
        //Debug.Break();
        //remove it from the older group
        agentIController.group.GetComponent<AgentGroupController>().agents.Remove(agentToSplit);

        //add the agent in the new group
        agentIController.group = newAgentGroup;
        newAgentGroupController.agents.Add(agentToSplit);

        //recalculate its path (DONT HAVE THE WRITTEN PATH PLANNING HERE, USING UNITY ONE)
        //agentIController.CalculatePath(agentIController.go[agentIController.go.Count - 1]);
    }

    //draw cells
    public void DrawCells()
    {
        //if it is not set yet
        /*if (!terrain)
        {
            terrain = GameObject.Find("Terrain").GetComponent<Terrain>();
        }

        //reset the terrain size with the defined size
        terrain.terrainData.size = new Vector3(scenarioSizeX, terrain.terrainData.size.y, scenarioSizeZ);

        //get the cells parent
        GameObject cells = GameObject.Find("Cells");

        //first of all, create all cells (with this scene and this agentRadius)
        //since radius = 1; diameter = 2. So, iterate cellRadius*2
        //if the radius varies, this 2 operations adjust the cells
        Vector3 newPosition = new Vector3(cell.transform.position.x * cellRadius,
            cell.transform.position.y * cellRadius, cell.transform.position.z * cellRadius);
        Vector3 newScale = new Vector3(cell.transform.localScale.x * cellRadius,
            cell.transform.localScale.y * cellRadius, cell.transform.localScale.z * cellRadius);

        for (float i = 0; i < terrain.terrainData.size.x; i = i + cellRadius * 2)
        {
            for (float j = 0; j < terrain.terrainData.size.z; j = j + cellRadius * 2)
            {
                //verify if collides with some obstacle. We dont need cells in them.
                //for that, we need to check all 4 vertices of the cell. Otherwise, we may not have cells in some free spaces (for example, half of a cell would be covered by an obstacle, so that cell
                //would not be instantied)
                bool collideRight = CheckObstacle(new Vector3(newPosition.x + i + cellRadius, newPosition.y, newPosition.z + j), "Obstacle", 0.01f);
                bool collideLeft = CheckObstacle(new Vector3(newPosition.x + i - cellRadius, newPosition.y, newPosition.z + j), "Obstacle", 0.01f);
                bool collideTop = CheckObstacle(new Vector3(newPosition.x + i, newPosition.y, newPosition.z + j + cellRadius), "Obstacle", 0.01f);
                bool collideDown = CheckObstacle(new Vector3(newPosition.x + i, newPosition.y, newPosition.z + j - cellRadius), "Obstacle", 0.01f);
                bool collideRightTop = CheckObstacle(new Vector3(newPosition.x + i + cellRadius, newPosition.y, newPosition.z + j + cellRadius), "Obstacle", 0.01f);
                bool collideLeftBottom = CheckObstacle(new Vector3(newPosition.x + i - cellRadius, newPosition.y, newPosition.z + j - cellRadius), "Obstacle", 0.01f);
                bool collideTopLeft = CheckObstacle(new Vector3(newPosition.x + i - cellRadius, newPosition.y, newPosition.z + j + cellRadius), "Obstacle", 0.01f);
                bool collideDownRight = CheckObstacle(new Vector3(newPosition.x + i + cellRadius, newPosition.y, newPosition.z + j - cellRadius), "Obstacle", 0.01f);
                
                //if did collide it all, means we have found at least 1 obstacle in each case. So, the cell is covered by an obstacle
                //otherwise, we go on
                if (!collideRight || !collideLeft || !collideTop || !collideDown || !collideRightTop || !collideLeftBottom || !collideTopLeft || !collideDownRight)
                {
                    //instantiante a new cell
                    GameObject newCell = Instantiate(cell, new Vector3(newPosition.x + i, newPosition.y, newPosition.z + j), Quaternion.identity) as GameObject;
                    //change his name
                    newCell.name = "cell" + i + "-" + j;
                    //change scale
                    newCell.transform.localScale = newScale;
                    //change parent
                    newCell.transform.parent = cells.transform;
                    //start list
                    newCell.GetComponent<CellController>().StartList();
                }
            }
        }
        */

        //get rooms
        allRooms = GameObject.FindGameObjectsWithTag("Room");

        //foreach room, instantiate the cells
        foreach (GameObject room in allRooms)
        {
            room.GetComponent<RoomController>().StartLists();
            room.GetComponent<RoomController>().CreateTermicCells();
        }

        //create room doors
        foreach (GameObject room in allRooms)
        {
            room.GetComponent<RoomController>().CreateDoors();
        }

        //get all cells in scene
        allCells = GameObject.FindGameObjectsWithTag("Cell");
        //just to see how many cells were generated
        Debug.Log(allCells.Length);
    }

    //place auxins
    public void PlaceAuxins()
    {
        //get all cells in scene
        allCells = GameObject.FindGameObjectsWithTag("Cell");

        //lets set the qntAuxins for each cell according the density estimation
        float densityToQnt = PORC_QTD_Marcacoes;

        densityToQnt *= (cellRadius * 2f) / (2.0f * auxinRadius);
        densityToQnt *= (cellRadius * 2f) / (2.0f * auxinRadius);

        qntAuxins = (int)Mathf.Floor(densityToQnt);
        Debug.Log(qntAuxins);

        //for each cell, we generate his auxins
        for (int c = 0; c < allCells.Length; c++)
        {
            //Dart throwing auxins
            DartThrowMarkers(c);
            //Debug.Log(allCells[c].name+"--"+allCells[c].GetComponent<CellController>().GetAuxins().Count);
        }
    }

    //draw obstacles on the scene
    public void DrawObstacles()
    {
        //draw rectangle 1
        /*Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(5, 0, 5);
        vertices[1] = new Vector3(5, 0, 12);
        vertices[2] = new Vector3(12, 0, 12);
        vertices[3] = new Vector3(12, 0, 5);

        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 0;

        DrawObstacle(vertices, triangles);

        vertices = new Vector3[4];
        vertices[0] = new Vector3(18, 0, 18);
        vertices[1] = new Vector3(18, 0, 25);
        vertices[2] = new Vector3(25, 0, 25);
        vertices[3] = new Vector3(25, 0, 18);

        DrawObstacle(vertices, triangles);*/

        DrawObstacle(GameObject.CreatePrimitive(PrimitiveType.Plane), new Vector3(8.5f, 0, 8.5f), 0.75f);
        DrawObstacle(GameObject.CreatePrimitive(PrimitiveType.Plane), new Vector3(21.5f, 0, 21.5f), 0.75f);
    }
    
    //load the obstacle file
    public void LoadObstacles()
    {
        StreamReader theReader = new StreamReader(Application.dataPath + "/" + obstaclesFilename, System.Text.Encoding.Default);
        string line;
        int qntObstacles = 0;
        int qntVertices = 0;
        int qntTriangles = 0;
        Vector3[] vertices = new Vector3[qntVertices];
        int[] triangles = new int[qntTriangles];

        using (theReader)
        {
            int lineCount = 1;
            // While there's lines left in the text file, do this:
            do
            {
                line = theReader.ReadLine();

                if (line != null)
                {
                    //line 1 = qntObstacles
                    if(lineCount == 1)
                    {
                        string[] info = line.Split(':');
                        qntObstacles = System.Int32.Parse(info[1]);
                    }//else, if the line is "Obstacle", it is a new obstacle, so reset vertices
                    else if(line == "Obstacle")
                    {
                        //reset vertices
                        vertices = new Vector3[0];
                        triangles = new int[0];
                    }//else, if line contains "qntVertices", set it and start to read the vertices
                    else if (line.Contains("qntVertices"))
                    {
                        string[] info = line.Split(':');
                        qntVertices = System.Int32.Parse(info[1]);
                        vertices = new Vector3[qntVertices];
                    }//else, if line contains "qntTriangles", set it and start to read the triangles
                    else if (line.Contains("qntTriangles"))
                    {
                        string[] info = line.Split(':');
                        qntTriangles = System.Int32.Parse(info[1]);
                        triangles = new int[qntTriangles];
                    }//else, read the information
                    else
                    {
                        //if qntVertices is bigger than 0, reading vertices yet
                        if(qntVertices > 0)
                        {
                            string[] info = line.Split(';');
                            vertices[qntVertices - 1] = new Vector3(System.Convert.ToSingle(info[0]), System.Convert.ToSingle(info[1]), System.Convert.ToSingle(info[2]));

                            //decrement
                            qntVertices--;
                        }//else, we are already reading the triangles
                        else if (qntTriangles > 0)
                        {
                            triangles[qntTriangles - 1] = System.Int32.Parse(line);

                            //decrement
                            qntTriangles--;

                            //if reached 0, obstacle is ready to draw
                            if(qntTriangles == 0)
                            {
                                DrawObstacle(vertices, triangles);
                            }
                        }
                    }
                }

                lineCount++;
            }
            while (line != null);
        }
        //close file
        theReader.Close();
    }

    //load cells and auxins and obstacles and goals (static stuff)
    //this method is invoked on GameController Pre-Process, define on EditorController
    public void LoadCellsAuxins()
    {
        string line;

        //ReadOBJFile();
        LoadObstacles();

        // Create a new StreamReader, tell it which file to read and what encoding the file
        StreamReader theReader = new StreamReader(Application.dataPath + Path.DirectorySeparatorChar + configFilename, System.Text.Encoding.Default);

        //parents
        GameObject parentCells = GameObject.Find("Cells");

        int qntCells = 0;
        // Create a new StreamReader, tell it which file to read and what encoding the file
        theReader = new StreamReader(Path.Combine(Application.dataPath, configFilename), System.Text.Encoding.Default);
        Terrain terrain = GameObject.Find("Terrain").GetComponent<Terrain>();

        using (theReader)
        {
            int lineCount = 1;
            // While there's lines left in the text file, do this:
            do
            {
                line = theReader.ReadLine();

                if (line != null)
                {
                    //in first line, we have the terrain size
                    if (lineCount == 1)
                    {
                        string[] entries = line.Split(':');
                        entries = entries[1].Split(',');

                        scenarioSizeX = System.Convert.ToSingle(entries[0]);
                        scenarioSizeZ = System.Convert.ToSingle(entries[1]);

                        terrain.terrainData.size = new Vector3(scenarioSizeX, terrain.terrainData.size.y, scenarioSizeZ);
                    }
                    //in second line, we have the camera position
                    else if (lineCount == 2)
                    {
                        ConfigCamera();
                    }
                    //in the third line, we have the qntCells to instantiante
                    else if (lineCount == 3)
                    {
                        string[] entries = line.Split(':');
                        qntCells = System.Int32.Parse(entries[1]);
                    }
                    //else, if we are in the qntCells+4 line, we have the qntAuxins to instantiante
                    else if (lineCount == qntCells + 4)
                    {
                        string[] entries = line.Split(':');
                        qntAuxins = System.Int32.Parse(entries[1]);
                    }
                    else
                    {
                        //while we are til qntCells+3 line, we have cells. After that, we have auxins and then, agents
                        if (lineCount <= qntCells + 3)
                        {
                            string[] entries = line.Split(';');

                            if (entries.Length > 0)
                            {
                                GameObject newCell = Instantiate(cell, new Vector3(System.Convert.ToSingle(entries[1]), System.Convert.ToSingle(entries[2]), System.Convert.ToSingle(entries[3])),
                                    Quaternion.identity) as GameObject;
                                //change scale
                                newCell.transform.localScale *= System.Convert.ToSingle(entries[4]);
                                cellRadius = System.Convert.ToSingle(entries[4]);
                                //change his name
                                newCell.name = entries[0];
                                //change parent
                                newCell.transform.parent = parentCells.transform;
                            }
                        }
                        else if (lineCount <= qntCells + qntAuxins + 4)
                        {
                            string[] entries = line.Split(';');
                            if (entries.Length > 0)
                            {
                                //find his cell
                                GameObject hisCell = GameObject.Find(entries[5]);

                                AuxinController newAuxin = new AuxinController();
                                //change his name
                                newAuxin.name = entries[0];
                                //this auxin is from this cell
                                newAuxin.SetCell(hisCell);
                                //set position
                                newAuxin.position = new Vector3(System.Convert.ToSingle(entries[1]), System.Convert.ToSingle(entries[2]), System.Convert.ToSingle(entries[3]));
                                //alter auxinRadius
                                auxinRadius = System.Convert.ToSingle(entries[4]);
                                //add this auxin to this cell
                                hisCell.GetComponent<CellController>().AddAuxin(newAuxin);
                            }
                        }
                    }
                }
                lineCount++;
            }
            while (line != null);
            // Done reading, close the reader and return true to broadcast success    
            theReader.Close();
        }

        // Create a new StreamReader, tell it which file to read and what encoding the file
        //Goals file, with goals and their positions
        theReader = new StreamReader(Path.Combine(Application.dataPath, goalsFilename), System.Text.Encoding.Default);

        using (theReader)
        {
            int lineCount = 1;
            // While there's lines left in the text file, do this:
            do
            {
                line = theReader.ReadLine();

                if (line != null)
                {
                    //in first line, it is the qnt goals
                    if (lineCount == 1)
                    {
                        qntGoals = System.Int32.Parse(line);
                    }
                    else
                    {
                        //each line 1 agent, separated by " "
                        string[] entries = line.Split(';');

                        //goal position
                        Vector3 newPosition = new Vector3(System.Convert.ToSingle(entries[1]), goalP.transform.position.y, System.Convert.ToSingle(entries[2]));

                        //instantiante it
                        DrawGoal(entries[0], newPosition);
                    }
                }
                lineCount++;
            }
            while (line != null);
            // Done reading, close the reader and return true to broadcast success    
            theReader.Close();
        }
    }

    //Read the obstacle obj file
    public void ReadOBJFile()
    {
        StreamReader theReader = new StreamReader(Application.dataPath + "/" + obstaclesFilename, System.Text.Encoding.Default);
        string line;
        int qntVertices = 0;
        int qntTriangles = 0;
        Vector3[] vertices = new Vector3[qntVertices];
        int[] triangles = new int[qntTriangles];
        int controlVertice = 0;
        int controlTriangle = 0;

        using (theReader)
        {
            int lineCount = 1;
            // While there's lines left in the text file, do this:
            do
            {
                line = theReader.ReadLine();

                if (line != null)
                {
                    //if it contains #, it is a comment
                    if (line.Contains("#"))
                    {
                        if (line.Contains("vertices"))
                        {
                            string[] info = line.Split(' ');
                            qntVertices = System.Int32.Parse(info[1]);
                            vertices = new Vector3[qntVertices];
                        }
                        if (line.Contains("facets"))
                        {
                            string[] info = line.Split(' ');
                            qntTriangles = System.Int32.Parse(info[1]);
                            triangles = new int[qntTriangles * 3];
                        }
                    }
                    else if (line != "")
                    {
                        string[] entries = line.Split(' ');
                        //if it starts with v, it is vertice. else, if it starts with f, it is facet which form a triangle (hopefully!)
                        if (entries[0] == "v")
                        {
                            vertices[controlVertice] = new Vector3(System.Convert.ToSingle(entries[1]), System.Convert.ToSingle(entries[2]), System.Convert.ToSingle(entries[3]));
                            controlVertice++;
                        }
                        else if (entries[0] == "f")
                        {
                            triangles[controlTriangle] = System.Int32.Parse(entries[2]) - 1;
                            controlTriangle++;

                            triangles[controlTriangle] = System.Int32.Parse(entries[3]) - 1;
                            controlTriangle++;

                            triangles[controlTriangle] = System.Int32.Parse(entries[4]) - 1;
                            controlTriangle++;
                        }
                    }
                }

                lineCount++;
            }
            while (line != null);
        }
        //close file
        theReader.Close();

        DrawObstacle(vertices, triangles);
    }

    //save the new OBJ file
    public void SaveOBJFile()
    {
        //get all obstacles (which should already be on the scene, along cells, goals, signs and markers)
        allObstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        Vector3[] vertices = allObstacles[0].GetComponent<MeshFilter>().sharedMesh.vertices;
        int[] triangles = allObstacles[0].GetComponent<MeshFilter>().sharedMesh.triangles;
        float scale = allObstacles[0].transform.localScale.x;

        StreamWriter obsFile = new StreamWriter(Application.dataPath + "/Obstacles.obj");
        obsFile.WriteLine("# file written from Unity BioCrowds simulation tool in Wavefront obj format");
        obsFile.WriteLine("# "+ vertices.Length + " vertices");
        obsFile.WriteLine("# 1010 halfedges (i dont actually have this info, so, just copied from original)");
        obsFile.WriteLine("# " + (triangles.Length/3) + " facets");

        obsFile.WriteLine("");

        obsFile.WriteLine("# " + vertices.Length + " vertices");
        obsFile.WriteLine("# ------------------------------------------");

        obsFile.WriteLine("");

        foreach (Vector3 vert in vertices)
        {
            obsFile.WriteLine("v " + (vert.x*scale) + " " + (vert.y*scale) + " " + (vert.z*scale));
        }

        obsFile.WriteLine("");

        obsFile.WriteLine("# " + (triangles.Length / 3) + " facets");
        obsFile.WriteLine("# ------------------------------------------");

        obsFile.WriteLine("");

        for(int i = 0; i < triangles.Length; i = i + 3)
        {
            obsFile.WriteLine("f  "+ (triangles[i]+1) + " " + (triangles[i+1]+1) + " " + (triangles[i+2]+1) + "");
        }

        obsFile.WriteLine("");

        obsFile.WriteLine("# End of Wavefront obj format #");

        obsFile.Close();
    }

    //clear the scenario
    public void ClearScene()
    {
        //clear goals
        GameObject[] goalsToClear = GameObject.FindGameObjectsWithTag("Goal");
        foreach(GameObject gtc in goalsToClear)
        {
            //DestroyImmediate(gtc);
        }

        //clear signs
        GameObject[] signsToClear = GameObject.FindGameObjectsWithTag("Sign");
        foreach (GameObject stc in signsToClear)
        {
            //DestroyImmediate(stc);
        }

        //clear cells
        GameObject[] cellsToClear = GameObject.FindGameObjectsWithTag("Cell");
        foreach (GameObject ctc in cellsToClear)
        {
            DestroyImmediate(ctc);
        }

        //clear obstacles
        GameObject[] obstaclesToClear = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject otc in obstaclesToClear)
        {
            DestroyImmediate(otc);
        }
    }

    //draw some goals
    public void DrawGoals()
    {
        //instantiate goals and their signs
        DrawGoal("Goal1", new Vector3(15, 0, 15));
        DrawGoal("Goal2", new Vector3(5, 0, 28));
        DrawGoal("Goal3", new Vector3(2, 0, 2));
        DrawGoal("Goal4", new Vector3(25, 0, 1));
    }

    //fix the FD files
    public void FixFDFile()
    {
        //read from this
        StreamReader sr = new StreamReader(Application.dataPath + "/" + fDFileToFix, System.Text.Encoding.Default);
        //write on this
        StreamWriter sw = File.CreateText(Application.dataPath + "/newRodolfoFile.txt");
        string line;

        using (sr)
        {
            int agentCount = 0;
            // While there's lines left in the text file, do this:
            do
            {
                line = sr.ReadLine();

                if (line != null && line != "")
                {
                    string[] info = line.Split(' ');
                    //if size = 1, it is the name. Change it to P-number
                    if (info.Length == 1)
                    {
                        sw.WriteLine("");
                        sw.WriteLine("P-" + agentCount);
                        agentCount++;
                    }//else, it is the information
                    else
                    {
                        //if it is above the frame to consider, write
                        int frameCons = System.Int32.Parse(info[0]);
                        if (frameCons >= startingFrameToFix)
                        {
                            sw.WriteLine(line);
                        }
                    }
                }//else, if the line is "Obstacle", it is a new obstacle, so reset vertices
            }
            while (line != null);
        }
        //close file
        sr.Close();
        sw.Close();
    }

    //calculate the metrics
    public void CalculateMetrics()
    {
        //agents mean speed
        //read the mean speed file
        StreamReader theReader = new StreamReader(Application.dataPath + "/" + meanSpeedFilename, System.Text.Encoding.Default);
        string line;
        int qntFrames = 0;
        float sumSpeed = 0;

        //store all values to calculate variance
        List<float> allSpeeds = new List<float>();

        using (theReader)
        {
            // While there's lines left in the text file, do this:
            do
            {
                line = theReader.ReadLine();

                if (line != "" && line != null)
                {
                    string[] info = line.Split(';');
                    qntFrames = System.Int32.Parse(info[0]);
                    sumSpeed += System.Convert.ToSingle(info[1]);

                    allSpeeds.Add(System.Convert.ToSingle(info[1]));
                }
            }
            while (line != null);
        }
        //close file
        theReader.Close();
        float averageSpeed = sumSpeed / qntFrames;
        Debug.Log("Agents mean speed: " + (averageSpeed));

        //variance
        float variance = 0;
        foreach(float value in allSpeeds)
        {
            variance += Mathf.Pow(value - averageSpeed, 2);
        }
        variance /= qntFrames;
        //default deviation
        float defaultDeviation = Mathf.Abs(Mathf.Sqrt(variance));
        Debug.Log("Agents default deviation speed: " + (defaultDeviation));

        //agents ang var
        //read the ang var file
        theReader = new StreamReader(Application.dataPath + "/" + meanAngVarFilename, System.Text.Encoding.Default);
        qntFrames = 0;
        float sumAngVar = 0;

        //store all values to calculate variance
        List<float> allAngs = new List<float>();

        using (theReader)
        {
            // While there's lines left in the text file, do this:
            do
            {
                line = theReader.ReadLine();

                if (line != "" && line != null)
                {
                    string[] info = line.Split(';');
                    qntFrames = System.Int32.Parse(info[0]);
                    sumAngVar += System.Convert.ToSingle(info[1]);

                    allAngs.Add(System.Convert.ToSingle(info[1]));
                }
            }
            while (line != null);
        }
        //close file
        theReader.Close();
        float averageAngVar = sumAngVar / qntFrames;
        Debug.Log("Agents Mean Ang Var: " + averageAngVar);

        //variance
        variance = 0;
        foreach (float value in allAngs)
        {
            variance += Mathf.Pow(value - averageAngVar, 2);
        }
        variance /= qntFrames;
        //default deviation
        defaultDeviation = Mathf.Abs(Mathf.Sqrt(variance));
        Debug.Log("Agents default deviation AngVar: " + (defaultDeviation));

        //agents mean distances for each group
        //get the actual directory
        string[] entry = meanSpeedFilename.Split('/');
        string actualDir = "";
        for(int i = 0; i < entry.Length - 1; i++)
        {
            actualDir = actualDir + entry[i] + "/";
        }

        //find the files
        DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath + "/" + actualDir);
        FileInfo[] fileInfo = dirInfo.GetFiles("agentGroup???.csv");

        float totalMeanDistance = 0;
        int totalGroupsWithDistances = 0;

        //for each group, calculate the mean distance
        foreach (FileInfo file in fileInfo)
        {
            //open the file
            theReader = new StreamReader(file.ToString(), System.Text.Encoding.Default);
            float sumDistances = 0;
            float sumSpeeds = 0;
            float sumAngVars = 0;
            string groupName = "";
            qntFrames = 0;
            //qnt lines with info of the same frame
            //order: distance, speed, angvar
            int frameLine = 1;

            //store all values to calculate variance
            List<float> allDistances = new List<float>();

            using (theReader)
            {
                // While there's lines left in the text file, do this:
                do
                {
                    line = theReader.ReadLine();

                    if (line != "" && line != null)
                    {
                        if (line.Contains("agentGroup"))
                        {
                            groupName = line;
                        }
                        else
                        {
                            string[] info = line.Split(';');

                            //frameLine = 1 -> distance
                            if (frameLine == 1)
                            {
                                sumDistances += System.Convert.ToSingle(info[1]);

                                allDistances.Add(System.Convert.ToSingle(info[1]));
                            }//frameLine = 2 -> speed
                            else if (frameLine == 2)
                            {
                                sumSpeeds += System.Convert.ToSingle(info[1]);
                            }//frameLine = 3 -> angVar
                            else if (frameLine == 3)
                            {
                                sumAngVars += System.Convert.ToSingle(info[1]);
                            }

                            //update frameline
                            frameLine++;
                            //if is higher than 3, reset
                            if (frameLine > 3)
                            {
                                frameLine = 1;
                            }

                            //update qntFrames
                            qntFrames = System.Int32.Parse(info[0]);
                        }
                    }
                }
                while (line != null);
            }
            //close file
            theReader.Close();

            //show it
            if (qntFrames == 0)
            {
                Debug.Log(groupName + ": mean distance between agents = 0");
            }
            else
            {
                float averageDistance = sumDistances / qntFrames;
                float averageSpeeds = sumSpeeds / qntFrames;
                float averageAngVars = sumAngVars / qntFrames;
                Debug.Log(groupName + ": mean distance between agents = " + averageDistance);
                Debug.Log(groupName + ": mean group speed = " + averageSpeeds);
                Debug.Log(groupName + ": mean group ang var = " + averageAngVars);

                //variance
                variance = 0;
                foreach (float value in allDistances)
                {
                    variance += Mathf.Pow(value - averageDistance, 2);
                }
                variance /= qntFrames;
                //default deviation
                defaultDeviation = Mathf.Abs(Mathf.Sqrt(variance));
                Debug.Log("Agents default deviation Distance: " + (defaultDeviation));

                totalMeanDistance += averageDistance;
                totalGroupsWithDistances++;
            }
        }

        Debug.Log("Total mean distance between agents = " + (totalMeanDistance / totalGroupsWithDistances));
    }

    //calculate all the possible paths from each cell
    /*public void CalculateAllPaths()
    {
        //just to be sure
        allCells = GameObject.FindGameObjectsWithTag("Cell");

        PathPlanningClass paths = new PathPlanningClass(allCells.Length * 10);

        for(int i = 0; i < allCells.Length; i++)
        {
            for(int j = i+1; j < allCells.Length; j++)
            {
                //Debug.Log("From: " + allCells[i] + " to " + allCells[j]);

                List<GameObject> path = paths.FindPath(allCells[i], allCells[j]);

                if (path.Count > 0)
                {
                    //Debug.DrawLine(allCells[i].transform.position, path[0].transform.position, Color.red);

                    //add in this cell
                    allCells[i].GetComponent<CellController>().SetPath(allCells[j].name, path);
                }

                //and vice versa
                path = paths.FindPath(allCells[j], allCells[i]);

                if (path.Count > 0)
                {
                    //Debug.DrawLine(allCells[i].transform.position, path[0].transform.position, Color.red);

                    //add in this cell
                    allCells[j].GetComponent<CellController>().SetPath(allCells[i].name, path);
                }
            }
        }

        //save a file with the paths
        /*StreamWriter pathsFile = File.CreateText(Application.dataPath + "/" + allSimulations + "/Paths.txt");
        foreach (GameObject cl in allCells)
        {
            string wrtFile = cl.name + ":";

            
            //Debug.Log(cl.name + ": " + (cl.GetComponent<CellController>().GetCellPathByName("cell15-15").Count));
        }
        pathsFile.Close();*
    }*/

    //change group agent
    private void ChangeGroupAgent(GameObject agentToChange, GameObject newGroup)
    {
        AgentController agent1Controller = agentToChange.GetComponent<AgentController>();
        AgentGroupController group1Controller = agentToChange.GetComponent<AgentController>().group.GetComponent<AgentGroupController>();
        AgentGroupController group2Controller = newGroup.GetComponent<AgentGroupController>();

        Debug.Log(agentToChange.name + " joined " + newGroup.name);

        //find the newGroup LF
        GameObject newLF = group2Controller.go[0];
        foreach(GameObject lfs in group2Controller.go)
        {
            if(lfs.tag == "LookingFor")
            {
                newLF = lfs;
                break;
            }
        }

        //change agent looking for
        for (int g = 0; g < agent1Controller.go.Count; g++)
        {
            if (agent1Controller.go[g].tag == "LookingFor")
            {
                GameObject lfToDestroy = agent1Controller.go[g];
                agent1Controller.go[g] = newLF;
                break;
            }
        }

        //set a new speed for it (plus 1 to be invalid and enter the while)
        float newSpeed = -1;

        //while speed is invalid
        while (newSpeed <= 0)//newSpeed > agent.GetComponent<AgentController>().maxSpeed || 
        {
            newSpeed = Random.Range(group2Controller.GetMeanSpeed() - group2Controller.GetMeanSpeedDeviation(),
                group2Controller.GetMeanSpeed() + group2Controller.GetMeanSpeedDeviation());
        }
        //set agent speed
        agent1Controller.maxSpeed = newSpeed;

        //add the agent in the new group
        agent1Controller.group = newGroup;
        group2Controller.agents.Add(agentToChange);

        //remove it from the older group
        group1Controller.agents.Remove(agentToChange);

        //if this group is empty, remove
        if(group1Controller.agents.Count <= 0)
        {
            //remove its LF too
            foreach (GameObject lfs in group1Controller.go)
            {
                if (lfs.tag == "LookingFor")
                {
                    Destroy(lfs);
                    break;
                }
            }

            Destroy(group1Controller.gameObject);
        }
    }

    //remove goal from list
    private void RemoveGoalFromList(AgentController agentToRemove)
    {
        agentToRemove.go.RemoveAt(0);
        agentToRemove.intentions.RemoveAt(0);
        agentToRemove.RemoveDesire(0);

        //update group list
        agentToRemove.group.GetComponent<AgentGroupController>().go.Clear();
        agentToRemove.group.GetComponent<AgentGroupController>().intentions.Clear();
        agentToRemove.group.GetComponent<AgentGroupController>().desire.Clear();

        agentToRemove.group.GetComponent<AgentGroupController>().go.AddRange(agentToRemove.go);
        agentToRemove.group.GetComponent<AgentGroupController>().intentions.AddRange(agentToRemove.intentions);
        agentToRemove.group.GetComponent<AgentGroupController>().desire.AddRange(agentToRemove.desire);
    }

    //create groups and agents
    private void CreateGroupAgents(string cellName, int qntAgentsInGroup, List<GameObject> groupGoals, List<float> groupIntentions, 
        int[] hofvalues, List<float[]> duruvalues, List<float[]> favavalues, bool agentsIdle)
    {
        float x = 0, z = 0;
        GameObject[] agentsGroups = GameObject.FindGameObjectsWithTag("AgentGroup");
        GameObject[] allGoals = GameObject.FindGameObjectsWithTag("Goal");

        //sort out a cell
        GameObject foundCell = GameObject.Find(cellName);

        bool pCollider = true;
        //while collider inside obstacle or player
        while (pCollider)
        {
            //generate random position
            x = Random.Range(foundCell.transform.position.x - cellRadius, foundCell.transform.position.x + cellRadius);
            z = Random.Range(foundCell.transform.position.z - cellRadius, foundCell.transform.position.z + cellRadius);

            //check if there is any obstacle
            pCollider = CheckObstacle(new Vector3(x, 0, z), "Obstacle", 0.1f);
        }

        //instantiate
        GameObject newAgentGroup = Instantiate(agentGroup, new Vector3(x, 0f, z), Quaternion.identity) as GameObject;
        AgentGroupController newAgentGroupController = newAgentGroup.GetComponent<AgentGroupController>();
        //change its name
        groupsNameCounter++;
        newAgentGroup.name = "agentGroup" + groupsNameCounter;
        //set its cell
        newAgentGroupController.cell = foundCell;
        //default mean speed
        newAgentGroupController.SetMeanSpeed(defaultMeanSpeed);
        //default cohesion
        newAgentGroupController.SetCohesion(defaultCohesion);
        //default speed deviation
        newAgentGroupController.SetMeanSpeedDeviation(defaultMeanSpeedDeviation);
        //default angular variarion
        newAgentGroupController.SetAngularVariarion(defaultAngularVariation);

        //if using hofstede
        if (useHofstede)
        {
            //calculate the group hofstede
            newAgentGroupController.hofstede.CalculateHofstede(hofvalues[0], hofvalues[1], hofvalues[2], hofvalues[3], hofvalues[4]);
            //set group cohesion
            newAgentGroupController.SetCohesion(newAgentGroupController.hofstede.GetMeanCohesion());
            //once we have the hofstede calculated values, we can use the meanSpeed as a mean velocity for the group and calculate the meanSpeedDeviation according the cohesion
            //the more cohesion, the less deviation (between 0 and 0.2)
            newAgentGroupController.hofstede.SetMeanSpeedDeviation((3 - newAgentGroupController.GetCohesion()) / 15);
            //set group speed deviation
            newAgentGroupController.SetMeanSpeedDeviation(newAgentGroupController.hofstede.GetMeanSpeedDeviation());
            //set group mean speed
            newAgentGroupController.SetMeanSpeed(newAgentGroupController.hofstede.GetMeanSpeed());
            //the mean angular variation calculated based on HCD is a percentage with the max of 360 degrees.
            //so, calculate the angle based on this value. Max angle: 90 degrees
            newAgentGroupController.SetAngularVariarion(newAgentGroupController.hofstede.GetMeanAngVar() * 90.0f);
        }
        //durupinar is for each agent, so do it later

        //agent group goals
        for (int j = 0; j < groupGoals.Count; j++)
        {
            newAgentGroupController.go.Add(groupGoals[j]);
            //add a random intention
            newAgentGroupController.intentions.Add(groupIntentions[j]);
            //add a random desire
            newAgentGroupController.desire.Add(Random.Range(0f, 1f));
        }

        //add the Looking For state, with a random position
        if (exploratoryBehavior)
        {
            GameObject lookingFor = GenerateLookingFor();

            //if staticLookingFor is different than zero, it was set by the InitialValues file. So, use it
            if (staticLookingFor != Vector3.zero)
            {
                lookingFor.transform.position = staticLookingFor;
            }

            newAgentGroupController.go.Add(lookingFor);
            newAgentGroupController.intentions.Add(intentionThreshold);
            newAgentGroupController.desire.Add(1);
        }
        
        //reorder following intentions
        newAgentGroupController.ReorderGoals();

        //get the parent Agents
        GameObject agents = GameObject.Find("Agents");

        //instantiate qntAgents Agents
        for (int i = 0; i < qntAgentsInGroup; i++)
        {
            //generate the agent position, based on group cell
            x = Random.Range(newAgentGroupController.cell.transform.position.x - cellRadius, newAgentGroupController.cell.transform.position.x + cellRadius);
            z = Random.Range(newAgentGroupController.cell.transform.position.z - cellRadius, newAgentGroupController.cell.transform.position.z + cellRadius);

            //see if there are agents in this radius. if not, instantiante
            pCollider = CheckObstacle(new Vector3(x, 0, z), "Player", 0.1f);

            //even so, if we are an obstacle, cannot instantiate either
            //just need to check for obstacle if found no player, otherwise it will not be instantiated anyway
            if (!pCollider)
            {
                pCollider = CheckObstacle(new Vector3(x, 0, z), "Obstacle", 0.1f);
            }

            //if found a player in the radius, do not instantiante. try again
            if (pCollider)
            {
                //try again
                i--;
                continue;
            }
            else
            {
                GameObject newAgent = Instantiate(agent, new Vector3(x, 0f, z), Quaternion.identity) as GameObject;
                AgentController newAgentController = newAgent.GetComponent<AgentController>();
                //change his name
                newAgent.name = "agent" + controlQntAgents;
                //open file
                newAgent.GetComponent<AgentController>().OpenFile();
                //random agent color
                newAgentController.SetColor(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));
                //agent cell
                newAgentController.SetCell(newAgentGroupController.cell);
                //agent radius
                newAgentController.agentRadius = agentRadius;
                //is idle?
                newAgentController.isIdle = agentsIdle;

                newAgent.GetComponent<MeshRenderer>().material.color = newAgentController.GetColor();

                //group max speed
                newAgentController.maxSpeed = newAgentGroupController.GetMeanSpeed();

                //the agent maxSpeed will not be the defined maxSpeed, but the calculated meanSpeed of the group with a variation in the meanSpeedDeviation, defined by the group cohesion
                float newSpeed = -1;
                //while speed is invalid
                while (newSpeed <= 0)//newSpeed > agent.GetComponent<AgentController>().maxSpeed || 
                {
                    newSpeed = Random.Range(newAgentGroupController.GetMeanSpeed() - newAgentGroupController.GetMeanSpeedDeviation(),
                        newAgentGroupController.GetMeanSpeed() + newAgentGroupController.GetMeanSpeedDeviation());
                }
                //set agent speed
                newAgentController.maxSpeed = newSpeed;

                //agent goals from group
                newAgentController.go.AddRange(newAgentGroupController.go);
                newAgentController.intentions.AddRange(newAgentGroupController.intentions);
                newAgentController.desire.AddRange(newAgentGroupController.desire);

                newAgentController.group = newAgentGroup;
                newAgentGroupController.agents.Add(newAgent);

                //agent durupinar
                if (useDurupinar)
                {
                    newAgentController.durupinar.CalculateDurupinar(duruvalues[i][0], duruvalues[i][1], duruvalues[i][2], duruvalues[i][3], duruvalues[i][4]);
                }
                else if (useFavaretto)
                {
                    newAgentController.favaretto.CalculateFavaretto(favavalues[i][0], favavalues[i][1], favavalues[i][2], favavalues[i][3], favavalues[i][4]);
                }

                //change PPD bias according selected option
                float ppdBias = 0;
                if (thermalComfort)
                {
                    ppdBias = 1;
                }
                if (thermalComfort && densityComfort)
                {
                    ppdBias = 0.5f;
                }
                newAgentController.ppdBias = ppdBias;

                //change parent
                newAgent.transform.parent = agents.transform;

                controlQntAgents++;
            }
        }
    }

    //load a csv config file
    private void LoadConfigFile()
    {
        string line;
        StreamReader theReader;

        //list with all hofstede groups
        List<int[]> groupHof = new List<int[]>();
        //list with all Durupinar groups
        List<List<float[]>> groupDuru = new List<List<float[]>>();
        //list with all Favaretto groups
        List<List<float[]>> groupFava = new List<List<float[]>>();

        //Hofstede file, with Hofstede values for each group
        //just used if useHofstede = true
        if (useHofstede)
        {
            theReader = new StreamReader(hofstedeFilename, System.Text.Encoding.Default);

            using (theReader)
            {
                int lineCount = 1;
                // While there's lines left in the text file, do this:
                do
                {
                    line = theReader.ReadLine();

                    if (line != null && line != "")
                    {
                        //if line starts with #, ignore
                        if (line[0] == '#')
                        {
                            continue;
                        }

                        //each line 1 agent, separated by ";"
                        string[] entries = line.Split(';');

                        int[] hofvalues = new int[5];
                        hofvalues[0] = System.Int32.Parse(entries[0]);
                        hofvalues[1] = System.Int32.Parse(entries[1]);
                        hofvalues[2] = System.Int32.Parse(entries[2]);
                        hofvalues[3] = System.Int32.Parse(entries[3]);
                        hofvalues[4] = System.Int32.Parse(entries[4]);

                        groupHof.Add(hofvalues);
                    }
                    lineCount++;
                }
                while (line != null);
                // Done reading, close the reader and return true to broadcast success    
                theReader.Close();
            }
        }

        //Durupinar file, with OCEAN values for each group
        //just used if useDurupinar = true
        if (useDurupinar)
        {
            theReader = new StreamReader(durupinarFilename, System.Text.Encoding.Default);

            using (theReader)
            {
                int lineCount = 1;
                // While there's lines left in the text file, do this:
                do
                {
                    line = theReader.ReadLine();

                    if (line != null && line != "")
                    {
                        //if line starts with #, ignore
                        if (line[0] == '#')
                        {
                            continue;
                        }

                        //each line 1 agent, separated by ";"
                        string[] entries = line.Split(';');

                        //if size = 1, new group
                        if (entries.Length == 1)
                        {
                            groupDuru.Add(new List<float[]>());
                        }
                        else
                        {
                            //values for each agent
                            float[] duruvalues = new float[5];
                            duruvalues[0] = System.Convert.ToSingle(entries[0]);
                            duruvalues[1] = System.Convert.ToSingle(entries[1]);
                            duruvalues[2] = System.Convert.ToSingle(entries[2]);
                            duruvalues[3] = System.Convert.ToSingle(entries[3]);
                            duruvalues[4] = System.Convert.ToSingle(entries[4]);

                            groupDuru[groupDuru.Count - 1].Add(duruvalues);
                        }
                    }
                    lineCount++;
                }
                while (line != null);
                // Done reading, close the reader and return true to broadcast success    
                theReader.Close();
            }
        }

        //Favaretto file, with OCEAN values for each group
        //just used if useFavaretto = true
        if (useFavaretto)
        {
            theReader = new StreamReader(favarettoFilename, System.Text.Encoding.Default);

            using (theReader)
            {
                int lineCount = 1;
                // While there's lines left in the text file, do this:
                do
                {
                    line = theReader.ReadLine();

                    if (line != null && line != "")
                    {
                        //if line starts with #, ignore
                        if (line[0] == '#')
                        {
                            continue;
                        }

                        //each line 1 agent, separated by ";"
                        string[] entries = line.Split(';');

                        //if size = 1, new group
                        if (entries.Length == 1)
                        {
                            groupFava.Add(new List<float[]>());
                        }
                        else
                        {
                            //values for each agent
                            float[] favavalues = new float[5];
                            favavalues[0] = System.Convert.ToSingle(entries[0]);
                            favavalues[1] = System.Convert.ToSingle(entries[1]);
                            favavalues[2] = System.Convert.ToSingle(entries[2]);
                            favavalues[3] = System.Convert.ToSingle(entries[3]);
                            favavalues[4] = System.Convert.ToSingle(entries[4]);

                            groupFava[groupFava.Count - 1].Add(favavalues);
                        }
                    }
                    lineCount++;
                }
                while (line != null);
                // Done reading, close the reader and return true to broadcast success    
                theReader.Close();
            }
        }

        //parents
        GameObject parentAgents = GameObject.Find("Agents");

        // Create a new StreamReader, tell it which file to read and what encoding the file
        //schedule file, with agents and their schedules
        theReader = new StreamReader(scheduleFilename, System.Text.Encoding.Default);

        int qntAgentsPerGroup = 0;
        qntAgents = 0;
        qntGroups = 0;

        using (theReader)
        {
            int lineCount = 1;
            // While there's lines left in the text file, do this:
            do
            {
                line = theReader.ReadLine();

                if (line != null && line != "")
                {
                    //if line starts with #, ignore
                    if(line[0] == '#')
                    {
                        continue;
                    }

                    //each line 1 group, separated by ";"
                    string[] entries = line.Split(';');

                    //if entries just have 1, it is the qnt agents
                    if (entries.Length == 1)
                    {
                        qntAgentsPerGroup = System.Int32.Parse(line);
                        qntAgents += qntAgentsPerGroup;
                    }
                    else
                    {
                        //find the cell informed
                        GameObject chosenCell = GameObject.Find(entries[0]);

                        //are agents idle?
                        bool areAgentsIdle = false;
                        if(entries[1] == "true")
                        {
                            areAgentsIdle = true;
                        }

                        List<GameObject> groupGoals = new List<GameObject>();
                        List<float> groupIntentions = new List<float>();

                        //set his goals
                        //first and second information in the file are just the coordinates, which i already have in config file
                        //go 2 in 2, since it is a pair between goal and intention to that goal
                        for (int j = 2; j < entries.Length; j = j + 2)
                        {
                            //there is an empty space on the end of the line, dont know why.
                            if (entries[j] == "") continue;

                            //try to find this goal object
                            GameObject goalFound = GameObject.Find(entries[j]);

                            if (goalFound)
                            {
                                groupGoals.Add(goalFound);
                                groupIntentions.Add(System.Convert.ToSingle(entries[j + 1]));
                            }
                        }

                        //create the group
                        List<float[]> ooze = new List<float[]>();
                        if (useHofstede)
                        {
                            CreateGroupAgents(chosenCell.name, qntAgentsPerGroup, groupGoals, groupIntentions,
                                groupHof[qntGroups], ooze, ooze, areAgentsIdle);
                        }
                        else if (useDurupinar)
                        {
                            CreateGroupAgents(chosenCell.name, qntAgentsPerGroup, groupGoals, groupIntentions,
                                new int[4] { 0, 0, 0, 0 }, groupDuru[qntGroups], ooze, areAgentsIdle);
                        }
                        else if (useFavaretto)
                        {
                            CreateGroupAgents(chosenCell.name, qntAgentsPerGroup, groupGoals, groupIntentions,
                                new int[4] { 0, 0, 0, 0 }, ooze, groupFava[qntGroups], areAgentsIdle);
                        }
                        else
                        {
                            CreateGroupAgents(chosenCell.name, qntAgentsPerGroup, groupGoals, groupIntentions,
                                new int[4] { 0, 0, 0, 0 }, ooze, ooze, areAgentsIdle);
                        }

                        //increment
                        qntGroups++;
                    }
                }
                lineCount++;
            }
            while (line != null);
            // Done reading, close the reader and return true to broadcast success    
            theReader.Close();
        }

        // Create a new StreamReader, tell it which file to read and what encoding the file
        //signs file, with signs and their appeals
        theReader = new StreamReader(signsFilename, System.Text.Encoding.Default);

        using (theReader)
        {
            int lineCount = 1;
            // While there's lines left in the text file, do this:
            do
            {
                line = theReader.ReadLine();

                if (line != null && line != "")
                {
                    //if line starts with #, ignore
                    if (line[0] == '#')
                    {
                        continue;
                    }

                    //each line 1 agent, separated by ";"
                    string[] entries = line.Split(';');

                    //if entries just have 1, it is the qnt signs
                    if (entries.Length == 1)
                    {
                        qntSigns = System.Int32.Parse(line);
                    }
                    else
                    {
                        GameObject chosenCell = GameObject.Find(entries[0]);
                        if (chosenCell)
                        {
                            //sign position
                            Vector3 newPosition = chosenCell.transform.position;

                            //sign goal
                            GameObject signGoal = GameObject.Find(entries[1]);

                            DrawSign(newPosition, signGoal, System.Convert.ToSingle(entries[2]));

                            //add this position in the list, so we dont draw here again
                            //positionsSigns.Add(newPosition);
                        }
                    }
                }
                lineCount++;
            }
            while (line != null);
            // Done reading, close the reader and return true to broadcast success    
            theReader.Close();
            //Need it no more
            positionsSigns.Clear();
        }
    }

    //check if there is Obstacles or something on a given position
    public bool CheckObstacle(Vector3 checkPosition, string tag, float radius)
    {
        Collider[] hitCollider = Physics.OverlapSphere(checkPosition, radius);
        bool returning = false;

        foreach (Collider hit in hitCollider)
        {
            if (hit.gameObject.tag == tag)
            {
                returning = true;
                break;
            }
        }

        return returning;
    }

    //generate new simulation agents and signs
    private void GenerateAnew()
    {
        //start the filesController
        filesController = new FilesController(allSimulations, configFilename, obstaclesFilename, scheduleFilename, exitFilename, signsFilename, goalsFilename, agentsGoalFilename, 
            interactionsFilename, meanSpeedFilename, meanAngVarFilename);

        //get all cells and goals
        //allCells = GameObject.FindGameObjectsWithTag("Cell");
        GameObject[] allGoals = GameObject.FindGameObjectsWithTag("Goal");
        //Debug.Log(allCells.Length);

        int qntTries = 0;
        //create the groups
        for (int i = 0; i < qntGroups; i++)
        {
            float x = 0, z = 0;
            int centerCellX = 14;
            int centerCellZ = 14;

            GameObject foundCell;
            //if evacuation, start them all at the cell14-14
            if (evacuationBehavior)
            {
                foundCell = GameObject.Find("cell"+centerCellX+"-"+centerCellZ);
            }//else, sort out
            else
            {
                //sort out a cell
                foundCell = allCells[Random.Range(0, allCells.Length)];
            }

            bool pCollider = true;
            //while collider inside obstacle or player
            while (pCollider)
            {
                //generate random position
                x = Random.Range(foundCell.transform.position.x - cellRadius, foundCell.transform.position.x + cellRadius);
                z = Random.Range(foundCell.transform.position.z - cellRadius, foundCell.transform.position.z + cellRadius);

                //see if there are agents in this radius
                pCollider = CheckObstacle(new Vector3(x, 0, z), "AgentGroup", 0.1f);
                //if not, still need to check if there is any obstacle
                if (!pCollider)
                {
                    pCollider = CheckObstacle(new Vector3(x, 0, z), "Obstacle", 0.1f);
                }

                qntTries++;

                //if tried too much, change to a nearer cell
                if(qntTries > 100)
                {
                    if(centerCellX == 14 && centerCellZ == 14)
                    {
                        centerCellZ = 16;
                    }else if (centerCellX == 14 && centerCellZ == 16)
                    {
                        centerCellZ = 12;
                    }
                    else if (centerCellX == 14 && centerCellZ == 12)
                    {
                        centerCellX = 12;
                        centerCellZ = 14;
                    }
                    else if (centerCellX == 12 && centerCellZ == 14)
                    {
                        centerCellX = 16;
                    }
                    else if (centerCellX == 16 && centerCellZ == 14)
                    {
                        centerCellX = 12;
                        centerCellZ = 12;
                    }
                    else if (centerCellX == 12 && centerCellZ == 12)
                    {
                        centerCellX = 16;
                        centerCellZ = 16;
                    }
                    else if (centerCellX == 16 && centerCellZ == 16)
                    {
                        centerCellX = 12;
                        centerCellZ = 16;
                    }
                    else if (centerCellX == 12 && centerCellZ == 16)
                    {
                        centerCellX = 16;
                        centerCellZ = 12;
                    }
                    else if (centerCellX == 16 && centerCellZ == 12)
                    {
                        centerCellX = 10;
                        centerCellZ = 14;
                    }
                    else if (centerCellX == 10 && centerCellZ == 14)
                    {
                        centerCellX = 18;
                        centerCellZ = 14;
                    }
                    else if (centerCellX == 18 && centerCellZ == 14)
                    {
                        centerCellX = 14;
                        centerCellZ = 10;
                    }
                    else if (centerCellX == 14 && centerCellZ == 10)
                    {
                        centerCellX = 14;
                        centerCellZ = 18;
                    }
                    else if (centerCellX == 14 && centerCellZ == 18)
                    {
                        centerCellX = 10;
                        centerCellZ = 10;
                    }
                    else if (centerCellX == 10 && centerCellZ == 10)
                    {
                        centerCellX = 18;
                        centerCellZ = 18;
                    }
                    else if (centerCellX == 18 && centerCellZ == 18)
                    {
                        centerCellX = 10;
                        centerCellZ = 12;
                    }
                    else if (centerCellX == 10 && centerCellZ == 12)
                    {
                        centerCellX = 10;
                        centerCellZ = 16;
                    }
                    else if (centerCellX == 10 && centerCellZ == 16)
                    {
                        centerCellX = 10;
                        centerCellZ = 18;
                    }
                    else if (centerCellX == 10 && centerCellZ == 18)
                    {
                        centerCellX = 12;
                        centerCellZ = 18;
                    }
                    else if (centerCellX == 12 && centerCellZ == 18)
                    {
                        centerCellX = 16;
                        centerCellZ = 18;
                    }
                    else if (centerCellX == 16 && centerCellZ == 18)
                    {
                        centerCellX = 18;
                        centerCellZ = 16;
                    }
                    else if (centerCellX == 18 && centerCellZ == 16)
                    {
                        centerCellX = 18;
                        centerCellZ = 12;
                    }
                    else if (centerCellX == 18 && centerCellZ == 12)
                    {
                        centerCellX = 18;
                        centerCellZ = 10;
                    }
                    else if (centerCellX == 18 && centerCellZ == 10)
                    {
                        centerCellX = 16;
                        centerCellZ = 10;
                    }
                    else if (centerCellX == 16 && centerCellZ == 10)
                    {
                        centerCellX = 12;
                        centerCellZ = 10;
                    }
                    else
                    {
                        //not working, finish it
                        Debug.Log("too many agents!!");
                        Debug.Break();
                        break;
                    }
                    foundCell = GameObject.Find("cell" + centerCellX + "-" + centerCellZ);

                    qntTries = 0;
                }
            }

            qntTries = 0;

            //instantiate
            GameObject newAgentGroup = Instantiate(agentGroup, new Vector3(x, 0f, z), Quaternion.identity) as GameObject;
            AgentGroupController newAgentGroupController = newAgentGroup.GetComponent<AgentGroupController>();
            //change its name
            groupsNameCounter++;
            newAgentGroup.name = "agentGroup" + groupsNameCounter;
            //set its cell
            newAgentGroupController.cell = foundCell;
            //default mean speed
            newAgentGroupController.SetMeanSpeed(defaultMeanSpeed);
            //default cohesion
            newAgentGroupController.SetCohesion(defaultCohesion);
            //default speed deviation
            newAgentGroupController.SetMeanSpeedDeviation(defaultMeanSpeedDeviation);
            //default angular variarion
            newAgentGroupController.SetAngularVariarion(defaultAngularVariation);

            //if using hofstede
            if (useHofstede)
            {
                //calculate the group hofstede
                newAgentGroupController.hofstede.CalculateHofstede(50, 50, 50, 100, 30);
                //set group cohesion
                newAgentGroupController.SetCohesion(newAgentGroupController.hofstede.GetMeanCohesion());
                //once we have the hofstede calculated values, we can use the meanSpeed as a mean velocity for the group and calculate the meanSpeedDeviation according the cohesion
                //the more cohesion, the less deviation (between 0 and 0.2)
                newAgentGroupController.hofstede.SetMeanSpeedDeviation((3 - newAgentGroupController.GetCohesion()) / 15);
                //set group speed deviation
                newAgentGroupController.SetMeanSpeedDeviation(newAgentGroupController.hofstede.GetMeanSpeedDeviation());
                //set group mean speed
                newAgentGroupController.SetMeanSpeed(newAgentGroupController.hofstede.GetMeanSpeed());
                //so, calculate the angle based on this value. Max angle: 90 degrees
                newAgentGroupController.SetAngularVariarion(newAgentGroupController.hofstede.GetMeanAngVar() * 90.0f);
            }
            //durupinar is for each agent, so do it later

            //if evacuation behavior, just choose the nearest goal
            if (evacuationBehavior)
            {
                GameObject chosenGoal = allGoals[0];
                float closer = Vector3.Distance(newAgentGroup.transform.position, allGoals[0].transform.position);
                for (int j = 1; j < allGoals.Length; j++)
                {
                    //if it is closer
                    float newCloser = Vector3.Distance(newAgentGroup.transform.position, allGoals[j].transform.position);
                    if (newCloser < closer)
                    {
                        closer = newCloser;
                        chosenGoal = allGoals[j];
                    }
                }

                //add the chosen goal
                newAgentGroupController.go.Add(chosenGoal);
                //add a random intention
                newAgentGroupController.intentions.Add(Random.Range(0f, (intentionThreshold - 0.01f)));
                //add a random desire
                newAgentGroupController.desire.Add(Random.Range(0f, 1f));
            }//else, all goals
            else
            {
                //agent group goals
                for (int j = 0; j < allGoals.Length; j++)
                {
                    newAgentGroupController.go.Add(allGoals[j]);
                    //add a random intention
                    newAgentGroupController.intentions.Add(Random.Range(0f, (intentionThreshold - 0.01f)));
                    //add a random desire
                    newAgentGroupController.desire.Add(Random.Range(0f, 1f));
                }
            }

            //add the Looking For state, with a random position
            if (exploratoryBehavior)
            {
                GameObject lookingFor = GenerateLookingFor();
                newAgentGroupController.go.Add(lookingFor);
                newAgentGroupController.intentions.Add(intentionThreshold);
                newAgentGroupController.desire.Add(1);
            }

            //reorder following intentions
            newAgentGroupController.ReorderGoals();
        }

        //get the parent Agents
        GameObject agents = GameObject.Find("Agents");
        //get the agent groups
        GameObject[] agentsGroups = GameObject.FindGameObjectsWithTag("AgentGroup");

        //instantiate qntAgents Agents
        for (int i = 0; i < qntAgents; i++)
        {
            //sort out an agent group
            GameObject chosenGroup = agentsGroups[Random.Range(0, agentsGroups.Length)];
            //since we need to make sure that each group has, at least, 1 agent, we populate the groups 1 by 1 at first
            foreach (GameObject ag in agentsGroups)
            {
                //no agent inside this group
                if (ag.GetComponent<AgentGroupController>().agents.Count == 0)
                {
                    //so, chosen one
                    chosenGroup = ag;
                    break;
                }
            }

            //Debug.Log (x+"--"+z);

            //generate the agent position, based on group cell
            float x = Random.Range(chosenGroup.GetComponent<AgentGroupController>().cell.transform.position.x - cellRadius, chosenGroup.GetComponent<AgentGroupController>().cell.transform.position.x + cellRadius);
            float z = Random.Range(chosenGroup.GetComponent<AgentGroupController>().cell.transform.position.z - cellRadius, chosenGroup.GetComponent<AgentGroupController>().cell.transform.position.z + cellRadius);

            //see if there are agents in this radius. if not, instantiante
            bool pCollider = CheckObstacle(new Vector3(x, 0, z), "Player", 0.1f);

            //even so, if we are an obstacle, cannot instantiate either
            //just need to check for obstacle if found no player, otherwise it will not be instantiated anyway
            if (!pCollider)
            {
                pCollider = CheckObstacle(new Vector3(x, 0, z), "Obstacle", 0.1f);
            }

            //if found a player in the radius, do not instantiante. try again
            if (pCollider)
            {
                //try again
                i--;
                continue;
            }
            else
            {
                GameObject newAgent = Instantiate(agent, new Vector3(x, 0f, z), Quaternion.identity) as GameObject;
                AgentController newAgentController = newAgent.GetComponent<AgentController>();
                //change his name
                newAgent.name = "agent" + i;
                //open file
                newAgent.GetComponent<AgentController>().OpenFile();
                //random agent color
                newAgentController.SetColor(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));
                //agent cell
                newAgentController.SetCell(chosenGroup.GetComponent<AgentGroupController>().cell);
                //agent radius
                newAgentController.agentRadius = agentRadius;

                newAgent.GetComponent<MeshRenderer>().material.color = newAgentController.GetColor();

                //if using durupinar, set it
                if (useDurupinar)
                {
                    newAgentController.durupinar.CalculateDurupinar(0.2f, 0.5f, 0.6f, 0.3f, 0.1f);
                }//else, if using favaretto...
                else if (useFavaretto)
                {
                    newAgentController.favaretto.CalculateFavaretto(0.2f, 0.5f, 0.6f, 0.3f, 0.1f);
                }

                //group max speed
                newAgentController.maxSpeed = chosenGroup.GetComponent<AgentGroupController>().GetMeanSpeed();

                //the agent maxSpeed will not be the defined maxSpeed, but the calculated meanSpeed of the group with a variation in the meanSpeedDeviation, defined by the group cohesion
                float newSpeed = -1;
                //while speed is invalid
                while (newSpeed <= 0)//newSpeed > agent.GetComponent<AgentController>().maxSpeed || 
                {
                    newSpeed = Random.Range(chosenGroup.GetComponent<AgentGroupController>().GetMeanSpeed() - chosenGroup.GetComponent<AgentGroupController>().GetMeanSpeedDeviation(),
                        chosenGroup.GetComponent<AgentGroupController>().GetMeanSpeed() + chosenGroup.GetComponent<AgentGroupController>().GetMeanSpeedDeviation());
                }
                //set agent speed
                newAgentController.maxSpeed = newSpeed;

                //agent goals from group
                newAgentController.go.AddRange(chosenGroup.GetComponent<AgentGroupController>().go);
                newAgentController.intentions.AddRange(chosenGroup.GetComponent<AgentGroupController>().intentions);
                newAgentController.desire.AddRange(chosenGroup.GetComponent<AgentGroupController>().desire);

                //replace the looking for, so each agent will have its own
                /*for(int g = 0; g < newAgentController.go.Count; g++)
                {
                    if(newAgentController.go[g].tag == "LookingFor")
                    {
                        GameObject anotherLF = GenerateLookingFor();
                        anotherLF.transform.position = newAgentController.go[g].transform.position;
                        newAgentController.go[g] = anotherLF;
                    }
                }*/

                //change PPD bias according selected option
                float ppdBias = 0;
                if (thermalComfort)
                {
                    ppdBias = 1;
                }
                if (thermalComfort && densityComfort)
                {
                    ppdBias = 0.5f;
                }
                newAgentController.ppdBias = ppdBias;

                newAgentController.group = chosenGroup;
                chosenGroup.GetComponent<AgentGroupController>().agents.Add(newAgent);

                //change parent
                newAgent.transform.parent = agents.transform;
            }
        }
    }

    //set camera according terrain size
    private void ConfigCamera()
    {
        //camera height and center position
        //if scene is 3:2 scale, we divide for 3 or 2, depending the parameter
        //if they are equal, it is the same scale (i.e. 3:3). So, divide for 2
        float cameraHeight = scenarioSizeX / 3;
        float cameraPositionX = scenarioSizeX / 2;
        float cameraPositionZ = scenarioSizeZ / 2;
        if (scenarioSizeX == scenarioSizeZ)
        {
            cameraHeight = scenarioSizeX / 2;
            cameraPositionX = scenarioSizeX / 2;
            cameraPositionZ = scenarioSizeZ / 2;
        }
        GameObject camera = GameObject.Find("Camera");
        camera.transform.position = new Vector3(cameraPositionX, camera.transform.position.y, cameraPositionZ);
        camera.GetComponent<Camera>().orthographicSize = cameraHeight;
    }

    //generates a new looking for GameObject
    private GameObject GenerateLookingFor()
    {
        GameObject lookingFor = new GameObject();
        lookingFor.name = "LookingFor";
        lookingFor.tag = "LookingFor";

        //just generate a random cell for the LF
        GameObject cl = allCells[Random.Range(0, allCells.Length)];
        lookingFor.transform.position = cl.transform.position;

        return lookingFor;
    }

    //checks if simulation is over
    //for that, we check if is there still an agent in the scene
    //if there is none, we clear the scene, update the index, get the new set of files to setup the new simulation and start
    private void EndSimulation()
    {
        if (allAgents.Length == 0)
        {
            //save the Rodolfo file
            filesController.SaveFullFDFile(qntAgents);

            filesController.SaveMetrics(Time.time, maxQntGroups);

            Debug.Log("Simulation time (seconds): " + Time.time);
            Debug.Log(secondsTaken + " seconds taken");
            Debug.Log("Qnt Frames: " + qntFrames);
            Debug.Log("Max qnt of groups: " + maxQntGroups);

            filesController.Finish();

            //get a print (for the heatmap)
            ScreenCapture.CaptureScreenshot(Application.dataPath + "/" + allSimulations + "/" + "HeatMap.png");

            //set the text
            text.text = "Simulation " + simulationIndex + " Finished!";

            //update simulation index
            simulationIndex++;

            text.text += "\nLoading Simulation " + simulationIndex + "!";

            //if index is >= than allDirs, we have reached the end. So, GAME OVER!!!
            if (simulationIndex >= allDirs.Length)
            {
                gameOver = true;
                Debug.Log("FINISHED!!");

                //set the text
                text.text = "All Simulations Finished!";

                //reset the iterator
                var file = File.CreateText(Application.dataPath + "/" + allSimulations + "/SimulationIterator.txt");
                file.WriteLine("0");
                file.Close();

                //reset the frame count
                file = File.CreateText(Application.dataPath + "/" + allSimulations + "/FrameCount.txt");
                file.WriteLine("0");
                file.Close();

                Application.Quit();
            }
            else
            {
                //else, we keep it going
                var file = File.CreateText(Application.dataPath + "/" + allSimulations + "/SimulationIterator.txt");
                file.WriteLine(simulationIndex.ToString());
                file.Close();

                //update last frame count
                file = File.CreateText(Application.dataPath + "/" + allSimulations + "/FrameCount.txt");
                file.WriteLine(Time.frameCount.ToString());
                file.Close();

                //reset scene
                SceneManager.LoadScene(0);
            }
        }
    }

    //load the master file
    private void LoadMasterFile()
    {
        // Create a new StreamReader, tell it which file to read and what encoding the file
        StreamReader theReader = new StreamReader(Application.dataPath + "/" + masterFilename, System.Text.Encoding.Default);
        string line;
        using (theReader)
        {
            do
            {
                line = theReader.ReadLine();

                if (line != null && line != "")
                {
                    //if line starts with #, ignore
                    if (line[0] == '#')
                    {
                        continue;
                    }

                    //each line 1 group, separated by ";"
                    string[] entries = line.Split(':');

                    switch (entries[0])
                    {
                        case "Config":
                            configFilename = entries[1];
                            break;
                        case "Obstacles":
                            obstaclesFilename = entries[1];
                            break;
                        case "Agents":
                            scheduleFilename = entries[1];
                            break;
                        case "Signs":
                            signsFilename = entries[1];
                            break;
                        case "Goals":
                            goalsFilename = entries[1];
                            break;
                        case "MeanAgentSpeed":
                            meanSpeedFilename = entries[1];
                            break;
                        case "MeanAgentAngVar":
                            meanAngVarFilename = entries[1];
                            break;
                        case "AgentsGoal":
                            agentsGoalFilename = entries[1];
                            break;
                        case "Interactions":
                            interactionsFilename = entries[1];
                            break;
                        case "Hofstede":
                            hofstedeFilename = entries[1];
                            break;
                        case "Durupinar":
                            durupinarFilename = entries[1];
                            break;
                        case "Favaretto":
                            favarettoFilename = entries[1];
                            break;
                        case "SaveConfigFile":
                            if(entries[1].ToLower() == "true")
                            {
                                saveConfigFile = true;
                            }
                            else
                            {
                                saveConfigFile = false;
                            }
                            break;
                        case "LoadConfigFile":
                            if (entries[1].ToLower() == "true")
                            {
                                loadConfigFile = true;
                            }
                            else
                            {
                                loadConfigFile = false;
                            }
                            break;
                        case "QntAgents":
                            qntAgents = System.Int32.Parse(entries[1]);
                            break;
                        case "QntGroups":
                            qntGroups = System.Int32.Parse(entries[1]);

                            break;
                        case "UseHofstede":
                            if (entries[1].ToLower() == "true")
                            {
                                useHofstede = true;
                                useDurupinar = false;
                                useFavaretto = false;
                            }
                            else
                            {
                                useHofstede = false;
                            }
                            break;
                        case "UseDurupinar":
                            if (entries[1].ToLower() == "true")
                            {
                                useDurupinar = true;
                                useHofstede = false;
                                useFavaretto = false;
                            }
                            else
                            {
                                useDurupinar = false;
                            }
                            break;
                        case "UseFavaretto":
                            if (entries[1].ToLower() == "true")
                            {
                                useDurupinar = false;
                                useHofstede = false;
                                useFavaretto = true;
                            }
                            else
                            {
                                useFavaretto = false;
                            }
                            break;
                        case "Cohesion":
                            if (!useHofstede)
                            {
                                defaultCohesion = System.Convert.ToSingle(entries[1]);
                            }
                            break;
                        case "MeanSpeed":
                            if (!useHofstede)
                            {
                                defaultMeanSpeed = System.Convert.ToSingle(entries[1]);
                            }
                            break;
                        case "MeanSpeedDeviation":
                            if (!useHofstede)
                            {
                                defaultMeanSpeedDeviation = System.Convert.ToSingle(entries[1]);
                            }
                            break;
                        case "AngularVariation":
                            if (!useHofstede)
                            {
                                defaultAngularVariation = System.Convert.ToSingle(entries[1]);
                            }
                            break;
                        case "staticLFS":
                            //split by ,
                            string[] lfsPos = entries[1].Split(',');
                            staticLookingFor = new Vector3(System.Convert.ToSingle(lfsPos[0]), System.Convert.ToSingle(lfsPos[1]),
                                System.Convert.ToSingle(lfsPos[2]));
                            break;
                        case "PaintHeatMap":
                            if (entries[1].ToLower() == "true")
                            {
                                paintHeatMap = true;
                            }
                            else
                            {
                                paintHeatMap = false;
                            }
                            break;
                        case "ExploratoryBehavior":
                            if (entries[1].ToLower() == "true")
                            {
                                exploratoryBehavior = true;
                            }
                            else
                            {
                                exploratoryBehavior = false;
                            }
                            break;
                        case "GroupBehavior":
                            if (entries[1].ToLower() == "true")
                            {
                                groupBehavior = true;
                            }
                            else
                            {
                                groupBehavior = false;
                            }
                            break;
                        case "ThermalComfort":
                            if (entries[1].ToLower() == "true")
                            {
                                thermalComfort = true;
                            }
                            else
                            {
                                thermalComfort = false;
                            }
                            break;
                        case "DensityComfort":
                            if (entries[1].ToLower() == "true")
                            {
                                densityComfort = true;
                            }
                            else
                            {
                                densityComfort = false;
                            }
                            break;
                    }
                }
            }
            while (line != null);
        }
        theReader.Close();

        //if qntGroups is 0, set it equal number of agents
        if(qntGroups == 0)
        {
            qntGroups = qntAgents;
        }
    }

    //control all chained simulations
    //get the new set of files to setup the new simulation and start
    private void LoadChainSimulation()
    {
        // Create a new StreamReader, tell it which file to read and what encoding the file
        StreamReader theReader = new StreamReader(Application.dataPath + "/" + allSimulations + "/SimulationIterator.txt", System.Text.Encoding.Default);
        using (theReader)
        {
            //we get the updated simulation Index
            string line = theReader.ReadLine();
            simulationIndex = System.Int32.Parse(line);
        }
        theReader.Close();

        theReader = new StreamReader(Application.dataPath + "/" + allSimulations + "/FrameCount.txt", System.Text.Encoding.Default);
        using (theReader)
        {
            //we get the updated simulation Index
            string line = theReader.ReadLine();
            lastFrameCount = System.Int32.Parse(line);
        }
        theReader.Close();
        //Debug.Log(allDirs[simulationIndex]);
        //each directory within the defined config directory has a set of simulation files
        string[] allFiles = Directory.GetFiles(allDirs[simulationIndex]);

        string[] schNam = scheduleFilename.Split('/');
        string[] agfNam = agentsGoalFilename.Split('/');
        string[] extNam = exitFilename.Split('/');
        string[] sigNam = signsFilename.Split('/');
        string[] intNam = interactionsFilename.Split('/');
        string[] masNam = meanSpeedFilename.Split('/');
        string[] angNam = meanAngVarFilename.Split('/');
        string[] hofNam = hofstedeFilename.Split('/');
        string[] durNam = durupinarFilename.Split('/');
        string[] favNam = favarettoFilename.Split('/');

        for (int i = 0; i < allFiles.Length; i++)
        {
            //just csv and dat files
            if (Path.GetExtension(allFiles[i]) == ".csv" || Path.GetExtension(allFiles[i]) == ".dat")
            {
                if (allFiles[i].Contains(schNam[schNam.Length - 1]))
                {
                    scheduleFilename = allFiles[i];
                }
                else if (allFiles[i].Contains(agfNam[agfNam.Length - 1]))
                {
                    agentsGoalFilename = allFiles[i];
                }
                else if (allFiles[i].Contains(extNam[extNam.Length - 1]))
                {
                    exitFilename = allFiles[i];
                }
                else if (allFiles[i].Contains(sigNam[sigNam.Length - 1]))
                {
                    signsFilename = allFiles[i];
                }
                else if (allFiles[i].Contains(intNam[intNam.Length - 1]))
                {
                    interactionsFilename = allFiles[i];
                }
                else if (allFiles[i].Contains(masNam[masNam.Length - 1]))
                {
                    meanSpeedFilename = allFiles[i];
                }
                else if (allFiles[i].Contains(angNam[angNam.Length - 1]))
                {
                    meanAngVarFilename = allFiles[i];
                }
                else if (allFiles[i].Contains(hofNam[hofNam.Length - 1]))
                {
                    hofstedeFilename = allFiles[i];
                }
                else if (allFiles[i].Contains(durNam[durNam.Length - 1]))
                {
                    durupinarFilename = allFiles[i];
                }
                else if (allFiles[i].Contains(favNam[favNam.Length - 1]))
                {
                    favarettoFilename = allFiles[i];
                }
            }
        }
        //Debug.Log(exitFilename);
        //it is possible that the exit files do not exist yet. So, we check if they were found. If not, we create them
        if (exitFilename.Contains(":") || true)
        {
            //update the filename
            exitFilename = "";
            foreach(string reform in extNam)
            {
                if (reform.Contains(".csv"))
                {
                    exitFilename += reform;
                }
                else
                {
                    exitFilename += reform + "/";
                }
            }
            exitFilename = exitFilename.Replace("0", simulationIndex.ToString());
        }
        //Debug.Log(exitFilename);
        if (agentsGoalFilename.Contains(":") || true)
        {
            //update the filename
            agentsGoalFilename = "";
            foreach (string reform in agfNam)
            {
                if (reform.Contains(".csv"))
                {
                    agentsGoalFilename += reform;
                }
                else
                {
                    agentsGoalFilename += reform + "/";
                }
            }
            agentsGoalFilename = agentsGoalFilename.Replace("0", simulationIndex.ToString());
        }
        if (interactionsFilename.Contains(":") || true)
        {
            //update the filename
            interactionsFilename = "";
            foreach (string reform in intNam)
            {
                if (reform.Contains(".csv"))
                {
                    interactionsFilename += reform;
                }
                else
                {
                    interactionsFilename += reform + "/";
                }
            }
            interactionsFilename = interactionsFilename.Replace("0", simulationIndex.ToString());
        }
        if (meanSpeedFilename.Contains(":") || true)
        {
            //update the filename
            meanSpeedFilename = "";
            foreach (string reform in masNam)
            {
                if (reform.Contains(".csv"))
                {
                    meanSpeedFilename += reform;
                }
                else
                {
                    meanSpeedFilename += reform + "/";
                }
            }
            meanSpeedFilename = meanSpeedFilename.Replace("0", simulationIndex.ToString());
        }
        if (meanAngVarFilename.Contains(":") || true)
        {
            //update the filename
            meanAngVarFilename = "";
            foreach (string reform in angNam)
            {
                if (reform.Contains(".csv"))
                {
                    meanAngVarFilename += reform;
                }
                else
                {
                    meanAngVarFilename += reform + "/";
                }
            }
            meanAngVarFilename = meanAngVarFilename.Replace("0", simulationIndex.ToString());
        }

        //restart the filesController
        filesController = new FilesController(allSimulations, configFilename, obstaclesFilename, scheduleFilename, exitFilename, signsFilename, goalsFilename, agentsGoalFilename, 
            interactionsFilename, meanSpeedFilename, meanAngVarFilename);

        LoadConfigFile();
    }

    //draw each obstacle
    private void DrawObstacle(Vector3[] vertices, int[] triangles)
    {
        GameObject obstacles = GameObject.Find("Obstacles");

        GameObject go = new GameObject();
        go.transform.parent = obstacles.transform;

        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        MeshFilter mf = go.GetComponent<MeshFilter>();
        var mesh = new Mesh();
        mf.mesh = mesh;

        //set the vertices
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        //obstacle has center at 0x0, so, need to place it obstacleDisplacement forward
        go.transform.position = new Vector3(go.transform.position.x + obstacleDisplacement, 0, go.transform.position.z + obstacleDisplacement);

        go.AddComponent<MeshCollider>();
        //go.GetComponent<MeshCollider>().isTrigger = true;
        go.tag = "Obstacle";
        go.name = "Obstacle";

        //change the static navigation to draw it dinamically
        GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.NavigationStatic);
        GameObjectUtility.SetNavMeshArea(go, 1);
    }

    //NEW! draw each obstacle
    private void DrawObstacle(GameObject go, Vector3 obsPosition, float scaling)
    {
        GameObject obstacles = GameObject.Find("Obstacles");
        
        go.transform.parent = obstacles.transform;

        //positioning
        go.transform.position = obsPosition;

        //scaling
        go.transform.localScale = new Vector3(scaling, 1, scaling);

        Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;

        //obstacle has center at 0x0, so, need to place it obstacleDisplacement forward
        go.transform.position = new Vector3(go.transform.position.x + obstacleDisplacement, 0, go.transform.position.z + obstacleDisplacement);
        
        go.tag = "Obstacle";
        go.name = "Obstacle";

        Material mat = Resources.Load("Brick", typeof(Material)) as Material;
        go.GetComponent<MeshRenderer>().material = mat;

        //change the static navigation to draw it dinamically
        GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.NavigationStatic);
        GameObjectUtility.SetNavMeshArea(go, 1);
    }

    //draw a sign
    private void DrawSign(Vector3 signPosition, GameObject signGoal, float signAppeal)
    {
        //parent
        GameObject parentSigns = GameObject.Find("Signs");

        GameObject newSign = Instantiate(sign, signPosition, sign.transform.rotation) as GameObject;
        //change name
        newSign.name = "Sign" + GameObject.FindGameObjectsWithTag("Sign").Length;
        //change goal
        newSign.GetComponent<SignController>().SetGoal(signGoal);
        //change appeal
        newSign.GetComponent<SignController>().SetAppeal(signAppeal);
        //change parent
        newSign.transform.parent = parentSigns.transform;
    }

    //draw a goal
    private void DrawGoal(string goalName, Vector3 goalPosition)
    {
        //parent
        GameObject parentGoal = GameObject.Find("Goals");

        GameObject newGoal = Instantiate(goalP, goalPosition, goalP.transform.rotation) as GameObject;
        //change name
        newGoal.name = goalName;
        //change parent
        newGoal.transform.parent = parentGoal.transform;
        //draw a sign on this position too, so if the agent is looking for around, he finds it
        DrawSign(goalPosition, newGoal, 1);
    }

    //dart throwing markers
    private void DartThrowMarkers(int c)
    {
        //use this flag to break the loop if it is taking too long (maybe there is no more space)
        int flag = 0;
        for (int i = 0; i < qntAuxins; i++)
        {
            float x = Random.Range(allCells[c].transform.position.x - cellRadius, allCells[c].transform.position.x + cellRadius);
            float z = Random.Range(allCells[c].transform.position.z - cellRadius, allCells[c].transform.position.z + cellRadius);

            //see if there are auxins in this radius. if not, instantiante
            List<AuxinController> allAuxinsInCell = allCells[c].GetComponent<CellController>().GetAuxins();
            bool canIInstantiante = true;

            for (int j = 0; j < allAuxinsInCell.Count; j++)
            {
                float distanceAA = Vector3.Distance(new Vector3(x, 0f, z), allAuxinsInCell[j].position);

                //if it is too near, i cant instantiante. found one, so can Break
                if (distanceAA < auxinRadius)
                {
                    canIInstantiante = false;
                    break;
                }
            }

            //if i have found no auxin, i still need to check if is there obstacles on the way
            if (canIInstantiante)
            {
                canIInstantiante = !CheckObstacle(new Vector3(x, 0f, z), "Obstacle", auxinRadius / 10);
            }

            //canIInstantiante???                
            if (canIInstantiante)
            {
                AuxinController newAuxin = new AuxinController();
                //change his name
                newAuxin.name = "auxin" + c + "-" + i;
                //this auxin is from this cell
                newAuxin.SetCell(allCells[c]);
                //set position
                newAuxin.position = new Vector3(x, 0f, z);

                //add this auxin to this cell
                allCells[c].GetComponent<CellController>().AddAuxin(newAuxin);

                //reset the flag
                flag = 0;
            }
            else
            {
                //else, try again
                flag++;
                i--;
            }

            //if flag is above qntAuxins (*2 to have some more), break;
            if (flag > qntAuxins * 2)
            {
                //reset the flag
                flag = 0;
                break;
            }
        }
    }
}