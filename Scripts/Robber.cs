using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;

public class Robber : MonoBehaviour
{
    Transform leftArm;
    Transform rightArm;
    Transform pistol2RTransform;
    private bool isScaling = false;
    private Vector3 targetScale = Vector3.zero;
    private float scaleDuration = 2f;
    private float scaleSpeed;
    private Rigidbody robberRigidbody; // Rigidbody component of the robber
    GameObject copPrefab;
    Vector3 currentPosition = new Vector3(0f, 0f, 0f);
    Vector3 previousPosition = new Vector3(0f, 0f, 0f);
    private bool isMovement = false;
    public float rotationSpeed = 20f;

    private GameObject[] bullets;
    private GameObject[] waypoints;

    Transform targetWaypoint = null;

    private float fireRate = 0.50f; // Adjust the fire rate as desired (e.g., time between each bullet)
    private float timeSinceLastShot = 0.50f;

    private bool isFirstCollision = true;

    private void Start()
    {
        leftArm = this.transform.Find("leftarm");
        rightArm = this.transform.Find("rightarm");
        pistol2RTransform = rightArm.Find("Pistol2");
        Renderer pistol2Renderer = pistol2RTransform.GetComponent<Renderer>();

        pistol2RTransform.transform.localPosition = new Vector3(1.5f, 0.58f, 0f);
        pistol2RTransform.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
        pistol2RTransform.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Add Rigidbody component to the robber prefab for movement
        robberRigidbody = gameObject.AddComponent<Rigidbody>();
        robberRigidbody.useGravity = true;
        robberRigidbody.isKinematic = true;

        // Add BoxCollider to the Robber object
        BoxCollider robberCollider = gameObject.AddComponent<BoxCollider>();
        robberCollider.isTrigger = false; // Set to true if you want it to be a trigger collider
        robberCollider.center = new Vector3(0f, 0.3f, 0f); // Set the collider center position

        scaleSpeed = 1f / scaleDuration;

        copPrefab = GameObject.Find("cop");

        GameObject waypoint0 = GameObject.Find("waypoint 0");
        Vector3 waypointPosition = waypoint0.transform.position;
        transform.transform.position = waypointPosition;

        leftArm.localRotation = Quaternion.Euler(0f, 0f, 90f);

        rightArm.transform.localPosition = new Vector3(-0.1f, 0.65f, -0.5f);
        rightArm.localRotation = Quaternion.Euler(-90f, 0f, -90f);

        GameObject quadObject = new GameObject("copQuad");
        quadObject.transform.SetParent(this.transform);
        quadObject.transform.localPosition = new Vector3(0f, 100f, 0f);
        quadObject.transform.localScale = new Vector3(7f, 7f, 7f);

        MeshFilter meshFilter = quadObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = quadObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = this.transform.Find("head").GetComponent<MeshFilter>().mesh;

        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = Color.red;
    }

    private void Update()
    {
        if (isScaling)
        {
            // Scale the robber
            targetWaypoint = null;
            ScaleRobber();
        }

        // Get the current position of the copPrefab
        currentPosition = copPrefab.transform.position;

        // Check if the copPrefab's position has changed since the last frame
        if (currentPosition != previousPosition)
        {
            robberRigidbody.constraints = RigidbodyConstraints.None;
            isMovement = true;
            timeSinceLastShot += Time.deltaTime;
        }
        else
        {
            robberRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            isMovement = false;
        }

        if (isMovement && isScaling == false)
        {
            // Calculate distance between cop and robber
            float distanceToCop = Vector3.Distance(
                transform.position,
                copPrefab.transform.position
            );

            if (distanceToCop > 20)
            {
                robberRigidbody.isKinematic = false;
                // Move between random empty waypoints
                if (targetWaypoint == null)
                {
                    FindRandomEmptyWaypoint();
                }
                else
                {
                    MoveToWaypoint();
                }
            }
            else
            {
                targetWaypoint = null;
                FindClosestBullet();
                AttackMovement();
                // Look at the cop and stop random movement
                LookAtCop();
                if (timeSinceLastShot >= fireRate * 2)
                {
                    shoot();
                }
                robberRigidbody.isKinematic = true;
            }
        }

        previousPosition = currentPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFirstCollision && collision.gameObject.name == "Bullet Cop")
        {
            isScaling = true;
            isFirstCollision = false;
            // Find the TextMeshPro component with the tag "DeathCounter"
            GameObject deathCounterObject = GameObject.FindGameObjectWithTag("DeathCounter");
            TextMeshProUGUI deathCounterText = deathCounterObject.GetComponent<TextMeshProUGUI>();
            int oldNumber = Int32.Parse(deathCounterText.text.Split(' ')[1]);
            deathCounterText.text = "Deaths: " + (oldNumber + 1).ToString();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        string[] collisionString = collision.gameObject.name.Split(' ');

        if (collisionString[0] == "waypoint" && isScaling == false)
        {
            collision.gameObject.name = collisionString[0] + " " + collisionString[1] + " FULL";
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        string[] collisionString = collision.gameObject.name.Split(' ');

        if (
            collisionString.Length == 3
            && collisionString[0] == "waypoint"
            && collisionString[2] == "FULL"
        )
        {
            collision.gameObject.name = collisionString[0] + " " + collisionString[1];
        }
    }

    private void ScaleRobber()
    {
        if (isMovement == true)
        {
            // Decrease the scale of the robber over time
            this.transform.localScale -= scaleSpeed * Time.deltaTime * Vector3.one;
        }

        // If the robber has reached the target scale, destroy it
        if (
            this.transform.localScale.x <= 0f
            && this.transform.localScale.y <= 0f
            && this.transform.localScale.z <= 0f
        )
        {
            Destroy(gameObject);
        }
    }

    private int GetWaypointID(GameObject waypoint)
    {
        string[] nameParts = waypoint.name.Split(' ');
        int waypointID = int.Parse(nameParts[1]);
        return waypointID;
    }

    private void FindClosestBullet()
    {
        bullets = GameObject.FindGameObjectsWithTag("Bullet");
        float minDistance = Mathf.Infinity;
        GameObject closestBullet = null;

        foreach (GameObject bullet in bullets)
        {
            float distance = Vector3.Distance(transform.position, bullet.transform.position);
            if (distance < minDistance)
            {
                closestBullet = bullet;
                minDistance = distance;
            }
        }

        if (closestBullet != null)
        {
            DecideRobberWaypoint(closestBullet.transform.position);
        }
    }

    private void DecideRobberWaypoint(Vector3 bulletPosition)
    {
        waypoints = GameObject.FindGameObjectsWithTag("Waypoint");

        targetWaypoint = null;
        float maxDistance = 0;

        foreach (GameObject waypoint in waypoints)
        {
            float distanceToBullet = Vector3.Distance(waypoint.transform.position, bulletPosition);
            float distanceToCop = Vector3.Distance(
                waypoint.transform.position,
                copPrefab.transform.position
            );
            float combinedDistance = distanceToBullet - distanceToCop - 10;

            if (combinedDistance > maxDistance)
            {
                targetWaypoint = waypoint.transform;
                maxDistance = combinedDistance;
            }
        }
    }

    private void AttackMovement()
    {
        if (targetWaypoint != null)
        {
            Vector3 direction = targetWaypoint.position - transform.position;

            float distance = direction.magnitude;

            if (distance != 0)
            {
                direction.Normalize();
                Vector3 movement = direction * 5f * Time.deltaTime;
                transform.position += movement;
                float armRotation = 0f;
                if (movement.magnitude > 0f)
                {
                    armRotation = Mathf.Sin(Time.time * 10f) * 15f;
                }
                Transform leftArm = this.transform.Find("leftarm");
                leftArm.localRotation = Quaternion.Euler(armRotation, 0f, 90f);
            }
            else
            {
                targetWaypoint = null;
            }
        }
    }

    private void FindRandomEmptyWaypoint()
    {
        waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
        List<Transform> emptyWaypoints = new List<Transform>();

        foreach (GameObject waypoint in waypoints)
        {
            string waypointName = waypoint.name;
            if (!waypointName.EndsWith("FULL"))
            {
                emptyWaypoints.Add(waypoint.transform);
            }
        }

        if (emptyWaypoints.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, emptyWaypoints.Count);
            targetWaypoint = emptyWaypoints[randomIndex];
        }
    }

    private void MoveToWaypoint()
    {
        Vector3 direction = targetWaypoint.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance > 0.05)
        {
            direction.Normalize();
            Vector3 movement = direction * 2f * Time.deltaTime;
            transform.position += movement;
            float armRotation = 0f;
            if (movement.magnitude > 0f)
            {
                armRotation = Mathf.Sin(Time.time * 5f) * 15f;
            }
            Transform leftArm = this.transform.Find("leftarm");
            leftArm.localRotation = Quaternion.Euler(armRotation, 0f, 90f);

            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else
        {
            transform.position = targetWaypoint.position;
            Debug.Log("Reached waypoint");
            targetWaypoint = null;
        }
    }

    private void LookAtCop()
    {
        Vector3 direction = copPrefab.transform.position - transform.position;
        direction.y = 0f; // Ignore the vertical component for 2D movement
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private void shoot()
    {
        if (timeSinceLastShot >= fireRate)
        {
            // Create a bullet object
            GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            bullet.name = "bullet " + this.name;
            // Attach the Bullet script to the bullet GameObject
            Bullet bulletScript = bullet.AddComponent<Bullet>();

            timeSinceLastShot = 0f;
        }
    }
}
