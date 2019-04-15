using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

	public float moveSpeed;
	public float turnSpeed;

	public int swordCount;

	public float swordIdleSpeed;
	public float swordIdleSpin;

	public float shootRate;
	public float shootSpeed;

	public float swordCooldown;

	public GameObject swordPrefab;

	private Camera cam;
	private float swordRadius;

	private Rigidbody2D rb;
	private Transform swordParent;

	private GameObject[] swords;
	private Vector3[] swordOffsets;
	private Rigidbody2D[] swordrbs;
	private Collider2D[] swordColls;
	private SpriteRenderer[] swordsrs;

	private bool[] swordIdle;
	private bool[] swordCanShoot;

	private SwordController[] swordControllers;

	private float nextShootTime = 0f;
	private int swordShootIndex = 0;

	private List<int> swordIndexesTaken = new List<int>();

	void Start()
    {
		Initialize();

		SpawnSwords();
    }

	void Initialize()
	{
		rb = GetComponent<Rigidbody2D>();
		swordParent = GameObject.FindWithTag("SwordParent").transform;
		cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

		swords = new GameObject[swordCount];
		swordOffsets = new Vector3[swordCount];
		swordrbs = new Rigidbody2D[swordCount];
		swordColls = new Collider2D[swordCount];
		swordsrs = new SpriteRenderer[swordCount];

		swordIdle = new bool[swordCount];
		swordCanShoot = new bool[swordCount];
		swordControllers = new SwordController[swordCount];
	}

	void SpawnSwords()
	{
		swordRadius = 0.75f + 0.15f * swordCount;

		for (int i = 0; i < swordCount; i++)
		{
			float arcAngle = ((Mathf.PI * 1.5f) / (swordCount + 1)) * (i + 1);

			swordOffsets[i] = new Vector3(Mathf.Cos(arcAngle - Mathf.PI * 0.25f), -Mathf.Sin(arcAngle - Mathf.PI * 0.25f)) * swordRadius;
			Vector3 spawnPos = transform.position + swordOffsets[i];
			Quaternion spawnAngle = Quaternion.Euler(0f, 0f, rb.rotation);

			swords[i] = Instantiate(swordPrefab, spawnPos, spawnAngle, swordParent);
			swordrbs[i] = swords[i].GetComponent<Rigidbody2D>();
			swordColls[i] = swords[i].GetComponent<Collider2D>();
			swordsrs[i] = swords[i].GetComponent<SpriteRenderer>();

			swordControllers[i] = swords[i].GetComponent<SwordController>();

			int currentIndex = Random.Range(0, swordControllers[i].swordSprites.Length);
			int safeCount = 0;
			while (swordIndexesTaken.Contains(currentIndex) && safeCount < 100)
			{
				currentIndex = Random.Range(0, swordControllers[i].swordSprites.Length);
				safeCount++;
			}
			swordIndexesTaken.Add(currentIndex);

			swordControllers[i].InitializeIndex(currentIndex);

			swordControllers[i].spinSpeed = swordIdleSpin * 1.5f;
			swordIdle[i] = true;
			swordCanShoot[i] = true;
		}
	}

	private void Update()
	{
		if (rb.velocity.sqrMagnitude >= 0.1f)
		{
			rb.MoveRotation(Mathf.MoveTowardsAngle(rb.rotation, Vector2.SignedAngle(Vector2.up, rb.velocity), turnSpeed));
		}
		
		if (Input.GetMouseButton(0) && Time.time >= nextShootTime)
		{
			StartCoroutine(ShootSword());
		}
	}

	IEnumerator ShootSword()
	{
		nextShootTime = Time.time + shootRate;

		if (swordIdle[swordShootIndex] && swordCanShoot[swordShootIndex])
		{
			int currentSword = swordShootIndex;

			swordShootIndex++;
			if (swordShootIndex == swordCount)
			{
				swordShootIndex = 0;
			}
			
			//Shoot this sword
			swordIdle[currentSword] = false;
			swordCanShoot[currentSword] = false;
			swordControllers[currentSword].AttackEffect(cam.ScreenToWorldPoint(Input.mousePosition));

			yield return new WaitForSeconds(0.4f);

			float savedDrag = swordrbs[currentSword].drag;
			swordrbs[currentSword].drag = 0f;
			swordrbs[currentSword].AddForce(swords[currentSword].transform.up * shootSpeed * 5f, ForceMode2D.Impulse);

			swordColls[currentSword].enabled = true;
			swordsrs[currentSword].color = new Color(1f, 1f, 1f, 1f);

			yield return new WaitForSeconds(0.3f);

			swordrbs[currentSword].drag = 7f;

			yield return new WaitForSeconds(0.3f);

			swordrbs[currentSword].drag = savedDrag;
			swordIdle[currentSword] = true;
			swordColls[currentSword].enabled = false;
			swordsrs[currentSword].color = new Color(1f, 1f, 1f, 0.6f);

			yield return new WaitForSeconds(swordCooldown);

			swordCanShoot[currentSword] = true;			
		}
	}

	void FixedUpdate()
	{
		BasicMovement();
		CorrectSwords();
	}

	void BasicMovement()
	{
		Vector2 moveVector = Vector2.zero;

		if (Input.GetKey(KeyCode.W))
		{
			moveVector += Vector2.up;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			moveVector += Vector2.down;
		}

		if (Input.GetKey(KeyCode.A))
		{
			moveVector += Vector2.left;
		}
		else if (Input.GetKey(KeyCode.D))
		{
			moveVector += Vector2.right;
		}

		rb.AddForce(moveVector.normalized * moveSpeed);
	}

	void CorrectSwords()
	{
		Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
		float posAngle = Vector2.SignedAngle(Vector2.up, mousePos - new Vector2 (transform.position.x, transform.position.y));

		for (int i = 0; i < swordCount; i++)
		{

			if (swordIdle[i])
			{
				Vector3 goalPos = transform.position + Polar.RotateByAngle(swordOffsets[i], posAngle);

				Vector2 moveVector = Vector2.zero;

				if (swords[i].transform.position.x > goalPos.x)
				{
					moveVector += Vector2.left;
				}
				if (swords[i].transform.position.x < goalPos.x)
				{
					moveVector += Vector2.right;
				}

				if (swords[i].transform.position.y > goalPos.y)
				{
					moveVector += Vector2.down;
				}
				if (swords[i].transform.position.y < goalPos.y)
				{
					moveVector += Vector2.up;
				}

				float dist = Mathf.Clamp((swords[i].transform.position - goalPos).magnitude, 0f, 5f);
				if (dist >= 0.1f)
				{
					swordrbs[i].AddForce(moveVector * swordIdleSpeed * dist);
				}

				float rotationAngle = Mathf.Atan2(mousePos.y - swords[i].transform.position.y, mousePos.x - swords[i].transform.position.x) * Mathf.Rad2Deg - 90f;
				swordrbs[i].rotation = Mathf.LerpAngle(swordrbs[i].rotation, rotationAngle, swordIdleSpin);
			}
		}
	}
}
