using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WFC
{
    [System.Serializable]
    public struct WFCTile
    {
        public Tile tile;
        public bool isCollapsed;
        public List<Tile> superpositions;

        public int GetEntropy() => superpositions.Count;

        public WFCTile(Tile[] defaultSuperpositions) 
        {
            tile = null;
            isCollapsed = false;
            superpositions = defaultSuperpositions.ToList();
        }
    }

	public struct ChunckBound 
    {
        public List<WFCTile> top;
        public List<WFCTile> bottom;
        public List<WFCTile> left;
        public List<WFCTile> right;

        public ChunckBound(List<WFCTile> top, List<WFCTile> bottom, List<WFCTile> left, List<WFCTile> right) 
        {
            this.top = top;
            this.bottom = bottom;
            this.left = left;
            this.right = right;
        }
    }

    public class WaveFunctionCollapse : MonoBehaviour
    {
        [Header("Wave Function Collapse data")]
        [SerializeField] Tile[] tileList;

        [Space]

        [Header("Dimensions")]
        [SerializeField] int xDim;
        [SerializeField] int yDim;

        [Space]

        [Header("Rendering & Infite Generation Settings")]

        [SerializeField] Vector3 chunckOrigin;

		[Space]

        [Header("Editor Settings")]
        [SerializeField][Range(0.0f, 100.0f)] float timeBetweenIterations;
        [SerializeField] Transform parent;
        [SerializeField] bool generateOnStart = true;
        [SerializeField] GameObject obj;

        Dictionary<Vector2Int, WFCTile> map = new Dictionary<Vector2Int, WFCTile>();

		private void Start()
		{
			if (generateOnStart) 
            {
                Clear();
                Generate();
            }
		}

		public void Generate()
        {
            Initialize();

            StartCoroutine(IterateEnumerator());
        }

        IEnumerator IterateEnumerator()
        {
            if (IsCollapsed())
                yield break;

            Iterate();
            yield return new WaitForSeconds(timeBetweenIterations);
            StartCoroutine(IterateEnumerator());
        }

        // Use this for procedural generation
        public ChunckBound DoMagic(Vector2 basePosition, Transform parent, ChunckBound bound) 
        {
            map.Clear();
            chunckOrigin = basePosition;
            this.parent = parent;
            Initialize();

            for(int x = 0; x < xDim; x++) 
            {
                if (bound.top != null)
                {
                    CollapseAt(new Vector2Int(x, 0), bound.top[x].tile);
                    Propagate(new Vector2Int(x, 0));
                }

				if (bound.bottom != null)
				{
					CollapseAt(new Vector2Int(x, yDim - 1), bound.bottom[x].tile);
					Propagate(new Vector2Int(x, yDim - 1));
				}
			}

            for(int y = 0; y < yDim; y++) 
            {
				if (bound.left != null)
				{
					CollapseAt(new Vector2Int(0, y), bound.left[y].tile);
					Propagate(new Vector2Int(0, y));
				}

				if (bound.right != null)
				{
					CollapseAt(new Vector2Int(xDim - 1, y), bound.right[y].tile);
					Propagate(new Vector2Int(xDim - 1, y));
				}
			}

            while (true)
            {
                if (IsCollapsed()) break;

                Iterate();
            }

            print("Finished Generation Chunck");

            List<WFCTile> top_ = (from entry in map where entry.Key.y == 0 select entry.Value).ToList();
            List<WFCTile> bottom_ = (from entry in map where entry.Key.y == xDim - 1 select entry.Value).ToList();
            List<WFCTile> left_ = (from entry in map where entry.Key.x == 0 select entry.Value).ToList();
            List<WFCTile> right_ = (from entry in map where entry.Key.x == xDim - 1 select entry.Value).ToList();

            ChunckBound chunckBound = new ChunckBound(top_, bottom_, left_, right_);
            return chunckBound;
        }

        // The same as IterateEnumerator but doesn't have a timer (better to use during non runtime testing)
        public void DoMagic()
        {
            Initialize();

            while (true)
            {
                if (IsCollapsed()) break;

                Iterate();
            }
            print("Finished Generation Chunck");
        }

        // Use to render a tile relative to the origin (0, 0)
        void RenderTile(Vector2Int pos, WFCTile tile)
        {
            if (pos.y == yDim || pos.y == 0) return;
            if (pos.x == 0 || pos.x == xDim) return;

            GameObject obj = new GameObject();
            obj.AddComponent<SpriteRenderer>();
            obj.GetComponent<SpriteRenderer>().sprite = tile.tile.sprite;

            obj.transform.position = chunckOrigin + (Vector3)new Vector2(pos.x, pos.y);
            obj.transform.rotation = Quaternion.Euler(0, 0, tile.tile.rotation);
            obj.transform.parent = parent;
        }

        // Use to render a tile relative to a custom origin (useful for chuck generation)
        void RenderTile(Vector2Int pos, WFCTile tile, Vector3 origin_) 
        {
			if (pos.y == yDim - 1 || pos.y == 0) return;
			if (pos.x == 0 || pos.x == xDim - 1) return;

			GameObject obj = new GameObject();
            obj.AddComponent<SpriteRenderer>();
            obj.GetComponent<SpriteRenderer>().sprite = tile.tile.sprite;

            obj.transform.position = origin_ + (Vector3)new Vector2(pos.x, pos.y);
            obj.transform.rotation = Quaternion.Euler(0, 0, tile.tile.rotation);
            obj.transform.parent = parent;
        }

        public void Clear() 
        {
            foreach(Transform child in parent) 
            {
                DestroyImmediate(child.gameObject);
            }
        }

        #region Generation

        /*Return true if all tiles are collapsed*/
        bool IsCollapsed()
        {
            foreach(WFCTile tile in map.Values) 
            {
                if (!tile.isCollapsed) return false;
            }

            return true;
        }

        /* Use to initialize the map with specific dimensions */
        void Initialize()
        {
            for (int y = 0; y < yDim; y++)
            {
                for (int x = 0; x < xDim ; x++)
                {
                    map.Add(new Vector2Int(x, y), new WFCTile(tileList));
                }
            }
        }

        void Iterate()
        {
            // Pick the tile with the lowest entropy
            Vector2Int minEntropyCell = GetMinEntropyCell();

            // Collapse the tile with the lowest entropy
            CollapseAt(minEntropyCell);

            // Propagate the consequences of the collapse to all neighboring cells
            Propagate(minEntropyCell);
        }

        /* Use to propagate the collapse of one cell to all neighboring cells */
        void Propagate(Vector2Int origin)
        {
            List<Vector2Int> stack = new List<Vector2Int>();
            stack.Add(origin);

            // While the stack is not empty
            do
            {
                Vector2Int current = stack.PopBack();

                foreach (Vector2Int direction in GetDirections())
                {
                    Vector2Int neighbor = current + direction;

                    if (!IsValidCoord(neighbor)) continue;

                    if (map[neighbor].GetEntropy() == 0 || map[neighbor].isCollapsed) continue;

                    List<Tile> currentPossibleNeighbor = GetPossibleNeighborsAtDirection(map[current], direction);
                    List<Tile> neighborCurrentSuperpositions = new List<Tile>(map[neighbor].superpositions);

                    /*
                    string build = "";

					if (map[current.y][current.x].isCollapsed) 
                    {
                        build += map[current.y][current.x].tile.name;

					}
					else 
                    {
                        build += "[";

						foreach (Tile tile in map[current.y][current.x].superpositions) 
                        {
                            build += $"{tile.name}, ";
                        }

                        build += "]";
                    }

                    build += $" ({direction}) : ";

                    foreach (Tile tile in currentPossibleNeighbor) 
                    {
                        build += $"{tile.name}, ";
                    }

                    Debug.Log(build);
                    */

                    foreach (Tile tile in neighborCurrentSuperpositions)
                    {
                        /*
                        GameObject test = Instantiate(obj);
                        test.transform.position = new Vector3(neighbor.x, neighbor.y, 0);
                        */

                        if (currentPossibleNeighbor.Contains(tile)) continue;

                        map[neighbor].superpositions.Remove(tile);

                        if (!stack.Contains(neighbor)) stack.Add(neighbor);
                    }

                    if (map[neighbor].GetEntropy() == 1)
                    {
                        CollapseAt(neighbor);
                    }
                }
            } while (stack.Count > 0);
        }

        /* Return all the neighbors that can be adjacent to a tile at a specific direction */
        List<Tile> GetPossibleNeighborsAtDirection(WFCTile tile, Vector2Int direction) 
        {
            List<Tile> output = new List<Tile>();

			switch (tile.isCollapsed) 
            {
                case true:
                    int possibleSocket = GetSocketAtDir(tile.tile, direction);

                    foreach(Tile tile_ in tileList) 
                    {
                        if(GetSocketAtDir(tile_, -direction) == possibleSocket) 
                        {
                            output.Add(tile_);
                        }
                    }

                    break;

                case false:
                    List<int> possibleSockets = new List<int>();

                    foreach(Tile tile_ in tile.superpositions) 
                    {
                        int socket = GetSocketAtDir(tile_, direction);

						if (!possibleSockets.Contains(socket)) 
                        {
                            possibleSockets.Add(socket);
                        }
                    }

                    foreach (Tile _tile in tileList)
                    {
                        if (possibleSockets.Contains(GetSocketAtDir(_tile, -direction)))
                        {
                            output.Add(_tile);
                        }
                    }

                    break;
            }

            /*
            if (!tile.isCollapsed)
            {
                string strOutput = $"({direction}) (";

                foreach (Tile tile_ in tile.superpositions)
                {
                    strOutput += $"{tile_.name}, ";
                }

                strOutput += ") : (";

                foreach (Tile tile_ in output)
                {
                    strOutput += $"{tile_.name}, ";
                }

                Debug.Log($"{strOutput})");
            }
            */

            return output;
        }

        /* Return the socket of a tile at a specific direction */
        int GetSocketAtDir(Tile tile, Vector2Int direction) 
        {
			switch (direction.x) 
            {
                case -1:
                    return tile.left;
                case 1:
                    return tile.right;
                default:
                    break;
            }

			switch (direction.y) 
            {
                case -1:
                    return tile.top;
                case 1:
                    return tile.bottom;
                default:
                    break;
            }

            return 0;
        }

        /* Return an array of vectors to get neighboring vectors */
        Vector2Int[] GetDirections() => new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        /* Returns true if a coord is valid */
        bool IsValidCoord(Vector2Int coord) 
        {
            if (coord.y < 0 || coord.y >= yDim) return false;

            if (coord.x < 0 || coord.x >= xDim) return false;

            return true;
        }

        /* Returns the coordonate of the cell with the lowest entropy (with randomisation in case there is a tie) */
        Vector2Int GetMinEntropyCell() 
        {
            // A dict with the key being cell coordonates and value being their entropy
            Dictionary<Vector2Int, int> tileDict = new Dictionary<Vector2Int, int>();

			// Generate the dictionnary
			foreach(KeyValuePair<Vector2Int, WFCTile> kv in map)
            {
                if(kv.Value.isCollapsed) continue;

                tileDict.Add(kv.Key, kv.Value.GetEntropy());
			}

            // Sort the dictionnary by descending order
            var sortedDict = from entry in tileDict orderby entry.Value ascending select entry;

            // All the cell with the lowest entropy
            List<Vector2Int> minEntropyCells = new List<Vector2Int>();

            // Use to gather all the cell with the lowest entropy
            int min_entropy = 999;

            foreach(var cell in sortedDict) 
            {
                if (cell.Value < min_entropy) min_entropy = cell.Value;

                if (cell.Value != min_entropy) continue;

                minEntropyCells.Add(cell.Key);
            }

            // Returns a random value from the list of cell with the lowest entropy
            return minEntropyCells[Random.Range(0, minEntropyCells.Count)];
        }

        /* Use to collapse a cell at a specific position */
        void CollapseAt(Vector2Int coords) 
        {
            // Take the tile and collapse it
            WFCTile tile = map[coords];

            if (tile.isCollapsed) return;

            // Make sure that they are still available tiles
            Debug.Assert(tile.superpositions.Count != 0, "There was no tile to choose from, chunck generation failed");

            tile.isCollapsed = true;
            tile.tile = tile.superpositions[Random.Range(0, tile.superpositions.Count)];
            tile.superpositions.Clear();

            // Render it
            RenderTile(coords, tile);

            // Apply it back to the map
            map[coords] = tile;
        }

        void CollapseAt(Vector2Int coords, Tile tile_)
        {
            // Take the tile and collapse it
            WFCTile tile = map[coords];

            tile.isCollapsed = true;
            tile.tile = tile_;
            tile.superpositions.Clear();

            // Render it
            RenderTile(coords, tile);

            // Apply it back to the map
            map[coords] = tile;
        }

        #endregion
    }
}

/* Just a class that contains custom methods for list */
static class ListExtension
{
    /* Method to be able to pop back from a list */
    public static T PopBack<T>(this List<T> list) 
    {
        T last = list.Last();
        list.RemoveAt(list.Count - 1);
        return last;
    }
}
