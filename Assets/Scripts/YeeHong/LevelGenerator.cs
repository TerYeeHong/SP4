using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

using Hashtable = ExitGames.Client.Photon.Hashtable;


public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator m_instance = null;
    public NavMeshSurface navMeshSurface;

    [Header("Prefab details")]
    [SerializeField] GameObject platform_default_prefab;
    [SerializeField] GameObject platform_connector_prefab;

    [SerializeField] GameObject spawn_monument_prefab;

    [Header("Island Generation details")]
    //[SerializeField] [Range(2, 100)] int size = 5;
    [SerializeField] [Range(0, 100)] int spawn_percent = 50; //out of 100
    //[SerializeField] [Range(2, 100)] int depth = 4;
    //[SerializeField] [Range(2, 100)] int height_variation_percent = 10; //out of 100
    //[SerializeField] [Range(2, 20)] int max_height = 4; //out of 100

    [SerializeField] [Range(1, 200)] int x_length;
    [SerializeField] [Range(1, 200)] int z_length;
    [SerializeField] int island_min_size = 7;
    [SerializeField] int island_max_size = 21;
    [SerializeField] [Range(0, 200)] int gap;
    [SerializeField] [Range(0, 200)] int island_depth;

    [Header("Bridge Details")]
    [SerializeField] [Range(5, 100)] int bridge_length_min;
    [SerializeField] [Range(5, 100)] int bridge_length_max;
    [SerializeField] [Range(0, 100)] int neighbour_spawn_chance;



    //int size_x_level, size_z_level;
    //Grid[] grid_level;

    List<Island> islands_list = new(); //queue so players unlock section by section

    List<GameObject> island_objects = new();
    List<Grid> spawn_points = new();


    Vector3 spawn_center_position;
    bool level_generated = false;


    private void OnEnable()
    {
        RaiseEvents.GenerateLevelEvent += OnGenerateLevel;
    }
    private void OnDisable()
    {
        RaiseEvents.GenerateLevelEvent -= OnGenerateLevel;
    }

    public List<Grid> SpawnSpoints { get { return spawn_points; } }

    public void RaiseEventGenerateLevel()
    {
        //data: team, new_score
        string dataSent = $"{(int)System.DateTime.Now.Ticks}/" + //seed
            $"{spawn_percent}/" +
            $"{x_length}/" +
            $"{z_length}/" +
            $"{island_min_size}/" +
            $"{island_max_size}/" +
            $"{gap}/" +
            $"{island_depth}/" +
            $"{bridge_length_min}/" +
            $"{bridge_length_max}/" +
            $"{neighbour_spawn_chance}";

        // Update other clients
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(RaiseEvents.GENERATELEVEL, dataSent, raiseEventOptions, SendOptions.SendReliable);
    }

    void OnGenerateLevel(string data)
    {
        //Set all the settings from masterclient
        //Every client including master client will generate islands only know to ensure seed is okay

        string[] dataSplit = data.Split("/");
        spawn_percent = int.Parse(dataSplit[1]);
        x_length = int.Parse(dataSplit[2]);
        z_length = int.Parse(dataSplit[3]);
        island_min_size = int.Parse(dataSplit[4]);
        island_max_size = int.Parse(dataSplit[5]);
        gap = int.Parse(dataSplit[6]);
        island_depth = int.Parse(dataSplit[7]);
        bridge_length_min = int.Parse(dataSplit[8]);
        bridge_length_max = int.Parse(dataSplit[9]);
        neighbour_spawn_chance = int.Parse(dataSplit[10]);

        navMeshSurface.BuildNavMesh();
        Debug.Log("dog hi");

        RemakeIsland(int.Parse(dataSplit[0]));
    }

    //Each level consists of multiple islands, Only the main island is shown at the start
    //Triggers can rise more islands to continue
    //An Island can consists of multiple islands, each linked with a connector


    private void Awake()
    {
        if (m_instance == null)
            m_instance = this;
        islands_list = new();
        Debug.Log("dog awake");
        //RemakeIsland(1);
    }
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //    RemakeIsland((int)System.DateTime.Now.Ticks);

        //if (Input.GetKeyDown(KeyCode.V))
        //    RemakeIsland(1);
    }

    void RemakeIsland(int seed)
    {
        level_generated = false;
        UpdateClientLoadedMap();

        Random.InitState(seed);

        //Clear any existing ones
        foreach (GameObject island in island_objects)
        {
            Destroy(island);
        }
        island_objects.Clear();
        islands_list.Clear();
        spawn_points.Clear();

        //Create island

        IslandBoundary islandBoundary = new(0, Random.Range(island_min_size, island_max_size), 0, Random.Range(island_min_size, island_max_size));
        //ISLAND_SHAPE shape = ISLAND_SHAPE.ROUND;
        //if (Random.Range(0, 3) == 1)
        //    shape = ISLAND_SHAPE.DONUT;
        //else
        //    shape = ISLAND_SHAPE.T_SHAPE;

        //shape = ISLAND_SHAPE.T_SHAPE;

        //InstantiateIsland(GenerateIsland(islandBoundary, shape));
        Island main_island = GenerateIsland(islandBoundary);

        //3 by 3 around island center
        for (int i = -1; i < 2; ++i) {
            for (int j = -1; j < 2; ++j) {
                spawn_points.Add(new Grid(main_island.center.x + i, main_island.center.y + j));
            }
        }
        //make sure there are all ground around
        for (int i = -3; i < 4; ++i)
        {
            for (int j = -3; j < 4; ++j)
            {
                main_island.island_grid.Add(new Grid(main_island.center.x + i, main_island.center.y + j));
            }
        }

        islands_list.Add(main_island);
        GenerateNextIsland(main_island, island_depth);

        //Create all
        foreach (Island island in islands_list)
        {
            InstantiateIsland(island);
        }

        level_generated = true;

        navMeshSurface.BuildNavMesh();
        Debug.Log("dog build");
        UpdateClientLoadedMap();
    }

    void UpdateClientLoadedMap()
    {
        Hashtable props = new Hashtable
            {
                {JLGame.PLAYER_LOADED_MAP, level_generated}
            };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    
    //void GenerateLevel()
    //{
    //    islands_list.Clear();
    //    int section_amount = Random.Range(3, 12);

    //    //Randomise how many big islands there are gonna be
    //    //Settle the size of each big island
    //    IslandBoundary islandBoundary = new(0, Random.Range(7, 21), 0, Random.Range(7, 21));
    //    //islands_list.Enqueue(GenerateIsland(islandBoundary));

    //    //Create sections
    //    for (int i = 0; i < section_amount; ++i)
    //    {
            

    //    }

    //}

    void GenerateNextIsland(Island curr, int depth)
    {
        ////Loop all directions, try generating a island
        //int neighbours = 1;
        //for (int i = 0; i < 2; ++i)
        //{
        //    if (Random.Range(0, 100) < neighbour_spawn_chance)
        //        ++neighbours;
        //    else break;
        //}
        //bool up_neighbour = (Random.Range(0, 100) < neighbour_spawn_chance)? true : false;
        //bool down_neighbour = (Random.Range(0, 100) < neighbour_spawn_chance) ? true : false;
        //bool left_neighbour = (Random.Range(0, 100) < neighbour_spawn_chance) ? true : false;
        //bool right_neighbour = (Random.Range(0, 100) < neighbour_spawn_chance) ? true : false;

        //while (neighbours > 0)
        //{
        //    //Try create neighbours
        //    for (int i = 0; i < 4; ++i)
        //    {
        //        int bridge_length = Random.Range(bridge_length_min, bridge_length_max);
        //        IslandBoundary islandBoundary = new(0, Random.Range(7, 21), 0, Random.Range(7, 21));
        //        islandBoundary.DisplaceX(curr.center.x);
        //        islandBoundary.DisplaceZ(curr.center.y);

        //        //if alr added to this dir, skip, else displace this dir again
        //        switch (i)
        //        {
        //            case 0: 
        //                if (up_neighbour) 
        //                    continue;
        //                islandBoundary.DisplaceZ(bridge_length);
        //                break;
        //            case 1: 
        //                if (down_neighbour) 
        //                    continue; 
        //                islandBoundary.DisplaceZ(-bridge_length);
        //                break;
        //            case 2: 
        //                if (left_neighbour) 
        //                    continue;
        //                islandBoundary.DisplaceX(bridge_length);
        //                break;
        //            case 3: 
        //                if (right_neighbour) 
        //                    continue;
        //                islandBoundary.DisplaceX(-bridge_length);
        //                break;
        //        }

        //        Island island = GenerateIsland(islandBoundary);

        //        //Make sure is valid, dont overlap with any existing ones
        //        bool valid = true;

        //        if (valid)
        //        {
        //            sections_queue.Enqueue(island);
        //            --neighbours; 
        //        }

        //    }
        //}

        if (depth <= 0)
            return;


        bool up_neighbour = (Random.Range(0, 100) < neighbour_spawn_chance) ? true : false;
        bool down_neighbour = (Random.Range(0, 100) < neighbour_spawn_chance) ? true : false;
        bool left_neighbour = (Random.Range(0, 100) < neighbour_spawn_chance) ? true : false;
        bool right_neighbour = (Random.Range(0, 100) < neighbour_spawn_chance) ? true : false;
        if (up_neighbour == false && down_neighbour == false && left_neighbour == false && right_neighbour == false)
        {
            int odds = Random.Range(0, 4);
            switch (odds)
            {
                case 0: up_neighbour = true; break;
                case 1: down_neighbour = true; break;
                case 2: left_neighbour = true; break;
                case 3: right_neighbour = true; break;
            }
        }

        int bridge_length = Random.Range(bridge_length_min, bridge_length_max);
        IslandBoundary islandBoundary = new(0, Random.Range(island_min_size, island_max_size), 0, Random.Range(island_min_size, island_max_size));
        int tries = 3;
        if (up_neighbour)
        {
            //tries n times to generate, else give up on generating
            for (int i = 0; i < tries; ++i)
            {
                //Generate one, make sure it doesnt overlap anything 
                islandBoundary.DisplaceX(curr.center.x);
                islandBoundary.DisplaceZ(curr.island_boundary.max_z + bridge_length);

                Island island = GenerateIsland(islandBoundary);

                //check all existing islands make sure it doesnt overlap
                bool valid = true;
                foreach (Island island_existing in islands_list)
                {
                    if (OverlapIsland(island_existing, island))
                    {
                        valid = false;
                        break;
                    }
                }

                //Nice lets make this island and add to the list
                if (valid)
                {
                    islands_list.Add(island);

                    GenerateNextIsland(island, depth - 1);
                    LinkBridge(curr.GetRandomConnector(IslandConnectors.DIRECTION.Z_NEGATIVE),
                        island.GetRandomConnector(IslandConnectors.DIRECTION.Z_POSITIVE),
                        island);
                    break;
                }
            }
        }
        if (down_neighbour)
        {
            //tries n times to generate, else give up on generating
            for (int i = 0; i < tries; ++i)
            {
                //Generate one, make sure it doesnt overlap anything 
                islandBoundary.DisplaceX(curr.center.x);
                islandBoundary.DisplaceZ(curr.island_boundary.min_z - bridge_length);

                Island island = GenerateIsland(islandBoundary);

                //check all existing islands make sure it doesnt overlap
                bool valid = true;
                foreach (Island island_existing in islands_list)
                {
                    if (OverlapIsland(island_existing, island))
                    {
                        valid = false;
                        break;
                    }
                }

                //Nice lets make this island and add to the list
                if (valid)
                {
                    islands_list.Add(island);
                    
                    GenerateNextIsland(island, depth - 1);
                    LinkBridge(curr.GetRandomConnector(IslandConnectors.DIRECTION.Z_POSITIVE),
                        island.GetRandomConnector(IslandConnectors.DIRECTION.Z_NEGATIVE),
                        island);
                    break;
                }
            }
        }
        if (right_neighbour)
        {
            //tries n times to generate, else give up on generating
            for (int i = 0; i < tries; ++i)
            {
                //Generate one, make sure it doesnt overlap anything 
                islandBoundary.DisplaceX(curr.island_boundary.max_x + bridge_length);
                islandBoundary.DisplaceZ(curr.center.y);

                Island island = GenerateIsland(islandBoundary);

                //check all existing islands make sure it doesnt overlap
                bool valid = true;
                foreach (Island island_existing in islands_list)
                {
                    if (OverlapIsland(island_existing, island))
                    {
                        valid = false;
                        break;
                    }
                }

                //Nice lets make this island and add to the list
                if (valid)
                {
                    islands_list.Add(island);
                    
                    GenerateNextIsland(island, depth - 1);
                    LinkBridge(curr.GetRandomConnector(IslandConnectors.DIRECTION.X_NEGATIVE),
                        island.GetRandomConnector(IslandConnectors.DIRECTION.X_POSITIVE),
                        island);
                    break;
                }
            }
        }
        if (left_neighbour)
        {
            //tries n times to generate, else give up on generating
            for (int i = 0; i < tries; ++i)
            {
                //Generate one, make sure it doesnt overlap anything 
                islandBoundary.DisplaceX(curr.island_boundary.min_x - bridge_length);
                islandBoundary.DisplaceZ(curr.center.y);

                Island island = GenerateIsland(islandBoundary);

                //check all existing islands make sure it doesnt overlap
                bool valid = true;
                foreach (Island island_existing in islands_list)
                {
                    if (OverlapIsland(island_existing, island))
                    {
                        valid = false;
                        break;
                    }
                }

                //Nice lets make this island and add to the list
                if (valid)
                {
                    islands_list.Add(island);
                    
                    GenerateNextIsland(island, depth - 1);
                    LinkBridge(curr.GetRandomConnector(IslandConnectors.DIRECTION.X_POSITIVE),
                        island.GetRandomConnector(IslandConnectors.DIRECTION.X_NEGATIVE),
                        island);
                    break;
                }
            }
        }

    }
    bool OverlapIsland(Island island_1, Island island_2)
    {
        return (island_1.PointInIsland(island_2.island_boundary.min_x, island_2.island_boundary.min_z)
            || island_1.PointInIsland(island_2.island_boundary.min_x, island_2.island_boundary.min_z)
            || island_1.PointInIsland(island_2.island_boundary.min_x, island_2.island_boundary.min_z)
            || island_1.PointInIsland(island_2.island_boundary.min_x, island_2.island_boundary.min_z)
            );
    }

   
    Island CreateIsland()
    {
        int size_x_section;
        int size_z_section;

        size_x_section = Random.Range(3, 8);
        size_z_section = Random.Range(3, 8);
        Island section = new Island(size_x_section, size_z_section);

        //islands_list.Enqueue(section);
        return section;
    }
    void NextIsland()
    {

    }


    void InstantiateIsland(Island island)
    {
        foreach (Grid grid in island.island_grid)
        {
            GameObject island_go = Instantiate(platform_default_prefab,
                new Vector3(
                    grid.x * x_length + grid.x * gap,
                    grid.height * 0.5f,
                    grid.y * z_length + grid.y * gap),
                Quaternion.identity);
            island_go.transform.localScale = new Vector3(x_length, grid.height, z_length);
            island_objects.Add(island_go);

            //child to generator
            island_go.transform.parent = transform;
        }
        foreach (Grid grid in island.island_grid_outline)
        {
            GameObject island_go = Instantiate(platform_connector_prefab,
                new Vector3(
                    grid.x * x_length + grid.x * gap,
                    grid.height * 0.5f,
                    grid.y * z_length + grid.y * gap),
                Quaternion.identity);
            island_go.transform.localScale = new Vector3(x_length, grid.height, z_length);
            island_objects.Add(island_go);

            //child to generator
            island_go.transform.parent = transform;
        }
        foreach (Grid grid in island.island_bridge_grid)
        {
            GameObject island_go = Instantiate(platform_connector_prefab,
                new Vector3(
                    grid.x * x_length + grid.x * gap,
                    grid.height * 0.5f,
                    grid.y * z_length + grid.y * gap),
                Quaternion.identity);
            island_go.transform.localScale = new Vector3(x_length, grid.height, z_length);
            island_objects.Add(island_go);

            //child to generator
            island_go.transform.parent = transform;
        }

        //Generate a spawn monument
        Instantiate(spawn_monument_prefab, new Vector3(island.center.x, 2, island.center.y + 3), Quaternion.identity);
    }

    //Given boundary and Shape,
    //Create an island and return
    Island GenerateIsland(IslandBoundary island_boundary, ISLAND_SHAPE island_shape)
    {
        //Create island
        Island island = new Island(island_boundary.max_x - island_boundary.min_x, island_boundary.max_z - island_boundary.min_z);
        //Get the center
        Grid island_center = new Grid((int)((island_boundary.min_x + island_boundary.max_x) * 0.5f),
            (int)((island_boundary.min_z + island_boundary.max_z) * 0.5f));
        int depth;

        //Generate the shape
        switch (island_shape)
        {
            case ISLAND_SHAPE.ROUND:
                //Fill up entire island first, stop cutting off bit by bit from the side
                for (int i = island_boundary.min_x; i <= island_boundary.max_x; ++i) {
                    for (int j = island_boundary.min_z; j <= island_boundary.max_z; ++j) {
                        Grid curr = new Grid(i, j);
                        island.island_grid.Add(curr);
                    }
                }

                //Get the four corners and start cutting up with slight depth
                for (int corner_index = 0; corner_index < 4; ++corner_index)
                {
                    Grid corner = new Grid(0,0);
                    switch (corner_index) {
                        case 0: corner.x = island_boundary.min_x; corner.y = island_boundary.min_z; break;
                        case 1: corner.x = island_boundary.max_x; corner.y = island_boundary.min_z; break;
                        case 2: corner.x = island_boundary.min_x; corner.y = island_boundary.max_z; break;
                        case 3: corner.x = island_boundary.max_x; corner.y = island_boundary.max_z; break;
                    }

                    //Get the shorter distance
                    depth = (island_boundary.max_z - island_boundary.min_z > island_boundary.max_x - island_boundary.min_x) ?
                        (int)((island_boundary.max_x - island_boundary.min_x)* 0.5f):
                        (int)((island_boundary.max_z - island_boundary.min_z)* 0.5f);
                  

                    List <Grid> grid_area = RandomDir(new List<Grid>(), corner, depth);

                    //Clear this area inside the grid
                    int grid_area_size = grid_area.Count;
                    for (int i_grid = grid_area_size - 1; i_grid > -1; --i_grid)
                    {
                        for (int j_grid = island.island_grid.Count - 1; j_grid > -1; --j_grid)
                        {
                            if (grid_area[i_grid] == island.island_grid[j_grid])
                                island.island_grid.RemoveAt(j_grid);
                        }
                    }
                }
                break;
            case ISLAND_SHAPE.DONUT:
                //Fill up entire island first, stop cutting off bit by bit from the side
                for (int i = island_boundary.min_x; i <= island_boundary.max_x; ++i)
                {
                    for (int j = island_boundary.min_z; j <= island_boundary.max_z; ++j)
                    {
                        Grid curr = new Grid(i, j);
                        island.island_grid.Add(curr);
                    }
                }

                //Get the four corners and start cutting up with slight depth and also the center
                for (int corner_index = 0; corner_index < 5; ++corner_index)
                {
                    Grid corner = new Grid(0,0);
                    switch (corner_index)
                    {
                        case 0: corner.x = island_boundary.min_x; corner.y = island_boundary.min_z; break;
                        case 1: corner.x = island_boundary.max_x; corner.y = island_boundary.min_z; break;
                        case 2: corner.x = island_boundary.min_x; corner.y = island_boundary.max_z; break;
                        case 3: corner.x = island_boundary.max_x; corner.y = island_boundary.max_z; break;
                        case 4: corner = island_center; break;
                    }

                    //Get the shorter distance
                    depth = (island_boundary.max_z - island_boundary.min_z > island_boundary.max_x - island_boundary.min_x) ?
                        (int)((island_boundary.max_x - island_boundary.min_x) * 0.5f) :
                        (int)((island_boundary.max_z - island_boundary.min_z) * 0.5f);

                    List<Grid> grid_area = RandomDir(new List<Grid>(), corner, depth);

                    //Clear this area inside the grid
                    int grid_area_size = grid_area.Count;
                    for (int i_grid = grid_area_size - 1; i_grid > -1; --i_grid)
                    {
                        for (int j_grid = island.island_grid.Count - 1; j_grid > -1; --j_grid)
                        {
                            if (grid_area[i_grid] == island.island_grid[j_grid])
                                island.island_grid.RemoveAt(j_grid);
                        }
                    }
                }
                break;
            case ISLAND_SHAPE.T_SHAPE:
                //Draw one line across the center
                bool z_axis = (Random.Range(0, 2) == 0) ? true : false;

                //Add one line across
                if (z_axis)
                {
                    for (int i = island_boundary.min_x; i <= island_boundary.max_x; ++i)
                    {
                        Grid curr = new Grid(i, island_center.y);
                        island.island_grid.Add(curr);
                    }
                }
                else
                {
                    for (int i = island_boundary.min_z; i <= island_boundary.max_z; ++i)
                    {
                        Grid curr = new Grid(island_center.x, i);
                        island.island_grid.Add(curr);
                    }
                }

                //End of the T at one of the connecting ends
                if (z_axis)
                {
                    int end_x = (Random.Range(0, 2) == 0) ? island_boundary.min_x : island_boundary.max_x;
                    for (int i = island_boundary.min_z; i <= island_boundary.max_z; ++i)
                    {
                        if (i == island_center.y)
                            continue;
                        Grid curr = new Grid(end_x, i);
                        island.island_grid.Add(curr);
                    }
                }
                else
                {
                    int end_z = (Random.Range(0, 2) == 0) ? island_boundary.min_z : island_boundary.max_z;
                    for (int i = island_boundary.min_x; i <= island_boundary.max_x; ++i)
                    {
                        if (i == island_center.x)
                            continue;
                        Grid curr = new Grid(i, end_z);
                        island.island_grid.Add(curr);
                    }
                }

                //thicken dam lines
                int size_x = island_boundary.max_x - island_boundary.min_x;
                int size_z = island_boundary.max_z - island_boundary.min_z;
                depth = (size_x > size_z) ? (int)(Random.Range(size_z, size_x) * 0.3f) : (int)(Random.Range(size_x, size_z) * 0.3f);
                int size_T = island.island_grid.Count;
                for (int i = 0; i < size_T; ++i)
                {
                    RandomDir(island.island_grid, island.island_grid[i], depth);
                }

                break;
            case ISLAND_SHAPE.L_SHAPE:
                break;
            case ISLAND_SHAPE.C_SHAPE:
                break;
        }

        //Get outline so can identify connectors
        for (int grid_index = island.island_grid.Count - 1; grid_index >= 0; --grid_index)
        {
            Grid grid = island.island_grid[grid_index];
            if (grid.x == island_boundary.min_x
                || grid.x == island_boundary.max_x
                || grid.y == island_boundary.min_z
                || grid.y == island_boundary.max_z)
            {
                island.island_grid_outline.Add(grid);
                island.island_grid.RemoveAt(grid_index);
            }
        }
        return island;
    }
    Island GenerateIsland(IslandBoundary island_boundary)
    {
        int size_x = island_boundary.max_x - island_boundary.min_x;
        int size_z = island_boundary.max_z - island_boundary.min_z;
        int depth = (size_x > size_z) ? Random.Range(size_z, size_x) : Random.Range(size_x, size_z);

        //Create island
        Island island = new Island(island_boundary.max_x - island_boundary.min_x, island_boundary.max_z - island_boundary.min_z);
        //Get the center
        Grid island_center = new Grid((int)((island_boundary.min_x + island_boundary.max_x) * 0.5f),
            (int)((island_boundary.min_z + island_boundary.max_z) * 0.5f));
         
        //Draw one line across the center
        bool z_axis = (Random.Range(0, 2) == 0) ? true : false;

        //Add one line across
        if (z_axis)
        {
            for (int i = island_boundary.min_x; i <= island_boundary.max_x; ++i)
            {
                Grid curr = new Grid(i, island_center.y);
                island.island_grid.Add(curr);
            }
        }
        else
        {
            for (int i = island_boundary.min_z; i <= island_boundary.max_z; ++i)
            {
                Grid curr = new Grid(island_center.x, i);
                island.island_grid.Add(curr);
            }
        }

        //End of the T at one of the connecting ends
        if (z_axis)
        {
            int end_x = (Random.Range(0, 2) == 0) ? island_boundary.min_x : island_boundary.max_x;
            for (int i = island_boundary.min_z; i <= island_boundary.max_z; ++i)
            {
                if (i == island_center.y)
                    continue;
                Grid curr = new Grid(end_x, i);
                island.island_grid.Add(curr);
            }
        }
        else
        {
            int end_z = (Random.Range(0, 2) == 0) ? island_boundary.min_z : island_boundary.max_z;
            for (int i = island_boundary.min_x; i <= island_boundary.max_x; ++i)
            {
                if (i == island_center.x)
                    continue;
                Grid curr = new Grid(i, end_z);
                island.island_grid.Add(curr);
            }
        }

        //thicken dam lines
        int size_T = island.island_grid.Count;
        for (int i = 0; i < size_T; ++i)
        {
            RandomDir(island.island_grid, island.island_grid[i], depth);
        }

        //Get new min and max x and z
        foreach (Grid grid in island.island_grid)
        {
            if (grid.x < island_boundary.min_x)
                island_boundary.min_x = grid.x;
            else if (grid.x > island_boundary.max_x)
                island_boundary.max_x = grid.x;
            if (grid.y < island_boundary.min_z)
                island_boundary.min_z = grid.y;
            else if (grid.y > island_boundary.max_z)
                island_boundary.max_z = grid.y;
        }

        //Get outline so can identify connectors
        for (int grid_index = island.island_grid.Count - 1; grid_index >= 0; --grid_index)
        {
            Grid grid = island.island_grid[grid_index];
            if (grid.x == island_boundary.min_x
                || grid.x == island_boundary.max_x
                || grid.y == island_boundary.min_z
                || grid.y == island_boundary.max_z)
            {
                island.island_grid_outline.Add(grid);
                island.island_grid.RemoveAt(grid_index);

                //Add connector
                IslandConnectors islandConnector = new();
                islandConnector.connector_grid = grid;
                if (grid.x == island_boundary.min_x)
                    islandConnector.direction = IslandConnectors.DIRECTION.X_NEGATIVE;
                else if (grid.x == island_boundary.max_x)
                    islandConnector.direction = IslandConnectors.DIRECTION.X_POSITIVE;
                else if (grid.y == island_boundary.min_z)
                    islandConnector.direction = IslandConnectors.DIRECTION.Z_NEGATIVE;
                else if (grid.y == island_boundary.max_z)
                    islandConnector.direction = IslandConnectors.DIRECTION.Z_POSITIVE;

                island.island_connectors.Add(islandConnector);
            }
        }

        //Set the stats
        island.center = island_center;
        island.island_boundary = island_boundary;
        island.size_x = island_boundary.max_x - island_boundary.min_x;
        island.size_z = island_boundary.max_z - island_boundary.min_z;
        return island;
    }
    
    void LinkBridge(Grid island_connector, Grid island_connector_next, Island bridge_owner)
    {
        if (island_connector.x == -999 || island_connector_next.x == -999)
            return;

        //Check x and z is longer distance, 
        bool x_longer = (Mathf.Abs(island_connector.x - island_connector_next.x) > Mathf.Abs(island_connector.y - island_connector_next.y)) ?
            true : false;

        //Draw line to z
        if (x_longer)
        {
            //see which one has is closer to origin
            int first = island_connector.x;
            int second = island_connector_next.x;
            if (island_connector.x > island_connector_next.x)
            {
                first = island_connector_next.x;
                second = island_connector.x;
            }

            //Draw straight path
            for (int i = first + 1; i < second; ++i)
            {
                bridge_owner.island_bridge_grid.Add(new Grid(i, island_connector.y));
            }

            //draw second line
            first = island_connector.y;
            second = island_connector_next.y;
            if (island_connector.y > island_connector_next.y)
            {
                first = island_connector_next.y;
                second = island_connector.y;
            }

            //Draw straight path
            for (int i = first; i < second; ++i)
            {
                bridge_owner.island_bridge_grid.Add(new Grid(island_connector_next.x, i));
            }
        }
        else
        {
            //see which one has is closer to origin
            int first = island_connector.y;
            int second = island_connector_next.y;
            if (island_connector.y > island_connector_next.y)
            {
                first = island_connector_next.y;
                second = island_connector.y;
            }

            //Draw straight path
            for (int i = first + 1; i < second; ++i)
            {
                bridge_owner.island_bridge_grid.Add(new Grid(island_connector.x, i));
            }

            //draw second line
            first = island_connector.x;
            second = island_connector_next.x;
            if (island_connector.x > island_connector_next.x)
            {
                first = island_connector_next.x;
                second = island_connector.x;
            }

            //Draw straight path
            for (int i = first; i < second; ++i)
            {
                bridge_owner.island_bridge_grid.Add(new Grid(i, island_connector_next.y));
            }
        }

    }
    List<Grid> RandomDir(List<Grid> grid_list, Grid curr, int depth)
    {
        //Stop
        if (depth <= 0)
            return grid_list;

        //Create this grid
        grid_list.Add(curr);

        //Only valid if there is at least 1 neighbour
        bool valid = false;
        while (!valid)
        {
            //Generate up to 4 neighbours for each grid
            for (int i = 0; i < 4; ++i)
            {
                //Check direction of this neighbour
                Grid neighbour = new(curr.x, curr.y);
                switch (i)
                {
                    case 0: neighbour.x += 1; break;
                    case 1: neighbour.x -= 1; break;
                    case 2: neighbour.y += 1; break;
                    case 3: neighbour.y -= 1; break;
                }

                //Check if neighbour is already there
                if (GridContain(grid_list, neighbour))
                {
                    valid = true;
                    continue; //check next direction
                }

                //Chance to travel to neighbour
                int odds = Random.Range(0, 100); //0-99
                if (odds < spawn_percent)
                {
                    grid_list = RandomDir(grid_list, neighbour, depth - 1);
                    valid = true;
                }
            }
        }

        return grid_list;
    }

    bool GridContain(List<Grid> grid_list, Grid grid)
    {
        foreach (Grid _grid in grid_list) if (grid == _grid) return true;
        return false;
    }
    bool IsValid(Grid grid, IslandBoundary island_boundary)
    {
        return (grid.x >= island_boundary.min_x 
            && grid.x <= island_boundary.max_x 
            && grid.y >= island_boundary.min_z
            && grid.y <= island_boundary.max_z);
    }


    public enum ISLAND_SHAPE
    {
        ROUND = 0,
        DONUT,
        T_SHAPE,
        L_SHAPE,
        C_SHAPE,

        MAX_SHAPES,
    }
}

public class Island
{
    //List<Island> main_islands;
    public int size_x, size_z;
    public IslandBoundary island_boundary;

    public List<Grid> island_grid;
    public List<Grid> island_grid_outline;
    public List<IslandConnectors> island_connectors;
    public Grid center;

    public List<Grid> island_bridge_grid;

    public Island(int size_x, int size_z)
    {
        island_grid = new();
        island_grid_outline = new();
        island_connectors = new();
        island_bridge_grid = new();
        this.size_x = size_x;
        this.size_z = size_z;
    }

    public void Displace(int x, int z)
    {
        //Set new boundary n center
        island_boundary.DisplaceX(x);
        island_boundary.DisplaceZ(z);
        center.x += x; center.y += z;

        //Add to all grids
        foreach (Grid grid in island_grid)
        {
            grid.x += x;
            grid.y += z;
        }
        foreach (Grid grid in island_grid_outline)
        {
            grid.x += x;
            grid.y += z;
        }
        foreach (IslandConnectors islandConnector in island_connectors)
        {
            islandConnector.connector_grid.x += x;
            islandConnector.connector_grid.y += z;
        }
    } 
    public Grid GetRandomConnector(IslandConnectors.DIRECTION coming_dir)
    {
        IslandConnectors.DIRECTION returning_dir = GetOppositeConnectorDirection(coming_dir);

        //Add the right direction
        List<Grid> all_connector_positions = new();
        foreach (IslandConnectors islandConnector in island_connectors)
        {
            if (islandConnector.direction == returning_dir)
                all_connector_positions.Add(islandConnector.connector_grid);
        }

        if (all_connector_positions.Count == 0)
        {
            Debug.LogWarning("no valid connectors here...");
            return new Grid(-999, -999);

        }

        int odds = Random.Range(0, all_connector_positions.Count);
        return all_connector_positions[odds];
    }
    public IslandConnectors.DIRECTION GetOppositeConnectorDirection(IslandConnectors.DIRECTION coming_dir)
    {
        switch (coming_dir) { 
            case IslandConnectors.DIRECTION.Z_POSITIVE: return IslandConnectors.DIRECTION.Z_NEGATIVE;
            case IslandConnectors.DIRECTION.X_POSITIVE: return IslandConnectors.DIRECTION.X_NEGATIVE;
            case IslandConnectors.DIRECTION.Z_NEGATIVE: return IslandConnectors.DIRECTION.Z_POSITIVE;
            case IslandConnectors.DIRECTION.X_NEGATIVE: return IslandConnectors.DIRECTION.X_POSITIVE;
        }
        return IslandConnectors.DIRECTION.Z_POSITIVE;
    }
    public bool PointInIsland(int x, int z)
    {
        return (x > island_boundary.min_x
            && x < island_boundary.max_x
            && z > island_boundary.min_z
            && z < island_boundary.max_z);
    }
}
public class IslandBoundary
{
    public int min_x;
    public int max_x;
    public int min_z;
    public int max_z;

    public IslandBoundary(int min_x, int max_x, int min_z, int max_z)
    {
        this.min_x = min_x;
        this.max_x = max_x;
        this.min_z = min_z;
        this.max_z = max_z;
    }
    public void DisplaceX(int amount)
    {
        this.min_x += amount;
        this.max_x += amount;
    }
    public void DisplaceZ(int amount)
    {
        this.min_z += amount;
        this.max_z += amount;
    }
}
//Each island has 1 or 4 connectors, each connector stores the grid position at the start of the bridge connector
public class IslandConnectors
{
    public enum DIRECTION { Z_POSITIVE, X_POSITIVE, Z_NEGATIVE, X_NEGATIVE }
    public DIRECTION direction;
    public Grid connector_grid; //1 or more

}
