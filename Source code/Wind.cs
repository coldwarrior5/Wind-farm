using System;
using UnityEngine;

namespace Assets.Seminar.Scripts
{
	public class Wind : MonoBehaviour
	{
		private int minLength = 1000;
		private int maxLength = 5000;
		private int howLong;

		private int minStrength = 0;
		private int maxStrength = 50;
		private float howStrong;
		private int tick;
		private System.Random rand;
		private Vector3 terrainSize;
		private Vector3 pivotPoint = new Vector3(0, 0, 0);
		private WindZone zone;

		// Use this for initialization
		void Start()
		{
			zone = GameObject.Find("Vjetar").GetComponent<WindZone>();
			howLong = minLength;
			rand = new System.Random();
			howStrong = rand.Next(minStrength, maxStrength);
			zone.windMain = howStrong;
		}

		// Update is called once per frame
		void Update()
		{
			tick++;
			if (tick > howLong)
			{
				howStrong = (float)Gauss(12, 2);
				howStrong = Mathf.Clamp(howStrong, minStrength, maxStrength);
				howLong = rand.Next(minLength, maxLength);
				tick = 0;
				int angle = rand.Next(0, 359);
				transform.RotateAround(pivotPoint, Vector3.up, angle);
				zone.windMain = howStrong;
			}
		}

		double Gauss(double mean, double stdDev)
		{
			double u1 = 1.0 - rand.NextDouble();
			double u2 = 1.0 - rand.NextDouble();
			double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
			double randNormal = mean + stdDev * randStdNormal;
			return randNormal;
		}
	}
}
