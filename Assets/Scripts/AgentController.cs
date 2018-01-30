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
    }

    void Start() {
        goal = go[0].transform.position;
        diff = goal - transform.position;
        diffMod = Vector3.Distance(diff, new Vector3(0f, 0f, 0f));
        g = diff / diffMod;
        path = new UnityEngine.AI.NavMeshPath();

        //get all signs, on the environment
        allSigns = GameObject.FindGameObjectsWithTag("Sign");
    }

    void Update() {
        //angle test (Update: works!)
        //Debug.Log(Vector3.Angle(goal - transform.position, m));
        //clear agent´s informations
        ClearAgent();

        // Update the way to the goal every second.
        elapsed += Time.deltaTime;

        //each 1 seconds, recalculate the side biggerThanZero for angles
        if(elapsed >= 1)
        {
            elapsed = 0;
            biggerThanZero = (Random.Range(0, 2) == 1);
        }

        //calculate agent path
        UnityEngine.AI.NavMesh.CalculatePath(transform.position, go[0].transform.position, UnityEngine.AI.NavMesh.AllAreas, path);

        //update his goal
        if (path.corners.Length > 1)
        {
            goal = new Vector3(path.corners[1].x, 0f, path.corners[1].z);
        }

        //testing the cached path planning
        /*List<GameObject> pat = cell.GetComponent<CellController>().GetCellPathByName("cell" + ((int)go[0].transform.position.x) + "-" + (int)(go[0].transform.position.z));
        if (pat.Count > 0)
        {
            goal = new Vector3(pat[0].transform.position.x, 0f, pat[0].transform.position.z);
        }*/

        diff = goal - transform.position;
        diffMod = Vector3.Distance(diff, new Vector3(0f, 0f, 0f));
        g = diff / diffMod;

        //Debug.Log(path.corners.Length);
        //just to draw the path
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
        }
        /*if (pat.Count > 0)
        {
            Debug.DrawLine(transform.position, pat[0].transform.position, Color.red);
            for (int i = 0; i < pat.Count - 1; i++)
            {
                Debug.DrawLine(pat[i].transform.position, pat[i + 1].transform.position, Color.red);
            }
        }*/

        //check interaction with possible signs
        CheckSignsInView();

        //update agents in this cell
        if (!cell.GetComponent<CellController>().agentsDensity.Contains(gameObject))
        {
            cell.GetComponent<CellController>().agentsDensity.Add(gameObject);
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
        for(float i = startX; i <= endX; i = i + (cellRadius * 2))
        {
            for (float j = startZ; j <= endZ; j = j + (cellRadius * 2))
            {
                float nameX = i - cellRadius;
                float nameZ = j - cellRadius;
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
                }
            }
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
