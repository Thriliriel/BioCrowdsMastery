using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TimeMachine
{
	public int agents, groups, i, ag, obst, ob, time, n_obj, j, agents_region;
	public float diam_ag, tot_area, weight_obs, obs_area = 0, alpha, C_g, area_obj, area_region, each_ag_area, densidade_ag, erro;
	public Dictionary<int, float> vel, C_l;
	public Dictionary<int, int> frame, ag_group;
	public Dictionary<int, Vector3> goal, pos, pos_est, pos_C, pos_P, obst_v0, obst_v1, obst_v2, obst_v3, delta, unit_vec,
		region_obst_menor, region_obst_maior;
	private bool readingObstacles;

	// Start is called before the first frame update
	public void StartClass()
    {
		vel = new Dictionary<int, float>();
		C_l = new Dictionary<int, float>();
		goal = new Dictionary<int, Vector3>();
		pos = new Dictionary<int, Vector3>();
		pos_est = new Dictionary<int, Vector3>();
		pos_C = new Dictionary<int, Vector3>();
		pos_P = new Dictionary<int, Vector3>();
		obst_v0 = new Dictionary<int, Vector3>();
		obst_v1 = new Dictionary<int, Vector3>();
		obst_v2 = new Dictionary<int, Vector3>();
		obst_v3 = new Dictionary<int, Vector3>();
		delta = new Dictionary<int, Vector3>();
		unit_vec = new Dictionary<int, Vector3>();
		region_obst_menor = new Dictionary<int, Vector3>();
		region_obst_maior = new Dictionary<int, Vector3>();
		frame = new Dictionary<int, int>();
		ag_group = new Dictionary<int, int>();
		readingObstacles = false;

		//ReadFile();
		//CalculateTravel();
		//WriteFile();
	}

	public void JumpTime()
	{
		ReadFile();
		CalculateTravel();
		WriteFile();
	}

	private void ReadFile()
	{
		//read the input file
		StreamReader readingLTM = new StreamReader(Application.dataPath + "/CLIC.txt", System.Text.Encoding.Default);
		using (readingLTM)
		{
			string line;
			do
			{
				line = readingLTM.ReadLine();

				if (line != "" && line != null)
				{
					//skip comments
					if (line.Contains("#") || line.Contains("POSITIONX") || line.Contains("ENVIRONMENT")) continue;

					line = line.Trim();

					string[] info = line.Split(' ');

					//qnt agent
					if (line.Contains("AGENTS"))
					{
						agents = int.Parse(info[1]);
					}
					else if (line.Contains("DIAM_AG"))
					{
						diam_ag = float.Parse(info[1]);
					}
					else if (line.Contains("GROUPS"))
					{
						groups = int.Parse(info[1]);
					}
					else if (line.Contains("TOTAL_AREA"))
					{
						tot_area = float.Parse(info[1]);
					}
					else if (line.Contains("WEIGHT_OBSTACLES"))
					{
						weight_obs = float.Parse(info[1]);
					}
					else if (line.Contains("OBSTACLES"))
					{
						obst = int.Parse(info[1]);
						readingObstacles = true;
					}
					else if (!readingObstacles)
					{
						int idAgent = int.Parse(info[0]);

						ag_group.Add(idAgent, int.Parse(info[1]));
						frame.Add(idAgent, int.Parse(info[2]));
						vel.Add(idAgent, float.Parse(info[3]));
						goal.Add(idAgent, new Vector3(float.Parse(info[4]), float.Parse(info[5]), float.Parse(info[6])));
						pos.Add(idAgent, new Vector3(float.Parse(info[7]), float.Parse(info[8]), float.Parse(info[9])));
					}
					else if (readingObstacles)
					{
						int idObst = int.Parse(info[0]);

						obst_v0.Add(idObst, new Vector3(float.Parse(info[1]), 0, float.Parse(info[2])));
						obst_v1.Add(idObst, new Vector3(float.Parse(info[3]), 0, float.Parse(info[4])));
						obst_v2.Add(idObst, new Vector3(float.Parse(info[5]), 0, float.Parse(info[6])));
						obst_v3.Add(idObst, new Vector3(float.Parse(info[7]), 0, float.Parse(info[8])));
					}
				}
			} while (line != null);
		}
		readingLTM.Close();
	}

	private float Maior(float x, float y)
	{
		if (x > y) return x;
		else return y;
	}

	private float Menor(float x, float y)
	{
		if (x < y) return x;
		else return y;
	}

	private void CalculateTravel()
	{
		/* Calcula vetores de movimento*/
		for (i = 0; i < agents; i++)
		{
			delta.Add(i, new Vector3(goal[i].x - pos[i].x, goal[i].y - pos[i].y, goal[i].z - pos[i].z));
			unit_vec.Add(i, new Vector3(
				delta[i].x / Mathf.Sqrt(((goal[i].x - pos[i].x) * (goal[i].x - pos[i].x)) + ((goal[i].y - pos[i].y) * (goal[i].y - pos[i].y)) +
				((goal[i].z - pos[i].z) * (goal[i].z - pos[i].z))),
				delta[i].y / Mathf.Sqrt(((goal[i].x - pos[i].x) * (goal[i].x - pos[i].x)) + ((goal[i].y - pos[i].y) * (goal[i].y - pos[i].y)) +
				((goal[i].z - pos[i].z) * (goal[i].z - pos[i].z))),
				delta[i].z / Mathf.Sqrt(((goal[i].x - pos[i].x) * (goal[i].x - pos[i].x)) + ((goal[i].y - pos[i].y) * (goal[i].y - pos[i].y)) +
				((goal[i].z - pos[i].z) * (goal[i].z - pos[i].z)))
				));
		}

		float dist1, dist2, dist3;
		/* Calcula area obstacles*/
		obs_area = 0;
		for (i = 0; i < obst; i++)
		{
			dist1 = Vector3.Distance(obst_v0[i], obst_v1[i]);
			dist2 = Vector3.Distance(obst_v0[i], obst_v2[i]);
			dist3 = Vector3.Distance(obst_v0[i], obst_v3[i]);

			if ((dist1 > dist2) && (dist1 > dist3)) obs_area += dist2 * dist3;
			else
				if ((dist2 > dist1) && (dist2 > dist3)) obs_area += dist1 * dist3;
			else
				obs_area += dist1 * dist2;
		}

		/*Calcula Complexidade global*/
		alpha = 100 / 100; //Paulo: what the hell Clic? =D
		each_ag_area = 2.141681f * (diam_ag / 2) * (diam_ag / 2);
		//alpha = (obst*obs_area)*((weight_obs / 100)*(weight_obs / 100));
		//alpha = (obst*obs_area)*((weight_obs / 100)*(weight_obs / 100));
		C_g = ((agents * each_ag_area) + (alpha * obst)) / ((tot_area - obs_area) + 0.001f);
		//C_g = ((alpha*obst)) / ((tot_area - obs_area) + 0.001);

		float val_rand;
		int val;

		//erro= 0.0285; /* entre 0 e 1*/
		erro = 0.0285f * Mathf.Exp(0.34f * (agents / 80)); /* entre 0 e 1*/

		Random.InitState(50);

		/*Calcula posições estimadas*/
		for (i = 0; i < agents; i++)
		{
			val = 8;
			//val_rand = Random.Range(0,9) / 10.0;
			val_rand = (Random.Range(0, 9) / (8 + 1) * 2) - 1.0f;

			pos_est.Add(i, new Vector3(
				pos[i].x + ((vel[i] * (1.0f - erro)) * unit_vec[i].x) * (time - frame[i]),
				pos[i].y + ((vel[i] * (1.0f - erro)) * unit_vec[i].y) * (time - frame[i]),
				pos[i].z + ((vel[i] * (1.0f - erro)) * unit_vec[i].z) * (time - frame[i])
				));

			region_obst_menor.Add(i, new Vector3(Menor(pos_est[i].x, pos[i].x),
				Menor(pos_est[i].y, pos[i].y),
				Menor(pos_est[i].z, pos[i].z)));

			region_obst_maior.Add(i, new Vector3(Maior(pos_est[i].x, pos[i].x),
				Maior(pos_est[i].y, pos[i].y),
				Maior(pos_est[i].z, pos[i].z)));
		}

		/*Calcula posições estimadas com Complexidade global C*/
		for (i = 0; i<agents; i++)
		{
			pos_C.Add(i, new Vector3(
				pos_est[i].x - ((vel[i] * unit_vec[i].x) * (time - frame[i]) * C_g),
				pos_est[i].y - ((vel[i] * unit_vec[i].y) * (time - frame[i]) * C_g),
				pos_est[i].z - ((vel[i] * unit_vec[i].z) * (time - frame[i]) * C_g)
				));
		}
	}

	//crtl K C -> comment all lines
	private void WriteFile()
	{
		StreamWriter writingResult;
		writingResult = File.CreateText(Application.dataPath + "/CLIC.out");

		int k, cont = 0;
		float ag_rand, r = 0, penaliza, dens, acc_r = 0;
		Vector3 p1, p2;

		for (i = 0; i < agents; i++)
		{
			p1 = new Vector3(pos_est[i].x - 1.0f, pos_est[i].y - 1.0f, 0);
			p2 = new Vector3(pos_est[i].x + 1.0f, pos_est[i].y + 1.0f, 0);

			/*Calcula Densidade*/
			densidade_ag = 0;
			for (j = 0; j < agents; j++)
			{
				densidade_ag += AgentInsideArea(pos_C[j], p1, p2);
			}

			dens = densidade_ag / 4;

			//abre arquivo density
			StreamReader readingLTM = AbreArquivo(dens);

			//o arquivo tem 5000 valores, então vamos sortear um valor aleatorio
			int chosenOne = Random.Range(1, 5000);
			int contChosen = 0;

			if (readingLTM != null)
			{
				using (readingLTM)
				{
					string line;
					do
					{
						line = readingLTM.ReadLine();

						if (line != "" && line != null)
						{
							//skip comments
							if (line.Contains("#")) continue;

							contChosen++;

							//se não é o escolhido, pula
							if (chosenOne != contChosen) continue;

							//nao entendi o que ela pega desse arquivo, vou soh pegar tudo e por enquanto era isso =D
							//UPDATE: pelo que a Soraia explicou, esse arquivo tem todos os possíveis valores (de Weibul, i guess). O que faz é pegar um deles aleatoriamente.
							r = float.Parse(line);

							//como ja pegamos o escolhido, bye
							break;
						}
					} while (line != null);
				}
				readingLTM.Close();

				acc_r += r;
				cont++;

				r = r / 25;
				penaliza = ((time - frame[i]) / 24) * r;
			}
			else
			{
				penaliza = 0;
			}

			pos_C[i] = new Vector3(
				(((vel[i] * (1.0f - erro)) * unit_vec[i].x) * (time - frame[i]) * C_g),
				(((vel[i] * (1.0f - erro)) * unit_vec[i].y) * (time - frame[i]) * C_g),
				(((vel[i] * (1.0f - erro)) * unit_vec[i].z) * (time - frame[i]) * C_g)
				);

			pos_P.Add(i, new Vector3(
				pos_est[i].x - pos_C[i].x - penaliza * unit_vec[i].x,
				pos_est[i].y - pos_C[i].y - penaliza * unit_vec[i].y,
				pos_est[i].z - pos_C[i].z - penaliza * unit_vec[i].z
				));
		}

		writingResult.WriteLine(C_g);
		for (i = 0; i < agents; i++)
		{
			writingResult.WriteLine(i + " " + unit_vec[i].x);
		}

		for (i = 0; i < agents; i++)
		{
			writingResult.WriteLine(i + " " + pos_est[i].x + " " + pos_est[i].y + " " + pos_est[i].z);
		}

		for (i = 0; i < agents; i++)
		{
			writingResult.WriteLine(i + " " + pos_C[i].x + " " + pos_C[i].y + " " + pos_C[i].z);
		}

		writingResult.WriteLine("USING");

		for (i = 0; i < agents; i++)
		{
			writingResult.WriteLine(i + " " + pos_P[i].x + " " + pos_P[i].y + " " + pos_P[i].z);
		}

		//writingResult.WriteLine(acc_r + " " + cont); //nobody cares =D

		writingResult.Close();
	}

	private StreamReader AbreArquivo(float dens)
	{
		StreamReader read = null;
		if (dens <= 0.25f) return null;
		else if(dens <= 0.5f)
		{
			read = new StreamReader(Application.dataPath + "/Dens0.5.txt", System.Text.Encoding.Default);
		}
		else if (dens <= 0.75f)
		{
			read = new StreamReader(Application.dataPath + "/Dens0.75.txt", System.Text.Encoding.Default);
		}
		else if (dens <= 1f)
		{
			read = new StreamReader(Application.dataPath + "/Dens1.txt", System.Text.Encoding.Default);
		}
		else if (dens <= 1.25f)
		{
			read = new StreamReader(Application.dataPath + "/Dens1.25.txt", System.Text.Encoding.Default);
		}
		else if (dens <= 1.5f)
		{
			read = new StreamReader(Application.dataPath + "/Dens1.5.txt", System.Text.Encoding.Default);
		}
		else if (dens <= 1.75f)
		{
			read = new StreamReader(Application.dataPath + "/Dens1.75.txt", System.Text.Encoding.Default);
		}
		else if (dens <= 2)
		{
			read = new StreamReader(Application.dataPath + "/Dens2.txt", System.Text.Encoding.Default);
		}
		else
		{
			read = new StreamReader(Application.dataPath + "/Dens2.25.txt", System.Text.Encoding.Default);
		}

		return read;
	}

	private int AgentInsideArea(Vector3 pos_C, Vector3 p1, Vector3 p2)
	{
		if ((pos_C.x >= Menor(p1.x, p2.x)) && (pos_C.x <= Maior(p1.x, p2.x)) &&
			(pos_C.y >= Menor(p1.y, p2.y)) && (pos_C.y <= Maior(p1.y, p2.y)))
			return (1);
		else
			return (0);
	}
}
