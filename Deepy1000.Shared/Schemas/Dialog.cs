using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeepPavlov.Dream.Schemas
{
    public class EmotionClassification
    {
        [JsonProperty("anger")]
        public double Anger { get; set; }

        [JsonProperty("fear")]
        public double Fear { get; set; }

        [JsonProperty("joy")]
        public double Joy { get; set; }

        [JsonProperty("love")]
        public double Love { get; set; }

        [JsonProperty("sadness")]
        public double Sadness { get; set; }

        [JsonProperty("surprise")]
        public double Surprise { get; set; }

        [JsonProperty("neutral")]
        public double Neutral { get; set; }
    }

    public class Annotations
    {
        [JsonProperty("emotion_classification")]
        public List<EmotionClassification> EmotionClassifications { get; set; }
    }

    public class DebugOutput
    {
        [JsonProperty("skill_name")]
        public string SkillName { get; set; }

        [JsonProperty("annotations")]
        public Annotations Annotations { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }

        [JsonProperty("ssml_tagged_text")]
        public string SsmlTaggedText { get; set; }
    }

    public class Dialog
    {
        [JsonProperty("dialog_id")]
        public string DialogId { get; set; }

        [JsonProperty("utt_id")]
        public string UttId { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("response")]
        public string Response { get; set; }

        [JsonProperty("active_skill")]
        public string ActiveSkill { get; set; }

        [JsonProperty("debug_output")]
        public List<DebugOutput> DebugOutput { get; set; }
    } // class
} // namespace
