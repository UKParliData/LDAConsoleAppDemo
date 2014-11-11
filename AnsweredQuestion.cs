using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDAConsoleAppDemo
{
    public class AnsweredQuestion
    {
        public string Id { get; set; }
        public DateTime TabledDate { get; set; }
        public DateTime AnswerDate { get; set; }
        public string AnsweringDepartment { get; set; }
        public string QuestionText { get; set; }
        public bool IsAnswered { get; set; }
    }
}
