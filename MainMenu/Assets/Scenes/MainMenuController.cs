using UnityEngine;
using UnityEngine.UI; // Import the UI namespace to interact with buttons and text
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    // Declare the buttons and text
    public Button cpuSchedulingButton;
    public Button processSynchronizationButton;
    public Button memoryManagementButton;
    public Button exitButton;
    public Text infoText;

    void Start()
    {
        // Set up button click listeners
        cpuSchedulingButton.onClick.AddListener(OnCpuSchedulingClicked);
        processSynchronizationButton.onClick.AddListener(OnProcessSynchronizationClicked);
        memoryManagementButton.onClick.AddListener(OnMemoryManagementClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        // Set initial info text
        infoText.text = "Choose an option from the menu:";
    }

    // Function to handle CPU Scheduling button click
    void OnCpuSchedulingClicked()
    {
        infoText.text = "Running CPU Scheduling Algorithms...";
        StartCoroutine(RunCpuSchedulingSimulation());
    }

    // Function to handle Process Synchronization button click
    void OnProcessSynchronizationClicked()
    {
        infoText.text = "Running Process Synchronization (Peterson's n Solution)...";
        StartCoroutine(RunProcessSynchronizationSimulation());
    }

    // Function to handle Memory Management button click
    void OnMemoryManagementClicked()
    {
        infoText.text = "Running Memory Management (FIFO Page Replacement)...";
        StartCoroutine(RunMemoryManagementSimulation());
    }

    // Function to handle Exit button click
    void OnExitClicked()
    {
        Application.Quit();
        Debug.Log("Exiting application...");
    }

    // Simulate CPU Scheduling (First-Come-First-Serve & SJF)
    IEnumerator RunCpuSchedulingSimulation()
    {
        // Simulate waiting (for example purposes, using a simple delay)
        yield return new WaitForSeconds(2f);
        
        // Display CPU Scheduling options
        infoText.text = "Choose scheduling algorithm:\n1. FCFS\n2. SJF (Preemptive)";

        // Implement logic for running the algorithms here (you could use pop-up buttons for selecting)
        // For simplicity, we'll just display something after selection
        yield return new WaitForSeconds(2f);
        infoText.text = "CPU Scheduling simulation complete!";
    }

    // Simulate Process Synchronization (Peterson's n Solution)
    IEnumerator RunProcessSynchronizationSimulation()
    {
        // Simulate Peterson's n Solution (for example, 3 processes)
        yield return new WaitForSeconds(2f);
        
        infoText.text = "Simulating Peterson's n Solution...\n(Processes will enter critical section one by one)";
        
        // Add your Peterson's solution logic here
        
        yield return new WaitForSeconds(2f);
        infoText.text = "Process Synchronization complete!";
    }

    // Simulate Memory Management (FIFO Page Replacement)
    IEnumerator RunMemoryManagementSimulation()
    {
        // Simulate running FIFO Page Replacement algorithm
        yield return new WaitForSeconds(2f);
        
        infoText.text = "Simulating FIFO Page Replacement...\n(Displays page faults and frame content)";
        
        // Implement FIFO page replacement logic here (showing frames as pages come in)
        
        yield return new WaitForSeconds(2f);
        infoText.text = "Memory Management complete!";
    }
}
