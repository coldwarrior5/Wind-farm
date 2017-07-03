using System.Collections.Generic;
using UnityEngine;

namespace Assets.Seminar.Scripts
{
	public class MoveCamera : MonoBehaviour
	{
		public Rigidbody rb;
		public float turnSpeed = 100.0f;      // Speed of camera turning when mouse moves in along an axis
		public float panSpeed =100.0f;       // Speed of the camera when being panned
		public float zoomSpeed = 100.0f;      // Speed of the camera going back and forth
		public float increment = 100.0f;
		public float speedLimit = 500.0f;
		public float screenPercentage = 0.1f;

		private Vector3 mouseOrigin;    // Position of cursor when mouse dragging starts
		private bool isPanning;     // Is the camera being panned?
		private bool isRotating;    // Is the camera being rotated?
		private bool isZooming;     // Is the camera zooming?
		private float rotX = 0;
		private float rotY = 0;
		private float angleMin = 80;
		private float angleMax = 280;

		private Vector3 zooming;


		void Start()
		{
			zooming = new Vector3();
			rb.GetComponent<Rigidbody>();
		}


		void FixedUpdate()
		{
			// Player has clicked something
			if (Input.GetMouseButtonDown(0))
			{
				
			}
			// Rotating
			if (Input.GetMouseButtonDown(2))
			{
				// Get mouse origin
				mouseOrigin = Input.mousePosition;
				isRotating = true;
			}

			// Check if the mouse is near the edge of the screen
			if (!isRotating && !isPanning && CheckZooming(Input.mousePosition))
			{
				// Get mouse origin
				mouseOrigin = Input.mousePosition;
				isZooming = true;
			}
			else if(!isRotating && !isPanning)
			{
				zooming.x = 0;
				zooming.y = 0;
				isZooming = false;
				rb.constraints = RigidbodyConstraints.FreezeAll;
			}

			// Get the middle mouse button
			if (Input.GetMouseButtonDown(1))
			{
				// Get mouse origin
				mouseOrigin = Input.mousePosition;
				isPanning = true;
			}

			if (Input.GetAxis("Mouse ScrollWheel") != 0)
			{
				GetValue(Input.GetAxis("Mouse ScrollWheel"));
			}

			// Disable movements on button release
			if (isRotating && !Input.GetMouseButton(2))
			{
				isRotating = false;
				rb.constraints = RigidbodyConstraints.FreezeAll;
			}

			if (isPanning && !Input.GetMouseButton(1))
			{
				isPanning = false;
				rb.constraints = RigidbodyConstraints.FreezeAll;
			}

			// Rotate camera along X and Y axis
			if (isRotating)
			{
				rb.constraints = RigidbodyConstraints.FreezePosition;
				Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);
				rotY = pos.x * turnSpeed * Time.deltaTime;
				pos.y = pos.y * turnSpeed * Time.deltaTime;
				rotX = rb.rotation.eulerAngles.x - pos.y >= angleMax || rb.rotation.eulerAngles.x - pos.y <= angleMin ? -pos.y : 0;    //either bigger than 270 or not bigger than 90
				Quaternion deltaRotationX = Quaternion.AngleAxis(rotX, rb.transform.right);
				rb.MoveRotation(deltaRotationX * rb.rotation);
				Quaternion deltaRotationY = Quaternion.AngleAxis(rotY, Vector3.up);
				rb.MoveRotation(deltaRotationY * rb.rotation);
			}

			// Move the camera on it's XY plane
			if (isPanning)
			{
				rb.constraints = RigidbodyConstraints.FreezeRotation;
				Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

				Vector3 moveVertically = pos.y * panSpeed * rb.transform.up;
				Vector3 moveHorizontally = pos.x * panSpeed * rb.transform.right;
				rb.velocity = moveHorizontally + moveVertically;
			}

			// Move the camera linearly along Z axis
			if (isZooming)
			{
				rb.constraints = RigidbodyConstraints.FreezeRotation;
				Vector3 pos = Camera.main.ScreenToViewportPoint(zooming * 1000);
				float rotY = pos.x * zoomSpeed * Time.deltaTime;
				Vector3 forward = rb.transform.forward * pos.y * zoomSpeed;
				rb.velocity = forward;
				Quaternion deltaRotationY = Quaternion.AngleAxis(rotY, Vector3.up);
				rb.MoveRotation(deltaRotationY * rb.rotation);

			}
		}

		private void ScrollPan(float howMuch)
		{
			rb.constraints = RigidbodyConstraints.FreezeRotation;
			isZooming = true;
			zooming.x = 0;
			zooming.y = howMuch * 30 * zoomSpeed * Time.deltaTime;
		}

		private void GetValue(float howMuch)
		{
			if (isPanning)
			{
				panSpeed += howMuch * increment;
				panSpeed = Mathf.Clamp(panSpeed, 0, speedLimit);
			}
			else if (isZooming)
			{
				zoomSpeed += howMuch * increment;
				zoomSpeed = Mathf.Clamp(zoomSpeed, 0, speedLimit);
			}
			else if (isRotating)
			{
				turnSpeed += howMuch * increment;
				turnSpeed = Mathf.Clamp(turnSpeed, 0, speedLimit);
			}
			else
				ScrollPan(Input.GetAxis("Mouse ScrollWheel"));
		}

		bool CheckZooming(Vector3 mousePosition)
		{
			bool returnValue = false;
			int screenWidth = Camera.main.pixelWidth;
			int screenHeight = Camera.main.pixelHeight;

			if (mousePosition.x > (1 - screenPercentage) * screenWidth)
			{
				zooming.x = (mousePosition.x - (1 - screenPercentage) * screenWidth) / (screenPercentage * screenWidth);
				returnValue = true;
			}
			else if (mousePosition.x < screenPercentage * screenWidth)
			{
				zooming.x = mousePosition.x / (screenPercentage * screenWidth) - 1;
				returnValue = true;
			}
			if (mousePosition.y > (1 -screenPercentage) * screenHeight)
			{
				zooming.y = (mousePosition.y - (1 - screenPercentage) * screenHeight) / (screenPercentage * screenHeight);
				returnValue = true;
			}
			else if (mousePosition.y < (screenPercentage) * screenHeight)
			{
				zooming.y = mousePosition.y / (screenPercentage * screenHeight) - 1;
				returnValue = true;
			}
			return returnValue;
		}

		private void RemoveTrees(Vector3 position, int radius)
		{
			TerrainData theIsland;
			List<TreeInstance> newTrees = new List<TreeInstance>();
			theIsland = GameObject.Find("Pometeno brdo").GetComponent<Terrain>().terrainData;
			// For every tree on the island
			foreach (TreeInstance tree in theIsland.treeInstances)
			{
				// Find its local position scaled by the terrain size (to find the real world position)
				Vector3 worldTreePos = Vector3.Scale(tree.position, theIsland.size) + Terrain.activeTerrain.transform.position;
				if (Vector3.Distance(position, worldTreePos) > radius)
					newTrees.Add(tree);
			}
			// Then delete all trees on the island
			theIsland.treeInstances = newTrees.ToArray();
			return;
		}
	}
}

