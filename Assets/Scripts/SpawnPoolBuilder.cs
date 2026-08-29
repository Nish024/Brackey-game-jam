using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Static utility class to build the daily spawn pool for customers.
/// </summary>
public static class SpawnPoolBuilder
{
    public static Queue<GameObject> BuildPool(
        GameObject[] availableModels,
        int maxModelsPerDay,
        int legitCount,
        int fakeCount,
        int stolenCount,
        int windowSize)
    {
        List<GameObject> rawPool = new List<GameObject>();
        if (availableModels == null || availableModels.Length == 0) return new Queue<GameObject>();

        // 1. Pick today's featured models (e.g. max 2 per day)
        List<GameObject> featuredModels = new List<GameObject>();
        List<GameObject> tempAvailable = new List<GameObject>(availableModels);
        
        for (int i = 0; i < maxModelsPerDay && tempAvailable.Count > 0; i++)
        {
            int r = Random.Range(0, tempAvailable.Count);
            featuredModels.Add(tempAvailable[r]);
            tempAvailable.RemoveAt(r);
        }

        // Gather all variations from the featured models
        List<GameObject> todayLegit = new List<GameObject>();
        List<GameObject> todayFake = new List<GameObject>();
        List<GameObject> todayStolen = new List<GameObject>();

        foreach (var model in featuredModels)
        {
            if (model == null) continue;
            todayLegit.Add(model);

            var holder = model.GetComponent<GunDataHolder>();
            if (holder != null && holder.Data != null)
            {
                if (holder.Data.fakeVariations != null)
                    todayFake.AddRange(holder.Data.fakeVariations);
                if (holder.Data.stolenVariations != null)
                    todayStolen.AddRange(holder.Data.stolenVariations);
            }
        }

        // If a category is completely empty, fallback to spawning legits
        if (todayFake.Count == 0) todayFake.AddRange(todayLegit);
        if (todayStolen.Count == 0) todayStolen.AddRange(todayLegit);

        // 2. Populate the raw pool by drawing from today's variations
        AddFromPool(rawPool, todayLegit, legitCount);
        AddFromPool(rawPool, todayFake, fakeCount);
        AddFromPool(rawPool, todayStolen, stolenCount);

        // 3. Shuffle the combined list using Fisher-Yates
        Shuffle(rawPool);

        // 4. Apply the window guarantee (at least one legit item per window)
        HashSet<GameObject> legitSet = new HashSet<GameObject>(todayLegit);
        ApplyWindowGuarantee(rawPool, legitSet, windowSize);

        // 5. Apply anti-clustering (prevent same model back-to-back)
        ApplyAntiClustering(rawPool, legitSet);

        // 6. Return as a Queue
        return new Queue<GameObject>(rawPool);
    }

    private static void AddFromPool(List<GameObject> targetList, List<GameObject> sourcePrefabs, int count)
    {
        if (sourcePrefabs == null || sourcePrefabs.Count == 0 || count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, sourcePrefabs.Count);
            targetList.Add(sourcePrefabs[index]);
        }
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[r];
            list[r] = temp;
        }
    }

    private static void ApplyWindowGuarantee(List<GameObject> pool, HashSet<GameObject> legitPrefabs, int windowSize)
    {
        if (windowSize <= 0 || pool.Count < windowSize) return;

        for (int i = 0; i < pool.Count; i += windowSize)
        {
            int windowEnd = Mathf.Min(i + windowSize, pool.Count);
            bool hasLegit = false;

            for (int j = i; j < windowEnd; j++)
            {
                if (legitPrefabs.Contains(pool[j]))
                {
                    hasLegit = true;
                    break;
                }
            }

            if (!hasLegit)
            {
                int swapIndex = -1;
                for (int k = windowEnd; k < pool.Count; k++)
                {
                    if (legitPrefabs.Contains(pool[k]))
                    {
                        swapIndex = k;
                        break;
                    }
                }

                if (swapIndex != -1)
                {
                    int targetIndex = Random.Range(i, windowEnd);
                    GameObject temp = pool[targetIndex];
                    pool[targetIndex] = pool[swapIndex];
                    pool[swapIndex] = temp;
                }
            }
        }
    }

    private static void ApplyAntiClustering(List<GameObject> pool, HashSet<GameObject> legitPrefabs)
    {
        for (int i = 0; i < pool.Count - 1; i++)
        {
            string currentModel = GetModelName(pool[i]);
            string nextModel = GetModelName(pool[i + 1]);

            if (currentModel == nextModel)
            {
                bool isNextLegit = legitPrefabs.Contains(pool[i + 1]);

                int swapIndex = -1;
                for (int j = i + 2; j < pool.Count; j++)
                {
                    string candidateModel = GetModelName(pool[j]);
                    bool isCandidateLegit = legitPrefabs.Contains(pool[j]);

                    // Try to find a different model with the same legitimacy
                    if (candidateModel != currentModel && isCandidateLegit == isNextLegit)
                    {
                        swapIndex = j;
                        break;
                    }
                }

                if (swapIndex != -1)
                {
                    GameObject temp = pool[i + 1];
                    pool[i + 1] = pool[swapIndex];
                    pool[swapIndex] = temp;
                }
            }
        }
    }

    private static string GetModelName(GameObject prefab)
    {
        if (prefab == null) return "Unknown";
        var holder = prefab.GetComponent<GunDataHolder>();
        if (holder != null && holder.Data != null)
            return holder.Data.gunModelName;
        return prefab.name;
    }
}
