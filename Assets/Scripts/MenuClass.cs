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
    //load config file?
    public bool loadConfigFile = false;
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

    private void Awake()
    {
        hofPanel.SetActive(false);
        duruPanel.SetActive(false);
        comfortsPanel.SetActive(false);
    }

    //toggle the loadConfigFile
    public void ToggleLoadConfigFile()
    {
        loadConfigFile = !loadConfigFile;

        //(de)active options according load config file
        options.SetActive(!loadConfigFile);
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
        if (!loadConfigFile)
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
                            newText += line + "\r\n";
                            continue;
                        }

                        //split
                        string[] entries = line.Split(':');

                        switch (entries[0])
                        {
                            /*case "LoadConfigFile":
                                newText += "LoadConfigFile:" + loadConfigFile + "\r\n";

                                break;*/
                            case "QntAgents":
                                int qntAg = System.Int32.Parse(qntAgentsInput.GetComponent<InputField>().text);

                                //write
                                newText += "QntAgents:" + qntAg + "\r\n";
                                break;
                            case "QntGroups":
                                int qntGr = System.Int32.Parse(qntGroupsInput.GetComponent<InputField>().text);

                                //write
                                newText += "QntGroups:" + qntGr + "\r\n";
                                break;
                            case "ExploratoryBehavior":
                                newText += "ExploratoryBehavior:" + ebInput.GetComponent<Toggle>().isOn + "\r\n";

                                break;
                            case "GroupBehavior":
                                newText += "GroupBehavior:" + gbInput.GetComponent<Toggle>().isOn + "\r\n";

                                break;
                            case "PaintHeatMap":
                                newText += "PaintHeatMap:" + hmInput.GetComponent<Toggle>().isOn + "\r\n";

                                break;
                            case "ThermalComfort":
                                newText += "ThermalComfort:" + tcInput.GetComponent<Toggle>().isOn + "\r\n";

                                break;
                            case "DensityComfort":
                                newText += "DensityComfort:" + dcInput.GetComponent<Toggle>().isOn + "\r\n";

                                break;
                            case "UseHofstede":
                                newText += "UseHofstede:" + hofInput.GetComponent<Toggle>().isOn + "\r\n";

                                break;
                            case "UseDurupinar":
                                newText += "UseDurupinar:" + duruInput.GetComponent<Toggle>().isOn + "\r\n";

                                break;
                            case "UseFavaretto":
                                newText += "UseFavaretto:" + favInput.GetComponent<Toggle>().isOn + "\r\n";

                                break;
                            default:
                                //default just write original
                                newText += line + "\r\n";
                                break;
                        }
                    }
                    else
                    {
                        //add a blank line too
                        newText += "\r\n";
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
                            newText += line + "\r\n";
                            continue;
                        }

                        //split
                        string[] entries = line.Split(':');

                        switch (entries[0])
                        {
                            case "LoadConfigFile":
                                newText += "LoadConfigFile:" + loadConfigFile.ToString() + "\r\n";

                                break;
                            default:
                                //default just write original
                                newText += line + "\r\n";
                                break;
                        }
                    }
                    else
                    {
                        //add a blank line too
                        newText += "\r\n";
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
                        newText += line + "\r\n";
                        continue;
                    }

                    //for each group, create a new hof for it
                    int qntGr = System.Int32.Parse(qntGroupsInput.GetComponent<InputField>().text);
                    for (int i = 0; i < qntGr; i++)
                    {
                        newText += "0;" + inputMAS.GetComponent<InputField>().text + ";" + inputLTO.GetComponent<InputField>().text + ";" + inputING.GetComponent<InputField>().text
                            + ";0";
                    }

                    //already saved, get out
                    break;
                }
                else
                {
                    //add a blank line too
                    newText += "\r\n";
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
                        newText += line + "\r\n";
                        continue;
                    }

                    //for each group, create a new group. For each agent in the group, create its OCEAN
                    int qntGr = System.Int32.Parse(qntGroupsInput.GetComponent<InputField>().text);
                    int qntAg = System.Int32.Parse(qntAgentsInput.GetComponent<InputField>().text);
                    for (int i = 0; i < qntGr; i++)
                    {
                        newText += "Group" + (i + 1) + "\r\n";

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
                                + inputA.GetComponent<InputField>().text + "," + inputN.GetComponent<InputField>().text + "\r\n";
                        }

                        //break line
                        newText += "\r\n";
                    }

                    //already saved, get out
                    break;
                }
                else
                {
                    //add a blank line too
                    newText += "\r\n";
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
}
