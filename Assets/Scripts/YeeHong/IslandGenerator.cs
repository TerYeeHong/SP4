using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IslandGenerator : MonoBehaviour
{
    public static IslandGenerator m_instance = null;
    private void Awake()
    {
        m_instance = this;
    }

    [Header("Generation details")]
    [SerializeField] [Range(2, 100)] int size = 5;
    [SerializeField] [Range(0, 100)] int spawn_percent = 50; //out of 100
    [SerializeField] [Range(2, 100)] int depth = 4;
    [SerializeField] [Range(2, 100)] int height_variation_percent = 10; //out of 100
    [SerializeField] [Range(2, 20)] int max_height = 4; //out of 100

    [Header("Island details")]
    [SerializeField] GameObject island_prefab;
    
    [SerializeField] [Range(2, 200)] int x_length;
    [SerializeField] [Range(2, 200)] int z_length;
    [SerializeField] [Range(0, 200)] int gap;

    List<Grid> island_grids = new();
    List<GameObject> island_objects = new();

    Grid start_island;



    public void RemakeIsland()
    {
        ResetIslands();
        GenerateIsland();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            RemakeIsland();
    }


    private void Start()
    {
        GenerateIsland();
    }

    void GenerateIsland()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);

        //Dont allow size of one
        if (size <= 1) return;

        //Generate a start and end that does not overlap
        start_island = new (Random.Range(0, size), Random.Range(0, size));
        //do { end_island = new(Random.Range(0, size), Random.Range(0, size)); } while (end_island != start_island);

        //Loop and generate a island map
        RandomDir(start_island, depth);
        CreateIslands();
    }
    void RandomDir(Grid curr, int depth)
    {
        //Stop
        if (depth <= 0)
            return;

        //Create this grid
        island_grids.Add(curr);
       
        //Only valid if there is at least 1 neighbour
        bool valid = false;
        while (!valid)
        {
            //Generate up to 4 neighbours for each grid
            for (int i = 0; i < 4; ++i)
            {
                //Check direction of this neighbour
                Grid neighbour = new(curr.x, curr.y);
                switch (i) {
                    case 0: neighbour.x += 1; break;
                    case 1: neighbour.x -= 1; break;
                    case 2: neighbour.y += 1; break;
                    case 3: neighbour.y -= 1; break;
                }

                //Check if neighbour is a valid tile
                if (!IsValid(neighbour))
                    continue;

                //Check if neighbour is already there
                if (GridContain(island_grids, neighbour))
                {
                    valid = true;
                    continue; //check next direction
                }

                //Chance to spawn neighbour
                int odds = Random.Range(0, 100); //0-99
                if (odds < spawn_percent)
                {
                    //Vary height //Spread height or increase/lower by 1
                    neighbour.height = curr.height;
                    //Go up (priority)
                    if (curr.height < max_height
                        && Random.Range(0, 100) < height_variation_percent) {
                        neighbour.height += 1;
                    }
                    //go down (less)
                    else if (curr.height > 1
                            && Random.Range(0, 100) < height_variation_percent) {
                        neighbour.height -= 1;
                    }

                    RandomDir(neighbour, depth - 1);
                    valid = true;
                }
            }
        }
    }
    void CreateIslands()
    {
        foreach (Grid grid in island_grids)
        {
            GameObject island = Instantiate(island_prefab, 
                new Vector3(
                    grid.x * x_length + grid.x * gap, 
                    grid.height * 0.5f,
                    grid.y * z_length + grid.y * gap),
                Quaternion.identity);
            island.transform.localScale = new Vector3(x_length, grid.height, z_length);
            island_objects.Add(island);

            //child to generator
            island.transform.parent = transform;
        }
    }
    void ResetIslands()
    {
        foreach (GameObject island in island_objects)
        {
            Destroy(island);
        }
        island_objects.Clear();
        island_grids.Clear();
    }
    int Get1DIndex(int x, int y)
    {
        return y * size + x;
    }
    bool IsValid(Grid grid)
    {
        return (grid.x > -1 && grid.x < size && grid.y > -1 && grid.y < size);
    }
    bool IsBeside(Grid main_grid, Grid grid)
    {
        return (main_grid.x + 1 == grid.x && main_grid.y == grid.y)
            || (main_grid.x - 1 == grid.x && main_grid.y == grid.y)
            || (main_grid.x == grid.x && main_grid.y + 1 == grid.y)
            || (main_grid.x == grid.x && main_grid.y - 1 == grid.y);
    }
    bool GridContain(List<Grid> grid_list, Grid grid)
    {
        foreach (Grid _grid in grid_list)if (grid == _grid)return true;
        return false;
    }




    //FIT WITH THE LEVEL GEN

}
[System.Serializable]
public struct IslandData
{
    //int size_x, size_z;
    //ISLAND_SHAPE island_shape;

}



public class Grid
{
    public int x;
    public int y;

    public int height;

    public Grid(int x, int y) {
        this.x = x;
        this.y = y;

        height = 1;
    }
    public static bool operator == (Grid lhs, Grid rhs)
    {
        return (lhs.x == rhs.x && lhs.y == rhs.y);
    }
    public static bool operator != (Grid lhs, Grid rhs)
    {
        return !(lhs.x == rhs.x && lhs.y == rhs.y);
    }


    //public bool IsBeside(Grid grid)
    //{
    //    return (x + 1 == grid.x && y == grid.y)
    //        || (x - 1 == grid.x && y == grid.y)
    //        || (x == grid.x && y + 1 == grid.y)
    //        || (x == grid.x && y - 1 == grid.y);
    //}
}