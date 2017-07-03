using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Seminar.Scripts
{
	public class Body : MonoBehaviour
	{
		private int rotationSpeed = 1;      // RPM
		private int revolution = 360;       // 360 degrees in ove revolution
		private const int minute = 60;      // seconds

		private float turnOnWindSpeed = 2.5f;       // m/s

		private GameObject windObject;
		private WindZone zone;
		Vector3 windmillDirection;
		Vector3 windDirection;
		public float angleDifference;

		// Use this for initialization
		void Start()
		{
			windObject = GameObject.Find("Vjetar");
			zone = windObject.GetComponent<WindZone>();
			DetermineDirections();
			CalculateAngle();
		}

		// Update is called once per frame
		void Update()
		{
			DetermineDirections();
			CalculateAngle();
			ApplyRotation();
		}

		void DetermineDirections()
		{
			windmillDirection = transform.localEulerAngles;
			windmillDirection.y = (windmillDirection.z + 90) % 360;
			windmillDirection.z = 0;
			
			windDirection = windObject.transform.localEulerAngles;
		}

		void CalculateAngle()
		{
			angleDifference = windmillDirection.y - windDirection.y;
		}

		void ApplyRotation()
		{
			if (Mathf.Abs(angleDifference) > Mathf.Epsilon && zone.windMain >= turnOnWindSpeed)
			{
				float angle = RpmToAngle(rotationSpeed);
				if (angle <= Mathf.Abs(angleDifference))
				{
					angle = angleDifference < 0 ? angle : -angle;
					transform.Rotate(Vector3.forward, angle);
				}
				else
					transform.Rotate(Vector3.forward, -angleDifference);
			}
		}

		float RpmToAngle(int rpm)
		{
			return (float)rpm / minute * revolution * Time.deltaTime;
		}
	}

}