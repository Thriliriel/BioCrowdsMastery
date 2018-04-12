using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//method just to control the menu interaction
public class MenuClass : MonoBehaviour {
    //master filename
    public string masterFilename;
    //hofstede filename
    public string hofFilename;
    //durupinar filename
    public string duruFilename;
    //agents filename
    public string agentsFilename;
    //config filename
    public string configFilename;
    //obstacle filename
    public string obsFilename;
    //load config file?
    public bool editFile = false;
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
    //is comfort active?
    public bool isComfortActive;
    //options
    public GameObject options;
    //files
    public GameObject files;
    //Hof Panel
    public GameObject hofPanel;
    //Durupinar Panel
    public GameObject duruPanel;
    //comforts Panel
    public GameObject comfortsPanel;
    //qnt agents input
    public GameObject qntAgentsInput;
    //qnt groups input
    public GameObject qntGroupsInput;
    //exploratory behavior input
    public GameObject ebInput;
    //group behavior input
    public GameObject gbInput;
    //heat map input
    public GameObject hmInput;
    //thermal comfort input
    public GameObject tcInput;
    //density comfort input
    public GameObject dcInput;
    //hofstede input
    public GameObject hofInput;
    //durupinar input
    public GameObject duruInput;
    //favaretto input
    public GameObject favInput;
    //MAS input
    public GameObject inputMAS;
    //ING input
    public GameObject inputING;
    //LTO input
    public GameObject inputLTO;
    //O input
    public GameObject inputO;
    //C input
    public GameObject inputC;
    //E input
    public GameObject inputE;
    //A input
    public GameObject inputA;
    //N input
    public GameObject inputN;
    //file text
    public GameObject fileText;
    //start button
    public GameObject startButton;
    //save button
    public GameObject saveButton;
    //string file name
    public string fileName;
    //scenario size x
    public GameObject sizeX;
    //scenario size z
    public GameObject sizeZ;
    //marker density
    public GameObject markerDensity;
    //cell prefab
    public GameObject cell;
    //wait time
    public GameObject waitPanel;
    //cell radius
    private int cellRadius;
    //auxin radius
    private float auxinRadius;
    //qnt auxins
    private int qntAuxins;
    //all auxins
    private List<AuxinController> allAuxins;
    //break line char
    private string breakLine;

    private void Awake()
    {
        hofPanel.SetActive(false);
        duruPanel.SetActive(false);
        comfortsPanel.SetActive(false);
        //files.SetActive(false);
        options.SetActive(false);
        fileText.SetActive(false);
        saveButton.SetActive(false);
        cellRadius = 1;
        auxinRadius = 0.1f;
        qntAuxins = 0;
        allAuxins = new List<AuxinController>();
        waitPanel.SetActive(false);
        breakLine = "\r\n";
    }

    //toggle the loadConfigFile
    public void ToggleEditConfigFile()
    {
        editFile = !editFile;

        //(de)active options according load config file and set the files
        options.SetActive(!editFile);
        files.SetActive(editFile);
    }

    //toggle the comforts
    public void ToggleComforts()
    {
        isComfortActive = !isComfortActive;

        if (isComfortActive)
        {
            comfortsPanel.SetActive(true);
        }
        else
        {
            comfortsPanel.SetActive(false);
        }
    }

    //toggle the useHofstede
    public void ToggleUseHofstede()
    {
        useHofstede = hofInput.GetComponent<Toggle>().isOn;

        //if use hof, not using others
        if (useHofstede)
        {
            useDurupinar = useFavaretto = false;

            //show hof parameters
            hofPanel.SetActive(true);

            //hide other
            duruPanel.SetActive(false);

            //deactivate other 2
            duruInput.GetComponent<Toggle>().isOn = false;
            favInput.GetComponent<Toggle>().isOn = false;
        }//else, hide the hof panel
        else
        {
            hofPanel.SetActive(false);
        }
    }

    //toggle the useDurupinar
    public void ToggleUseDurupinar()
    {
        useDurupinar = duruInput.GetComponent<Toggle>().isOn;

        //if use hof, not using others
        if (useDurupinar)
        {
            useHofstede = useFavaretto = false;

            //show hof parameters
            duruPanel.SetActive(true);

            //hide other
            hofPanel.SetActive(false);

            //deactivate other 2
            hofInput.GetComponent<Toggle>().isOn = false;
            favInput.GetComponent<Toggle>().isOn = false;
        }//else, hide the duru panel
        else
        {
            duruPanel.SetActive(false);
        }
    }

    //when activate one comfort, deactivate another
    public void ChangeComfortThermal()
    {
        if (tcInput.GetComponent<Toggle>().isOn)
        {
            dcInput.GetComponent<Toggle>().isOn = false;
        }
    }
    public void ChangeComfortDensity()
    {
        if (dcInput.GetComponent<Toggle>().isOn)
        {
            tcInput.GetComponent<Toggle>().isOn = false;
        }
    }

    //save the information chosen and start the simulation
    public void SaveAndStart()
    {
        //if not load config File, need to save info first
        if (!editFile)
        {
            //need to rewrite the master file with the new input
            //first, read it all
            StreamReader theReader = new StreamReader(Application.dataPath + "/" + masterFilename, System.Text.Encoding.Default);
            string line;
            string newText = "";
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
                            //just rewrite and keep going
                            newText += line + breakLine;
                            continue;
                        }

                        //split
                        string[] entries = line.Split(':');

                        switch (entries[0])
                        {
                            /*case "LoadConfigFile":
                                newText += "LoadConfigFile:" + loadConfigFile + breakLine;

                                break;*/
                            case "QntAgents":
                                int qntAg = System.Int32.Parse(qntAgentsInput.GetComponent<InputField>().text);

                                //write
                                newText += "QntAgents:" + qntAg + breakLine;
                                break;
                            case "QntGroups":
                                int qntGr = System.Int32.Parse(qntGroupsInput.GetComponent<InputField>().text);

                                //write
                                newText += "QntGroups:" + qntGr + breakLine;
                                break;
                            case "ExploratoryBehavior":
                                newText += "ExploratoryBehavior:" + ebInput.GetComponent<Toggle>().isOn + breakLine;

                                break;
                            case "GroupBehavior":
                                newText += "GroupBehavior:" + gbInput.GetComponent<Toggle>().isOn + breakLine;

                                break;
                            case "PaintHeatMap":
                                newText += "PaintHeatMap:" + hmInput.GetComponent<Toggle>().isOn + breakLine;

                                break;
                            case "ThermalComfort":
                                newText += "ThermalComfort:" + tcInput.GetComponent<Toggle>().isOn + breakLine;

                                break;
                            case "DensityComfort":
                                newText += "DensityComfort:" + dcInput.GetComponent<Toggle>().isOn + breakLine;

                                break;
                            case "UseHofstede":
                                newText += "UseHofstede:" + hofInput.GetComponent<Toggle>().isOn + breakLine;

                                break;
                            case "UseDurupinar":
                                newText += "UseDurupinar:" + duruInput.GetComponent<Toggle>().isOn + breakLine;

                                break;
                            case "UseFavaretto":
                                newText += "UseFavaretto:" + favInput.GetComponent<Toggle>().isOn + breakLine;

                                break;
                            default:
                                //default just write original
                                newText += line + breakLine;
                                break;
                        }
                    }
                    else
                    {
                        //add a blank line too
                        newText += breakLine;
                    }
                }
                while (line != null);
            }
            theReader.Close();

            //now, write the new string
            StreamWriter theWriter = File.CreateText(Application.dataPath + "/" + masterFilename);

            //update
            theWriter.Write(newText);

            //close
            theWriter.Close();

            //save agents file
            SaveAgentsFile();

            //now, save the hof or duru file, if using it
            if (useHofstede)
            {
                SaveHofstedeFile();
            }else if (useDurupinar)
            {
                SaveDurupinarFile();
            }
        }//else, just save the load config file
        /*else
        {
            //need to rewrite the master file with the new input
            //first, read it all
            StreamReader theReader = new StreamReader(Application.dataPath + "/" + masterFilename, System.Text.Encoding.Default);
            string line;
            string newText = "";
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
                            //just rewrite and keep going
                            newText += line + breakLine;
                            continue;
                        }

                        //split
                        string[] entries = line.Split(':');

                        switch (entries[0])
                        {
                            case "LoadConfigFile":
                                newText += "LoadConfigFile:" + loadConfigFile.ToString() + breakLine;

                                break;
                            default:
                                //default just write original
                                newText += line + breakLine;
                                break;
                        }
                    }
                    else
                    {
                        //add a blank line too
                        newText += breakLine;
                    }
                }
                while (line != null);
            }
            theReader.Close();

            //now, write the new string
            StreamWriter theWriter = File.CreateText(Application.dataPath + "/" + masterFilename);

            //update
            theWriter.Write(newText);

            //close
            theWriter.Close();
        }*/

        //start
        SceneManager.LoadScene(1);
    }

    //save agents file
    private void SaveAgentsFile()
    {
        StreamReader theReader = new StreamReader(Application.dataPath + "/" + agentsFilename, System.Text.Encoding.Default);
        string line;
        string newText = "";
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
                        //just rewrite and keep going
                        newText += line + breakLine;
                        continue;
                    }

                    //for each group, create the info for it
                    //first line: qnt of agents inside the group
                    //second line: group info
                    int qntGr = System.Int32.Parse(qntGroupsInput.GetComponent<InputField>().text);
                    int qntAg = System.Int32.Parse(qntAgentsInput.GetComponent<InputField>().text);
                    for (int i = 0; i < qntGr; i++)
                    {
                        //sort out a number of agents
                        int sortQntAg = qntAg / qntGr;

                        //if it is the last group, just take the remaining agents
                        if (i == qntGr - 1)
                        {
                            sortQntAg = qntAg - (sortQntAg * i);
                        }

                        //write first line
                        newText += sortQntAg + breakLine;

                        //write second line
                        //@TODO: get this info too
                        newText += "cell25-25;false;Goal4;0.359452;Goal3;0.0276675;Goal2;0.675362;Goal1;0.557535" + breakLine;

                        //break line
                        newText += breakLine;
                    }

                    //already saved, get out
                    break;
                }
                else
                {
                    //add a blank line too
                    newText += breakLine;
                }
            }
            while (line != null);
        }
        theReader.Close();

        //now, write the new string
        StreamWriter theWriter = File.CreateText(Application.dataPath + "/" + agentsFilename);

        //update
        theWriter.Write(newText);

        //close
        theWriter.Close();
    }

    //save hofstede file
    private void SaveHofstedeFile()
    {
        StreamReader theReader = new StreamReader(Application.dataPath + "/" + hofFilename, System.Text.Encoding.Default);
        string line;
        string newText = "";
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
                        //just rewrite and keep going
                        newText += line + breakLine;
                        continue;
                    }

                    //for each group, create a new hof for it
                    int qntGr = System.Int32.Parse(qntGroupsInput.GetComponent<InputField>().text);
                    for (int i = 0; i < qntGr; i++)
                    {
                        newText += "0;" + inputMAS.GetComponent<InputField>().text + ";" + inputLTO.GetComponent<InputField>().text + ";" + inputING.GetComponent<InputField>().text
                            + ";0" + breakLine;
                    }

                    //already saved, get out
                    break;
                }
                else
                {
                    //add a blank line too
                    newText += breakLine;
                }
            }
            while (line != null);
        }
        theReader.Close();

        //now, write the new string
        StreamWriter theWriter = File.CreateText(Application.dataPath + "/" + hofFilename);

        //update
        theWriter.Write(newText);

        //close
        theWriter.Close();
    }

    //save durupinar file
    private void SaveDurupinarFile()
    {
        StreamReader theReader = new StreamReader(Application.dataPath + "/" + duruFilename, System.Text.Encoding.Default);
        string line;
        string newText = "";
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
                        //just rewrite and keep going
                        newText += line + breakLine;
                        continue;
                    }

                    //for each group, create a new group. For each agent in the group, create its OCEAN
                    int qntGr = System.Int32.Parse(qntGroupsInput.GetComponent<InputField>().text);
                    int qntAg = System.Int32.Parse(qntAgentsInput.GetComponent<InputField>().text);
                    for (int i = 0; i < qntGr; i++)
                    {
                        newText += "Group" + (i + 1) + breakLine;

                        //sort out a number of agents
                        int sortQntAg = qntAg / qntGr;

                        //if it is the last group, just take the remaining agents
                        if(i == qntGr - 1)
                        {
                            sortQntAg = qntAg - (sortQntAg * i);
                        }

                        for (int j = 0; j < sortQntAg; j++)
                        {
                            newText += inputO.GetComponent<InputField>().text + "," + inputC.GetComponent<InputField>().text + "," + inputE.GetComponent<InputField>().text + "," 
                                + inputA.GetComponent<InputField>().text + "," + inputN.GetComponent<InputField>().text + breakLine;
                        }

                        //break line
                        newText += breakLine;
                    }

                    //already saved, get out
                    break;
                }
                else
                {
                    //add a blank line too
                    newText += breakLine;
                }
            }
            while (line != null);
        }
        theReader.Close();

        //now, write the new string
        StreamWriter theWriter = File.CreateText(Application.dataPath + "/" + duruFilename);

        //update
        theWriter.Write(newText);

        //close
        theWriter.Close();
    }

    //load a file text
    public void LoadFileText(string fileName)
    {
        //read the file
        StreamReader theReader = new StreamReader(Application.dataPath + "/" + fileName, System.Text.Encoding.Default);
        string newText = theReader.ReadToEnd();
        theReader.Close();

        //place on the text element
        fileText.GetComponent<InputField>().text = newText;

        //show the text stuff
        fileText.SetActive(true);

        //set the upper file name
        this.fileName = fileName;

        //Toggle buttons
        saveButton.SetActive(true);
        startButton.SetActive(false);
    }

    //save a file text
    public void SaveFileText()
    {
        //write the new string
        StreamWriter theWriter = File.CreateText(Application.dataPath + "/" + fileName);

        //update
        theWriter.Write(fileText.GetComponent<InputField>().text);

        //close
        theWriter.Close();

        //hide file text
        fileText.SetActive(false);

        //Toggle buttons
        saveButton.SetActive(false);
        startButton.SetActive(true);
    }

    //show hide panel and go to pre-compile
    public void ShowWaitPanel()
    {
        //show wait panel
        waitPanel.SetActive(true);

        StartCoroutine(PreCompile(1));
    }

    //pre-compile the scenario, saving in config filename
    public IEnumerator PreCompile(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        //environment size
        int envSizeX = System.Int32.Parse(sizeX.GetComponent<InputField>().text);
        int envSizeZ = System.Int32.Parse(sizeZ.GetComponent<InputField>().text);

        //load the obstacles
        StreamReader theReader = new StreamReader(Application.dataPath + "/" + obsFilename, System.Text.Encoding.Default);
        string line;
        int qntObstacles = 0;
        int qntVertices = 0;
        int qntTriangles = 0;
        Vector3[] vertices = new Vector3[qntVertices];
        int[] triangles = new int[qntTriangles];

        //all obstacles
        GameObject[] allObs;
        //all cells
        GameObject[] allCells;

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

                    //line 1 = qntObstacles
                    if (lineCount == 1)
                    {
                        string[] info = line.Split(':');
                        qntObstacles = System.Int32.Parse(info[1]);
                    }//else, if the line is "Obstacle", it is a new obstacle, so reset vertices
                    else if (line == "Obstacle")
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
                        if (qntVertices > 0)
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
                            if (qntTriangles == 0)
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
        allObs = GameObject.FindGameObjectsWithTag("Obstacle");

        //create the cells
        //first of all, create all cells (with this scene and this agentRadius)
        //since radius = 1; diameter = 2. So, iterate cellRadius*2
        //if the radius varies, this 2 operations adjust the cells
        Vector3 newPosition = new Vector3(cell.transform.position.x * cellRadius,
            cell.transform.position.y * cellRadius, cell.transform.position.z * cellRadius);
        Vector3 newScale = new Vector3(cell.transform.localScale.x * cellRadius,
            cell.transform.localScale.y * cellRadius, cell.transform.localScale.z * cellRadius);

        for (float i = cellRadius; i < envSizeX; i = i + cellRadius * 2)
        {
            for (float j = cellRadius; j < envSizeZ; j = j + cellRadius * 2)
            {
                
                //instantiante a new cell
                GameObject newCell = Instantiate(cell, new Vector3(newPosition.x + (i - cellRadius), newPosition.y, newPosition.z + (j - cellRadius)), Quaternion.identity) as GameObject;
                //change his name
                newCell.name = "cell" + i + "-" + j;
                //change scale
                newCell.transform.localScale = newScale;
                //start list
                //newCell.GetComponent<CellController>().StartList();
            }
        }
        allCells = GameObject.FindGameObjectsWithTag("Cell");

        //create the markers
        //lets set the qntAuxins for each cell according the density estimation
        float PORC_QTD_Marcacoes = (System.Int32.Parse(markerDensity.GetComponent<InputField>().text)) / 100.0f;
        float densityToQnt = PORC_QTD_Marcacoes;

        densityToQnt *= (cellRadius * 2f) / (2.0f * auxinRadius);
        densityToQnt *= (cellRadius * 2f) / (2.0f * auxinRadius);

        qntAuxins = (int)Mathf.Floor(densityToQnt);
        //Debug.Log(qntAuxins);

        //for each cell, we generate his auxins
        for (int c = 0; c < allCells.Length; c++)
        {
            //Dart throwing auxins
            DartThrowMarkers(c, allCells);

            //Debug.Log(allCells[c].name + " Done!");
        }

        //save config file
        SaveConfigFile(envSizeX, envSizeZ);

        //after all, clear the stuff
        ClearScene();

        //hide wait panel
        waitPanel.SetActive(false);
    }

    //save a csv config file
    //files saved: Config.csv
    public void SaveConfigFile(int envSizeX, int envSizeZ)
    {
        //config file
        StreamWriter file = File.CreateText(Application.dataPath + "/" + configFilename);

        //first, we save the terrain dimensions
        file.WriteLine("terrainSize:" + envSizeX + "," + envSizeZ);

        //then, camera position and height
        file.WriteLine("camera:" + (envSizeX / 2) + "," + 10 + "," + (envSizeZ / 2) + "," + ((envSizeX + envSizeZ) / 2) / 2);
        
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
    }

    //dart throwing markers
    private void DartThrowMarkers(int c, GameObject[] allCells)
    {
        //use this flag to break the loop if it is taking too long (maybe there is no more space)
        int flag = 0;
        for (int i = 0; i < qntAuxins; i++)
        {
            float x = Random.Range(allCells[c].transform.position.x - cellRadius, allCells[c].transform.position.x + cellRadius);
            float z = Random.Range(allCells[c].transform.position.z - cellRadius, allCells[c].transform.position.z + cellRadius);

            //see if there are auxins in this radius. if not, instantiante
            List<AuxinController> allAuxinsInCell = allAuxins;
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
                allAuxins.Add(newAuxin);

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

    //check if there is Obstacles or something on a given position
    private bool CheckObstacle(Vector3 checkPosition, string tag, float radius)
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

    //clear the scenario
    private void ClearScene()
    {
        //clear goals
        GameObject[] goalsToClear = GameObject.FindGameObjectsWithTag("Goal");
        foreach (GameObject gtc in goalsToClear)
        {
            DestroyImmediate(gtc);
        }

        //clear signs
        GameObject[] signsToClear = GameObject.FindGameObjectsWithTag("Sign");
        foreach (GameObject stc in signsToClear)
        {
            DestroyImmediate(stc);
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

    //draw each obstacle
    private void DrawObstacle(Vector3[] vertices, int[] triangles)
    {
        GameObject go = new GameObject();

        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        MeshFilter mf = go.GetComponent<MeshFilter>();
        var mesh = new Mesh();
        mf.mesh = mesh;

        //set the vertices
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        //obstacle has center at 0x0, so, need to place it obstacleDisplacement forward
        go.transform.position = new Vector3(go.transform.position.x, 0, go.transform.position.z);

        go.AddComponent<MeshCollider>();
        //go.GetComponent<MeshCollider>().isTrigger = true;
        go.tag = "Obstacle";
        go.name = "Obstacle";

        //change the static navigation to draw it dinamically
        //GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.NavigationStatic);
        //GameObjectUtility.SetNavMeshArea(go, 1);
    }
}
