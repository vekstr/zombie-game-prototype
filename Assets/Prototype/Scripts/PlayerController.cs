using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
	[Header("Component References")]
	public Animator anim;
	public Rigidbody body;

	[Header("Settings")]
	public int lives = 5;
	public float moveSpeed = 5;
	public float turnSpeed = 5;
	public float cameraSpeed = 5;
	[Space]
	public float bodyRadius = 0.8f;
	[Space]
	public int joystickRadius = 160;
	[Space]
	public Transform shoulderCameraPivotTransform;
	public float shoulderCameraMinAttackAngle;
	public float shoulderCameraMaxAttackAngle;
	[Space]
	public Transform bulletContainer;
	public Transform bulletOriginTransform;
	public Bullet bulletPrefab;
	public Sparks sparksPrefab;
	public float bulletSpread = 0.5f;
	public int bulletPerClip = 24;
	public float fireDelay = 0.4f;
	public float reloadTime = 2.5f;

	[Header("Cameras")]
	public Camera shoulderCamera;
	public Camera topCamera;

	[Header("Control UI References")]
	public RectTransform moveJoystickOriginTransform;
	public RectTransform moveJoystickTransform;
	public RectTransform rotateJoystickOriginTransform;
	public RectTransform rotateJoystickTransform;
	public RectTransform shootJoystickOriginTransform;
	public RectTransform shootJoystickTransform;
	public RectTransform shootButton;
	public GameObject crosshairObject;

	[Header("Status UI Reference")]
	public TextMeshProUGUI bulletText;
	public TextMeshProUGUI livesText;

	[Header("End UI Objects")]
	public GameObject endUIObject;
	public GameObject loseUIObject;

	//--------------------------------------------------------------------

	private List<Bullet> bulletPool = new List<Bullet>();
	private List<Sparks> sparksPool = new List<Sparks>();

	//--------------------------------------------------------------------

	private Vector3 defaultMoveJoystickOriginPosition;
	private Vector3 defaultRotateJoystickOriginPosition;
	private Vector3 defaultShootJoystickOriginPosition;
	private Vector3 defaultShootButtonPosition;

	private Vector3 leftFingerOrigin;
	private Vector3 leftFingerPosition;

	private Vector3 rightFingerOrigin;
	private Vector3 rightFingerPosition;

	private int leftFingerId;
	private int rightFingerId;

	//--------------------------------------------------------------------

	private Transform cameraTransform;
	private Vector3 projectedCameraForward;
	private Vector3 projectedCameraRight;
	private bool isInTopDownView;
	private float shoulderCameraAttackAngle;

	//--------------------------------------------------------------------

	private bool isShooting;
	private int bulletsInClip;
	private float lastShotTime;

	//--------------------------------------------------------------------

	private bool gameIsEnded;

	//--------------------------------------------------------------------

	private void Awake()
	{
		InitializeVariables();
		UpdateStatusUI();
	}

	private void Start()
	{
		UpdateCameraTransform();
		UpdateCameraProjections();
		UpdateShoulderCameraAttackAngle();
	}

	private void Update()
	{
		if (gameIsEnded) { return; }

		HandleTouchInput();
		HandleReload();

		if (isShooting)
		{
			HandleShootings();
		}
	}

	private void FixedUpdate()
	{
		if (gameIsEnded) { return; }

		if (!isInTopDownView)
		{
			UpdateCameraProjections();
		}

		HandleMovement();
		HandleRotation();
		HandleAnimation();
	}

	private void LateUpdate()
	{
		if (gameIsEnded) { return; }

		UpdateControlUI();
	}

	//--------------------------------------------------------------------

	private void InitializeVariables()
	{
		leftFingerId = -1;
		rightFingerId = -1;

		isInTopDownView = true;

		defaultMoveJoystickOriginPosition = moveJoystickOriginTransform.position;
		defaultRotateJoystickOriginPosition = rotateJoystickOriginTransform.position;
		defaultShootJoystickOriginPosition = shootJoystickOriginTransform.position;
		defaultShootButtonPosition = shootButton.position;

		for (int i = 0; i < bulletPerClip * 5; i++)
		{
			bulletPool.Add(Instantiate(bulletPrefab, bulletContainer));
			sparksPool.Add(Instantiate(sparksPrefab, bulletContainer));
		}

		bulletsInClip = bulletPerClip;
		gameIsEnded = false;
	}

	//--------------------------------------------------------------------

	private void HandleTouchInput()
	{
		var foundLeftFinger = false;
		var foundRightFinger = false;
		var newFingers = 0;

		foreach (var touch in Input.touches)
		{
			if (touch.fingerId == leftFingerId)
			{
				foundLeftFinger = true;
				leftFingerPosition = touch.position;
			}
			else if (touch.fingerId == rightFingerId)
			{
				foundRightFinger = true;
				rightFingerPosition = touch.position;

				if (!isInTopDownView && touch.phase == TouchPhase.Stationary)
				{
					//rightFingerOrigin = Vector3.Lerp(rightFingerOrigin, rightFingerPosition, 30f * Time.deltaTime);
					rightFingerOrigin = rightFingerPosition;
				}
			}
			else if (touch.phase == TouchPhase.Began)
			{
				newFingers += 1;
			}
		}

		leftFingerId = foundLeftFinger ? leftFingerId : -1;
		rightFingerId = foundRightFinger ? rightFingerId : -1;

		if (newFingers != 0 && (leftFingerId == -1 || rightFingerId == -1))
		{
			foreach (var touch in Input.touches)
			{
				if (touch.phase == TouchPhase.Began)
				{
					if (touch.position.x < Screen.width / 2f)
					{
						if (leftFingerId == -1)
						{
							leftFingerId = touch.fingerId;
							leftFingerOrigin = touch.position;
							leftFingerPosition = touch.position;
						}
					}
					else
					{
						if (rightFingerId == -1)
						{
							rightFingerId = touch.fingerId;
							rightFingerOrigin = touch.position;
							rightFingerPosition = touch.position;

							if (isInTopDownView)
							{
								StartShooting();
							}
						}
					}
				}

				if (leftFingerId != -1 && rightFingerId != -1)
				{
					break;
				}
			}
		}

		if (isShooting && rightFingerId == -1)
		{
			isShooting = false;
			anim.SetBool("Shoot_b", false);

			if (bulletsInClip != 0)
			{
				lastShotTime = 0; // allow spamming
			}
		}
	}

	private void UpdateControlUI()
	{
		moveJoystickOriginTransform.position = leftFingerId != -1 ? leftFingerOrigin : defaultMoveJoystickOriginPosition;
		moveJoystickTransform.anchoredPosition = Vector3.ClampMagnitude(leftFingerPosition - leftFingerOrigin, joystickRadius);
		moveJoystickTransform.gameObject.SetActive(leftFingerId != -1);

		var joystickKnobOffset = Vector3.ClampMagnitude(rightFingerPosition - rightFingerOrigin, joystickRadius);

		rotateJoystickOriginTransform.gameObject.SetActive(rightFingerId != -1 && !isInTopDownView && !isShooting);
		rotateJoystickOriginTransform.position = rightFingerId != -1 ? rightFingerOrigin : defaultRotateJoystickOriginPosition;
		rotateJoystickTransform.anchoredPosition = joystickKnobOffset;
		rotateJoystickTransform.gameObject.SetActive(rightFingerId != -1);

		shootJoystickOriginTransform.gameObject.SetActive(isInTopDownView);
		shootJoystickOriginTransform.position = rightFingerId != -1 ? rightFingerOrigin : defaultShootJoystickOriginPosition;
		shootJoystickTransform.anchoredPosition = joystickKnobOffset;
		shootJoystickTransform.gameObject.SetActive(rightFingerId != -1);

		shootButton.gameObject.SetActive(!isInTopDownView);
		shootButton.position = isShooting ? rightFingerOrigin + joystickKnobOffset : defaultShootButtonPosition;
	}

	private void UpdateStatusUI()
	{
		bulletText.SetText(bulletsInClip != 0 ? $"Bullets : {bulletsInClip}" : $"Reloading.. {Mathf.Max(0f, reloadTime - (Time.time - lastShotTime)):0.00}s");
		livesText.SetText($"Lives : {lives}");
	}

	private void HandleAnimation()
	{
		anim.SetFloat("Speed_f", body.velocity.magnitude);
	}

	//--------------------------------------------------------------------

	private void HandleMovement()
	{
		if (leftFingerId != -1)
		{
			var inputVector = leftFingerPosition - leftFingerOrigin;
			var inputVectorNormalized = inputVector.normalized;
			var inputVectorMagnitude = inputVector.magnitude;

			body.velocity = (projectedCameraForward * inputVectorNormalized.y + projectedCameraRight * inputVectorNormalized.x) * Mathf.Clamp01(inputVectorMagnitude / joystickRadius) * moveSpeed;
		}
		else
		{
			body.velocity = Vector3.zero;
		}
	}

	private void HandleRotation()
	{
		if (rightFingerId != -1)
		{
			var inputVector = rightFingerPosition - rightFingerOrigin;

			if (isInTopDownView)
			{
				var virtualTarget = projectedCameraForward * inputVector.y + projectedCameraRight * inputVector.x;

				transform.LookAt(transform.position + virtualTarget, transform.up);
			}
			else
			{
				transform.Rotate(transform.up, inputVector.x * turnSpeed * Time.fixedDeltaTime);

				var deltaAngle = -inputVector.y * cameraSpeed * Time.fixedDeltaTime;
				var nextAngle = shoulderCameraAttackAngle + deltaAngle;
				var clampedNextAngle = Mathf.Clamp(nextAngle, shoulderCameraMinAttackAngle, shoulderCameraMaxAttackAngle);

				cameraTransform.RotateAround(shoulderCameraPivotTransform.position, cameraTransform.right, deltaAngle + clampedNextAngle - nextAngle);
				shoulderCameraAttackAngle = clampedNextAngle;
			}
		}
		else if (isInTopDownView)
		{
			transform.LookAt(transform.position + body.velocity, transform.up);
		}
	}

	//--------------------------------------------------------------------

	private void UpdateCameraTransform()
	{
		cameraTransform = isInTopDownView ? topCamera.transform : shoulderCamera.transform;
	}

	private void UpdateCameraProjections()
	{
		projectedCameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
		projectedCameraRight = cameraTransform.right;
	}

	private void UpdateShoulderCameraAttackAngle()
	{
		var shoulderCameraTransform = shoulderCamera.transform;
		shoulderCameraAttackAngle = Vector3.Angle(Vector3.ProjectOnPlane(shoulderCameraTransform.forward, Vector3.up), shoulderCameraPivotTransform.position - shoulderCameraTransform.position);
	}

	//--------------------------------------------------------------------

	private void HandleReload()
	{
		if (bulletsInClip == 0)
		{
			if (Time.time - lastShotTime > reloadTime)
			{
				bulletsInClip = bulletPerClip;
				anim.SetBool("Reload_b", false);
			}

			UpdateStatusUI();
		}
	}

	private void HandleShootings()
	{
		var elapsedTimeSinceLastShot = Time.time - lastShotTime;

		if (bulletsInClip != 0)
		{
			if (elapsedTimeSinceLastShot > fireDelay)
			{
				var bullet = bulletPool[0];
				lastShotTime = elapsedTimeSinceLastShot > fireDelay * 2f ? Time.time : Time.time - elapsedTimeSinceLastShot % fireDelay;
				bulletsInClip = Mathf.Max(0, bulletsInClip - 1);

				if (bulletsInClip == 0)
				{
					anim.SetBool("Reload_b", true);
				}

				bullet.transform.position = bulletOriginTransform.position;

				if (isInTopDownView)
				{
					bullet.transform.LookAt(bulletOriginTransform.position + transform.forward + UnityEngine.Random.onUnitSphere * bulletSpread, Vector3.up);
				}
				else
				{
					RaycastHit hitInfo;
					Vector3 hitTargetDirection;
					if (Physics.Raycast(new Ray(cameraTransform.position, cameraTransform.forward), out hitInfo))
					{
						hitTargetDirection = Vector3.Normalize(hitInfo.point - bulletOriginTransform.position);
					}
					else
					{
						var hitTargetPosition = cameraTransform.position + cameraTransform.forward * 100f;
						hitTargetDirection = Vector3.Normalize(hitTargetPosition - bulletOriginTransform.position);
					}

					bullet.transform.LookAt(bulletOriginTransform.position + hitTargetDirection + UnityEngine.Random.onUnitSphere * bulletSpread, Vector3.up);
				}

				bullet.Activate(bulletPool, sparksPool);

				UpdateStatusUI();
			}
		}

	}

	//--------------------------------------------------------------------

	public void SwitchCamera()
	{
		isInTopDownView = !isInTopDownView;
		topCamera.gameObject.SetActive(isInTopDownView);
		shoulderCamera.gameObject.SetActive(!isInTopDownView);
		crosshairObject.SetActive(!isInTopDownView);

		UpdateCameraTransform();
		UpdateCameraProjections();
		UpdateShoulderCameraAttackAngle();
	}

	public void StartShooting()
	{
		isShooting = true;
		anim.SetBool("Shoot_b", true);
	}

	public void Damage()
	{
		lives = Mathf.Max(0, lives - 1);

		UpdateStatusUI();

		if (lives == 0)
		{
			anim.SetBool("Death_b", true);

			EndGame();

			endUIObject.SetActive(true);
			loseUIObject.SetActive(true);
		}
	}

	public void EndGame()
	{
		gameIsEnded = true;

		moveJoystickOriginTransform.gameObject.SetActive(false);
		rotateJoystickOriginTransform.gameObject.SetActive(false);
		shootJoystickOriginTransform.gameObject.SetActive(false);
		shootButton.gameObject.SetActive(false);

		bulletText.gameObject.SetActive(false);
		livesText.gameObject.SetActive(false);
	}
}
