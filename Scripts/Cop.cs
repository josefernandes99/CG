using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class Cop : MonoBehaviour
{
    private Rigidbody copRigidbody; // Rigidbody component of the cop
    private bool isRightArmExtended = false;
    private bool isFirstPerson = false;
    private Camera ThirdPersonCamera;
    private Camera FirstPersonCamera;
    private sbyte handMenu = 0;
    private bool isPistol2Active = false;
    private bool isAssault1Active = false;
    private float raycastDistance = 1f; // Distance of the raycast

    private bool isFirstCollision = true;

    // Start is called before the first frame update
    void Start()
    {
        // Add BoxCollider to the Cop object
        BoxCollider copCollider = gameObject.AddComponent<BoxCollider>();
        copCollider.isTrigger = false; // Set to true if you want it to be a trigger collider
        copCollider.center = new Vector3(0f, 0.3f, 0f); // Set the collider center position

        // Add Rigidbody component to the cop prefab for movement
        copRigidbody = gameObject.AddComponent<Rigidbody>();
        copRigidbody.useGravity = true;

        copRigidbody.constraints =
            RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Create the third-person camera
        ThirdPersonCamera = new GameObject("ThirdPersonCamera").AddComponent<Camera>();
        ThirdPersonCamera.transform.SetParent(this.transform); // Set the camera's parent to the cop prefab instance
        ThirdPersonCamera.transform.localPosition = new Vector3(0f, 20f, -7f); // Set the camera position above the cop (2x higher)
        ThirdPersonCamera.transform.localRotation = Quaternion.Euler(60f, 0f, 0f); // Set the camera rotation to be top-down
        ThirdPersonCamera.gameObject.SetActive(true);

        // Create an empty GameObject as the first-person camera holder
        GameObject FirstPersonCameraHolder = new GameObject("FirstPersonCameraHolder");
        FirstPersonCameraHolder.transform.SetParent(this.transform);
        FirstPersonCameraHolder.transform.localPosition = new Vector3(0f, 1.3f, 0f); // Set the camera holder position slightly higher

        // Create the first-person camera as a child of the camera holder
        FirstPersonCamera = FirstPersonCameraHolder.AddComponent<Camera>();
        FirstPersonCamera.transform.localPosition = Vector3.zero; // Set the camera position at the holder's origin
        FirstPersonCamera.transform.localRotation = Quaternion.identity; // Set the camera rotation to match the cop's rotation
        FirstPersonCamera.gameObject.SetActive(false);
        FirstPersonCamera.transform.position =
            this.transform.Find("head").position + new Vector3(0f, 0.95f, 0f);
        FirstPersonCamera.fieldOfView = 70f;

        GameObject quadObject = new GameObject("copQuad");
        quadObject.transform.SetParent(this.transform);
        quadObject.transform.localPosition = new Vector3(0f, 100f, 0f);
        quadObject.transform.localScale = new Vector3(7f, 7f, 7f);

        MeshFilter meshFilter = quadObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = quadObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = this.transform.Find("head").GetComponent<MeshFilter>().mesh;

        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = Color.blue;
    }

    // Update is called once per frame
    void Update()
    {
        // Get the input for movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate the movement vector
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);
        movement *= Time.deltaTime * 5f; // Adjust the movement speed as needed

        // Apply the movement to the cop's position
        this.transform.Translate(movement);

        float mouseX = Input.GetAxis("Mouse X");
        this.transform.Rotate(Vector3.up, mouseX * 5f);

        float armRotation = 0f;

        if (movement.magnitude > 0f)
        {
            armRotation = Mathf.Sin(Time.time * 10f) * 15f;
        }

        Transform leftArm = this.transform.Find("leftarm");
        leftArm.localRotation = Quaternion.Euler(armRotation, 0f, 90f);

        Transform rightArm = this.transform.Find("rightarm");

        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;

            if (isFirstPerson)
            {
                rightArm.transform.localPosition = new Vector3(0f, 0.5f, -0.3f);
                this.transform.Find("head").GetComponent<MeshRenderer>().enabled = false;
                this.transform.Find("body").GetComponent<MeshRenderer>().enabled = false;
                this.transform.Find("hatbody").GetComponent<MeshRenderer>().enabled = false;
            }
            else if (!isFirstPerson)
            {
                this.transform.Find("head").GetComponent<MeshRenderer>().enabled = true;
                this.transform.Find("body").GetComponent<MeshRenderer>().enabled = true;
                this.transform.Find("hatbody").GetComponent<MeshRenderer>().enabled = true;
                if (isRightArmExtended)
                {
                    rightArm.transform.localPosition = new Vector3(-0.1f, 0.5f, -0.5f);
                }
                else
                {
                    rightArm.transform.localPosition = new Vector3(-0.1f, 0.5f, -0.5f);
                }
            }

            ThirdPersonCamera.gameObject.SetActive(!isFirstPerson);
            FirstPersonCamera.gameObject.SetActive(isFirstPerson);
        }

        if (!isRightArmExtended)
        {
            rightArm.localRotation = Quaternion.Euler(-armRotation, 0f, -90f);
        }

        if (
            ((Input.GetKeyDown(KeyCode.Alpha1) || handMenu == 1) && isRightArmExtended == false)
            || ((Input.GetKeyDown(KeyCode.Alpha1) || handMenu == 1) && isAssault1Active == true)
        )
        {
            handMenu = 1;
            resetGuns(rightArm);
            isRightArmExtended = !isRightArmExtended;
            isPistol2Active = true;
            isAssault1Active = false;

            rightArm.localRotation = Quaternion.Euler(-90f, 0f, -90f);

            if (isFirstPerson)
            {
                rightArm.transform.localPosition = new Vector3(0f, 0.5f, -0.3f);
            }
            else if (!isFirstPerson)
            {
                rightArm.transform.localPosition = new Vector3(-0.1f, 0.5f, -0.5f);
            }

            Transform pistol2Transform = rightArm.Find("Pistol2");
            Transform assault1Transform = rightArm.Find("Assault1");
            Renderer pistol2Renderer = pistol2Transform.GetComponent<Renderer>();
            pistol2Renderer.enabled = true;
            Renderer assault1Renderer = assault1Transform.GetComponent<Renderer>();
            assault1Renderer.enabled = false;
        }
        else if (
            ((Input.GetKeyDown(KeyCode.Alpha2) || handMenu == 2) && isRightArmExtended == false)
            || ((Input.GetKeyDown(KeyCode.Alpha2) || handMenu == 2) && isPistol2Active == true)
        )
        {
            handMenu = 2;
            resetGuns(rightArm);
            isRightArmExtended = !isRightArmExtended;
            isPistol2Active = false;
            isAssault1Active = true;

            rightArm.localRotation = Quaternion.Euler(-90f, 0f, -90f);

            if (isFirstPerson)
            {
                rightArm.transform.localPosition = new Vector3(0f, 0.5f, -0.3f);
            }
            else if (!isFirstPerson)
            {
                rightArm.transform.localPosition = new Vector3(-0.1f, 0.5f, -0.5f);
            }

            Transform pistol2Transform = rightArm.Find("Pistol2");
            Transform assault1Transform = rightArm.Find("Assault1");
            Renderer pistol2Renderer = pistol2Transform.GetComponent<Renderer>();
            pistol2Renderer.enabled = false;
            Renderer assault1Renderer = assault1Transform.GetComponent<Renderer>();
            assault1Renderer.enabled = true;
        }
        else if (
            ((Input.GetKeyDown(KeyCode.Alpha1) || handMenu == 0) && isPistol2Active == true)
            || ((Input.GetKeyDown(KeyCode.Alpha2) || handMenu == 0) && isAssault1Active == true)
        )
        {
            handMenu = 0;
            resetGuns(rightArm);
        }

        if (Input.mouseScrollDelta.y > 0)
        {
            handMenu++;
            if (handMenu > 3)
            {
                handMenu = 0;
            }
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            handMenu--;
            if (handMenu < 0)
            {
                handMenu = 3;
            }
        }

        // Perform a raycast in front of the cop to check for collisions
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            // If the raycast hits an object, move the cop away from the collision point
            Vector3 direction = (transform.position - hit.point).normalized;
            copRigidbody.MovePosition(hit.point + direction * raycastDistance);
        }
    }

    void resetGuns(Transform rightArm)
    {
        rightArm.localRotation = Quaternion.Euler(0f, 0f, -90f);
        rightArm.transform.localPosition = new Vector3(-0.15f, 1.2f, 0f);

        if (isPistol2Active == true)
        {
            Transform pistol2Transform = rightArm.Find("Pistol2");
            if (pistol2Transform != null)
            {
                Renderer pistol2Renderer = pistol2Transform.GetComponent<Renderer>();
                pistol2Renderer.enabled = false;
            }
        }
        else if (isAssault1Active == true)
        {
            Transform assault1Transform = rightArm.Find("Assault1");
            if (assault1Transform != null)
            {
                Renderer assault1Renderer = assault1Transform.GetComponent<Renderer>();
                assault1Renderer.enabled = false;
            }
        }

        isPistol2Active = false;
        isAssault1Active = false;
        isRightArmExtended = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision = " + collision.gameObject.name);
        if (
            isFirstCollision
            && collision.gameObject.name != "Ground"
            && collision.gameObject.name.Split(" ")[1] == "robber"
        )
        {
            this.tag = "Untagged";
        }
    }
}
