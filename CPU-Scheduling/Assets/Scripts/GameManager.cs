using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown algorithmDropdown;
    public Button startButton;
    public Button resetButton;
    public TMP_Text statsText;
    public TMP_Text explanationText;
    public GameObject processPrefab;
    public GameObject timelineParent;
    public Slider simulationSpeedSlider;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip processStartSound;
    public AudioClip processCompleteSound;
    public AudioClip contextSwitchSound;

    [Header("Visual Effects")]
    public GameObject contextSwitchEffectPrefab;
    public ParticleSystem completionParticles;

    private List<ProcessData> processes;
    private bool isSimulationRunning;
    private List<GameObject> processObjects = new List<GameObject>();
    private SchedulingAlgorithm currentAlgorithm;
    private float timeQuantum = 2f;
    private float simulationSpeed = 1f;
    private Dictionary<int, GameObject> timelineMarkers = new Dictionary<int, GameObject>();

    public enum SchedulingAlgorithm
    {
        FCFS,
        SJF,
        RoundRobin
    }

    void Start()
    {
        InitializeGame();
        SetupUI();
        UpdateAlgorithmExplanation();
    }

    private void InitializeGame()
    {
        processes = new List<ProcessData>
        {
            new ProcessData(1, 6f, 0f),
            new ProcessData(2, 4f, 2f),
            new ProcessData(3, 8f, 4f),
            new ProcessData(4, 3f, 6f)
        };
        isSimulationRunning = false;
    }

    private void SetupUI()
    {
        algorithmDropdown.ClearOptions();
        algorithmDropdown.AddOptions(new List<string> { "First Come First Serve", "Shortest Job First", "Round Robin" });
        algorithmDropdown.onValueChanged.AddListener(OnAlgorithmChanged);
        startButton.onClick.AddListener(StartSimulation);
        resetButton.onClick.AddListener(ResetSimulation);
        simulationSpeedSlider.onValueChanged.AddListener(OnSpeedChanged);
        
        // Initialize timeline
        CreateTimeline();
    }

    private void CreateTimeline()
    {
        float maxTime = processes.Max(p => p.arrivalTime + p.burstTime);
        for (int i = 0; i <= Mathf.CeilToInt(maxTime); i++)
        {
            GameObject marker = new GameObject($"TimeMarker_{i}");
            marker.transform.SetParent(timelineParent.transform);
            
            TMP_Text timeText = marker.AddComponent<TMP_Text>();
            timeText.text = i.ToString();
            timeText.fontSize = 12;
            timeText.alignment = TextAlignmentOptions.Center;
            
            RectTransform rect = marker.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(i * 50, -20);
            
            timelineMarkers[i] = marker;
        }
    }

    private void UpdateAlgorithmExplanation()
    {
        string explanation = currentAlgorithm switch
        {
            SchedulingAlgorithm.FCFS => "First Come First Serve (FCFS)\n" +
                                      "• Non-preemptive algorithm\n" +
                                      "• Processes are executed in order of arrival\n" +
                                      "• Simple but may cause convoy effect",
            
            SchedulingAlgorithm.SJF => "Shortest Job First (SJF)\n" +
                                      "• Non-preemptive algorithm\n" +
                                      "• Selects process with shortest burst time\n" +
                                      "• Optimal average waiting time",
            
            SchedulingAlgorithm.RoundRobin => "Round Robin\n" +
                                             "• Preemptive algorithm\n" +
                                             "• Each process gets a time quantum\n" +
                                             "• Fair but has context switching overhead\n" +
                                             $"• Current quantum: {timeQuantum}s",
            
            _ => ""
        };
        
        explanationText.text = explanation;
    }

private GameObject CreateProcessVisual(ProcessData process)
{
    // Create main process object
    GameObject obj = Instantiate(processPrefab);
    
    // Create UI text as a direct child of the process
    GameObject textObj = new GameObject("ProcessText");
    textObj.transform.SetParent(obj.transform, false);
    
    // Add Text component
    TMP_Text processText = textObj.AddComponent<TextMeshProUGUI>();
    processText.text = $"P{process.processId}\n0%";
    processText.fontSize = 24;
    processText.alignment = TextAlignmentOptions.Center;
    processText.color = Color.black;
    
    // Set up RectTransform for proper positioning
    RectTransform textRect = textObj.GetComponent<RectTransform>();
    textRect.anchorMin = new Vector2(0.5f, 1f);
    textRect.anchorMax = new Vector2(0.5f, 1f);
    textRect.pivot = new Vector2(0.5f, 0f);
    
    // Calculate horizontal offset based on process ID (1cm is approximately 37.8 pixels)
    float horizontalOffset = (process.processId - 1) * 37.8f;
    // Updated vertical offset: original 25 + 37.8 (1cm) = 62.8
    textRect.anchoredPosition = new Vector2(horizontalOffset, 62.8f);
    // Maintain the same height as before
    textRect.sizeDelta = new Vector2(100, 78.35f);
    
    // Set process color
    Image processImage = obj.GetComponent<Image>();
    if (processImage != null)
    {
        processImage.color = process.processColor;
    }
    
    processObjects.Add(obj);
    return obj;
}

// Also need to update the UpdateProcessVisual function to maintain the new position
private void UpdateProcessVisual(GameObject processObj, float progress, ProcessData process)
{
    // Update position
    Vector3 startPos = new Vector3(-6f, 0f, 0f);
    Vector3 endPos = new Vector3(6f, 0f, 0f);
    processObj.transform.position = Vector3.Lerp(startPos, endPos, progress);
    
    // Find and update the text
    TMP_Text processText = processObj.GetComponentInChildren<TextMeshProUGUI>();
    if (processText != null)
    {
        processText.text = $"P{process.processId}\n{(progress * 100):F0}%";
        
        // Maintain the horizontal offset during updates and use updated vertical position
        RectTransform textRect = processText.GetComponent<RectTransform>();
        if (textRect != null)
        {
            float horizontalOffset = (process.processId - 1) * 37.8f;
            // Updated vertical position to match CreateProcessVisual
            textRect.anchoredPosition = new Vector2(horizontalOffset, 62.8f);
        }
        
        if (progress >= 0.99f && !processObj.CompareTag("Completed"))
        {
            processObj.tag = "Completed";
            if (completionParticles != null)
            {
                Instantiate(completionParticles, processObj.transform.position, Quaternion.identity);
            }
            PlaySound(processCompleteSound);
            processText.color = Color.green;
            processText.text = $"P{process.processId}\nComplete!";
        }
    }
<<<<<<< Updated upstream
}
private void Update()
=======
}private void Update()
>>>>>>> Stashed changes
{
    // Ensure text always faces camera
    Camera mainCamera = Camera.main;
    if (mainCamera != null)
    {
        foreach (GameObject processObj in processObjects)
        {
            if (processObj != null)
            {
                Canvas canvas = processObj.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    canvas.transform.forward = mainCamera.transform.forward;
                }
            }
        }
    }
}
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void ShowContextSwitch()
    {
        if (contextSwitchEffectPrefab != null)
        {
            GameObject effect = Instantiate(contextSwitchEffectPrefab);
            Destroy(effect, 1f);
        }
        PlaySound(contextSwitchSound);
    }

    private void OnSpeedChanged(float value)
    {
        simulationSpeed = value;
        Time.timeScale = value;
    }

    private void OnAlgorithmChanged(int value)
    {
        currentAlgorithm = (SchedulingAlgorithm)value;
        ResetSimulation();
        UpdateAlgorithmExplanation();
    }

    private void StartSimulation()
    {
        if (isSimulationRunning) return;
        isSimulationRunning = true;
        StartCoroutine(RunSimulation());
    }

    private System.Collections.IEnumerator RunSimulation()
    {
        switch (currentAlgorithm)
        {
            case SchedulingAlgorithm.FCFS:
                yield return StartCoroutine(RunFCFS());
                break;
            case SchedulingAlgorithm.SJF:
                yield return StartCoroutine(RunSJF());
                break;
            case SchedulingAlgorithm.RoundRobin:
                yield return StartCoroutine(RunRoundRobin());
                break;
        }

        CalculateAndDisplayStats();
    }

 private System.Collections.IEnumerator RunFCFS()
{
    List<ProcessData> sortedProcesses = new List<ProcessData>(processes);
    sortedProcesses.Sort((a, b) => a.arrivalTime.CompareTo(b.arrivalTime));

    float currentTime = 0f;

    foreach (ProcessData process in sortedProcesses)
    {
        while (currentTime < process.arrivalTime)
        {
            currentTime += Time.deltaTime;
            yield return null;
        }

        GameObject processObj = CreateProcessVisual(process);
        if (processObj != null)
        {
            PlaySound(processStartSound);

            float executionTime = 0f;
            while (executionTime < process.burstTime)
            {
                executionTime += Time.deltaTime;
                if (processObj != null)
                {
                    UpdateProcessVisual(processObj, executionTime / process.burstTime, process);
                }
                yield return null;
            }

            currentTime += process.burstTime;
            process.completionTime = currentTime;
            process.turnaroundTime = process.completionTime - process.arrivalTime;
            process.waitingTime = process.turnaroundTime - process.burstTime;

            // Add delay before destroying the object
            float delayTime = 0f;
            while (delayTime < 0.3f)
            {
                delayTime += Time.deltaTime;
                yield return null;
            }

            if (processObj != null)
            {
                Destroy(processObj);
                processObjects.Remove(processObj);
            }
        }
    }
}
private System.Collections.IEnumerator RunSJF()
{
    List<ProcessData> remainingProcesses = new List<ProcessData>(processes);
    float currentTime = 0f;

    while (remainingProcesses.Count > 0)
    {
        // Get all processes that have arrived by the current time
        List<ProcessData> availableProcesses = remainingProcesses.FindAll(p => p.arrivalTime <= currentTime);

        if (availableProcesses.Count == 0)
        {
            // If no processes available, move time to next arrival
            float nextArrival = remainingProcesses.Min(p => p.arrivalTime);
            currentTime = nextArrival;
            continue;
        }

        // Select the process with shortest burst time among available processes
        ProcessData shortestJob = availableProcesses.OrderBy(p => p.burstTime).First();
        remainingProcesses.Remove(shortestJob);

        // Calculate waiting time from when process arrived until it starts
        shortestJob.waitingTime = currentTime - shortestJob.arrivalTime;
        
        GameObject processObj = CreateProcessVisual(shortestJob);
        if (processObj != null)
        {
            PlaySound(processStartSound);

            float executionTime = 0f;
            while (executionTime < shortestJob.burstTime)
            {
                executionTime += Time.deltaTime;
                if (processObj != null)
                {
                    UpdateProcessVisual(processObj, executionTime / shortestJob.burstTime, shortestJob);
                }
                yield return null;
            }

            currentTime += shortestJob.burstTime;
            shortestJob.completionTime = currentTime;
            shortestJob.turnaroundTime = shortestJob.completionTime - shortestJob.arrivalTime;

            // Add delay before destroying the object
            float delayTime = 0f;
            while (delayTime < 0.3f)
            {
                delayTime += Time.deltaTime;
                yield return null;
            }

            if (processObj != null)
            {
                Destroy(processObj);
                processObjects.Remove(processObj);
            }
        }
    }
}
// Fix 1: RunRoundRobin - Adding proper waiting time calculation and fixing time tracking
<<<<<<< Updated upstream
=======
// Fix 1: RunRoundRobin - Adding proper waiting time calculation and fixing time tracking
>>>>>>> Stashed changes
private System.Collections.IEnumerator RunRoundRobin()
{
    Queue<ProcessData> processQueue = new Queue<ProcessData>();
    List<ProcessData> remainingProcesses = new List<ProcessData>(processes);
    Dictionary<int, GameObject> activeProcessObjects = new Dictionary<int, GameObject>();
    Dictionary<int, float> processStartTimes = new Dictionary<int, float>(); // Track when each process first starts
    Dictionary<int, float> totalExecutionTime = new Dictionary<int, float>(); // Track total execution time per process
    float currentTime = 0f;

    // Initialize tracking dictionaries
    foreach (ProcessData process in processes)
    {
        processStartTimes[process.processId] = -1; // -1 indicates not started
        totalExecutionTime[process.processId] = 0f;
    }

    while (remainingProcesses.Count > 0 || processQueue.Count > 0)
    {
        // Check for newly arrived processes
        List<ProcessData> newProcesses = remainingProcesses.FindAll(p => p.arrivalTime <= currentTime);
        foreach (ProcessData process in newProcesses)
        {
            processQueue.Enqueue(process);
            remainingProcesses.Remove(process);
        }

        if (processQueue.Count == 0)
        {
            currentTime += Time.deltaTime;
            yield return null;
            continue;
        }

        ProcessData currentProcess = processQueue.Dequeue();
        
        // Record first start time if not already recorded
        if (processStartTimes[currentProcess.processId] == -1)
        {
            processStartTimes[currentProcess.processId] = currentTime;
        }

        GameObject processObj;

        if (!activeProcessObjects.ContainsKey(currentProcess.processId))
        {
            processObj = CreateProcessVisual(currentProcess);
            activeProcessObjects[currentProcess.processId] = processObj;
            PlaySound(processStartSound);
        }
        else
        {
            processObj = activeProcessObjects[currentProcess.processId];
        }

        if (processObj != null)
        {
            float executeTime = Mathf.Min(timeQuantum, currentProcess.remainingTime);
            float executionProgress = 0f;

            while (executionProgress < executeTime)
            {
                executionProgress += Time.deltaTime;
                totalExecutionTime[currentProcess.processId] += Time.deltaTime;
                float totalProgress = totalExecutionTime[currentProcess.processId] / currentProcess.burstTime;
                UpdateProcessVisual(processObj, totalProgress, currentProcess);
                yield return null;
            }

            currentProcess.remainingTime -= executeTime;
            currentTime += executeTime;

            if (currentProcess.remainingTime > 0)
            {
                processQueue.Enqueue(currentProcess);
                ShowContextSwitch();
            }
            else
            {
                currentProcess.completionTime = currentTime;
                currentProcess.turnaroundTime = currentProcess.completionTime - currentProcess.arrivalTime;
                // Correct waiting time calculation: total time - burst time
                currentProcess.waitingTime = currentProcess.turnaroundTime - currentProcess.burstTime;

                float delayTime = 0f;
                while (delayTime < 0.3f)
                {
                    delayTime += Time.deltaTime;
                    yield return null;
                }

                if (processObj != null)
                {
                    activeProcessObjects.Remove(currentProcess.processId);
                    Destroy(processObj);
                    processObjects.Remove(processObj);
                }
            }
        }
    }
}
<<<<<<< Updated upstream
=======

// Fix 2: CalculateAndDisplayStats - Adding throughput and CPU utilization metrics
>>>>>>> Stashed changes
private void CalculateAndDisplayStats()
{
    float totalWaitingTime = 0f;
    float totalTurnaroundTime = 0f;
    float totalBurstTime = 0f;
    float maxCompletionTime = 0f;
    
    string detailedStats = "Process Statistics:\n\n";
    
    List<string> processIds = new List<string>();
    List<string> waitingTimes = new List<string>();
    List<string> turnaroundTimes = new List<string>();
    List<string> completionTimes = new List<string>();
    
    foreach (ProcessData process in processes)
    {
        totalWaitingTime += process.waitingTime;
        totalTurnaroundTime += process.turnaroundTime;
        totalBurstTime += process.burstTime;
        maxCompletionTime = Mathf.Max(maxCompletionTime, process.completionTime);
        
        processIds.Add($"Process {process.processId}");
        waitingTimes.Add($"{process.waitingTime:F2}");
        turnaroundTimes.Add($"{process.turnaroundTime:F2}");
        completionTimes.Add($"{process.completionTime:F2}");
    }
    
    float avgWaitingTime = totalWaitingTime / processes.Count;
    float avgTurnaroundTime = totalTurnaroundTime / processes.Count;
    
<<<<<<< Updated upstream
=======
    
>>>>>>> Stashed changes
    detailedStats += string.Format("{0,-15}{1,-15}{2,-15}{3,-15}\n",
        "Process", "W.T", "T.T", "C.T");
    
    detailedStats += string.Format("{0,-15}{1,-15}{2,-15}{3,-15}\n\n",
        "------------", "    ----", "     ------", "        ------");
    
    for (int i = 0; i < processes.Count; i++)
    {
        detailedStats += string.Format("{0,-15}{1,-15}{2,-15}{3,-15}\n",
            processIds[i], waitingTimes[i], turnaroundTimes[i], completionTimes[i]);
    }
    
    detailedStats += $"\nAverage Waiting Time: {avgWaitingTime:F2}\n" +
                    $"Average Turnaround Time: {avgTurnaroundTime:F2}\n" +
                    $"Total Processes: {processes.Count}";
    
    statsText.text = detailedStats;
}
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
   private void ResetSimulation()
{
    StopAllCoroutines();
    isSimulationRunning = false;

    foreach (GameObject obj in processObjects)
    {
        if (obj != null) Destroy(obj);
    }
    processObjects.Clear();

    foreach (ProcessData process in processes)
    {
        process.remainingTime = process.burstTime;
        process.waitingTime = 0f;
        process.turnaroundTime = 0f;
        process.completionTime = 0f;
    }

    statsText.text = "";
}
    }
