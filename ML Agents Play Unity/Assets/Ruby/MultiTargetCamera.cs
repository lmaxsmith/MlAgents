using UnityEngine;
using System.Collections.Generic;

public class MultiTargetCamera : MonoBehaviour
{
	public List<Transform> targets;
	public Vector3 offset;
	public float smoothTime = 0.5f;
	public float minZoom = 40f;
	public float maxZoom = 10f;
	public float zoomLimiter = 50f;
	public bool rotateCamera = false;
	public float rotationSpeed = 20f;

	private Vector3 velocity;
	private Camera cam;

	void Start()
	{
		cam = GetComponent<Camera>();
	}

	void LateUpdate()
	{
		if (targets.Count == 0)
			return;

		Vector3 centerPoint = GetCenterPoint();
		Vector3 newPosition = centerPoint + offset;
		transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
        
		// Make the camera look at the center point
		transform.LookAt(centerPoint);

		Zoom();

		if (rotateCamera)
		{
			transform.RotateAround(centerPoint, Vector3.up, rotationSpeed * Time.deltaTime);
		}
	}

	void Zoom()
	{
		float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
		cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newZoom, Time.deltaTime);
	}

	float GetGreatestDistance()
	{
		var bounds = new Bounds(targets[0].position, Vector3.zero);
		for (int i = 0; i < targets.Count; i++)
		{
			bounds.Encapsulate(targets[i].position);
		}
		return bounds.size.x;
	}

	Vector3 GetCenterPoint()
	{
		if (targets.Count == 1)
		{
			return targets[0].position;
		}

		var bounds = new Bounds(targets[0].position, Vector3.zero);
		for (int i = 0; i < targets.Count; i++)
		{
			bounds.Encapsulate(targets[i].position);
		}

		return bounds.center;
	}
}