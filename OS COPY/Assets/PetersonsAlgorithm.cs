using UnityEngine;
using UnityEngine.UI;

public class PetersonsAlgorithm : MonoBehaviour
{
    // UI Elements for displaying process status
    public Text processAStatus;
    public Text processBStatus;
    public Text criticalSectionStatus;
    // Extend UI Elements
    public Text processCStatus;
    public Text processDStatus;
    //public Text waitingStatus;
    public Image criticalSectionImage;

    // Buttons for interactivity
    public Button processAButton;
    public Button processBButton;
    public Button pauseResumeButton;
    public Button processCButton;
    public Button exitCButton;

    public Button processDButton;
    public Button exitDButton;
    public Button resetButton;
    public Button exitButton;
    public Button exitAButton;
    public Button exitBButton;
    public Button automaticSimulationButton;

    // Sounds
    public AudioSource waitingSound;
    public AudioSource successSound;

    // Visual indicators for processes
    public GameObject processAIcon;
    public GameObject processBIcon;
    // Visual indicators for new processes
    public GameObject processCIcon;
    public GameObject processDIcon;
    public Transform criticalSectionPosition;
    public Transform idlePositionA;
    public Transform idlePositionB;
    public Transform idlePositionC;
    public Transform idlePositionD;
    public Transform waitingPosition;

    // Variables for Peterson's Algorithm
    private bool[] flag = new bool[4];
    private int turn;
    private bool isPaused = false;
    private bool isAutomatic = false;
    private int criticalSectionProcess = -1;
    private int waitingProcess = -1;

    // Timing variables for automatic simulation
    private float autoTimer = 0f;
    private float switchInterval = 4f;

    void Start()
    {
        // Initialize flags and turn
        flag[0] = false;
        flag[1] = false;
        turn = 0;

        // Button listeners
        pauseResumeButton.onClick.AddListener(TogglePause);
        resetButton.onClick.AddListener(ResetSimulation);
        processAButton.onClick.AddListener(() => RequestCriticalSection(0));
        processBButton.onClick.AddListener(() => RequestCriticalSection(1));

        exitButton.onClick.AddListener(ExitCriticalSection);
        exitAButton.onClick.AddListener(ExitCriticalSectionA);
        exitBButton.onClick.AddListener(ExitCriticalSectionB);
        automaticSimulationButton.onClick.AddListener(ToggleAutomaticSimulation);
        processCButton.onClick.AddListener(() => RequestCriticalSection(2));
        processDButton.onClick.AddListener(() => RequestCriticalSection(3));
        exitCButton.onClick.AddListener(ExitCriticalSectionC);
        exitDButton.onClick.AddListener(ExitCriticalSectionD);

        UpdateUI();
    }

    void Update()
    {
        // Run automatic simulation if enabled and not paused
        if (isAutomatic && !isPaused)
        {
            autoTimer += Time.deltaTime;

            if (autoTimer >= switchInterval)
            {
                autoTimer = 0f;

                // Automatically follow the order A -> B -> C -> D
                if (criticalSectionProcess == -1) // If critical section is free
                {
                    StartRequestCriticalSection(0); // Start with Process A
                }
                else if (criticalSectionProcess == 0) // If A is in the critical section
                {
                    ExitCriticalSection(); // Exit A
                    StartRequestCriticalSection(1); // Enter B
                }
                else if (criticalSectionProcess == 1) // If B is in the critical section
                {
                    ExitCriticalSection(); // Exit B
                    StartRequestCriticalSection(2); // Enter C
                }
                else if (criticalSectionProcess == 2) // If C is in the critical section
                {
                    ExitCriticalSection(); // Exit C
                    StartRequestCriticalSection(3); // Enter D
                }
                else if (criticalSectionProcess == 3) // If D is in the critical section
                {
                    ExitCriticalSection(); // Exit D
                    StartRequestCriticalSection(0); // Restart with A
                }
            }
        }
    }



    private void UpdateUI()
    {
        // Update critical section and process statuses
        criticalSectionStatus.text = criticalSectionProcess == -1 ? "Critical Section: Free" : $"Critical Section: Process {(char)(criticalSectionProcess + 'A')} is in";
        criticalSectionImage.color = criticalSectionProcess == -1 ? Color.green : Color.red;

        // Update each process status
        processAStatus.text = GetProcessStatus(0);
        processBStatus.text = GetProcessStatus(1);
        processCStatus.text = GetProcessStatus(2);
        processDStatus.text = GetProcessStatus(3);

        // Update process icon positions
        UpdateIconPosition(processAIcon, 0, idlePositionA);
        UpdateIconPosition(processBIcon, 1, idlePositionB);
        UpdateIconPosition(processCIcon, 2, idlePositionC);
        UpdateIconPosition(processDIcon, 3, idlePositionD);
    }


    private string GetProcessStatus(int processId)
    {
        if (criticalSectionProcess == processId)
            return $"Process {(char)(processId + 'A')}: In Critical Section";

        return flag[processId] ? $"Process {(char)(processId + 'A')}: Waiting..." : $"Process {(char)(processId + 'A')}: Idle";
    }

    private void UpdateIconPosition(GameObject icon, int processId, Transform idlePosition)
    {
        icon.transform.position = criticalSectionProcess == processId ? criticalSectionPosition.position :
            (waitingProcess == processId ? waitingPosition.position : idlePosition.position);
    }

    private void RequestCriticalSection(int processId)
    {
        if (!isPaused && processId >= 0 && processId < flag.Length)
        {
            // If another process is in the critical section
            if (criticalSectionProcess != -1 && criticalSectionProcess != processId)
            {
                waitingProcess = processId; // Set the current process to waiting
                flag[processId] = true;    // Mark the process as waiting
                DisplayWaitingState(processId); // Display "Waiting..." status
                waitingSound.Play();
            }
            else
            {
                // If no process in the critical section, start request
                StartRequestCriticalSection(processId);
            }
        }
    }


    private void StartRequestCriticalSection(int processId)
    {
        if (processId < 0 || processId >= flag.Length) return;

        // Allow Peterson's algorithm to handle the critical section request
        flag[processId] = true;
        int nextProcess = (processId + 1) % flag.Length; // Determine the next process
        turn = nextProcess;

        // Wait for the other process to finish
        while (flag[nextProcess] && turn == nextProcess)
        {
            DisplayWaitingState(processId);
            break;
        }

        EnterCriticalSection(processId);
    }

    private void DisplayWaitingState(int processId)
    {
        switch (processId)
        {
            case 0:
                processAStatus.text = "Process A: Waiting...";
                break;
            case 1:
                processBStatus.text = "Process B: Waiting...";
                break;
            case 2:
                processCStatus.text = "Process C: Waiting...";
                break;
            case 3:
                processDStatus.text = "Process D: Waiting...";
                break;
        }
    }



    private void EnterCriticalSection(int processId)
    {
        criticalSectionProcess = processId;
        waitingProcess = -1;
        UpdateUI();
        successSound.Play();
    }

    private void ExitCriticalSection()
    {
        if (criticalSectionProcess == -1) return;

        flag[criticalSectionProcess] = false;
        criticalSectionProcess = -1;

        if (waitingProcess != -1)
        {
            EnterCriticalSection(waitingProcess);
        }

        UpdateUI();
    }

    private void ExitCriticalSectionA()
    {
        if (criticalSectionProcess == 0)
        {
            flag[0] = false;
            criticalSectionProcess = -1;

            if (waitingProcess != -1)
            {
                EnterCriticalSection(waitingProcess);
            }

            UpdateUI();
        }
    }

    private void ExitCriticalSectionB()
    {
        if (criticalSectionProcess == 1)
        {
            flag[1] = false;
            criticalSectionProcess = -1;

            if (waitingProcess != -1)
            {
                EnterCriticalSection(waitingProcess);
            }

            UpdateUI();
        }
    }
    private void ExitCriticalSectionC()
    {
        if (criticalSectionProcess == 2)
        {
            flag[2] = false;
            criticalSectionProcess = -1;

            if (waitingProcess != -1)
            {
                EnterCriticalSection(waitingProcess);
            }

            UpdateUI();
        }
    }

    private void ExitCriticalSectionD()
    {
        if (criticalSectionProcess == 3)
        {
            flag[3] = false;
            criticalSectionProcess = -1;

            if (waitingProcess != -1)
            {
                EnterCriticalSection(waitingProcess);
            }

            UpdateUI();
        }
    }


    private void TogglePause()
    {
        isPaused = !isPaused;
        pauseResumeButton.GetComponentInChildren<Text>().text = isPaused ? "Resume" : "Pause";
    }

    private void ResetSimulation()
    {
        flag[0] = false;
        flag[1] = false;
        turn = 0;
        criticalSectionProcess = -1;
        waitingProcess = -1;
        isPaused = false;
        isAutomatic = false;
        autoTimer = 0f;

        UpdateUI();
        pauseResumeButton.GetComponentInChildren<Text>().text = "Pause";
        automaticSimulationButton.GetComponentInChildren<Text>().text = "Start Automatic Simulation";
    }

    private void ToggleAutomaticSimulation()
    {
        isAutomatic = !isAutomatic;
        automaticSimulationButton.GetComponentInChildren<Text>().text = isAutomatic ? "Stop Automatic Simulation" : "Start Automatic Simulation";
    }
}