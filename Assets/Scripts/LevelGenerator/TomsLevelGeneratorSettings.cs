using UnityEngine;

public class TomsLevelGeneratorSettings : ScriptableObject {
	public bool useSeed;
	public int seed = 0;
	public int levelDiameter = 12;
	public int maxHeight = 10;
	public int minCityHeight = 2;
}