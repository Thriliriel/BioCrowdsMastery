using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour {
    
    //thermical confort variables
    //air temperature (ta)
    public float airTemperature;
    //environment radiant temperature (tr0)
    //for indoor scenarios, it is set equal to the air temperature
    //for outdoor (sunlit) environments, it is defined by the user to account mostly for solar radiation
    public float environmentRadTemp;
    //air speed (v)
    public float airSpeed;
    //humidity (rh) [0,100]
    public float humidity;
    //list of termic cells
    public List<GameObject> termicCells;
    //termic cell prefab
    public GameObject termicCellPrefab;
    //internal or external rooms?
    public bool isInternal;
    //type of place
    public GameController.roomTypes roomType;
    //fixed frame step for calculus (in seconds)
    public float fixedStep;
    //room density
    public float density;
    //room size
    public Vector2 roomSize;

    //game controller
    private GameController gameController;

    // Use this for initialization
    void Awake () {
        /*airTemperature = 25;
        environmentRadTemp = airTemperature;
        airSpeed = 0.1f;
        humidity = 40;*/
        //termicCells = new List<GameObject>();
        //isInternal = false;

        fixedStep = GameObject.Find("GameController").GetComponent<GameController>().fixedStep;
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
	}

    void Start()
    {
        environmentRadTemp = airTemperature;

        //add all children cells on the list
        termicCells.Clear();
        CellController[] allCells = gameObject.GetComponentsInChildren<CellController>();
        foreach(CellController hell in allCells)
        {
            termicCells.Add(hell.gameObject);
        }
    }

    // Update is called once per frame
    void Update () {
        //just calculate if thermal comfort is active
        if (gameController.thermalComfort)
        {
            //do not calculate for corridor
            if (GameController.roomTypes.corridor != roomType)
            {
                //update oldTemperature
                foreach (GameObject ceu in termicCells)
                {
                    //update old air temperature
                    ceu.GetComponent<CellController>().oldAirTemperature = ceu.GetComponent<CellController>().airTemperature;
                }

                //pray... 0.016666 = 60FPS
                SolveGaussSeidel(fixedStep, 50, 0.0001f);

                //room temperature is a mean temperature of all termic cells
                float temp = 0;
                int qntAgents = 0;
                foreach (GameObject ceu in termicCells)
                {
                    temp += ceu.GetComponent<CellController>().airTemperature;

                    //add the qnt agents
                    qntAgents += ceu.GetComponent<CellController>().agents.Count;
                }
                airTemperature = temp / termicCells.Count;
                environmentRadTemp = airTemperature;

                //density is an average of qnt of agents / qnt of cells
                density = (float)qntAgents / (float)termicCells.Count;
            }
        }

        //just calculate if density comfort is active
        if (gameController.densityComfort)
        {
            //do not calculate for corridor
            if (GameController.roomTypes.corridor != roomType)
            {
                int qntAgents = 0;
                foreach (GameObject ceu in termicCells)
                {
                    //add the qnt agents
                    qntAgents += ceu.GetComponent<CellController>().agents.Count;
                }

                //density is an average of qnt of agents / qnt of cells
                density = (float)qntAgents / (float)termicCells.Count;
            }
        }
    }

    public void StartLists()
    {
        termicCells = new List<GameObject>();
    }

    //create the termic cells for this room
    public void CreateTermicCells()
    {
        float startX = (transform.position.x - (roomSize.x / 2));
        float startZ = (transform.position.z - (roomSize.y / 2));
        float endX = (transform.position.x + (roomSize.x / 2));
        float endZ = (transform.position.z + (roomSize.y / 2)); 
        GameController gc = GameObject.Find("GameController").GetComponent<GameController>();
        float cellRadius = gc.cellRadius;

        termicCellPrefab.transform.localScale = new Vector3((cellRadius * 2) / 10, (cellRadius * 2) / 10, (cellRadius * 2) / 10);

        for (float i = startX; i < endX; i = i + (cellRadius * 2))
        {
            for (float j = startZ; j < endZ; j = j + (cellRadius * 2))
            {
                Vector3 newPosition = new Vector3(i + (termicCellPrefab.transform.localScale.x * 5), termicCellPrefab.transform.position.y, j + (termicCellPrefab.transform.localScale.x * 5));

                //verify if collides with some obstacle. We dont need cells in them.
                //for that, we need to check all 4 vertices of the cell. Otherwise, we may not have cells in some free spaces (for example, half of a cell would be covered by an obstacle, so that cell
                //would not be instantied)
                bool collideRight = gc.CheckObstacle(new Vector3(newPosition.x + cellRadius, newPosition.y, newPosition.z), "Obstacle", 0.01f);
                bool collideLeft = gc.CheckObstacle(new Vector3(newPosition.x - cellRadius, newPosition.y, newPosition.z), "Obstacle", 0.01f);
                bool collideTop = gc.CheckObstacle(new Vector3(newPosition.x, newPosition.y, newPosition.z + cellRadius), "Obstacle", 0.01f);
                bool collideDown = gc.CheckObstacle(new Vector3(newPosition.x, newPosition.y, newPosition.z - cellRadius), "Obstacle", 0.01f);
                bool collideRightTop = gc.CheckObstacle(new Vector3(newPosition.x + cellRadius, newPosition.y, newPosition.z + cellRadius), "Obstacle", 0.01f);
                bool collideLeftBottom = gc.CheckObstacle(new Vector3(newPosition.x - cellRadius, newPosition.y, newPosition.z - cellRadius), "Obstacle", 0.01f);
                bool collideTopLeft = gc.CheckObstacle(new Vector3(newPosition.x - cellRadius, newPosition.y, newPosition.z + cellRadius), "Obstacle", 0.01f);
                bool collideDownRight = gc.CheckObstacle(new Vector3(newPosition.x + cellRadius, newPosition.y, newPosition.z - cellRadius), "Obstacle", 0.01f);
                
                //if did collide it all, means we have found at least 1 obstacle in each case. So, the cell is covered by an obstacle
                //otherwise, we go on
                //if (!collideRight || !collideLeft || !collideTop || !collideDown || !collideRightTop || !collideLeftBottom || !collideTopLeft || !collideDownRight)
                //{
                GameObject newCell = Instantiate(termicCellPrefab, newPosition, Quaternion.identity) as GameObject;

                //name
                newCell.name = "cell" + newCell.transform.position.x + "-" + newCell.transform.position.z;
                //parent
                newCell.transform.parent = transform;
                //same air temperature, as default
                newCell.GetComponent<CellController>().airTemperature = airTemperature;
                //belongs to this room
                newCell.GetComponent<CellController>().room = gameObject;

                //if it is an internal environment, the boundaries are wall
                if (isInternal)
                {
                    if (i == startX || j == startZ || i == endX - 1 || j == endZ - 1)
                    {
                        newCell.GetComponent<CellController>().isWall = true;
                        //since it is wall, counter the diffusion
                        newCell.GetComponent<CellController>().wallFilter = 0;
                    }
                }

                //if it collides with all, it is a wall
                if (collideRight && collideLeft && collideTop && collideDown && collideRightTop && collideLeftBottom && collideTopLeft && collideDownRight)
                {
                    newCell.GetComponent<CellController>().isWall = true;
                }

                //add to list
                termicCells.Add(newCell);
                //}
            }
        }

        //once it has all cells, define the wall filter
        //@TODO: gaussian (so far) useless stuff
        //double[,] test = GaussianBlur(5, 1);
    }

    //create room doors
    public void CreateDoors()
    {
        //if it is not corridor
        //do not worry about it now
        if (roomType != GameController.roomTypes.corridor && false)
        {
            //find the corridor
            GameObject[] rooms = GameObject.FindGameObjectsWithTag("Room");
            GameObject corridor = rooms[0];
            foreach (GameObject room in rooms)
            {
                if (room.GetComponent<RoomController>().roomType == GameController.roomTypes.corridor)
                {
                    corridor = room;
                    break;
                }
            }

            //find the extreme x axis
            float startX = (transform.position.x - (transform.localScale.x * 5));
            float endX = (transform.position.x + (transform.localScale.x * 5));

            //cell radius
            GameController gc = GameObject.Find("GameController").GetComponent<GameController>();
            float cellRadius = gc.cellRadius;

            //find the cells
            GameObject cellLeft = GameObject.Find("cell" + (startX + cellRadius) + "-" + (transform.position.z + cellRadius));
            GameObject cellRight = GameObject.Find("cell" + (endX - cellRadius) + "-" + (transform.position.z + cellRadius));

            //see which one is nearer of corridor
            GameObject nearer = cellLeft;
            float distance = Vector3.Distance(cellLeft.transform.position, corridor.transform.position);
            if(Vector3.Distance(cellRight.transform.position, corridor.transform.position) < distance)
            {
                distance = Vector3.Distance(cellRight.transform.position, corridor.transform.position);
                nearer = cellRight;
            }

            //now, get the nearer and create the door
            nearer.GetComponent<CellController>().isWall = false;
            nearer.GetComponent<CellController>().isDoor = true;
            GameObject.Find("cell" + (nearer.transform.position.x) + "-" + (nearer.transform.position.z + 2 * cellRadius)).GetComponent<CellController>().isWall = false;
            GameObject.Find("cell" + (nearer.transform.position.x) + "-" + (nearer.transform.position.z + 2 * cellRadius)).GetComponent<CellController>().isDoor = true;
            GameObject.Find("cell" + (nearer.transform.position.x) + "-" + (nearer.transform.position.z - 2 * cellRadius)).GetComponent<CellController>().isWall = false;
            GameObject.Find("cell" + (nearer.transform.position.x) + "-" + (nearer.transform.position.z - 2 * cellRadius)).GetComponent<CellController>().isDoor = true;
            GameObject.Find("cell" + (nearer.transform.position.x) + "-" + (nearer.transform.position.z - 4 * cellRadius)).GetComponent<CellController>().isWall = false;
            GameObject.Find("cell" + (nearer.transform.position.x) + "-" + (nearer.transform.position.z - 4 * cellRadius)).GetComponent<CellController>().isDoor = true;
        }
    }

    //generates a lenght x lenght gaussian matrix
    private static double[,] GaussianBlur(int lenght, double weight)
    {
        double[,] kernel = new double[lenght, lenght];
        double kernelSum = 0;
        int foff = (lenght - 1) / 2;
        double distance = 0;
        double constant = 1d / (2 * Mathf.PI * weight * weight);
        for (int y = -foff; y <= foff; y++)
        {
            for (int x = -foff; x <= foff; x++)
            {
                distance = ((y * y) + (x * x)) / (2 * weight * weight);
                kernel[y + foff, x + foff] = constant * Mathf.Exp((float)(-distance));
                kernelSum += kernel[y + foff, x + foff];
            }
        }
        for (int y = 0; y < lenght; y++)
        {
            for (int x = 0; x < lenght; x++)
            {
                kernel[y, x] = kernel[y, x] * 1d / kernelSum;
            }
        }
        return kernel;
    }

    private void SolveGaussSeidel(float dt, int maxiter, float tol)
    {
        float peopleWatts = 15; // heat generated by people
        float cellWeight = 1.2f * 1.5f * (termicCellPrefab.transform.localScale.x * 10) * (termicCellPrefab.transform.localScale.x * 10); //weight of a 3D cell in the discrete domain (in kg)
        float airheatcapacity = 1000; //in J/kgC
        float sourceWatts = 5000;
        //float sourceWatts = 100000;
        int niter = 0; // number of iter
        float error = 1; // loop control
        int M = (int)(roomSize.x);
        int N = (int)(roomSize.y);
        float newvalue = 0; //updated value at each iteration
        float cellRadius = GameObject.Find("GameController").GetComponent<GameController>().cellRadius;

        while (niter < maxiter && error > tol)
        {
            error = -1;
            //changing the iteration to take into consideration the boundaries of the room, if it is an external ambient
            for (int i = 0; i < M; i = i + (int)(cellRadius * 2))
            {
                for (int j = 0; j < N; j = j + (int)(cellRadius * 2))
                {
                    //if it is internal ambient, just avoid the boundaries
                    if (isInternal && (i == 0 || j == 0 || i ==  M - 1 || j == N - 1))
                    {
                        continue;
                    }

                    //
                    // rhs
                    //
                    /*float rhs = walls_filtered.at<float>(i, j) * (uk.at<float>(i - 1, j) + uk.at<float>(i + 1, j) + uk.at<float>(i, j - 1) + uk.at<float>(i, j + 1)) + (people.at<float>(i, j) * peopleWatts + sources.at<float>(i, j) * sourceWatts) / cellWeight / airheatcapacity;
                    newvalue = (u.at<float>(i, j) + dt * rhs) / (1 + 4 * dt * walls_filtered.at<float>(i, j));
                    error = max(error, fabs(uk.at<float>(i, j) - newvalue));
                    uk.at<float>(i, j) = newvalue;*/
                    //crazy formula to get the right index of the cell
                    //int indCell = (int)((((i - cellRadius) / (cellRadius * 2)) * (N / (cellRadius * 2))) + ((j - cellRadius) / (cellRadius * 2)));
                    //int indCellLeft = (int)((((i - 1 - cellRadius) / (cellRadius * 2)) * (N / (cellRadius * 2))) + ((j - cellRadius) / (cellRadius * 2)));
                    //int indCellRight = (int)((((i + 1 - cellRadius) / (cellRadius * 2)) * (N / (cellRadius * 2))) + ((j - cellRadius) / (cellRadius * 2)));
                    //int indCellUp = (int)((((i - cellRadius) / (cellRadius * 2)) * (N / (cellRadius * 2))) + ((j + 1 - cellRadius) / (cellRadius * 2)));
                    //int indCellDown = (int)((((i - cellRadius) / (cellRadius * 2)) * (N / (cellRadius * 2))) + ((j - 1 - cellRadius) / (cellRadius * 2)));
                    int indCell = (int)((((i) / (cellRadius * 2)) * (N / (cellRadius * 2))) + ((j) / (cellRadius * 2)));
                    int indCellLeft = (int)((((i - 1) / (cellRadius * 2)) * (N / (cellRadius * 2))) + ((j) / (cellRadius * 2)));
                    int indCellRight = (int)((((i + 1) / (cellRadius * 2)) * (N / (cellRadius * 2))) + ((j) / (cellRadius * 2)));
                    int indCellUp = (int)((((i) / (cellRadius * 2)) * (N / (cellRadius * 2))) + ((j + 1) / (cellRadius * 2)));
                    int indCellDown = (int)((((i) / (cellRadius * 2)) * (N / (cellRadius * 2))) + ((j - 1) / (cellRadius * 2)));

                    //sum them up
                    float neighboursSum = 0;
                    int neighbourCount = 0;
                    if(indCellLeft >= 0)
                    {
                        neighboursSum += termicCells[indCellLeft].GetComponent<CellController>().airTemperature;
                        neighbourCount++;
                    }
                    if (indCellRight < termicCells.Count)
                    {
                        neighboursSum += termicCells[indCellRight].GetComponent<CellController>().airTemperature;
                        neighbourCount++;
                    }
                    if (indCellUp < termicCells.Count)
                    {
                        neighboursSum += termicCells[indCellUp].GetComponent<CellController>().airTemperature;
                        neighbourCount++;
                    }
                    if (indCellDown >= 0)
                    {
                        neighboursSum += termicCells[indCellDown].GetComponent<CellController>().airTemperature;
                        neighbourCount++;
                    }
                    
                    //float rhs = termicCells[indCell].GetComponent<CellController>().airTemperature * neighboursSum;
                    float rhs = termicCells[indCell].GetComponent<CellController>().wallFilter * neighboursSum;

                    //something to add at rhs
                    float rhsPlus = 0;
                    float rhsPlusAgent = peopleWatts * termicCells[indCell].GetComponent<CellController>().agents.Count;

                    //if it has a source of heat on this cell, adds this value
                    if (termicCells[indCell].GetComponent<CellController>().heatSource)
                    {
                        //if this source has tag Furnace, add this watts
                        if (termicCells[indCell].GetComponent<CellController>().heatSource.tag == "Furnace")
                        {
                            //BUUURRRRNNNNNN!!!
                            rhsPlus = (rhsPlusAgent + sourceWatts) / cellWeight / airheatcapacity;
                        }//else, if it has tag Snowman, it is cooold
                        else if (termicCells[indCell].GetComponent<CellController>().heatSource.tag == "Snowman")
                        {
                            //FREEEEZEEEE!!!
                            rhsPlus = (rhsPlusAgent - sourceWatts) / cellWeight / airheatcapacity;
                        }
                    }//else, just add the plusAgent (if it has no agent, the value is 0 anyway)
                    else
                    {
                        rhsPlus = (rhsPlusAgent) / cellWeight / airheatcapacity;
                    }

                    //update rhs
                    rhs += rhsPlus;

                    /*newvalue = (termicCells[indCell].GetComponent<CellController>().airTemperature + dt * rhs) / 
                        (1 + neighbourCount * dt * termicCells[indCell].GetComponent<CellController>().airTemperature);*/
                    newvalue = (termicCells[indCell].GetComponent<CellController>().airTemperature + dt * rhs) /
                        (1 + neighbourCount * dt * termicCells[indCell].GetComponent<CellController>().wallFilter);

                    //clamp newValue between 0 and 30 degrees celsius
                    if (newvalue < 0)
                    {
                        newvalue = 0;
                    }
                    if(newvalue > 30)
                    {
                        newvalue = 30;
                    }

                    error = Mathf.Max(error, Mathf.Abs(termicCells[indCell].GetComponent<CellController>().airTemperature - newvalue));
                    
                    //update actual air temperature
                    termicCells[indCell].GetComponent<CellController>().airTemperature = newvalue;
                }
            }
            niter++;
        }
    }
}
