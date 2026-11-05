using System;
using System.Collections.Generic;

namespace EA
{
    /// <summary>
    /// 教学内容数据模型（JSON 反序列化用）。字段保持扁平、JsonUtility 兼容。
    /// 新增课程 = 改 StreamingAssets/Content 下的 JSON，不改核心脚本。
    /// </summary>

    [Serializable]
    public class WordData
    {
        public string id;        // 唯一，如 word.apple
        public string text;      // 英文单词
        public string ipa;       // 音标
        public string meaning;   // 中文释义
        public string image;     // StreamingAssets 相对路径，可为空
        public string audio;     // StreamingAssets 相对路径，可为空
        public string example;   // 例句 id（sentences.json），可为空
    }

    [Serializable]
    public class SentenceData
    {
        public string id;        // sent.xxx
        public string speaker;   // 角色标记（fox / girl / ...）
        public string text;      // 英文句子
        public string translation; // 中文翻译，可为空
    }

    [Serializable]
    public class PhonicsData
    {
        public string id;        // phon.xxx
        public string symbol;    // /æ/
        public string exampleWord; // 例词
        public string hint;      // 发音提示，可为空
    }

    [Serializable]
    public class LessonData
    {
        public string id;        // lesson.xxx
        public string areaId;    // Vocabulary / Phonics / Speaking / Challenge
        public string title;     // 课程名
        public List<string> wordRefs = new List<string>();    // WordData.id 列表
        public List<string> phonicsRefs = new List<string>(); // PhonicsData.id 列表
        public List<string> sentenceRefs = new List<string>(); // SentenceData.id 列表
    }

    [Serializable]
    public class QuizQuestion
    {
        public string prompt;
        public List<string> choices = new List<string>();
        public int answerIndex;
    }

    [Serializable]
    public class QuizData
    {
        public string id;        // quiz.xxx
        public string title;     // 题型名
        public string lessonRef; // 关联课程，可为空
        public List<QuizQuestion> questions = new List<QuizQuestion>();
    }

    [Serializable]
    public class AreaData
    {
        public string id;        // 对应 AreaId 枚举名
        public string cnTitle;
        public string enTitle;
        public string intro;
        public string foxLine;
        public float themeR = 0.5f, themeG = 0.5f, themeB = 0.5f;
        public float mapX, mapZ;
    }

    /// <summary>全部内容文件的聚合容器。</summary>
    [Serializable]
    public class ContentDb
    {
        public List<AreaData> areas = new List<AreaData>();
        public List<WordData> words = new List<WordData>();
        public List<SentenceData> sentences = new List<SentenceData>();
        public List<PhonicsData> phonics = new List<PhonicsData>();
        public List<LessonData> lessons = new List<LessonData>();
        public List<QuizData> quizzes = new List<QuizData>();
    }
}
