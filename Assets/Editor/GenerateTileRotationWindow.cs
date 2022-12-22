using UnityEditor;
using UnityEngine;
using WFC;

namespace WFC.Editor
{
	public class GenerateTileRotationWindow : EditorWindow
	{
		Tile tile;
		string outputPath;

		/*Just a fonction to make the window openable*/
		[MenuItem("Window/WFC/Tile rotations generator")]
		public static void ShowWindow()
		{
			GetWindow<GenerateTileRotationWindow>().Show();
		}

		/* Generate some sick UI for our window */
		void OnGUI()
		{
			GUILayout.Label("Generate tile variants", EditorStyles.boldLabel);

			GUILayout.Space(12);

			GUILayout.Label("Input original tile");

			tile = EditorGUILayout.ObjectField(tile, typeof(Tile), false) as Tile;

			GUILayout.Space(12);

			GUILayout.Label("Input output path");

			outputPath = EditorGUILayout.TextField(outputPath);

			if (GUILayout.Button("Generate tile variants"))
			{
				Debug.Log("Creating tile variants...");
				CreateTileVariants();
				Debug.Log("Generated tile variants...");
			}
		}

		// Just a fonction that is used to create a bunch of rotated file
		void CreateTileVariants()
		{
			int[] directions = new int[] { 90, 180, -90 };

			foreach (int rotation in directions)
			{
				Tile newTile = new Tile();
				newTile.rotation = rotation;
				newTile.sprite = tile.sprite;

				switch (rotation)
				{
					case 90:
						newTile.right = tile.bottom;
						newTile.left = tile.top;
						newTile.top = tile.right;
						newTile.bottom = tile.left;
						break;
					case -90:
						newTile.right = tile.top;
						newTile.left = tile.bottom;
						newTile.top = tile.left;
						newTile.bottom = tile.right;
						break;
					case 180:
						newTile.right = tile.left;
						newTile.left = tile.right;
						newTile.top = tile.bottom;
						newTile.bottom = tile.top;
						break;
				}
				AssetDatabase.CreateAsset(newTile, $"Assets/{outputPath}/{tile.name}_{rotation}.asset");
			}
		}
	}
}
