using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Services;

namespace MailProcessingWebService
{
    /// <summary>
    /// Summary description for mailProcessing
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class mailProcessing : System.Web.Services.WebService
    {

        [WebMethod]
        public List<icmDataMail> ReadMailItems()
        {
            List<clarityMailTable> lists;
            using (claritydbEntities db=new claritydbEntities())
            {
                lists = db.clarityMailTables.ToList();
               
            }

            string conversationId = string.Empty;
            var stringBuilder = new StringBuilder();
            List<icmDataMail> icmDatas = new List<icmDataMail>();
            try
            {

                foreach (clarityMailTable item in lists)
                {
                    if (addRecord(item.ConversationID, item.ConversationIndex))
                    {
                        icmDataMail datas = new icmDataMail
                        {
                            From = item.fromalias,
                            To = item.toalias,
                            Cc = item.toalias,
                            Subject = item.subject,
                            Body = item.body
                        };
                        icmDatas.Add(datas);
                    }

                    Marshal.ReleaseComObject(item);

                }

            }
            catch (System.Exception ex)
            {
                AddTologfile(ex.Message.ToString());
            }

            finally
            {
                
            }
            return icmDatas;
        }

      
        public bool readRecord(string searchTerm, string filePath, int positionOfSearchTerm)
        {
            positionOfSearchTerm--;
            string[] recordNotFound = { "record Not Found" };
            try
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] fields = lines[i].Split(',');
                    if (recordMatches(searchTerm, fields, positionOfSearchTerm))
                    {
                        AddTologfile("Record Not Found");
                        return true;
                    }
                }

                return false;

            }
            catch (System.Exception ex)
            {
                AddTologfile(ex.Message.ToString());
                return false;
            }

        }

        public bool recordMatches(string searchTerm, string[] record, int positionOfSearchTerm)
        {
            if (record[positionOfSearchTerm].Equals(searchTerm))
            {
                return true;
            }
            return false;
        }

        public void AddTologfile(string logDetails)
        {
            string path = ConfigurationManager.AppSettings["logFile"].ToString();
            
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
                    {
                        file.WriteLine("Run at : "+DateTime.Now+"---->"+" Message "+logDetails);
                return;
                    }
               
        }
        public bool addRecord(string conversationId, string conversationIndex)
        {
            string path = ConfigurationManager.AppSettings["CSVFilePath"].ToString();
            bool isNewRecode = false;
            try
            {
                if (!readRecord(conversationId, path, 1))
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
                    {
                        file.WriteLine(conversationId + "," + conversationIndex);
                        isNewRecode = true;

                    }
                }


            }
            catch (System.Exception ex)
            {

                AddTologfile(ex.Message.ToString());
            }
            return isNewRecode;
        }

    }
}
