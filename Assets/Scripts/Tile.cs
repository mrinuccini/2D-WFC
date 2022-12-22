using UnityEngine;

namespace WFC
{
    [CreateAssetMenu(fileName = "New Tile", menuName = "WFC/Tile")]
    public class Tile : ScriptableObject
    {
        [Header("In game graphics")]
        public Sprite sprite;
        public int rotation;

        [Space]

        [Header("Algorithm tile information")]
        public int left;
        public int right;
        public int top;
        public int bottom;
    }
}
