using System;
using System.IO;
using UnityEngine;

namespace GameOfLife
{
    public class GameOfLifeRunner : MonoBehaviour
    {
        [SerializeField] private string inputFileName = "input.txt";
        [SerializeField] private string outputFileName = "output.txt";
        [SerializeField] private int generations = 10;
 
        private void Start()
        {
            string root = Path.Combine(Application.dataPath, "Resources");
            string inPath = Path.Combine(root, inputFileName);
            string outPath = Path.Combine(root, outputFileName);
 
            try
            {
                var start = Life106Util.Parse(File.ReadLines(inPath));
                var end = LifeEngine.Run(start, generations);
                using (var writer = new StreamWriter(outPath, false))
                    Life106Util.Write(writer, end);

                Debug.Log($"Life: {start.Count} -> {end.Count} live cells after {generations} generations. Wrote {outPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Game of Life failed: {e.Message}");
            }
        }
    }
}