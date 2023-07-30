using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using TMPro;

public class GameStarter : MonoBehaviour
{
    public GameObject copPrefab; // Reference to the cop prefab asset
    public GameObject robberPrefab; // Reference to the robber prefab asset
    private Collider cubeCollider; // Collider component of the cube
    public float intensityMultiplier = 3f;

    public GameObject pistol2Prefab;
    public GameObject assault1Prefab;

    public GameObject BGround;
    public GameObject BLevel;
    public GameObject BRoof;

    public GameObject Garage;
    public GameObject GasStation;

    public GameObject waypointPrefab; // Prefab for the invisible sphere waypoint
    public float spacing = 2f; // Spacing between waypoints

    public int level = 1;

    public GameObject pauseMenu;
    public Button continueGameButton;
    private bool isPaused = false;

    public GameObject deathMenu;

    void Start()
    {
        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Physics.gravity *= 2;

        var wayMap = new GameObject("WayMap");

        int counter = 0;

        for (int i = 0; i < 41; i++)
        {
            for (int j = 0; j < 41; j++)
            {
                Vector3 position;
                GameObject waypoint;

                position =
                    transform.position
                    + i * spacing * transform.forward
                    + new Vector3(j * 2 - 40, 0.2f, -46f);

                waypoint = Instantiate(waypointPrefab, position, Quaternion.identity);
                waypoint.name = "waypoint " + counter;

                counter++;

                waypoint.transform.localScale = new Vector3(1f, 1f, 1f);
                waypoint.transform.parent = wayMap.transform;

                waypoint.tag = "Waypoint";

                Renderer wayRenderer = waypoint.GetComponent<Renderer>();
                wayRenderer.enabled = true;

                BoxCollider wayCollider = waypoint.AddComponent<BoxCollider>();
                wayCollider.isTrigger = true;
            }
        }

        // Instantiate the cop prefab
        copPrefab = Instantiate(copPrefab);
        copPrefab.name = "cop";
        copPrefab.tag = "Cop";
        Cop CopScript = copPrefab.AddComponent<Cop>();

        Transform leftarm = copPrefab.transform.Find("leftarm");
        Transform rightarm = copPrefab.transform.Find("rightarm");
        leftarm.transform.localPosition = new Vector3(0.1f, 1.2f, 0f);
        rightarm.transform.localPosition = new Vector3(-0.1f, 1.2f, 0f);

        GameObject pistol2Object = GameObject.Instantiate(pistol2Prefab, rightarm);
        pistol2Object.name = "Pistol2";
        pistol2Object.transform.SetParent(rightarm);
        Pistol2 Pistol2Script = pistol2Object.AddComponent<Pistol2>();
        Renderer pistol2Renderer = pistol2Object.GetComponent<Renderer>();
        pistol2Renderer.enabled = false;

        GameObject assault1Object = GameObject.Instantiate(assault1Prefab, rightarm);
        assault1Object.name = "Assault1";
        assault1Object.transform.SetParent(rightarm);
        Assault1 Assault1Script = assault1Object.AddComponent<Assault1>();
        Renderer assault1Renderer = assault1Object.GetComponent<Renderer>();
        assault1Renderer.enabled = false;

        // Create the sunlight (directional light)
        GameObject sunlight = new GameObject("Sunlight");
        Light sunlightComponent = sunlight.AddComponent<Light>();
        sunlightComponent.type = LightType.Directional;
        sunlight.transform.rotation = Quaternion.Euler(new Vector3(45f, 0f, 0f));
        sunlight.transform.position = new Vector3(0f, 100f, -100f);
        sunlightComponent.shadows = LightShadows.Soft;
        sunlightComponent.shadowStrength = 0.5f;

        // Get the current Lighting Settings
        var lightingSettings = new SphericalHarmonicsL2();

        if (RenderSettings.ambientMode == AmbientMode.Skybox)
        {
            lightingSettings = RenderSettings.ambientProbe;
        }
        else
        {
            Color ambientColor = RenderSettings.ambientLight;
            lightingSettings.AddAmbientLight(ambientColor * intensityMultiplier);
        }

        // Modify the intensity of each coefficient
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                lightingSettings[i, j] *= intensityMultiplier;
            }
        }

        // Apply the modified Lighting Settings
        if (RenderSettings.ambientMode == AmbientMode.Skybox)
        {
            RenderSettings.ambientProbe = lightingSettings;
        }
        else
        {
            RenderSettings.ambientLight = new Color(
                lightingSettings[0, 0],
                lightingSettings[0, 0],
                lightingSettings[0, 0]
            );
        }

        // Create the ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10f, 1f, 10f);

        // Set the material for the ground to cement-grey color
        Renderer groundRenderer = ground.GetComponent<Renderer>();
        Material groundMaterial = new Material(Shader.Find("Standard"));
        groundMaterial.color = new Color(0.5f, 0.5f, 0.5f); // Cement-grey color
        groundRenderer.material = groundMaterial;

        // Position the cop and robber on top of the ground
        copPrefab.transform.position = new Vector3(0f, 0f, 0f);
        robberPrefab.transform.position = new Vector3(2f, 0f, 0f);

        // Hide the cube GameObject
        GameObject cube = GameObject.Find("Cube");
        if (cube != null)
        {
            Renderer cubeRenderer = cube.GetComponent<Renderer>();
            cubeRenderer.enabled = false;
            cubeCollider = cube.GetComponent<Collider>();
            cubeCollider.enabled = false;
        }

        //build border
        for (int i = 0; i < 4; i++)
        {
            //4 sides
            GameObject BuildingComplex = new GameObject("BuildingComplex");
            BuildingComplex.transform.SetParent(ground.transform);
            BoxCollider buildingCollider = BuildingComplex.AddComponent<BoxCollider>();
            buildingCollider.size = new Vector3(300f, 100f, 2f);

            if (i == 0)
            {
                BuildingComplex.transform.localPosition = new Vector3(-1.166f, 3.5f, -4.735f);
                BuildingComplex.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                BuildingComplex.transform.localScale = new Vector3(0.04f, 0.3f, 0.04f);
            }
            else if (i == 1)
            {
                BuildingComplex.transform.localPosition = new Vector3(-4.098f, 3.5f, 0.66f);
                BuildingComplex.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                BuildingComplex.transform.localScale = new Vector3(0.04f, 0.3f, 0.04f);
            }
            else if (i == 2)
            {
                BuildingComplex.transform.localPosition = new Vector3(1.507f, 3.5f, 3.609f);
                BuildingComplex.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                BuildingComplex.transform.localScale = new Vector3(0.04f, 0.3f, 0.04f);
            }
            else if (i == 3)
            {
                BuildingComplex.transform.localPosition = new Vector3(4.062f, 3.5f, -1.789f);
                BuildingComplex.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                BuildingComplex.transform.localScale = new Vector3(0.04f, 0.3f, 0.04f);
            }

            for (int j = 0; j < 16; j++)
            {
                //16 apartments each side
                GameObject Building = new GameObject("Building");
                Building.transform.SetParent(BuildingComplex.transform);
                Building.transform.localPosition = new Vector3(60 - 23 * j, 0f, 0f);
                Building.transform.localRotation = Quaternion.Euler(new Vector3(0f, -90f, 0f));

                GameObject BGroundObject = GameObject.Instantiate(BGround, Building.transform);
                BGroundObject.name = "BGround";
                BGroundObject.transform.localPosition = new Vector3(6.5f, -3.6f, -3f);
                BGroundObject.transform.localScale = new Vector3(1.25f, 2f, 1.25f);

                GameObject BLevel1Object = GameObject.Instantiate(BLevel, Building.transform);
                BLevel1Object.name = "BLevel1";
                BLevel1Object.transform.localPosition = new Vector3(6.5f, 4.4f, -3f);
                BLevel1Object.transform.localScale = new Vector3(1.25f, 2f, 1.25f);

                GameObject BLevel2Object = GameObject.Instantiate(BLevel, Building.transform);
                BLevel2Object.name = "BLevel2";
                BLevel2Object.transform.localPosition = new Vector3(6.5f, 12f, -3f);
                BLevel2Object.transform.localScale = new Vector3(1.25f, 2f, 1.25f);

                GameObject BRoofObject = GameObject.Instantiate(BRoof, Building.transform);
                BRoofObject.name = "BRoof";
                BRoofObject.transform.localPosition = new Vector3(6.5f, 20f, -3f);
                BRoofObject.transform.localScale = new Vector3(1.25f, 2f, 1.25f);
            }
        }

        GameObject GarageObject = GameObject.Instantiate(Garage, ground.transform);
        GarageObject.name = "garage";
        GarageObject.transform.localPosition = new Vector3(-0.24f, 0f, -5.343f);
        GarageObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
        GarageObject.transform.localScale = new Vector3(30f, 300f, 30f);

        GameObject GasStationObject = GameObject.Instantiate(GasStation, ground.transform);
        GasStationObject.name = "gas station";
        GasStationObject.transform.localPosition = new Vector3(-3.952f, 0f, 3.945f);
        GasStationObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, -90f, 0f));
        GasStationObject.transform.localScale = new Vector3(15f, 150f, 15f);

        pauseMenu.SetActive(false);
        continueGameButton.onClick.AddListener(Resume);

        deathMenu.SetActive(false);
    }

    void Update()
    {
        if (GameObject.FindGameObjectsWithTag("Robber").Length == 0)
        {
            GameObject.Find("Ground").tag = "Robber";
            Invoke("startNewLevel", 5.0f);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Pause();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        if (GameObject.FindGameObjectsWithTag("Cop").Length == 0)
        {
            GameObject.Find("Sunlight").tag = "Cop";
            deathMenu.SetActive(true);
            Time.timeScale = 0f; // Pause game time
            isPaused = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            GameObject levelCounterObject = GameObject.FindGameObjectWithTag("LevelCounter");
            TextMeshProUGUI levelCounterText = levelCounterObject.GetComponent<TextMeshProUGUI>();
            GameObject levelObject = GameObject.FindGameObjectWithTag("Level");
            TextMeshProUGUI levelText = levelObject.GetComponent<TextMeshProUGUI>();
            levelText.text = levelCounterText.text;
            Debug.Log("levelCounterText = " + levelCounterText.text);
            Debug.Log("levelText = " + levelText.text);
            GameObject deathCounterObject = GameObject.FindGameObjectWithTag("DeathCounter");
            TextMeshProUGUI deathCounterText = deathCounterObject.GetComponent<TextMeshProUGUI>();
            GameObject deathObject = GameObject.FindGameObjectWithTag("Death");
            TextMeshProUGUI deathText = deathObject.GetComponent<TextMeshProUGUI>();
            deathText.text = deathCounterText.text;
            Debug.Log("deathCounterText = " + deathCounterText.text);
            Debug.Log("deathText = " + deathText.text);
        }
    }

    void startNewLevel()
    {
        for (int i = 0; i < level * 3; i++)
        {
            GameObject robberPrefab1 = Instantiate(robberPrefab);
            robberPrefab1.name = "robber " + i;
            robberPrefab1.tag = "Robber";
            Robber RobberScript = robberPrefab1.AddComponent<Robber>();

            Transform Rleftarm = robberPrefab1.transform.Find("leftarm");
            Transform Rrightarm = robberPrefab1.transform.Find("rightarm");
            Rleftarm.transform.localPosition = new Vector3(0.1f, 1.2f, 0f);
            Rrightarm.transform.localPosition = new Vector3(-0.1f, 1.2f, 0f);

            GameObject pistol2RObject = GameObject.Instantiate(pistol2Prefab, Rrightarm);
            pistol2RObject.name = "Pistol2";
            pistol2RObject.transform.SetParent(Rrightarm);
            Renderer pistol2RRenderer = pistol2RObject.GetComponent<Renderer>();
            pistol2RRenderer.enabled = true;

            GameObject assault1RObject = GameObject.Instantiate(assault1Prefab, Rrightarm);
            assault1RObject.name = "Assault1";
            assault1RObject.transform.SetParent(Rrightarm);
            Renderer assault1RRenderer = assault1RObject.GetComponent<Renderer>();
            assault1RRenderer.enabled = false;
        }

        // Find the TextMeshPro component with the tag "DeathCounter"
        GameObject levelCounterObject = GameObject.FindGameObjectWithTag("LevelCounter");
        TextMeshProUGUI levelCounterText = levelCounterObject.GetComponent<TextMeshProUGUI>();
        levelCounterText.text = "Level: " + (level).ToString();
        GameObject.Find("Ground").tag = "Untagged";
        level++;
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f; // Resume game time
        isPaused = false;
    }

    private void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f; // Pause game time
        isPaused = true;
    }
}
