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
        GameObject obj = Instantiate(processPrefab);
        obj.GetComponentInChildren<TMP_Text>().text = $"P{process.processId}";
        
        // Set process color
        Image processImage = obj.GetComponent<Image>();
        if (processImage != null)
        {
            processImage.color = process.processColor;
        }
        
        processObjects.Add(obj);
        return obj;
    }

    private void UpdateProcessVisual(GameObject processObj, float progress, ProcessData process)
    {
        // Update position
        Vector3 startPos = new Vector3(-5f, 0f, 0f);
        Vector3 endPos = new Vector3(5f, 0f, 0f);
        processObj.transform.position = Vector3.Lerp(startPos, endPos, progress);
        
        // Update progress text
        TMP_Text progressText = processObj.GetComponentInChildren<TMP_Text>();
        if (progressText != null)
        {
            progressText.text = $"P{process.processId}\n{(progress * 100):F0}%";
        }
        
        // Visual feedback
        if (progress >= 0.99f && !processObj.CompareTag("Completed"))
        {
            processObj.tag = "Completed";
            if (completionParticles != null)
            {
                Instantiate(completionParticles, processObj.transform.position, Quaternion.identity);
            }
            PlaySound(processCompleteSound);
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
            PlaySound(processStartSound);

            float executionTime = 0f;
            while (executionTime < process.burstTime)
            {
                executionTime += Time.deltaTime;
                UpdateProcessVisual(processObj, executionTime / process.burstTime, process);
                yield return null;
            }

            currentTime += process.burstTime;
            process.completionTime = currentTime;
            process.turnaroundTime = process.completionTime - process.arrivalTime;
            process.waitingTime = process.turnaroundTime - process.burstTime;

            Destroy(processObj, 1f);
        }
    }

    private System.Collections.IEnumerator RunSJF()
    {
        List<ProcessData> remainingProcesses = new List<ProcessData>(processes);
        float currentTime = 0f;

        while (remainingProcesses.Count > 0)
        {
            List<ProcessData> availableProcesses = remainingProcesses.FindAll(p => p.arrivalTime <= currentTime);

            if (availableProcesses.Count == 0)
            {
                currentTime += Time.deltaTime;
                yield return null;
                continue;
            }

            ProcessData shortestJob = availableProcesses.OrderBy(p => p.burstTime).First();
            remainingProcesses.Remove(shortestJob);

            GameObject processObj = CreateProcessVisual(shortestJob);
            PlaySound(processStartSound);

            float executionTime = 0f;
            while (executionTime < shortestJob.burstTime)
            {
                executionTime += Time.deltaTime;
                UpdateProcessVisual(processObj, executionTime / shortestJob.burstTime, shortestJob);
                yield return null;
            }

            currentTime += shortestJob.burstTime;
            shortestJob.completionTime = currentTime;
            shortestJob.turnaroundTime = shortestJob.completionTime - shortestJob.arrivalTime;
            shortestJob.waitingTime = shortestJob.turnaroundTime - shortestJob.burstTime;

            Destroy(processObj, 1f);
        }
    }

    private System.Collections.IEnumerator RunRoundRobin()
    {
        Queue<ProcessData> processQueue = new Queue<ProcessData>();
        List<ProcessData> remainingProcesses = new List<ProcessData>(processes);
        float currentTime = 0f;

        while (remainingProcesses.Count > 0 || processQueue.Count > 0)
        {
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
            GameObject processObj = CreateProcessVisual(currentProcess);
            PlaySound(processStartSound);

            float executeTime = Mathf.Min(timeQuantum, currentProcess.remainingTime);
            float executionProgress = 0f;

            while (executionProgress < executeTime)
            {
                executionProgress += Time.deltaTime;
                float totalProgress = 1f - (currentProcess.remainingTime - executionProgress) / currentProcess.burstTime;
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
                currentProcess.waitingTime = currentProcess.turnaroundTime - currentProcess.burstTime;
            }

            Destroy(processObj, 1f);
        }
    }

    private void CalculateAndDisplayStats()
    {
        float totalWaitingTime = 0f;
        float totalTurnaroundTime = 0f;
        
        string detailedStats = "Process Statistics:\n\n";
        
        foreach (ProcessData process in processes)
        {
            totalWaitingTime += process.waitingTime;
            totalTurnaroundTime += process.turnaroundTime;
            
            detailedStats += $"Process {process.processId}:\n" +
                            $"Waiting Time: {process.waitingTime:F2}\n" +
                            $"Turnaround Time: {process.turnaroundTime:F2}\n" +
                            $"Completion Time: {process.completionTime:F2}\n\n";
        }
        
        float avgWaitingTime = totalWaitingTime / processes.Count;
        float avgTurnaroundTime = totalTurnaroundTime / processes.Count;
        
        detailedStats += $"\nAverage Waiting Time: {avgWaitingTime:F2}\n" +
                        $"Average Turnaround Time: {avgTurnaroundTime:F2}\n\n" +
                        $"Total Processes: {processes.Count}";
        
        statsText.text = detailedStats;
    }

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