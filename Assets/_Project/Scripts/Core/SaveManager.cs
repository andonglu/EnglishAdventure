using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EA
{
    // ---- 学习进度数据结构（JsonUtility 兼容，全部用 List，不用 Dictionary） ----

    [Serializable]
    public class WordProgress
    {
        public string id;            // 对应 WordData.id
        public int seenCount;        // 见过次数
        public int correctCount;     // 答对次数
        public long lastSeenTicks;   // DateTime.Ticks，0 = 未学过
    }

    [Serializable]
    public class PhonicsProgress
    {
        public string id;            // 对应 PhonicsData.id
        public int practiceCount;    // 练习次数
        public bool unlocked;        // 是否已解锁（由课程系统写入）
        public long lastSeenTicks;
    }

    [Serializable]
    public class SpeakingRecord
    {
        public string wordId;        // 练习的单词
        public float score;          // 0~1 发音评分（后续接语音识别）
        public long ticks;
    }

    [Serializable]
    public class MistakeRecord
    {
        public string quizId;        // 来源测验
        public string questionId;    // 来源题目
        public string givenAnswer;   // 错选的答案（或空表示超时/跳过）
        public int wrongCount;       // 累计错次
        public long lastTicks;
    }

    [Serializable]
    public class DailyRecord
    {
        public string date;          // yyyy-MM-dd
        public int wordsLearned;     // 当日新学单词数
        public int quizzesDone;      // 当日完成测验数
        public int starsEarned;      // 当日获得星星
    }

    [Serializable]
    public class LearningProgressData
    {
        public List<WordProgress> words = new List<WordProgress>();
        public List<PhonicsProgress> phonics = new List<PhonicsProgress>();
        public List<SpeakingRecord> speaking = new List<SpeakingRecord>();
        public List<MistakeRecord> mistakes = new List<MistakeRecord>();
        public List<DailyRecord> daily = new List<DailyRecord>();
    }

    /// <summary>
    /// 学习进度存档（JSON，Offline 优先）。
    /// version=1 为 V0.1 格式（stars/streakDays/lastOpenDate/visitedAreas/playerName），
    /// version=2 增加 LearningProgressData。旧档直接由 JsonUtility 读入（缺字段取默认），
    /// Load 内完成 v1→v2 迁移：仅补 version 标记，奖励数据原样保留。
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // 注意：默认 1 而非 2——JsonUtility 对 JSON 中缺失的字段保留声明默认值，
        // 这样 v1 旧档（无 version 字段）读入后 version==1，才能被识别并迁移。
        public int version = 1;

        // ---- 游戏奖励数据（与学习内容脱钩） ----
        public int stars;
        public int streakDays;
        public string lastOpenDate = "";          // yyyy-MM-dd
        public List<string> visitedAreas = new List<string>();  // 仅统计，不再发奖
        public string playerName = "Explorer";

        // ---- 学习进度（v2 新增） ----
        public LearningProgressData progress = new LearningProgressData();
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
                    if (loaded != null)
                    {
                        Data = loaded;
                        if (Data.version < 2)
                        {
                            Data.version = 2;
                            Debug.Log("[EA] 存档 v1→v2 迁移完成：stars=" + Data.stars +
                                      ", streakDays=" + Data.streakDays +
                                      ", visitedAreas=" + Data.visitedAreas.Count);
                            Save();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[EA] 存档读取失败，使用新存档: " + e.Message);
                Data = new SaveData();
            }
            Data.version = 2;   // 内存中始终为当前版本（文件中的旧版本号已在上面处理）

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

        // ==================== 奖励（星星只由学习行为获得） ====================

        /// <summary>奖励星星（完成课程/测验等学习行为时调用）。</summary>
        public static void AddStars(int count)
        {
            if (count <= 0) return;
            Data.stars += count;
            TodayRecord().starsEarned += count;
            Save();
        }

        // ==================== 区域统计（不发放奖励） ====================

        /// <summary>记录进入过某区域，仅作统计。返回是否首次。</summary>
        public static bool MarkVisited(string areaId)
        {
            if (Data.visitedAreas.Contains(areaId)) return false;
            Data.visitedAreas.Add(areaId);
            Save();
            return true;
        }

        public static bool HasVisited(string areaId) => Data.visitedAreas.Contains(areaId);

        // ==================== 单词掌握度 ====================

        public static WordProgress GetWordProgress(string wordId)
        {
            var list = Data.progress.words;
            for (int i = 0; i < list.Count; i++)
                if (list[i].id == wordId) return list[i];
            return null;
        }

        /// <summary>记录一次单词学习行为。isCorrect 为是否答对/跟读成功。</summary>
        public static void RecordWordPractice(string wordId, bool isCorrect)
        {
            if (string.IsNullOrEmpty(wordId)) return;
            bool isNew = false;
            var wp = GetWordProgress(wordId);
            if (wp == null)
            {
                wp = new WordProgress { id = wordId };
                Data.progress.words.Add(wp);
                isNew = true;
            }
            wp.seenCount++;
            if (isCorrect)
            {
                wp.correctCount++;
                AddStars(1);           // 答对一个单词 +1 星（学习行为）
            }
            wp.lastSeenTicks = DateTime.Now.Ticks;
            if (isNew) TodayRecord().wordsLearned++;
            Save();
        }

        /// <summary>掌握度 0~1：correctCount / 目标次数（3 次答对算掌握）。</summary>
        public static float GetWordMastery(string wordId)
        {
            var wp = GetWordProgress(wordId);
            if (wp == null) return 0f;
            return Mathf.Clamp01(wp.correctCount / 3f);
        }

        // ==================== 音标进度 ====================

        public static PhonicsProgress GetPhonicsProgress(string phonicsId)
        {
            var list = Data.progress.phonics;
            for (int i = 0; i < list.Count; i++)
                if (list[i].id == phonicsId) return list[i];
            return null;
        }

        public static void RecordPhonicsPractice(string phonicsId)
        {
            if (string.IsNullOrEmpty(phonicsId)) return;
            var pp = GetPhonicsProgress(phonicsId);
            if (pp == null)
            {
                pp = new PhonicsProgress { id = phonicsId };
                Data.progress.phonics.Add(pp);
            }
            pp.practiceCount++;
            pp.lastSeenTicks = DateTime.Now.Ticks;
            Save();
        }

        // ==================== 口语练习记录 ====================

        public static void RecordSpeaking(string wordId, float score)
        {
            if (string.IsNullOrEmpty(wordId)) return;
            Data.progress.speaking.Add(new SpeakingRecord
            {
                wordId = wordId,
                score = Mathf.Clamp01(score),
                ticks = DateTime.Now.Ticks
            });
            if (score >= 0.6f) AddStars(1);   // 发音达标 +1 星
            Save();
        }

        // ==================== 错题本 ====================

        public static void RecordMistake(string quizId, string questionId, string givenAnswer)
        {
            var list = Data.progress.mistakes;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].questionId == questionId)
                {
                    list[i].wrongCount++;
                    list[i].givenAnswer = givenAnswer ?? "";
                    list[i].lastTicks = DateTime.Now.Ticks;
                    Save();
                    return;
                }
            }
            list.Add(new MistakeRecord
            {
                quizId = quizId ?? "",
                questionId = questionId ?? "",
                givenAnswer = givenAnswer ?? "",
                wrongCount = 1,
                lastTicks = DateTime.Now.Ticks
            });
            Save();
        }

        // ==================== 每日记录 ====================

        public static DailyRecord TodayRecord()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            var list = Data.progress.daily;
            for (int i = list.Count - 1; i >= 0; i--)
                if (list[i].date == today) return list[i];
            // 只保留最近 60 天
            while (list.Count >= 60) list.RemoveAt(0);
            var rec = new DailyRecord { date = today };
            list.Add(rec);
            return rec;
        }
    }
}
