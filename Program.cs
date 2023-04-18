using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Text;

public class Program
{
	struct CellularAutomata {
		public bool IsAlive;
		public bool Immutable;
		public int MapType;
		public float ContinuousType;
		public float CellSize; 
		public float z;
		public bool Ambivalent;
		public int CellColor;

		public CellularAutomata Copy()
		{
			CellularAutomata copy = new CellularAutomata();
			copy.IsAlive = this.IsAlive;
			copy.Immutable = this.Immutable;
			copy.MapType = this.MapType;
			copy.ContinuousType = this.ContinuousType;
			copy.CellSize = this.CellSize; 
			copy.Ambivalent = this.Ambivalent;
			copy.CellColor = this.CellColor;
			return copy;
		}
	};
	
	public int WIDTH = 64;
	public int HEIGHT = 64;
	public int AUTOMATAITERATIONS = 5;
	public float accessibleCells = 0.8f;
	public int birthLimit = 4;
	public int deathLimit = 3;
	public int executionNumber = 0;
	public bool enableAmbivalence = false;
	public bool enableImmutability = false;
	public int testIterations = 10000;

	Stopwatch stopwatch = new Stopwatch();
	List<List<CellularAutomata>> listMap = new List<List<CellularAutomata>>();
	HashSet<Tuple<int,int>> localFloodVisited = new HashSet<Tuple<int,int>>();

	public string filePath = Directory.GetCurrentDirectory();
	public string method = "";
	
	static void Main()
	{
		Program myProgram = new Program();
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
		myProgram.InitializeListMap();

		// while(myProgram.executionNumber < myProgram.testIterations)
		// {
		// 	myProgram.stopwatch = new Stopwatch();
		// 	myProgram.stopwatch.Start();
		// 	myProgram.listMap = new List<List<CellularAutomata>>();
		// 	myProgram.method = "Neumann";
		// 	myProgram.listMap = myProgram.Start(myProgram.method);
		// 	myProgram.ArticleStats();
		// 	myProgram.executionNumber++;
		// }

		// myProgram.executionNumber = 0;
		// while(myProgram.executionNumber < myProgram.testIterations)
		// {
		// 	myProgram.stopwatch = new Stopwatch();
		// 	myProgram.stopwatch.Start();
		// 	myProgram.listMap = new List<List<CellularAutomata>>();
		// 	myProgram.method = "Diagonal";
		// 	myProgram.Start(myProgram.method);
		// 	myProgram.ArticleStats();
		// 	myProgram.executionNumber++;
		// }
		
		myProgram.executionNumber = 0;
		while(myProgram.executionNumber < myProgram.testIterations)
		{
			myProgram.stopwatch = new Stopwatch();
			myProgram.stopwatch.Start();
			myProgram.listMap = new List<List<CellularAutomata>>();
			myProgram.method = "Moore";
			myProgram.Start(myProgram.method);
			myProgram.ArticleStats();
			myProgram.executionNumber++;
		}
	}

	int GetSurroundingWallCountMoore(int gridX, int gridY)
	{
		int wallCount = 0;
		if(gridX-1 > -1) if(listMap[gridX-1][gridY].IsAlive) wallCount++;
		if(gridX+1 < WIDTH) if(listMap[gridX+1][gridY].IsAlive) wallCount++;
		if(gridY-1 > -1) if(listMap[gridX][gridY-1].IsAlive) wallCount++;
		if(gridY+1 < HEIGHT) if(listMap[gridX][gridY+1].IsAlive) wallCount++;
		if(gridX-1 > -1 && gridY-1 > -1) if(listMap[gridX-1][gridY-1].IsAlive) wallCount++;
		if(gridX+1 < WIDTH && gridY+1 < HEIGHT) if(listMap[gridX+1][gridY+1].IsAlive) wallCount++;
		if(gridX+1 < WIDTH && gridY-1 > -1) if(listMap[gridX+1][gridY-1].IsAlive) wallCount++;
		if(gridX-1 > -1 && gridY+1 < HEIGHT) if(listMap[gridX-1][gridY+1].IsAlive) wallCount++;
		return wallCount;
	}
	int GetSurroundingWallCountNeumann(int gridX, int gridY)
	{
		int wallCount = 0;
		if(gridX-1 > -1) if(listMap[gridX-1][gridY].IsAlive) wallCount++;
		if(gridX+1 < WIDTH) if(listMap[gridX+1][gridY].IsAlive) wallCount++;
		if(gridY-1 > -1) if(listMap[gridX][gridY-1].IsAlive) wallCount++;
		if(gridY+1 < HEIGHT) if(listMap[gridX][gridY+1].IsAlive) wallCount++;
		return wallCount;
	}

	int GetSourroundingWallDiagonal(int gridX, int gridY)
	{
		int wallCount = 0;
		if(gridX-1 > -1 && gridY-1 > -1) if(listMap[gridX-1][gridY-1].IsAlive) wallCount++;
		if(gridX+1 < WIDTH && gridY+1 < HEIGHT) if(listMap[gridX+1][gridY+1].IsAlive) wallCount++;
		if(gridX+1 < WIDTH && gridY-1 > -1) if(listMap[gridX+1][gridY-1].IsAlive) wallCount++;
		if(gridX-1 > -1 && gridY+1 < HEIGHT) if(listMap[gridX-1][gridY+1].IsAlive) wallCount++;
		return wallCount;
	}
	
	List<List<CellularAutomata>> NextMapGeneration(List<List<CellularAutomata>> listMap, int MaximumDeadCells, int MaximumLiveCells){
		List<List<CellularAutomata>> newMap = new List<List<CellularAutomata>>();
		CellularAutomata tempCA = new CellularAutomata();

		for (int x = 0; x < WIDTH; x++){
			newMap.Add(new List<CellularAutomata>());
			for (int y = 0; y < HEIGHT; y++){
				newMap[x].Add(listMap[x][y].Copy());
			}
		}

		for (int x = 0; x < WIDTH; x++){
			for (int y = 0; y < HEIGHT; y++){
				int aliveNeighborCount = GetSurroundingWallCountNeumann(x, y);
				// Console.Write(aliveNeighborCount + "\n");
				// Console.Write(MaximumLiveCells+" "+MaximumDeadCells+"\n");
				if (!listMap[x][y].IsAlive && aliveNeighborCount == 3 && MaximumLiveCells < (int)(WIDTH*HEIGHT*0.80))
				{ // Reproduction rule: 3 neighbors alive, cell becomes alive
						tempCA = newMap[x][y];
						tempCA.IsAlive = true;
						tempCA.CellColor = (int) new Random().NextInt64(1,5);
						newMap[x][y] = tempCA;
						MaximumLiveCells++;
						MaximumDeadCells--;
				}
				else if (MaximumDeadCells < (int)(WIDTH*HEIGHT*0.25) && listMap[x][y].IsAlive && (aliveNeighborCount < 2 || aliveNeighborCount > 4))
				{ // Underpopulation and Overpopulation rule: cell dies
						tempCA = newMap[x][y];
						tempCA.IsAlive = false;
						newMap[x][y] = tempCA;
						MaximumDeadCells++;
						MaximumLiveCells--;
				} else 
				{ // Continues at the same state
					newMap[x][y] = listMap[x][y];
				}
			}
		}
		return newMap;
	}
	
	void GeneratePNG(int number)
	{
		// string content = "";
		// int count = 0;

		string path = Path.Combine(Directory.GetCurrentDirectory(), method);
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		path = Path.Combine(Directory.GetCurrentDirectory()+"\\"+method, ""+executionNumber);
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		byte[] bytes = new byte[WIDTH*HEIGHT];
		string toBeWritten = "";
		for (int x = 0; x < WIDTH; x++)
		{
			for(int y = 0; y < HEIGHT; y++)
			{
				/*
				using (StreamWriter writer = new StreamWriter(filePath+"\\"+method+"\\"+executionNumber+"\\"+"bitmap-"+number+".txt",true))
				{
				*/
					if(listMap[x][y].CellColor == 0 || !listMap[x][y].IsAlive)
					{
						// writer.WriteLine(
						// 	"0.0 " +
						// 	"0.0 " +
						// 	"0.0 "
						// );
						toBeWritten += 
							"0.0 " +
							"0.0 " +
							"0.0 " + 
						"\n";
					}
					if(listMap[x][y].CellColor == 1 && listMap[x][y].IsAlive)
					{
						// writer.WriteLine(
							// "0.0 " +
							// "0.0 " +
							// "1.0 "
						// );
						toBeWritten += 
							"0.0 " +
							"0.0 " +
							"1.0 " +
						"\n";
					}
					if(listMap[x][y].CellColor == 2 && listMap[x][y].IsAlive)
					{
						// writer.WriteLine(
							// "0.0 " +
							// "1.0 " +
							// "0.0 "
						// );
						toBeWritten += 
							"0.0 " +
							"1.0 " +
							"0.0 " +
						"\n";
					}
					if(listMap[x][y].CellColor == 3 && listMap[x][y].IsAlive)
					{
						// writer.WriteLine(
							// "1.0 " +
							// "1.0 " +
							// "1.0 "
						// );
						toBeWritten += 
							"1.0 " +
							"1.0 " +
							"1.0 " +
						"\n";
					}
					if(listMap[x][y].CellColor == 4 && listMap[x][y].IsAlive)
					{
						// writer.WriteLine(
							// "0.5 " +
							// "0.5 " +
							// "0.5 "
						// );
						toBeWritten += 
							"0.5 " +
							"0.5 " +
							"0.5 " +
						"\n";
					}
					// count++;
					// if(x == WIDTH - 1 && y == HEIGHT -1) writer.Close();
				//}
			}
			// using (StreamWriter writer = new StreamWriter(filePath+"\\"+method+"\\"+executionNumber+"\\"+"bitmap-"+number+".txt",true))
			// {
			// 	writer.WriteLine(toBeWritten);
			// 	writer.Close();
			// }
			// toBeWritten = "";
		}
	}
	
	int countAliveCells(List<List<CellularAutomata>> listMap)
	{
		int count = 0;
		for (int x = 0; x < WIDTH; x++){
			for (int y = 0; y < HEIGHT; y++){
				if(listMap[x][y].IsAlive) count++;
			}
		}
		return count;
	}

	List<List<CellularAutomata>> Start(string method)
	{
		int image_number = 0;
		float aliveFactor = 0.5f;
		CellularAutomata ca = new CellularAutomata();
		// false : true;
		int MaximumDeadCells =  0;
		int MaximumLiveCells = 0;
		for(int x = 0; x < WIDTH; x++)
		{
			listMap.Add(new List<CellularAutomata>());
			for(int y = 0; y < HEIGHT; y++)
			{
				ca.IsAlive = aliveFactor < (float)((new Random()).NextDouble() * (1.0f - 0.0f) + 0.0f) ? true : false;
				if(ca.IsAlive) MaximumLiveCells++;
				if(!ca.IsAlive) { ca.z = 1.0f; ca.CellColor = 0; MaximumDeadCells++; }
				else {
					ca.z = (float)((new Random()).NextDouble() * (0.99f - 0.0f) + 0.0f);
					ca.CellColor = 1;
				}
				// ca.Immutable = 0.1 > (float)((new Random()).NextDouble() * (1.0f - 0.0f) + 0.0f) ? true : false;
				// if(!ca.Immutable) ca.Ambivalent = 0.1 > (float)((new Random()).NextDouble() * (1.0f - 0.0f) + 0.0f) ? true : false;
				// else ca.Ambivalent = false;
				listMap[x].Add(ca);
			}
		}
		
		MaximumLiveCells = countAliveCells(listMap);

		for(int nmg = 0; nmg < AUTOMATAITERATIONS; nmg++)
		{
			listMap = NextMapGeneration(listMap, MaximumDeadCells, MaximumLiveCells);
			image_number++;
			GeneratePNG(image_number);
		}

		for(int x = 0; x < WIDTH; x++)
		{
			for(int y = 0; y < HEIGHT; y++)
			{
				ca.IsAlive = aliveFactor < (float)((new Random()).NextDouble() * (1.0f - 0.0f) + 0.0f) ? true : false;
				// if(!ca.IsAlive) continue;
				// ca.Immutable = 0.1 > (float)((new Random()).NextDouble() * (1.0f - 0.0f) + 0.0f) ? true : false;
				// if(!ca.Immutable) ca.Ambivalent = 0.1 > (float)((new Random()).NextDouble() * (1.0f - 0.0f) + 0.0f) ? true : false;
				// else ca.Ambivalent = false;
				ca.CellColor = 2;
				if(ca.IsAlive) MaximumLiveCells++;
				listMap[x][y] = ca;
			}
		}

		MaximumLiveCells = countAliveCells(listMap);

		for(int nmg = 0; nmg < AUTOMATAITERATIONS; nmg++)
		{
			listMap = NextMapGeneration(listMap, MaximumDeadCells, MaximumLiveCells);
			image_number++;
			GeneratePNG(image_number);
		}

		for(int x = 0; x < WIDTH; x++)
		{
			for(int y = 0; y < HEIGHT; y++)
			{
				ca.IsAlive = aliveFactor < (float)((new Random()).NextDouble() * (1.0f - 0.0f) + 0.0f) ? true : false;
				// if(!ca.IsAlive) continue;
				// ca.Immutable = 0.1 > (float)((new Random()).NextDouble() * (1.0f - 0.0f) + 0.0f) ? true : false;
				// if(!ca.Immutable) ca.Ambivalent = 0.1 > (float)((new Random()).NextDouble() * (1.0f - 0.0f) + 0.0f) ? true : false;
				// else ca.Ambivalent = false;
				ca.CellColor = 3;
				if(ca.IsAlive) MaximumLiveCells++;
				listMap[x][y] = ca;
			}
		}

		MaximumLiveCells = countAliveCells(listMap);

		for(int nmg = 0; nmg < AUTOMATAITERATIONS; nmg++)
		{
			listMap = NextMapGeneration(listMap, MaximumDeadCells, MaximumLiveCells);
			image_number++;
			GeneratePNG(image_number);
		}

		for(int x = 0; x < WIDTH; x++)
		{
			for(int y = 0; y < HEIGHT; y++)
			{
				ca.IsAlive = aliveFactor < (float)((new Random()).NextDouble() * (1.0f - 0.0f) + 0.0f) ? true : false;
				// if(!ca.IsAlive) continue;
				// ca.Immutable = 0.1 > (float)((new Random()).NextDouble() * (1.0f - 0.0f) + 0.0f) ? true : false;
				// if(!ca.Immutable) ca.Ambivalent = 0.1 > (float)((new Random()).NextDouble() * (1.0f - 0.0f) + 0.0f) ? true : false;
				// else ca.Ambivalent = false;
				ca.CellColor = 4;
				if(ca.IsAlive) MaximumLiveCells++;
				listMap[x][y] = ca;
			}
		}

		MaximumLiveCells = countAliveCells(listMap);
		
		for(int nmg = 0; nmg < AUTOMATAITERATIONS; nmg++)
		{
			listMap = NextMapGeneration(listMap, MaximumDeadCells, MaximumLiveCells);
			image_number++;
			GeneratePNG(image_number);
		}

		image_number++;
		GeneratePNG(image_number);
		return listMap;
	}
	
	void floodFill(ref List<List<CellularAutomata>> listMap, int x, int y, int startX, int startY)
	{
		if(x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT || listMap[x][y].IsAlive == false)
		{
			return;
		}
		if(localFloodVisited.Contains(new Tuple<int,int>(x, y)))
		{
			return;
		} else {
			localFloodVisited.Add(new Tuple<int,int>(x, y));
			floodFill(ref listMap, x-1, y, startX, startY);
			floodFill(ref listMap, x, y-1, startX, startY);
			floodFill(ref listMap, x+1, y, startX, startY);
			floodFill(ref listMap, x, y+1, startX, startY);
		}
	}
	
	void InitializeListMap()
	{
		CellularAutomata ca = new CellularAutomata();
		// float maxHeight = 0.99f;
		for(int x = 0; x < WIDTH; x++)
		{
			listMap.Add(new List<CellularAutomata>());
			for(int y = 0; y < HEIGHT; y++)
			{
				// ca.IsAlive = 0.7f > (float)((new Random()).NextDouble() * (accessibleCells - 0.0f) + 0.0f) ? true : false;
				// if(!ca.IsAlive) { ca.z = 1.0f; }
				// else {
				// 	ca.z = (float)((new Random()).NextDouble() * (0.99f - 0.0f) + 0.0f);
				// }
				// ca.Immutable = 0.1 > (float)((new Random()).NextDouble() * (1.0f - 0.0f) + 0.0f) ? true : false;
				// if(!ca.Immutable) ca.Ambivalent = 0.1 > (float)((new Random()).NextDouble() * (1.0f - 0.0f) + 0.0f) ? true : false;
				// else ca.Ambivalent = false;
				listMap[x].Add(ca);
			}
		}
		// Console.Write("Map initialized");
	}
	
	void ArticleStats()
	{
		string path = Path.Combine(Directory.GetCurrentDirectory(), method);

		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		
		float fitness = 0.0f;
		int countAplus = 0;
		List<List<bool>> Aplus = new List<List<bool>>();

		//for(int x = 0; x < WIDTH; x++)
		//{
			//for(int y = 0; y < HEIGHT; y++)
			//{
				//Console.Write(listMap[x][y].z+" ");
			//}
			//Console.Write("\n");
		//}

		List<List<bool>> Eplus = new List<List<bool>>();
		int countEplus = 0;
		
		for(int x = 0; x < WIDTH; x++)
		{
			Eplus.Add(new List<bool>());
			for(int y = 0; y < HEIGHT; y++)
			{
				if(!listMap[x][y].IsAlive)
				{
					Eplus[x].Add(true); countEplus++;
				} else { Eplus[x].Add(false); }
			}
		}


		// Flood Fill approach consumes a lot of memory and takes much time
		// (for a 1024x1024 grid would take up to 2 minutes to execute)
		// If number inaccessible blocks is higher than 30% of total blocks, it's probably good to use Flood Fill.
		if(countEplus/(WIDTH*HEIGHT) > 0.3)
		{
			bool nowbreak = false;
			for (int x = 0; x < WIDTH; x++)
			{
				for (int y = 0; y < HEIGHT; y++)
				{
					// Console.Write("Executing FloodFill at: "+x+" "+y+"\n");
					localFloodVisited = new HashSet<Tuple<int, int>>();
					
					Thread t = new Thread(() => floodFill(ref listMap, x, y, x, y), 1024 * 1024 * 1024);

					t.Start();
					t.Join();
					
					countAplus = System.Math.Max(localFloodVisited.Count, countAplus);
					// Console.Write("countAplus :"+countAplus+" "+ (int)(countAplus/2 )+"\n");
					if(countAplus > (int)(WIDTH*HEIGHT)/2)
					{
						nowbreak = true;
						break;
					}
					if(listMap[x][y].IsAlive) countAplus++;
				}
				if(nowbreak) break;
			}
		} 
		else 
		{
			for (int x = 0; x < WIDTH; x++)
			{
				for (int y = 0; y < HEIGHT; y++)
				{
					if(listMap[x][y].IsAlive) countAplus++;
				}
			}
		}
		
		float A = (float) (WIDTH * HEIGHT) / countAplus;
		float v = ((WIDTH * HEIGHT)/countAplus);

		float E = (float) (WIDTH * HEIGHT) / countEplus;
		float pe = (float) (100.0f*countEplus/(WIDTH * HEIGHT));
		float pa = (100-pe)/100.0f;
		float vt = (WIDTH * HEIGHT)/(WIDTH * HEIGHT * pa);
		float vs = System.Math.Abs(v - vt);
		float Et = (WIDTH * HEIGHT) / (float) System.Math.Ceiling((WIDTH * HEIGHT * pe));
		float Es = System.Math.Abs(E - Et);

		int edgeLengthCounter = 0;
		for(int x = 1; x < WIDTH - 1; x++)
		{
			for(int y = 1; y < HEIGHT - 1; y++)
			{
				if(8*listMap[x][y].z
				- listMap[x-1][y-1].z 
				- listMap[x-1][y].z
				- listMap[x-1][y+1].z 
				- listMap[x][y-1].z
				- listMap[x][y+1].z
				- listMap[x+1][y-1].z 
				- listMap[x+1][y].z
				- listMap[x+1][y+1].z > 0)
				{
					edgeLengthCounter++;
				}
			}
		}
		
		float wa = 0.8f; 
		float we = 0.2f;
		fitness = wa * vs + we * Es;
		stopwatch.Stop();
		// Console.Write("pa: "+pa+"\nv: "+v+"\nvt: "+vt+"\nvs: "+ vs +"\nA: "+A+"\npe: "+pe+"\nE: "+E+"\nEt: "+Et+"\nEs: "+Es +"\nfitness: " + fitness + "\nAplus: " + countAplus+ "\nEplus: "+ countEplus + "\nEdgelength: " + edgeLengthCounter+"\nTime: "+stopwatch.ElapsedMilliseconds/1000);
		using (StreamWriter writer = new StreamWriter(filePath+"\\"+method+"\\"+"stats-"+executionNumber+".yaml", true))
		{
			// writer.WriteLine(pa+"|"+v+"|"+vt+"|"+ vs +"|"+A+"|"+pe+"|"+E+"|"+Et+"|"+Es +"|"+fitness+"|"+countAplus+"|"+countEplus+"|"+edgeLengthCounter+"|"+ stopwatch.ElapsedMilliseconds/1000);
			// .ToString("0.00", CultureInfo.InvariantCulture)
			writer.WriteLine("Método: "+method+ "\nFitness: " + fitness + "\nPa: "+pa+"\nv: "+v+"\nvt: "+vt+"\nvs: "+ vs +"\nA: "+A+"\nPe: "+pe+"\nE: "+E+"\nEt: "+Et+"\nEs: "+Es + "\nAplus: " + countAplus+ "\nEplus: "+ countEplus + "\nwe: " + we + "\nwa: " + wa +  "\nTempo de execução: "+stopwatch.ElapsedMilliseconds+"ms");
		}
		// Console.ReadLine();
	}
}
