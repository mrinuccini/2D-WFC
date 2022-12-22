using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    [RequireComponent(typeof(WaveFunctionCollapse))]
    public class InfinteWFC : MonoBehaviour
    {
        [Range(10, 60)]
        public int maxViewDist = 40;
        public Transform viewer;

        [SerializeField] Transform parent;
        [SerializeField] WaveFunctionCollapse wfc;

        public static Vector2 viewerPosition;
        int chunckSize = 10;
        int chuncksVisibleInViewDst;

        Dictionary<Vector2, TerrainChunck> terrainChunckDictionnary = new Dictionary<Vector2, TerrainChunck>();
        List<TerrainChunck> terrainChunkVisibleLastUpdate = new List<TerrainChunck>();

        private void Start()
        {
            chuncksVisibleInViewDst = (int)(maxViewDist / chunckSize);
        }

        private void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.y);
            UpdateVisibleChuncks();
        }

        void UpdateVisibleChuncks()
        {
            /* Disable all the chunck rendered in the previous frame */
            foreach (TerrainChunck chunk in terrainChunkVisibleLastUpdate)
            {
                chunk.SetVisible(false);
            }
            terrainChunkVisibleLastUpdate.Clear();

            /* For every chunk that's visible */
            int currentChunckCoordX = Mathf.RoundToInt(viewerPosition.x / chunckSize);
            int currentChunckCoordY = Mathf.RoundToInt(viewerPosition.y / chunckSize);

            for (int yOffset = -chuncksVisibleInViewDst; yOffset < chuncksVisibleInViewDst; yOffset++)
            {
                for (int xOffset = -chuncksVisibleInViewDst; xOffset < chuncksVisibleInViewDst; xOffset++)
                {
                    Vector2 viewedChunckCoord = new Vector2(currentChunckCoordX + xOffset, currentChunckCoordY + yOffset);

                    /* Either spawn a new chunck or re-use the old one */
                    if (terrainChunckDictionnary.ContainsKey(viewedChunckCoord))
                    {
                        terrainChunckDictionnary[viewedChunckCoord].UpdateTerrainChunck();

                        if (terrainChunckDictionnary[viewedChunckCoord].IsVisible())
                        {
                            terrainChunkVisibleLastUpdate.Add(terrainChunckDictionnary[viewedChunckCoord]);
                        }
                    }
                    else
                    {
                        terrainChunckDictionnary.Add(viewedChunckCoord, new TerrainChunck(viewedChunckCoord, chunckSize, this, wfc));
                    }
                }
            }
        }

        /* A class representing a terrain  chunck */
        public class TerrainChunck
        {
            GameObject meshObject;
            Vector2 position;
            Bounds bounds;
            public ChunckBound chunckBound;
            InfinteWFC infiniteWFC;

            public TerrainChunck(Vector2 coord, int size, InfinteWFC infiniteWFC, WaveFunctionCollapse wfc)
            {
                // set references
                this.infiniteWFC = infiniteWFC;

                // Initialize the chunck
                position = coord * size;
                bounds = new Bounds(position, Vector3.one * size);
                Vector3 positionV3 = new Vector3(position.x, position.y, 0); // Change this when you want to switch to 3D

                // Generate the chunck
                meshObject = new GameObject($"Chunck_({coord.x} ; {coord.y})");
                meshObject.transform.parent = infiniteWFC.parent;

                // Computes the bound of neighbors chuncks
                ChunckBound neighborChunckBound = new ChunckBound( infiniteWFC.terrainChunckDictionnary.ContainsKey(coord + new Vector2(0, -1)) ? infiniteWFC.terrainChunckDictionnary[coord + new Vector2(0, -1)].chunckBound.bottom : null,
					infiniteWFC.terrainChunckDictionnary.ContainsKey(coord + new Vector2(0, 1)) ? infiniteWFC.terrainChunckDictionnary[coord + new Vector2(0, 1)].chunckBound.top : null,
					infiniteWFC.terrainChunckDictionnary.ContainsKey(coord + new Vector2(-1, 0)) ? infiniteWFC.terrainChunckDictionnary[coord + new Vector2(-1, 0)].chunckBound.right : null,
					infiniteWFC.terrainChunckDictionnary.ContainsKey(coord + new Vector2(1, 0)) ? infiniteWFC.terrainChunckDictionnary[coord + new Vector2(1, 0)].chunckBound.left : null);

                chunckBound = wfc.DoMagic(positionV3, meshObject.transform, neighborChunckBound);

                SetVisible(false);
            }

            // Use to check if the chunk should be visible
            public void UpdateTerrainChunck()
            {
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool isVisible = viewerDstFromNearestEdge <= infiniteWFC.maxViewDist;
                SetVisible(isVisible);
            }

            // Set the chunck to visible or not
            public void SetVisible(bool visible)
            {
                meshObject.SetActive(visible);
            }

            // Get the state of the chunck
            public bool IsVisible() => meshObject.activeSelf;
        }
    }
}
