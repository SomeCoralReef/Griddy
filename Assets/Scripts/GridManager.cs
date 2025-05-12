using UnityEngine;

public class GridManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int columns = 8;
    public int rows = 4;
    public Vector2 tileSize = new Vector2(1f,1f);
    public GameObject tilePrefab;
    public float spacing = 1f;

    public Transform gridParent;
    private GameObject[,] gridTiles;
    private void Awake()
    {
        gridTiles = new GameObject[columns, rows];
        CreateGrid();
    }
    private void CreateGrid()
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject tile = Instantiate(tilePrefab, gridParent);
                tile.transform.localPosition = new Vector3(x*spacing, y*spacing, 0);
                gridTiles[x, y] = tile;
            }
        }
    }

    public Vector3 GetWorldPosition(int x, int y, float z = -0.1f)
    {
        return new Vector3(x * tileSize.x, y * tileSize.y, z);
    }
    
}
