using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EA
{
    /// <summary>学习进度存档（JSON，Offline 优先）。字段向后兼容：缺字段取默认。</summary>
    [Serializable]
    public class SaveData
    {
        public int stars;
        public int streakDays;
        public string lastOpenDate = "";          // yyyy-MM-dd
        public List<string> visitedAreas = new List<string>();
        public string playerName = "Explorer";
    }

    public static class SaveManager
    {
        static string PathFile => Path.Combine(Application.persistentDataPath, "ea_save.json");
        public static SaveData Data { get; private set; } = new SaveData();

        public static event Action OnChanged;

        public static void Load()
        {
            try
            {
                if (File.Exists(PathFile))
                {
                    var json = File.ReadAllText(PathFile);
                    var loaded = JsonUtility.FromJson<SaveData>(json);
                    if (loaded != null) Data = loaded;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[EA] 存档读取失败，使用新存档: " + e.Message);
                Data = new SaveData();
            }

            // 连续天数：今天打开且昨天打开 → +1；昨天没打开 → 重置为 1
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            if (Data.lastOpenDate != today)
            {
                Data.streakDays = (Data.lastOpenDate == yesterday) ? Data.streakDays + 1 : 1;
                Data.lastOpenDate = today;
                Save();
            }
        }

        public static void Save()
        {
            try { File.WriteAllText(PathFile, JsonUtility.ToJson(Data)); }
            catch (Exception e) { Debug.LogWarning("[EA] 存档写入失败: " + e.Message); }
            OnChanged?.Invoke();
        }

        /// <summary>首次访问某区域：奖励星星。返回是否为新访问。</summary>
        public static bool MarkVisited(string areaId, int starReward)
        {
            if (Data.visitedAreas.Contains(areaId)) return false;
            Data.visitedAreas.Add(areaId);
            Data.stars += Mathf.Max(0, starReward);
            Save();
            return true;
        }

        public static bool HasVisited(string areaId) => Data.visitedAreas.Contains(areaId);
    }
}
