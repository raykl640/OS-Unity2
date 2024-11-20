using UnityEngine;

[System.Serializable]
public class ProcessData
{
    public int processId;
    public float burstTime;
    public float arrivalTime;
    public float remainingTime;
    public float waitingTime;
    public float turnaroundTime;
    public float completionTime;
    public Color processColor;

    public ProcessData(int id, float burst, float arrival)
    {
        processId = id;
        burstTime = burst;
        arrivalTime = arrival;
        remainingTime = burst;
        waitingTime = 0;
        turnaroundTime = 0;
        completionTime = 0;
        processColor = new Color(Random.value, Random.value, Random.value);
    }
}