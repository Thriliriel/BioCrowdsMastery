using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class AgentController : MonoBehaviour {
	//agent radius
	public float agentRadius;
	//agent speed
	public Vector3 speed;
    //agent speed module
    public float speedModule;
    //max speed
    public float maxSpeed;
    //goals schedule
    public List<GameObject> go;
    //goals intentions
    public List<float> intentions;
    //goals desire
    public List<float> desire;
    //auxins distance vector from agent
    public List<Vector3> vetorDistRelacaoMarcacao;
    //field of view, to see signs
    public float fieldOfView;
    //to avoid locks
    public bool changePosition = true;
    //agent cell
    public GameObject cell;
    //goal position
    public Vector3 goal;
    //agent group
    public GameObject group;
    //how many iterations is agent far from group?
    public int farAwayTimer;
    //how many iterations agent can stay far from group?
    public int maxFarAwayTimer;
    //distance from the cneter of its group
    public float distanceToGroupCenter;
    //Durupinar class
    public DurupinarClass durupinar;
    //Durupinar class
    public FavarettoClass favaretto;
    //is agent an idle agent?
    public bool isIdle;

    //exit file
    private StreamWriter exitAgentFile;
    //list with all auxins in his personal space
    private List<AuxinController> myAuxins;
    //agent color
    private Color color;
    //path
    private UnityEngine.AI.NavMeshPath path;
    //time elapsed (to calculate path just between an interval of time)
    private float elapsed;
    //all signs
    private GameObject[] allSigns;
    // to calculate var m (blocks recalculation)
    private bool denominadorW  = false;
    // to calculate var m (blocks recalculation)
    private float valorDenominadorW;
    //orientation vector (movement)
    private Vector3 m;
    //diff between goal and agent
    private Vector3 diff;
    //diff module
    private float diffMod;
    //goal vector (diff / diffMod)
    private Vector3 g;
    //terrain limits
    private Vector3 terrainLimits;
    //Game Controller
    private GameController gameController;
    //sort out a side
    private bool biggerThanZero;
    //path planning
    public PathPlanningDClass paths;
    //corner path
    public List<NodeClass> cornerPath;
    //full path
    public List<NodeClass> fullPath;
    //original path
    public List<NodeClass> originalPath;

    //START THERMAL STUFF
    //thermical confort variables
    //metabolism (M)
    /*
    standing (default) = 1.6 met 
    walking at 2km/h (0,55m/s) = 1.9met
    walking at 3km/h (0.83m/s) = 2.4met
    walking at 4km/h (1,11m/s) = 2.8met
    walking at 5km/h (1,38m/s) = 3.4met
    variation met -> 1.8
    variation speed -> 1.38m/s
    relation between them -> 1.3
    */
    public float metabolism;
    //clothing insulation (Icl)
    /*
     Icl min = 0.4 clo (light clothes)
     Icl med = 1.2 (medium, default)
     Icl max = 2.0 clo (heavy jacket) 
    */
    public float clothingIns;
    //Heat transfer (tr1)
    /*The second component is due to the influence of other surrounding agents if they are sufficiently close to have direct, interpersonal, radiant
    heat transfer, which increases the first one*/
    public float tr1;
    //PMV - thermal confort
    /*
    PMV:
    -3 	Cold
    -2 	Cool
    -1 	Slightly cool
    0 	Neutral
    1 	Slightly warm
    2 	Warm
    3 	Hot
    desirable: between -0.5 and 0.5 
    */
    public float pmv;
    //PPD - Predicted Percentage of Dissatisfied - Termic
    public float ppdT;
    //PPDd - Density PPD
    public float ppdD;
    //total PPD (thermal and density)
    public float ppd;
    //ppd Bias (0 = only density; 1 = only thermal)
    public float ppdBias;
    //agents just start moving to a better place if they are uncomfortable for some time (in seconds)
    public int timeGap;

    //tp keep up the time which agent is uncomfortable
    private float notCozyTimer;
    //used timeGap
    private int usedTimeGap;
    //END THERMAL STUFF

    //on destroy agent, free auxins
    void OnDestroy()
    {
        //iterate all cell auxins to check distance between auxins and agent
        for (int i = 0; i < myAuxins.Count; i++)
        {
            myAuxins[i].ResetAuxin();
        }

        //clear it
        myAuxins.Clear();
        CloseFile();
    }

    void Awake(){
        //set inicial values
		myAuxins = new List<AuxinController>();
        desire = new List<float>();
        durupinar = new DurupinarClass();
        favaretto = new FavarettoClass();
        valorDenominadorW = 0;
        denominadorW = false;
        elapsed = 0f;
        farAwayTimer = 0;
        maxFarAwayTimer = 50;
        distanceToGroupCenter = 0;
        terrainLimits = new Vector3(GameObject.Find("Terrain").GetComponent<Terrain>().terrainData.size.x, 0, GameObject.Find("Terrain").GetComponent<Terrain>().terrainData.size.z);
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        biggerThanZero = (Random.Range(0, 2) == 1);
        
        //thermal stuff
        metabolism = 1.6f;
        clothingIns = 1.2f;
        tr1 = 0;
        notCozyTimer = 0;
        usedTimeGap = timeGap;
        ppdD = 0;
    }

    void Start() {
        goal = go[0].transform.position;
        diff = goal - transform.position;
        diffMod = Vector3.Distance(diff, new Vector3(0f, 0f, 0f));
        g = diff / diffMod;
        path = new UnityEngine.AI.NavMeshPath();

        //get all signs, on the environment
        allSigns = GameObject.FindGameObjectsWithTag("Sign");

        paths = new PathPlanningDClass(gameController.allCells.Length * 10);
        cornerPath = new List<NodeClass>();
        fullPath = new List<NodeClass>();
        originalPath = new List<NodeClass>();
    }

    void Update() {
        //angle test (Update: works!)
        //Debug.Log(Vector3.Angle(goal - transform.position, m));
        //clear agent´s informations
        ClearAgent();

        // Update the way to the goal every second.
        elapsed += Time.deltaTime;

        //just do all this stuff if agent is not marked as idle
        if (!isIdle)
        {
            //each 1 seconds, recalculate the side biggerThanZero for angles
            if (elapsed >= 1)
            {
                elapsed = 0;
                biggerThanZero = (Random.Range(0, 2) == 1);
            }

            //if pat is zeroed, calculate it
            if(cornerPath.Count == 0)
            {
                //path planning
                List<List<NodeClass>> newPath = paths.FindPath(cell, go[0]);

                cornerPath = newPath[0];
                fullPath = newPath[1];
                originalPath = newPath[1];

                if (cornerPath.Count > 0)
                {
                    goal = new Vector3(cornerPath[0].cell.transform.position.x, 0f, cornerPath[0].cell.transform.position.z);
                }
            }

            //calculate agent path
            //UnityEngine.AI.NavMesh.CalculatePath(transform.position, go[0].transform.position, UnityEngine.AI.NavMesh.AllAreas, path);

            //update his goal
            /*if (path.corners.Length > 1)
            {
                goal = new Vector3(path.corners[1].x, 0f, path.corners[1].z);
            }*/

            //testing the cached path planning
            //List<GameObject> pat = cell.GetComponent<CellController>().GetCellPathByName("cell" + (go[0].transform.position.x) + "-" + (go[0].transform.position.z));

            diff = goal - transform.position;
            diffMod = Vector3.Distance(diff, new Vector3(0f, 0f, 0f));
            g = diff / diffMod;

            //Debug.Log(path.corners.Length);
            //just to draw the path
            /*for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
            }*/
            if (cornerPath.Count > 0)
            {
                Debug.DrawLine(transform.position, cornerPath[0].cell.transform.position, Color.red);
                for (int i = 0; i < cornerPath.Count - 1; i++)
                {
                    Debug.DrawLine(cornerPath[i].cell.transform.position, cornerPath[i + 1].cell.transform.position, Color.red);
                }
            }

            //check interaction with possible signs
            CheckSignsInView();
        }

        //update agents in this cell
        if (!cell.GetComponent<CellController>().agentsDensity.Contains(gameObject))
        {
            cell.GetComponent<CellController>().agentsDensity.Add(gameObject);
        }

        //check the sub goal distance, to change sub-goal
        CheckSubGoalDistance();

        if (gameController.thermalComfort)
        {
            //START THERMAL STUFF
            //update the metabolism according agent speed
            metabolism = 1.6f + (Vector3.Dot(speed, speed));

            //calculate the agent radiant transfer
            CalculateTr1();

            //calculate the agent PMV
            CalculatePMV();
            //Debug.Log(pmv);

            //calculate the density PPD
            CalculateDensityPPD();

            //calculate the total PPD (thermal and density)
            CalculateTotalPPD();

            //update its material according its pmv
            if (pmv < -2)
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("Materials/Freezing") as Material;
            }
            else if (pmv >= -2 && pmv < -0.5f)
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("Materials/Cold") as Material;
            }
            else if (pmv >= -0.5f && pmv <= 0.5f)
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("Materials/Normal") as Material;
            }
            else if (pmv > 0.5f && pmv < 2)
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("Materials/Hot") as Material;
            }
            else if (pmv >= 2)
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("Materials/Hell") as Material;
            }

            /*
            //for ppdBias = 1: if PMV is lower than -0.5 or higher than 0.5, agent is uncomfortable
            //for ppdBias = 0: if ppdD is above 0, agent is uncomfortable
            //for 0 < ppdBias < 1, any can be true
            if ((ppdBias == 1 && (pmv < -0.5f || pmv > 0.5f)) || (ppdBias == 0 && ppdD > 0) ||
                ((0 < ppdBias && ppdBias < 1) && ((pmv < -0.5f || pmv > 0.5f) || ppdD > 0)))
            {
                //update the notCozytimer
                notCozyTimer += Time.deltaTime;

                //if it surpassed the timeGap, choose a better place
                if (notCozyTimer > usedTimeGap)
                {
                    //try to remove/put clothes
                    float randum = Random.Range(0.1f, 99);
                    if (randum < ppd)
                    {
                        //just if it is not moving towards a goal already
                        if (speed == Vector3.zero && group.GetComponent<AgentGroupController>().go.Count == 0)
                        {
                            //to control if clothes were put/removed
                            bool changedClothes = false;
                            //if it is too hot, try to remove clothes
                            if (pmv > 0.5f)
                            {
                                if (clothingIns == 2)
                                {
                                    clothingIns = 1.2f;
                                    changedClothes = true;
                                }
                                else if (clothingIns == 1.2f)
                                {
                                    clothingIns = 0.4f;
                                    changedClothes = true;
                                }
                            }//otherwise, if it is too cold, try to put some clothes on
                            else if (pmv < -0.5f)
                            {
                                if (clothingIns == 0.4f)
                                {
                                    clothingIns = 1.2f;
                                    changedClothes = true;
                                }
                                else if (clothingIns == 1.2f)
                                {
                                    clothingIns = 2;
                                    changedClothes = true;
                                }
                            }

                            //if agent could not change clothes, move
                            if (!changedClothes)
                            {
                                bool moving = false;
                                //all the group should move. So, verify if group has a leader
                                //if it has, the leader shall be uncomfortable for group to move
                                if (group.GetComponent<AgentGroupController>().leader)
                                {
                                    if(group.GetComponent<AgentGroupController>().leader.name == name)
                                    {
                                        moving = true;
                                    }
                                }//else, if cohesion is high (>=2), the majority of the group should be uncomfortable for group to move
                                else if (group.GetComponent<AgentGroupController>().cohesion >= 2)
                                {
                                    int qntBad = 0;
                                    foreach (GameObject agn in group.GetComponent<AgentGroupController>().agents)
                                    {
                                        AgentController agc = agn.GetComponent<AgentController>();
                                        //if(agc.pmv < -0.5f || agc.pmv > 0.5f)
                                        if ((agc.ppdBias == 1 && (agc.pmv < -0.5f || agc.pmv > 0.5f)) || (agc.ppdBias == 0 && agc.ppdD > 0) ||
                                            ((0 < agc.ppdBias && agc.ppdBias < 1) && ((agc.pmv < -0.5f || agc.pmv > 0.5f) || agc.ppdD > 0)))
                                        {
                                            qntBad++;
                                        }
                                    }

                                    if(qntBad > group.GetComponent<AgentGroupController>().agents.Count / 2)
                                    {
                                        moving = true;
                                    }
                                }//else, agent just split
                                else
                                {
                                    gameController.SplitAgent(gameObject);
                                    moving = true;
                                }

                                if (moving)
                                {
                                    //sort another room with the same type of the one it is already
                                    //sort out a new room
                                    int roomIndex = 0;

                                    while (allRooms[roomIndex].GetComponent<RoomController>().roomType != cell.GetComponent<CellController>().room.GetComponent<RoomController>().roomType
                                        || allRooms[roomIndex].name == cell.GetComponent<CellController>().room.name)
                                    {
                                        roomIndex = Random.Range(0, allRooms.Length);
                                    }

                                    //in this room, sort out an empty termic cell
                                    int termicCellIndex = Random.Range(0, allRooms[roomIndex].GetComponent<RoomController>().termicCells.Count);
                                    while (allRooms[roomIndex].GetComponent<RoomController>().termicCells[termicCellIndex].GetComponent<CellController>().agents.Count > 0 ||
                                        allRooms[roomIndex].GetComponent<RoomController>().termicCells[termicCellIndex].GetComponent<CellController>().isWall ||
                                        allRooms[roomIndex].GetComponent<RoomController>().termicCells[termicCellIndex].GetComponent<CellController>().isDoor)
                                    {
                                        termicCellIndex = Random.Range(0, allRooms[roomIndex].GetComponent<RoomController>().termicCells.Count);
                                    }

                                    //now, get the path for all agents of the group
                                    foreach (GameObject agn in group.GetComponent<AgentGroupController>().agents)
                                    {
                                        agn.GetComponent<AgentController>().CalculatePath(allRooms[roomIndex].GetComponent<RoomController>().termicCells[termicCellIndex]);
                                    }
                                }
                            }

                            //reset timer anyway
                            notCozyTimer = 0;
                            //usedTimeGap = 5;
                        }
                    }
                }
            }//else, he is cozy and warm, little ball of fur... does not need to walk anymore
            else
            {
                //agent.isStopped = true;
                notCozyTimer = 0;
                //usedTimeGap = timeGap;
            }*/
            //END THERMAL STUFF
        }
    }

    void OnGUI() {
        /*var pos = Camera.current.WorldToScreenPoint(transform.position);
        var rect = new Rect(pos.x-20, Screen.height - pos.y-20, 40, 40);
        GUI.Label(rect, name);*/
    }

    public void OpenFile()
    {
        //open exit files
        exitAgentFile = File.CreateText(Application.dataPath + "/" + name + ".txt");
        exitAgentFile.WriteLine(name);
    }

    public void CloseFile()
    {
        //close exit file
        exitAgentFile.Close();
    }

    public void SaveAgentsExitFile(int lastFrameCount)
    {
        //we save: Agent name, Goal name, Time he arrived
        exitAgentFile.WriteLine((Time.frameCount - lastFrameCount) + " " + transform.position.x + " " + transform.position.z);
    }

    //clear agent´s informations
    public void ClearAgent()
    {
        //re-set inicial values
        valorDenominadorW = 0;
        vetorDistRelacaoMarcacao.Clear();
        denominadorW = false;
        m = new Vector3(0f, 0f, 0f);
        diff = goal - transform.position;
        diffMod = Vector3.Distance(diff, new Vector3(0, 0, 0));
        g = diff / diffMod;

        changePosition = true;
    }

    //walk (default fixedStep = 50FPS)
    public void Walk(float fixedStep = 0.02f)
    {
        //if fixed time step is 0, use delta time
        if (fixedStep == 0)
        {
            transform.Translate(speed * Time.deltaTime, Space.World);
        }//else, fixed
        else
        {
            transform.Translate(speed * fixedStep, Space.World);
        }
    }

    //The calculation formula starts here
    //the ideia is to find m=SUM[k=1 to n](Wk*Dk)
    //where k iterates between 1 and n (number of auxins), Dk is the vector to the k auxin and Wk is the weight of k auxin
    //the weight (Wk) is based on the degree resulting between the goal vector and the auxin vector (Dk), and the
    //distance of the auxin from the agent
    public void CalculateMotionVector()
    {
        //for each agent´s auxin
        for (int k = 0; k < vetorDistRelacaoMarcacao.Count; k++)
        {
            //calculate W
            float valorW = CalculateWeight(k);
            if (valorDenominadorW < 0.0001)
            //if (valorDenominadorW == 0)
                valorW = 0.0f;
            
            //sum the resulting vector * weight (Wk*Dk)
            m += valorW * vetorDistRelacaoMarcacao[k] * maxSpeed;            
        }
    }

    //calculate speed vector    
    public void CalculateSpeed()
    {
        Debug.DrawLine(transform.position, transform.position + m, Color.green);
        //distance between movement vector and origin
        float moduloM = Vector3.Distance(m, new Vector3(0, 0, 0));

        //multiply for PI
        float s = moduloM * 3.14f;
        //float s = moduloM;
        //Debug.Log(s);
        float thisMaxSpeed = maxSpeed;

        float time = Time.deltaTime;

        //if it is bigger than maxSpeed, use maxSpeed instead
        if (s > thisMaxSpeed)
            s = thisMaxSpeed;

        speedModule = s;

        //Debug.Log("vetor M: " + m + " -- modulo M: " + s);
        if (moduloM > 0.0001)
        {
            //calculate speed vector
            speed = s * (m / moduloM);
        }
        else
        {
            //else, he is idle
            speed = new Vector3(0, 0, 0);
        }
    }

    //find all auxins near him (Voronoi Diagram)
    //call this method from game controller, to make it sequential for each agent
    public void FindNearAuxins(float cellRadius){
		//clear all agents auxins, to start again for this iteration
		myAuxins.Clear ();

        //check all auxins on agent's cell
        CheckAuxinsCell(cell);

        //find all neighbours cells
        float startX = (float)System.Math.Round(cell.transform.position.x - (cellRadius * 2f), 1);
        float startZ = (float)System.Math.Round(cell.transform.position.z - (cellRadius * 2f), 1);
        float endX = (float)System.Math.Round(cell.transform.position.x + (cellRadius * 2f), 1);
        float endZ = (float)System.Math.Round(cell.transform.position.z + (cellRadius * 2f), 1);

        //see if it is in some border
        if (cell.transform.position.x == cellRadius)
        {
            startX = cell.transform.position.x;
        }
        if (cell.transform.position.z == cellRadius)
        {
            startZ = cell.transform.position.z;
        }
        if (cell.transform.position.x == terrainLimits.x - cellRadius)
        //if ((int)cell.transform.position.x == 29)
        {
            endX = cell.transform.position.x;
        }
        if (cell.transform.position.z == terrainLimits.z - cellRadius)
        //if ((int)cell.transform.position.z == 29)
        {
            endZ = cell.transform.position.z;
        }

        //distance from agent to cell, to define agent new cell
        float distanceToCell = Vector3.Distance(transform.position, cell.transform.position);
        //Debug.Log(gameObject.name+" -- StartX: "+startX+" -- StartZ: "+startZ+" -- EndX: "+endX+" -- EndZ: "+endZ);
        //iterate to find the cells
        //2 in 2, since the radius of each cell is 1 = diameter 2
        for(float i = startX; i <= endX; i = i + (cellRadius * 2))
        {
            for (float j = startZ; j <= endZ; j = j + (cellRadius * 2))
            {
                float nameX = i;
                float nameZ = j;
                //find the cell
                //GameObject neighbourCell = GameObject.Find("cell"+nameX+"-"+nameZ);
                GameObject neighbourCell = CellController.GetCellByName("cell" + nameX + "-" + nameZ);

                //if it exists..
                if (neighbourCell)
                {
                    //check all auxins on this cell
                    CheckAuxinsCell(neighbourCell);

                    //see distance to this cell
                    //if it is lower, the agent is in another(neighbour) cell
                    float distanceToNeighbourCell = Vector3.Distance(transform.position, neighbourCell.transform.position);
                    if (distanceToNeighbourCell < distanceToCell)
                    {
                        distanceToCell = distanceToNeighbourCell;
                        SetCell(neighbourCell);
                    }
                }
            }
        }
    }

    //reorder goals/intentions
    public void ReorderGoals() {
        //to know if changed place
        bool changed = false;

        for (int i = 0; i < intentions.Count; i++)
        {
            for (int j = i+1; j < intentions.Count; j++)
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

                    changed = true;
                }
            }
        }

        //if changed, reset the path list
        if (changed)
        {
            //reset path list
            cornerPath.Clear();
        }
    }

    //check if the goals are the same as other agent
    //update: just check the actual goal
    public bool CheckGoals(AgentController otherAgent)
    {
        //for default, get the go[0]. If the first is lookingFor, ignore it and get next (go[1])
        string goalName = go[0].name;
        if (go[0].tag == "LookingFor" && go.Count > 1)
        {
            goalName = go[1].name;
        }

        //now, find out if otherAgent has this one as next goal too
        //follow the same logic as before
        string otherGoalName = otherAgent.go[0].name;
        if (otherAgent.go[0].tag == "LookingFor" && otherAgent.go.Count > 1)
        {
            otherGoalName = otherAgent.go[1].name;
        }

        //now, just compare
        if (goalName == otherGoalName)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //make the interaction between agents
    public void InteractionBetweenAgents(GameObject otherAgent, float distance, float cohesion)
    {
        //for each goal
        for(int z = 0; z < go.Count; z++)
        {
            //find its equivalent on otherAgent go vector
            int otherIndex = -1;
            for (int o = 0; o < otherAgent.GetComponent<AgentController>().go.Count; o++)
            {
                if (go[z].name == otherAgent.GetComponent<AgentController>().go[o].name)
                {
                    otherIndex = o;
                    break;
                }
            }

            //since this agent should have a higher or equal intention value than otherAgent, we just interact if it is truly higher
            if (otherIndex > -1 && (intentions[z] > otherAgent.GetComponent<AgentController>().intentions[otherIndex] && distance > 0))
            {
                //float deltaIntention = Interact(desire[z], distance, intentions[z], otherAgent->intentions[otherIndex], otherAgent->desire[otherIndex]);

                //other agent update his intention according Bosse interation
                //otherAgent.GetComponent<AgentController>().intentions[otherIndex] += deltaIntention;

                //other agent assumes this same intention
                otherAgent.GetComponent<AgentController>().intentions[otherIndex] = intentions[z];

                //making an interpolation
                //get the difference between this agent intention and otherAgent intention. This value would be the 100% cohesion interaction
                //float deltaIntention = intentions[z] - otherAgent.GetComponent<AgentController>().intentions[otherIndex];

                //since cohesion varies from 0 to 3, 0 would be no interaction (thus, 0%) and 3 would be 100% interaction. So, makes an interpolation between 0% and 100% (0.0 and 1.0 normalized)
                //float perCentCohesion = cohesion / 3;

                //multiplies deltaIntention by the perCentCohesion, which represents how strong the group cohesion is
                //deltaIntention *= perCentCohesion;

                //other agent updates his intention with deltaIntention
                //otherAgent.GetComponent<AgentController>().intentions[otherIndex] += deltaIntention;

                otherAgent.GetComponent<AgentController>().ReorderGoals();
            }
        }
    }

    //check if there is a sign in the agent Field of View
    private void CheckSignsInView()
    {
        //get all signs on scene
        //for each one of them, check the distance between it and the agent
        bool reorder = false;
        foreach (GameObject sign in allSigns)
        {
            float distance = Vector3.Distance(transform.position, sign.transform.position);
            //if distance <= agent field of view, the sign may affect the agent
            if (distance <= fieldOfView)
            {
                //now, lets see if this sign is from a goal that our agent has intention to go
                for (int i = 0; i < go.Count; i++)
                {
                    if (go[i] == sign.GetComponent<SignController>().GetGoal())
                    {
                        //well, lets do the interaction
                        Interaction(sign, distance, i);
                        reorder = true;
                        break;
                    }
                }
            }
        }

        //reorder our goals
        if (reorder)
        {
            ReorderGoals();

            //reset path list
            //pat = new List<GameObject>();

            //pass it to the group
            //group.GetComponent<AgentGroupController>().go.Clear();
            //group.GetComponent<AgentGroupController>().go.AddRange(go);
        }
    }

    //make the interaction between the agent and the signs he can see
    private void Interaction(GameObject sign, float distance, int index)
    {
        float deltaIntention;

        //alfa -> interaction environment
        float alfa;
        alfa = 1.0f / distance;
        if (alfa > 1)
            alfa = 1;

        // From the model in Bosse et al 2014 limited for 2 agents
        //float Sq = (intentions[index]);
        //sign intention will be always 1
        float Sq = 1;

        float gama = desire[index] * alfa * sign.GetComponent<SignController>().GetAppeal();

        deltaIntention = gama * (Sq - intentions[index]);

        intentions[index] = intentions[index] + deltaIntention;

        //get the game controller to write the file
        gameController.filesController.SaveInteractionsFile(gameObject, sign, gameController.lastFrameCount, deltaIntention, intentions[index]);
        //Debug.Log(gameObject.name+"--"+deltaIntention);
    }

    //calculate W
    private float CalculateWeight(int indiceRelacao)
    {
        float retorno = 0;

        //if auxin should be ignored, just return 0. Otherwise, calculate stuff
        if (!myAuxins[indiceRelacao].ignoreAuxin)
        {
            //calculate F (F is part of weight formula)
            float valorF = CalculateF(indiceRelacao);

            if (!denominadorW)
            {
                valorDenominadorW = 0f;

                //for each agent´s auxin
                for (int k = 0; k < vetorDistRelacaoMarcacao.Count; k++)
                {
                    //calculate F for this k index, and sum up
                    valorDenominadorW += CalculateF(k);
                }
                denominadorW = true;
            }

            retorno = valorF / valorDenominadorW;
        }
        
        //if agent is not alone in the group, calculate the product between the vector to the auxin and the vector to the center of the group
        if (group.GetComponent<AgentGroupController>().agents.Count > 1)
        {
            Vector3 diffCenter = group.transform.position - transform.position;
            float diffModCenter = Vector3.Distance(diffCenter, new Vector3(0f, 0f, 0f));
            Vector3 vectorCenter = diffCenter / diffModCenter;
            float productGroupCenter = vetorDistRelacaoMarcacao[indiceRelacao].x * vectorCenter.x +
                vetorDistRelacaoMarcacao[indiceRelacao].y * vectorCenter.y + vetorDistRelacaoMarcacao[indiceRelacao].z * vectorCenter.z;

            //Debug.LogWarning(CalculateCenterWill());
            return retorno + (productGroupCenter * CalculateCenterWill());
        }//else, just return the usual weight
        else
        {
            return retorno;
        }
    }

    //calculate F (F is part of weight formula)
    private float CalculateF(int indiceRelacao)
    {
        //if auxin should be ignored, just return 0
        if (myAuxins[indiceRelacao].ignoreAuxin){
            return 0;
        }

        //distance between auxin´s distance and origin (dont know why origin...)
        float moduloY = Vector3.Distance(vetorDistRelacaoMarcacao[indiceRelacao], new Vector3(0, 0, 0));
        //distance between goal vector and origin (dont know why origin...)
        float moduloX = Vector3.Distance(g, new Vector3(0, 0, 0));
        //vector * vector
        float produtoEscalar = vetorDistRelacaoMarcacao[indiceRelacao].x * g.x + vetorDistRelacaoMarcacao[indiceRelacao].y * g.y + vetorDistRelacaoMarcacao[indiceRelacao].z * g.z;
        
        if (moduloY < 0.00001)
        {
            return 0.0f;
        }

        //angle, just to know how to make it...
        //float dafuq = (Mathf.Acos((produtoEscalar) / (moduloX * moduloY))) * (180 / Mathf.PI);

        //angle difference
        /*float angleDiff = myAuxinsAngles[indiceRelacao] - (group.GetComponent<AgentGroupController>().GetAngularVariarion() / 2);
        //extra weight: the low the diff, the more weight
        float extraWeight = 1 - (angleDiff / 100);
        if(extraWeight < 0)
        {
            extraWeight = 0;
        }*/

        //return the formula, defined in tesis/paper
        //float retorno = (float)((1.0 / (1.0 + moduloY)) * (1.0 + ((produtoEscalar + extraWeight) / (moduloX * moduloY))));
        float retorno = (float)((1.0 / (1.0 + moduloY)) * (1.0 + ((produtoEscalar) / (moduloX * moduloY))));

        //float retorno = (float)((1.0 / (1.0 + moduloY)) * (1.0 + ((produtoEscalar) / (moduloX * moduloY)) + ((productGroupCenter) / (moduloX * moduloY))));
        return retorno;
    }

    //calculate how much should the agent try to stay in the center of the group
    private float CalculateCenterWill()
    {
        //the more cohesion, the more will
        //values between 0 and 0.06f. So, divide by 50
        return group.GetComponent<AgentGroupController>().GetCohesion() / 50;
    }

    //check auxins on a cell
    private void CheckAuxinsCell(GameObject checkCell)
    {
        //get all auxins on my cell
        List<AuxinController> cellAuxins = checkCell.GetComponent<CellController>().GetAuxins();

        //iterate all cell auxins to check distance between auxins and agent
        for (int i = 0; i < cellAuxins.Count; i++)
        {
            //see if the distance between this agent and this auxin is smaller than the actual value, and inside agent radius
            float distance = Vector3.Distance(transform.position, cellAuxins[i].position);
            if (distance < cellAuxins[i].GetMinDistance() && distance <= agentRadius)
            {
                //take the auxin!!
                //if this auxin already was taken, need to remove it from the agent who had it
                if (cellAuxins[i].taken == true)
                {
                    GameObject otherAgent = cellAuxins[i].GetAgent();
                    otherAgent.GetComponent<AgentController>().myAuxins.Remove(cellAuxins[i]);
                }
                //auxin is taken
                cellAuxins[i].taken = true;
                //auxin has agent
                cellAuxins[i].SetAgent(this.gameObject);
                //update min distance
                cellAuxins[i].SetMinDistance(distance);

                //verify the angle
                //check the angle to this auxin with the goal
                float angle = Vector3.Angle(goal - transform.position, cellAuxins[i].position - transform.position);
                //cross vector, to defined the orientation
                Vector3 cross = Vector3.Cross(goal - transform.position, cellAuxins[i].position - transform.position);
                //if it is inside the ang variation, diminish its weight
                //divide it by 2 because it is half to each side
                /*if (angle < group.GetComponent<AgentGroupController>().GetAngularVariarion() / 2)
                {
                    cellAuxins[i].ignoreAuxin = true;
                }*/

                //take one side out
                if (biggerThanZero && cross.y > 0)
                {
                    if (angle < (group.GetComponent<AgentGroupController>().GetAngularVariarion()))
                    {
                        cellAuxins[i].ignoreAuxin = true;
                    }
                }else if (!biggerThanZero && cross.y < 0)
                {
                    if (angle < (group.GetComponent<AgentGroupController>().GetAngularVariarion()))
                    {
                        cellAuxins[i].ignoreAuxin = true;
                    }
                }

                //update my auxins
                myAuxins.Add(cellAuxins[i]);
            }
        }
    }

    //check the sub-goal distance
    private void CheckSubGoalDistance()
    {
        //just check if the sub-goal is not the actual goal
        if(goal != go[0].transform.position)
        {
            float distanceSubGoal = Vector3.Distance(transform.position, goal);
            if(distanceSubGoal < agentRadius && cornerPath.Count > 1)
            {
                cornerPath.RemoveAt(0);
                goal = new Vector3(cornerPath[0].cell.transform.position.x, 0f, cornerPath[0].cell.transform.position.z);
            }

            //same to remove from the full path
            distanceSubGoal = Vector3.Distance(transform.position, fullPath[0].cell.transform.position);
            if (distanceSubGoal < agentRadius && fullPath.Count > 1)
            {
                fullPath.RemoveAt(0);
            }
        }
    }

    //START THERMAL STUFF
    //calculate the mean radiant temperature tr1
    //tr1 = 0.1 * (sum for all agents around this one, inside its intimate hall distance)((0.5 + |cos angle between this agent and agent_i|) / distance_pow(2))
    private void CalculateTr1()
    {
        //sum of all agents inside its intimate zone
        float sumAllAgents = 0;

        //first: find all agents inside its intimate zone (0,45m)
        Collider[] aroundAgents = Physics.OverlapSphere(transform.position, 0.45f);
        foreach (Collider col in aroundAgents)
        {
            //if it is an agent, and it is not the same agent
            if (col.gameObject.tag == "Player" && col.gameObject.name != name)
            {
                //calculate the angle
                Vector3 dir = col.gameObject.transform.position - transform.position;
                dir = col.gameObject.transform.InverseTransformDirection(dir);
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                //cos
                float cos = Mathf.Cos(angle);
                //Debug.Log(name + " with " + col.gameObject.name + ": " + angle + " - Cos: " + cos);

                sumAllAgents += (0.5f + Mathf.Abs(cos)) / Mathf.Pow(Vector3.Distance(transform.position, col.gameObject.transform.position), 2);
            }
        }

        //tr1
        tr1 = 0.1f * sumAllAgents;
    }

    //calculate the PMV
    private void CalculatePMV()
    {
        //just calculate if not in wall termic cell
        if (!cell.GetComponent<CellController>().isWall)
        {
            //converts met: converts 1Met=58.15W/m2
            float met = metabolism * 58.15f;
            //air velocity (from ambient)
            float v = cell.GetComponent<CellController>().room.GetComponent<RoomController>().airSpeed;
            //vapor pressure of air
            float pa = 12;
            //air temperature
            float airTemperature = cell.GetComponent<CellController>().airTemperature;
            //dont know what it is...? Probally Tr0... Summing up with tr1 so...
            float tmrt = airTemperature + tr1;
            //float tmrt = airTemperature;
            //external work (assumed 0)
            float W = 0;
            //for PMV
            float RMW = met - W;
            //Accuracy definition
            float tolerance = 0.001f;

            //Calculate FCl Value
            //Clothing Area Factor
            float FCL = 1.05f + 0.1f * clothingIns;

            if (clothingIns < 0.5)
            {
                FCL = 1.0f + 0.2f * clothingIns;
            }

            //Calculate TCL Value
            //First guess for surface temperature
            float TAA = airTemperature + 273;
            float TRA = tmrt + 273;
            float TCLA = TAA + (35.3f - airTemperature) / (3.5f * (clothingIns + 0.1f));
            float XN = TCLA / 100;
            float XF = XN;

            //Compute Surface Temperature of Clothing by Successive Substitution Iterations
            float FCIC = clothingIns * 0.155f * FCL;
            float P1 = FCIC * TAA;
            float P2 = FCIC * 3.96f;
            float P3 = FCIC * 100;
            float P4 = 308.7f - 0.028f * RMW + P2 * Mathf.Pow((TRA / 100), 4);

            int nIterations = 0;

            float HC = 0;

            while (nIterations < 150)
            {
                XF = (XF + XN) / 2;

                //HC Calculation
                float HCF = 12.1f * Mathf.Pow(v, 0.5f);
                float HCN = 2.38f * Mathf.Pow(Mathf.Abs(100 * XF - TAA), 0.25f);

                if (HCF > HCN)
                {
                    //Convective Heat Transfer Coefficient
                    HC = HCF;
                }
                else
                {
                    HC = HCN;
                }

                XN = (P4 + P1 * HC - P2 * Mathf.Pow(XF, 4)) / (100 + P3 * HC);
                nIterations++;
                if ((nIterations > 1) & ((Mathf.Abs(XN - XF)) < tolerance))
                {
                    break;
                }
            }

            if (nIterations < 150)
            {
                //Surface Temperature of Clothing
                float TCL = 100 * XN - 273;

                //Compute the Predicted Mean Vote(PMV)
                float PM1 = 3.96f * FCL * (Mathf.Pow(XN, 4) - Mathf.Pow((TRA / 100), 4));
                float PM2 = FCL * HC * (TCL - airTemperature);
                float PM3 = 0.303f * Mathf.Exp(-0.036f * met) + 0.028f;
                float PM4 = 0.0f;

                if (RMW > 58.15f)
                {
                    PM4 = 0.42f * (RMW - 58.15f);
                }

                float BMV = RMW - 3.05f * 0.001f * (5733 - 6.99f * RMW - pa);
                float CMV = -PM4 - 1.7f * 0.00001f * met * (5867 - pa) - 0.0014f * met * (34 - airTemperature) - PM1 - PM2;
                pmv = PM3 * (BMV + CMV);

                //Calculate the Predicted Percentage Dissatisfied(PPD)
                ppdT = 100 - 95 * Mathf.Exp(-0.03353f * Mathf.Pow(pmv, 4) - 0.2179f * Mathf.Pow(pmv, 2));
            }
            else
            {
                pmv = 999;
                ppdT = 100;
            }
        }
    }

    //calculate the density PPD
    /* 
    * PPDd = 100((ni + βnp) / (Mi + βMp))
    */
    private void CalculateDensityPPD()
    {
        //first, verifies all agents inside intimate and personal agent zones
        //instead to verify each termic cell in the room, since the size of each cell is 1 and the personal zone goes to 1.2, we can check only the termic cell the agent is, 
        //and the 2 neighbours for each side (total of 25 cells)
        float cellSize = cell.transform.localScale.x * 10;
        float cellRadius = cell.transform.localScale.x * 5;
        float startX = cell.transform.position.x - (2 * cellSize);
        float endX = cell.transform.position.x + (2 * cellSize);
        float startZ = cell.transform.position.z - (2 * cellSize);
        float endZ = cell.transform.position.z + (2 * cellSize);

        //see if it is in some border
        if (cell.transform.position.x <= cellSize + cellRadius)
        {
            startX = cell.transform.position.x;
        }
        if (cell.transform.position.z <= cellSize + cellRadius)
        {
            startZ = cell.transform.position.z;
        }
        if (cell.transform.position.x >= terrainLimits.x - (cellSize + cellRadius))
        //if ((int)cell.transform.position.x == 29)
        {
            endX = cell.transform.position.x;
        }
        if (cell.transform.position.z >= terrainLimits.z - (cellSize + cellRadius))
        //if ((int)cell.transform.position.z == 29)
        {
            endZ = cell.transform.position.z;
        }

        //qnt agents inside the hall zones
        int qntIntimate = 0;
        int qntPersonal = 0;

        for (float i = startX; i <= endX; i = i + cellSize)
        {
            for (float j = startZ; j <= endZ; j = j + cellSize)
            {
                //try to find this termic cell
                //GameObject teumiki = GameObject.Find("Cell" + i + "X" + j);
                GameObject teumiki = CellController.GetCellByName("cell" + i + "-" + j);
                //if found it, verifies if exists agents inside it
                if (teumiki)
                {
                    //foreach agent inside it
                    if (teumiki.GetComponent<CellController>().agents.Count > 0)
                    {
                        foreach (GameObject tcAgent in teumiki.GetComponent<CellController>().agents)
                        {
                            //if it is not this agent
                            if (tcAgent.name != name)
                            {
                                //check distance
                                float distance = Vector3.Distance(transform.position, tcAgent.transform.position);

                                //if it is inside intimate zone (0.45m), add it up
                                if (distance <= 0.45f)
                                {
                                    qntIntimate++;
                                }//else, if it is inside personal zone (1.2m), add it up
                                else if (distance <= 1.2f)
                                {
                                    qntPersonal++;
                                }
                            }
                        }
                    }
                }
            }
        }

        //now that we have the qntAgents inside each zone, calculate the PPDd
        //β, Mi and Mp defined in paper
        //β = 0.2
        //Mi = 6
        //Mp = 12
        /* 
        * PPDd = 100((ni + βnp) / (Mi + βMp))
        */
        ppdD = 100 * ((qntIntimate + (0.2f * qntPersonal)) / (6 + (0.2f * 12)));
    }

    //calculate the total PPD (thermal and density)
    /*
    PPD = αPPDt + (1 − α)PPDd
    α = PPD bias. If = 0, only density is used. If = 1, only thermal is used
    */
    private void CalculateTotalPPD()
    {
        ppd = (ppdBias * ppdT) + ((1 - ppdBias) * ppdD);
    }
    //END THERMAL STUFF

    //GET-SET
    public Color GetColor(){
		return this.color;
	}
	public void SetColor(Color color){
		this.color = color;
	}
    public GameObject GetCell()
    {
        return this.cell;
    }
    public void SetCell(GameObject cell)
    {
        this.cell = cell;
    }
    //add a new auxin on myAuxins
    public void AddAuxin(AuxinController auxin)
    {
        myAuxins.Add(auxin);
    }
    //return all auxins in this cell
    public List<AuxinController> GetAuxins()
    {
        return myAuxins;
    }
    //add a new desire on desire
    public void AddDesire(float newDesire)
    {
        desire.Add(newDesire);
    }
    //remove a desire on desire
    public void RemoveDesire(int index)
    {
        desire.RemoveAt(index);
    }
    //get m vector
    public Vector3 GetM()
    {
        return m;
    }
}
