using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Diagnostics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class JobTesting : MonoBehaviour
{
    [SerializeField] private bool useJobs;
    [SerializeField] private Transform pfPlayer;
    private List<Player> playerList;

    public class Player
    {
        public Transform transform;
        public float moveY;
    }

    private void Start()
    {
        playerList = new List<Player>();
        for (int i = 0; i < 1000; i++)
        {
            Transform playerTransform = Instantiate(pfPlayer, new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);
            playerList.Add(new Player
            {
                transform = playerTransform,
                moveY = UnityEngine.Random.Range(1f, 2f)
            });
        }
    }
    private void Update()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        if (useJobs)
        {
            NativeArray<float3> positionArray = new NativeArray<float3>(playerList.Count, Allocator.TempJob);
            NativeArray<float> moveYArray = new NativeArray<float>(playerList.Count, Allocator.TempJob);
            for (int i = 0; i < playerList.Count; i++)
            {
                positionArray[i] = playerList[i].transform.position;
                moveYArray[i] = playerList[i].moveY;
            }
            ReallyToughParallelJob reallyToughParallelJob = new ReallyToughParallelJob
            {
                deltaTime = Time.deltaTime,
                positionArray = positionArray,
                moveYArray = moveYArray
            };

            JobHandle handle = reallyToughParallelJob.Schedule(playerList.Count, 100);
            handle.Complete();

            for (int i = 0; i < playerList.Count; i++)
            {
                playerList[i].transform.position = positionArray[i];
                playerList[i].moveY = moveYArray[i];
            }
            positionArray.Dispose();
            moveYArray.Dispose();
        }
        else
        {
            foreach (var player in playerList)
            {
                player.transform.position += new Vector3(0, player.moveY * Time.deltaTime);
                if (player.transform.position.y > 5f)
                {
                    player.moveY = -math.abs(player.moveY);
                }
                if (player.transform.position.y < -5f)
                {
                    player.moveY = math.abs(player.moveY);
                }
                float value = 0f;
                for (int i = 0; i < 5000; i++)
                {
                    value = math.exp10(math.sqrt(value));
                }
            }
        }

        /*
        if (useJobs)
        {
            NativeList<JobHandle> handleList = new NativeList<JobHandle>(Allocator.Temp);
            for (int i = 0; i < 100; i++)
            {
                JobHandle handle = ReallyToughTaskJob();
                handleList.Add(handle);
            }
            JobHandle.CompleteAll(handleList);
            handleList.Dispose();
        }
        else
        {
            for (int i = 0; i < 100; i++)
            {
                ReallyToughTask();
            }
        }
        */
        sw.Stop();
        UnityEngine.Debug.Log(sw.ElapsedMilliseconds);
    }

    private void ReallyToughTask()
    {
        float value = 0f;
        for (int i = 0; i < 5000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

    private JobHandle ReallyToughTaskJob()
    {
        ReallyToughJob job = new ReallyToughJob();
        return job.Schedule();
    }
}

[BurstCompile]
public struct ReallyToughJob : IJob
{
    public void Execute()
    {
        float value = 0f;
        for (int i = 0; i < 5000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}

[BurstCompile]
public struct ReallyToughParallelJob : IJobParallelFor
{
    public NativeArray<float3> positionArray;
    public NativeArray<float> moveYArray;
    [ReadOnly] public float deltaTime;
    public void Execute(int index)
    {
        positionArray[index] += new float3(0, moveYArray[index] * deltaTime, 0f);
        if (positionArray[index].y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (positionArray[index].y < -5f)
        {
            moveYArray[index] = math.abs(moveYArray[index]);
        }
        float value = 0f;
        for (int i = 0; i < 5000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}
