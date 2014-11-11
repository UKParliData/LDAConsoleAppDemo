using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace LDAConsoleAppDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            runStuff();
        }

        private static void runStuff()
        {
            //internal id for MPs
            Console.Write("MnsId please (for example 4076): ");

            string mnsId = Console.ReadLine();
            string response= retriveData(mnsId);
            List<AnsweredQuestion> questions = getList(response);
            displayList(questions);
            Console.ReadKey();
        }

        /// <summary>
        /// Retrives last 5 written questions [_pageSize=5] 
        ///     for selected MP (Commons) [tablingMember=http://data.parliament.uk/members/{mnsId}]. 
        /// Data is ordered by tabled date [_sort=-dateTabled]. 
        /// Only selected properties [_view=basic]
        ///     (tabled date, answer due date, answering department, question text, actual answer date) 
        ///     are fetched [_properties=dateTabled,AnswerDate,answeringDepartment,questionText,answer,answer.dateOfAnswer].
        /// Data is returned in xml format [commonswrittenquestions.xml].
        /// </summary>
        /// <param name="mnsId">MP id</param>
        /// <returns></returns>
        private static string retriveData(string mnsId)
        {
            string responseText=null;
            string fullUrl = string.Format("http://lda.data.parliament.uk/commonswrittenquestions.xml?_sort=-dateTabled&tablingMember=http://data.parliament.uk/members/{0}&_properties=dateTabled,AnswerDate,answeringDepartment,questionText,answer,answer.dateOfAnswer&_view=basic&_pageSize=5&_page=0", mnsId);

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullUrl);
                request.Method = "GET";
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Accept = "application/xml";
                using (WebResponse response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    responseText = reader.ReadToEnd();
                }                
            }
            catch (Exception E)
            {
                Console.WriteLine("Lost communication");
            }
            return responseText;
        }

        /// <summary>
        /// Maps response from the server to custom object.
        /// </summary>
        /// <param name="response">Response from the server</param>
        /// <returns></returns>
        private static List<AnsweredQuestion> getList(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                Console.WriteLine("No data");
                return new List<AnsweredQuestion>();
            }
            List<AnsweredQuestion> result;
            XDocument xdoc = XDocument.Parse(response);

            result=xdoc.Element("result").Element("items").Elements("item").Select(item =>
                new AnsweredQuestion()
                {
                    AnswerDate = Convert.ToDateTime(item.Element("AnswerDate").Value),
                    AnsweringDepartment = item.Element("answeringDepartment").Value,
                    Id = item.Attribute("href").Value,
                    IsAnswered = (item.Element("answer") != null) && (item.Element("answer").Elements("dateOfAnswer").Any()),
                    QuestionText = item.Element("questionText").Value,
                    TabledDate = Convert.ToDateTime(item.Element("dateTabled").Value)
                }).ToList();

            Console.WriteLine("Total number of records: {0}", xdoc.Element("result").Element("totalResults").Value);

            return result;
        }

        /// <summary>
        /// Outputs all details to console.
        /// </summary>
        /// <param name="questions">List of retrived questions</param>
        private static void displayList(List<AnsweredQuestion> questions)
        {
            Console.WriteLine();
            foreach (AnsweredQuestion qa in questions.OrderByDescending(q=>q.TabledDate))
            {
                Console.WriteLine(qa.Id);
                Console.WriteLine("Tabled on {0}", qa.TabledDate.ToShortDateString());
                Console.WriteLine("Answer on {0}", qa.AnswerDate.ToShortDateString());
                Console.WriteLine("Question: {0}", qa.QuestionText);
                Console.WriteLine("Was answered: {0}", qa.IsAnswered);
                Console.WriteLine("Department: {0}", qa.AnsweringDepartment);
                Console.WriteLine("----");
            }
        }

    }
}
