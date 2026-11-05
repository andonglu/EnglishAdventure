using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EA
{
    /// <summary>
    /// 教学内容装载与校验：启动时读取 StreamingAssets/Content 下全部 JSON，
    /// 对必填字段、重复 ID、悬空引用（课程→单词/音标/句子）和缺失媒体文件
    /// （图片/音频）逐项校验，问题汇总后打到控制台。
    /// </summary>
    public static class ContentService
    {
        public static ContentDb Db { get; private set; } = new ContentDb();
        static bool _initialized;

        // 各文件的 JSON 包装（JsonUtility 要求字段名与 JSON 键一致）
        [System.Serializable] class AreaFile { public List<AreaData> areas; }
        [System.Serializable] class WordFile { public List<WordData> words; }
        [System.Serializable] class SentenceFile { public List<SentenceData> sentences; }
        [System.Serializable] class PhonicsFile { public List<PhonicsData> phonics; }
        [System.Serializable] class LessonFile { public List<LessonData> lessons; }
        [System.Serializable] class QuizFile { public List<QuizData> quizzes; }

        static string ContentDir => Path.Combine(Application.streamingAssetsPath, "Content");

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;
            Db = new ContentDb();
            var problems = new List<string>();

            Db.areas = LoadFile<AreaFile>("areas.json", problems)?.areas ?? new List<AreaData>();
            Db.words = LoadFile<WordFile>("words.json", problems)?.words ?? new List<WordData>();
            Db.sentences = LoadFile<SentenceFile>("sentences.json", problems)?.sentences ?? new List<SentenceData>();
            Db.phonics = LoadFile<PhonicsFile>("phonics.json", problems)?.phonics ?? new List<PhonicsData>();
            Db.lessons = LoadFile<LessonFile>("lessons.json", problems)?.lessons ?? new List<LessonData>();
            Db.quizzes = LoadFile<QuizFile>("quizzes.json", problems)?.quizzes ?? new List<QuizData>();

            Validate(problems);

            if (problems.Count == 0)
                Debug.Log($"[EA] 教学内容装载完成：{Db.areas.Count} 区域 / {Db.words.Count} 单词 / " +
                          $"{Db.sentences.Count} 句子 / {Db.phonics.Count} 音标 / {Db.lessons.Count} 课程 / {Db.quizzes.Count} 测验");
            else
            {
                Debug.LogWarning($"[EA] 教学内容装载完成，但有 {problems.Count} 个问题：");
                foreach (var p in problems) Debug.LogWarning("  [内容校验] " + p);
            }
        }

        static T LoadFile<T>(string filename, List<string> problems) where T : class
        {
            string path = Path.Combine(ContentDir, filename);
            if (!File.Exists(path))
            {
                problems.Add($"{filename} 文件缺失（{path}）");
                return null;
            }
            try
            {
                var wrapper = JsonUtility.FromJson<T>(File.ReadAllText(path));
                if (wrapper == null) problems.Add($"{filename} 解析失败（JSON 结构不符）");
                return wrapper;
            }
            catch (System.Exception e)
            {
                problems.Add($"{filename} 解析异常：{e.Message}");
                return null;
            }
        }

        // ---------------- 校验 ----------------

        static void Validate(List<string> problems)
        {
            CheckDup(Db.areas, a => a.id, "areas.json", problems);
            CheckDup(Db.words, w => w.id, "words.json", problems);
            CheckDup(Db.sentences, s => s.id, "sentences.json", problems);
            CheckDup(Db.phonics, p => p.id, "phonics.json", problems);
            CheckDup(Db.lessons, l => l.id, "lessons.json", problems);
            CheckDup(Db.quizzes, q => q.id, "quizzes.json", problems);

            var wordIds = new HashSet<string>();
            foreach (var w in Db.words)
            {
                Require(w.id, "words.json 存在缺 id 的单词", problems);
                Require(w.text, $"单词 {w.id} 缺 text", problems);
                wordIds.Add(w.id);
                CheckMedia(w.image, $"单词 {w.id} 的图片", problems);
                CheckMedia(w.audio, $"单词 {w.id} 的音频", problems);
                if (!string.IsNullOrEmpty(w.example) && FindSentence(w.example) == null)
                    problems.Add($"单词 {w.id} 引用了不存在的例句 {w.example}");
            }

            var sentIds = new HashSet<string>();
            foreach (var s in Db.sentences)
            {
                Require(s.id, "sentences.json 存在缺 id 的句子", problems);
                Require(s.text, $"句子 {s.id} 缺 text", problems);
                sentIds.Add(s.id);
            }

            foreach (var p in Db.phonics)
            {
                Require(p.id, "phonics.json 存在缺 id 的音标", problems);
                Require(p.symbol, $"音标 {p.id} 缺 symbol", problems);
            }

            foreach (var l in Db.lessons)
            {
                Require(l.id, "lessons.json 存在缺 id 的课程", problems);
                foreach (var r in l.wordRefs)
                    if (!wordIds.Contains(r)) problems.Add($"课程 {l.id} 引用了不存在的单词 {r}");
                foreach (var r in l.phonicsRefs)
                    if (FindPhonics(r) == null) problems.Add($"课程 {l.id} 引用了不存在的音标 {r}");
                foreach (var r in l.sentenceRefs)
                    if (FindSentence(r) == null) problems.Add($"课程 {l.id} 引用了不存在的句子 {r}");
            }

            foreach (var q in Db.quizzes)
            {
                Require(q.id, "quizzes.json 存在缺 id 的测验", problems);
                for (int i = 0; i < q.questions.Count; i++)
                {
                    var qq = q.questions[i];
                    Require(qq.prompt, $"测验 {q.id} 第 {i + 1} 题缺 prompt", problems);
                    if (qq.choices == null || qq.choices.Count < 2)
                        problems.Add($"测验 {q.id} 第 {i + 1} 题选项不足 2 个");
                    else if (qq.answerIndex < 0 || qq.answerIndex >= qq.choices.Count)
                        problems.Add($"测验 {q.id} 第 {i + 1} 题 answerIndex 越界（{qq.answerIndex}）");
                }
            }

            foreach (var a in Db.areas)
            {
                Require(a.id, "areas.json 存在缺 id 的区域", problems);
                if (!System.Enum.TryParse<AreaId>(a.id, out _))
                    problems.Add($"areas.json 的区域 id '{a.id}' 不是合法 AreaId（Vocabulary/Phonics/Speaking/Challenge）");
            }
        }

        static void CheckDup<T>(List<T> list, System.Func<T, string> idFn, string file, List<string> problems)
        {
            if (list == null) return;
            var seen = new HashSet<string>();
            foreach (var item in list)
            {
                string id = idFn(item);
                if (string.IsNullOrEmpty(id)) continue; // 缺 id 由专项报告
                if (!seen.Add(id)) problems.Add($"{file} 重复 ID：{id}");
            }
        }

        static void Require(bool ok, string msg, List<string> problems)
        {
            if (!ok) problems.Add(msg);
        }

        static void Require(string s, string msg, List<string> problems)
        {
            if (string.IsNullOrEmpty(s)) problems.Add(msg);
        }

        static void CheckMedia(string relPath, string what, List<string> problems)
        {
            if (string.IsNullOrEmpty(relPath)) return; // 媒体可选
            string full = Path.Combine(Application.streamingAssetsPath, relPath);
            if (!File.Exists(full))
                problems.Add($"{what} 文件缺失：{relPath}");
        }

        // ---------------- 查询 ----------------

        public static WordData FindWord(string id)
        {
            foreach (var w in Db.words) if (w.id == id) return w;
            return null;
        }

        public static LessonData FindLesson(string id)
        {
            foreach (var l in Db.lessons) if (l.id == id) return l;
            return null;
        }

        public static SentenceData FindSentence(string id)
        {
            foreach (var s in Db.sentences) if (s.id == id) return s;
            return null;
        }

        public static PhonicsData FindPhonics(string id)
        {
            foreach (var p in Db.phonics) if (p.id == id) return p;
            return null;
        }

        public static QuizData FindQuiz(string id)
        {
            foreach (var q in Db.quizzes) if (q.id == id) return q;
            return null;
        }

        /// <summary>某区域的第一个课程（V0.1.1 每区域一课）。</summary>
        public static LessonData FirstLessonOf(AreaId area)
        {
            foreach (var l in Db.lessons)
                if (l.areaId == area.ToString()) return l;
            return null;
        }
    }
}
