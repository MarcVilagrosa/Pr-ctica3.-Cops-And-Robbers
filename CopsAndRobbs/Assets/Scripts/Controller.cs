using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
                    
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }
        
   
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matriu = new int[Constants.NumTiles, Constants.NumTiles];

        //TODO: Inicializar matriz a 0's

        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                matriu[i, j] = 0;
            }
        }

        //TODO: Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)

        for (int i = 0; i < Constants.NumTiles; i++)
        {
            int row = i / Constants.TilesPerRow;
            int col = i % Constants.TilesPerRow;

            if (row > 0) matriu[i, i - Constants.TilesPerRow] = 1; //Arriba
            if (row < Constants.TilesPerRow - 1) matriu[i, i + Constants.TilesPerRow] = 1; //Abajo
            if (col > 0) matriu[i, i - 1] = 1; //Izquierda
            if (col < Constants.TilesPerRow - 1) matriu[i, i + 1] = 1; //Derecha
        }

        //TODO: Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes

        for (int i = 0; i < Constants.NumTiles; i++)
        {
            tiles[i].adjacency = new List<int>();

            for (int j = 0; j < Constants.NumTiles; j++)
            {
                if (matriu[i, j] == 1)
                {
                    tiles[i].adjacency.Add(tiles[j].numTile);
                }
            }
        }

    }

    
    public void ResetTiles()
    {
        foreach (Tile tile in tiles)
        {
            tile.Reset();
            tile.visited = false;
            tile.parent = null;
            tile.distance = 0;
            tile.selectable = false;

        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {
        clickedTile = t;

        switch (state)
        {
            case Constants.CopSelected:
                
                if (tiles[clickedTile].selectable && clickedTile != cops[clickedCop].GetComponent<CopMove>().currentTile)
                {
                    
                    if (!tiles[clickedTile].occupied)
                    {
                        cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                       
                        tiles[cops[clickedCop].GetComponent<CopMove>().currentTile].occupied = false;                       
                        tiles[clickedTile].occupied = true;
                       
                        cops[clickedCop].GetComponent<CopMove>().currentTile = clickedTile;                       
                        state = Constants.TileSelected;
                    }
                }
                break;
                // Otros casos
        }
    }


    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        /*TODO: Cambia el código de abajo para hacer lo siguiente
       - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
       - Movemos al caco a esa casilla
       - Actualizamos la variable currentTile del caco a la nueva casilla
       */
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        
        
        List<Tile> selectableTiles = new List<Tile>();
        foreach (Tile tile in tiles)
        {
            
            if (tile.selectable && tile.numTile != robber.GetComponent<RobberMove>().currentTile)
            {
                selectableTiles.Add(tile);
            }
        }

        
        if (selectableTiles.Count > 0)
        {
            Tile farthestTile = null;
            int maxDistance = -1;

            foreach (Tile tile in selectableTiles)
            {
                int distanceToCop1 = Mathf.Abs(tile.numTile / Constants.TilesPerRow - cops[0].GetComponent<CopMove>().currentTile / Constants.TilesPerRow) +
                                     Mathf.Abs(tile.numTile % Constants.TilesPerRow - cops[0].GetComponent<CopMove>().currentTile % Constants.TilesPerRow);
                int distanceToCop2 = Mathf.Abs(tile.numTile / Constants.TilesPerRow - cops[1].GetComponent<CopMove>().currentTile / Constants.TilesPerRow) +
                                     Mathf.Abs(tile.numTile % Constants.TilesPerRow - cops[1].GetComponent<CopMove>().currentTile % Constants.TilesPerRow);

                int minDistanceToCop = Mathf.Min(distanceToCop1, distanceToCop2);

                if (minDistanceToCop > maxDistance)
                {
                    maxDistance = minDistanceToCop;
                    farthestTile = tile;
                }
            }

            
            if (farthestTile != null)
            {
                robber.GetComponent<RobberMove>().MoveToTile(farthestTile);
                robber.GetComponent<RobberMove>().currentTile = farthestTile.numTile;
            }
        }
    }

   

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {
        int indexcurrentTile;
        int otherCopTile = -1; 

        if (cop == true)
        {
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
            otherCopTile = cops[(clickedCop + 1) % 2].GetComponent<CopMove>().currentTile;
        }
        else
        {
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;
        }

        
        tiles[indexcurrentTile].current = true;

        //Cola para realizar el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        tiles[indexcurrentTile].visited = true;
        tiles[indexcurrentTile].distance = 0;
        nodes.Enqueue(tiles[indexcurrentTile]);

        //Realizamos el BFS
        while (nodes.Count != 0)
        {
            Tile t = nodes.Dequeue();

            foreach (int i in t.adjacency)
            {
                Tile tile = tiles[i];

                if (!tile.visited)
                {
                    //Para evitar pasar por la casilla ocupada por el otro policía
                    if (tile.numTile == otherCopTile)
                    {
                        continue;
                    }

                    tile.parent = t;
                    tile.visited = true;
                    tile.distance = t.distance + 1;
                    nodes.Enqueue(tile);
                }
            }
        }

        foreach (Tile t in tiles)
        {
            if (t.visited && t.distance <= 2)
            {
                t.selectable = true;
            }
        }
    }
}



