using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace Assets.Seminar.Scripts
{
	public class Rotor : MonoBehaviour
	{
		private const int second = 1000;			// one ssecond is 1000 miliseconds
		private const int minute = 60;
		private int lastHour;
		private Timer aTimer;

		private float rotationSpeed;				// RPM
		private float currentRotationSpeed;			// RPM
		private float angularAcceleration = 40;		// RPM^2
		private const int minRevolutions = 5;		// RPM
		private const int maxRevolutions = 16;      // RPM

		private float bladeMotorSpeed = 3;			// RPM
		private float turnOnWindSpeed = 2.5f;       // m/s
		private float reductionWindSpeed = 12.5f;   // m/s
		private float shutDownWindSpeed = 25f;		// m/s

		private bool increment = true;
		private float pitch;
		private float currentPitch;
		private const int openThrottle = 0;
		private const int closedThrottle = 90;
		private int revolution = 360;				// 360 degrees in ove revolution

		private float airDensity = 1.225f;			// kg/m^3
		private float radius = 91f;					// m

		private Body myParent;
		private AudioSource audio;
		private GameObject windObject;
		private WindZone wind;
		private float windSpeed;
		public float power;
		private float angleDifference;
		private Dictionary<int, float> Cp = new Dictionary<int, float>()
		{
			{ 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0.48f }, { 4, 0.49f }, { 5, 0.49f },
			{ 6, 0.49f }, { 7, 0.49f }, { 8, 0.49f }, { 9, 0.49f }, { 10, 0.49f },
			{ 11, 0.48f }, { 12, 0.43f }, { 13, 0.33f }, { 14, 0.28f }, { 15, 0.22f },
			{ 16, 0.18f }, { 17, 0.15f }, { 18, 0.12f }, { 19, 0.102f }, { 20, 0.9f },
			{ 21, 0.18f }, { 22, 0.17f }, { 23, 0.16f }, { 24, 0.15f }, { 25, 0.145f }
		};

		// Use this for initialization
		void Start ()
		{
			currentRotationSpeed = 0;
			currentPitch = openThrottle;
			myParent = transform.parent.GetComponent<Body>();
			windObject = GameObject.Find("Vjetar");
			audio = GetComponent<AudioSource>();
			wind = windObject.GetComponent<WindZone>();
			aTimer = new Timer(minute * second);
			lastHour = DateTime.Now.Hour;
			aTimer.Elapsed += OnTimedEvent;
			aTimer.Enabled = true;
		}
	
		// Update is called once per frame
		void Update ()
		{
			SpeedControl();
			CalculatePower();
		}

		void SpeedControl()
		{
			float radAngle = Mathf.PI / 180 * myParent.angleDifference;
			windSpeed = wind.windMain * Mathf.Cos(radAngle);
			if (windSpeed >= shutDownWindSpeed)
				pitch = closedThrottle;
			else if (windSpeed >= turnOnWindSpeed && windSpeed <= reductionWindSpeed)
				pitch = openThrottle;
			else if(windSpeed > reductionWindSpeed)
			{
				float delta = (closedThrottle - openThrottle) / (shutDownWindSpeed - reductionWindSpeed);
				pitch = openThrottle + delta * (windSpeed - reductionWindSpeed);
			}

			AdjustPitch();

			float derivation = (maxRevolutions - minRevolutions) / (reductionWindSpeed - turnOnWindSpeed);
			rotationSpeed = minRevolutions + derivation * ((closedThrottle - currentPitch) / closedThrottle * windSpeed - turnOnWindSpeed);
			if (Mathf.Abs(closedThrottle - currentPitch) < Mathf.Epsilon || windSpeed < turnOnWindSpeed || windSpeed > shutDownWindSpeed)
				rotationSpeed = 0;

			ApplyRotation();
		}

		void AdjustPitch()
		{
			if (Mathf.Abs(pitch - currentPitch) > Mathf.Epsilon)
			{
				float angle = RpmToAngle(bladeMotorSpeed);
				if (angle > Mathf.Abs(pitch - currentPitch))
					angle = Mathf.Abs(pitch - currentPitch);
				currentPitch = pitch - currentPitch > 0 ? currentPitch + angle : currentPitch - angle;
				currentPitch = Mathf.Clamp(currentPitch, openThrottle, closedThrottle);
				
				ChangeBladePitch(currentPitch);
			}
		}

		void ApplyRotation()
		{
			if (Mathf.Abs(rotationSpeed - currentRotationSpeed) > Mathf.Epsilon)
			{
				float changeRotationSpeed = angularAcceleration * (Time.deltaTime / minute);
				if (changeRotationSpeed > Mathf.Abs(rotationSpeed - currentRotationSpeed))
					changeRotationSpeed = Mathf.Abs(rotationSpeed - currentRotationSpeed);
				currentRotationSpeed = rotationSpeed - currentRotationSpeed > 0 ?  currentRotationSpeed + changeRotationSpeed : currentRotationSpeed - changeRotationSpeed;
			}
			else
				currentRotationSpeed = rotationSpeed;
			float angle = RpmToAngle(currentRotationSpeed);
			if (currentRotationSpeed < 4)
				audio.mute = true;
			else
				audio.mute = false;
			transform.Rotate(Vector3.left, angle);
		}

		void CalculatePower()
		{
			float cp = 0;
			if (windSpeed >= turnOnWindSpeed && windSpeed < shutDownWindSpeed)
				cp = (Cp[(int) Mathf.Floor(windSpeed)] + Cp[(int) Mathf.Ceil(windSpeed)]) / 2;

			power = 0.5f * airDensity * Mathf.Pow(radius, 2) * Mathf.PI * Mathf.Pow(windSpeed, 3) * cp;
		}

		float RpmToAngle(float rpm)
		{
			return rpm / minute * revolution * Time.deltaTime;
		}

		// Protecting wildlife during two months for 5 hours during night
		void ProtectBirdsAndBats()
		{
			DateTime seasonStart = new DateTime(DateTime.Today.Year, 6, 15);
			DateTime seasonEnd = new DateTime(DateTime.Today.Year, 9, 15);
			if (DateTime.Today >= seasonStart && DateTime.Today <= seasonEnd && (DateTime.Now.Hour <= 3 || DateTime.Now.Hour >= 22))
			{
				ShutDown();
				Debug.Log("Protecting birds and bats!");
			}
		}

		void ShutDown()
		{
			ChangeBladePitch(closedThrottle);
		}

		void ChangeBladePitch(float degrees)
		{
			Vector3 rotation = new Vector3(0, 0, degrees);
			for (int i = 0; i < transform.childCount; i++)
			{
				transform.GetChild(i).localRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
				rotation.x += 360.0f / transform.childCount;
			}
		}

		private void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			if (lastHour < DateTime.Now.Hour || lastHour == 23 && DateTime.Now.Hour == 0)
			{
				lastHour = DateTime.Now.Hour;
				ProtectBirdsAndBats();
			}
		}
	}
}
