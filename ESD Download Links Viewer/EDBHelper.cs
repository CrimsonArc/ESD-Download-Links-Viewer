using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Isam.Esent.Interop;

namespace ESD_Download_Links_Viewer
{
    class EDBHelper
    {
        private static JET_COLUMNID columnidName;
        private static JET_COLUMNID columnidUrl;
        private static JET_COLUMNID columnidSize;
        private static JET_COLUMNID columnidModifiedDate;
        private static JET_COLUMNID columnidDigest;
        private static JET_COLUMNID columnidStrongDigest;

        public Task<ObservableCollection<DownloadItem>> ReadDataAsync(string filename)
        {
            return Task.Run(() =>
            {
                JET_INSTANCE instance = JET_INSTANCE.Nil;

                try
                {
                    string dbpath = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar;
                    JET_DBINFOMISC infomisc;
                    Api.JetGetDatabaseFileInfo(filename, out infomisc, JET_DbInfo.Misc);
                    SystemParameters.DatabasePageSize = infomisc.cbPageSize;
                    Api.JetCreateInstance(out instance, Guid.NewGuid().ToString("N"));
                    Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.CircularLog, 1, null);
                    Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.TempPath, 0, dbpath);
                    Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
                    Api.JetInit(ref instance);

                    using (var session = new Session(instance))
                    {
                        JET_DBID dbid = JET_DBID.Nil;

                        Api.JetAttachDatabase(session, filename, AttachDatabaseGrbit.ReadOnly);
                        Api.JetOpenDatabase(session, filename, null, out dbid, OpenDatabaseGrbit.ReadOnly);

                        using (var table = new Table(session, dbid, "tbFiles", OpenTableGrbit.ReadOnly))
                        {
                            // Load the columnids from the table. This should be done each time the database is attached
                            // because an offline defrag (esentutl /d) can change the name => columnid mapping.
                            IDictionary<string, JET_COLUMNID> columnids = Api.GetColumnDictionary(session, table);

                            columnidName = columnids["Name"];
                            columnidUrl = columnids["Urls"];
                            columnidSize = columnids["Size"];
                            columnidModifiedDate = columnids["Modified"];
                            columnidDigest = columnids["Digest"];
                            columnidStrongDigest = columnids["StrongDigest"];

                            return GetAllRecords(session, table);
                        }
                    }
                }
                finally
                {
                    if (instance != JET_INSTANCE.Nil) { Api.JetTerm(instance); }
                }
            });
        }

        /// <summary>
        /// Print all rows in the table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to dump the records from.</param>
        private ObservableCollection<DownloadItem> GetAllRecords(JET_SESID sesid, JET_TABLEID tableid)
        {
            if (Api.TryMoveFirst(sesid, tableid))
            {
                return RetriveRecordsToEnd(sesid, tableid);
            }
            else
            {
                return new ObservableCollection<DownloadItem>();
            }
        }

        /// <summary>
        /// Print records from the current point to the end of the table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to dump the records from.</param>
        private ObservableCollection<DownloadItem> RetriveRecordsToEnd(JET_SESID sesid, JET_TABLEID tableid)
        {
            ObservableCollection<DownloadItem> itemsList = new ObservableCollection<DownloadItem>();

            do
            {
                itemsList.Add(GetOneRow(sesid, tableid));
            }
            while (Api.TryMoveNext(sesid, tableid));

            return itemsList;
        }

        /// <summary>
        /// Get the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to use.</param>
        private DownloadItem GetOneRow(JET_SESID sesid, JET_TABLEID tableid)
        {
            string fileName = Api.RetrieveColumnAsString(sesid, tableid, columnidName);
            fileName = fileName.Substring(0, fileName.Length - 1);

            string url = "";
            string tempUrl = Api.RetrieveColumnAsString(sesid, tableid, columnidUrl);
            if(!String.IsNullOrEmpty(tempUrl))
            {
                url = tempUrl.Substring(8).Trim();
                url = url.Substring(0, url.Length - 2);
            }

            ulong fileSize = (ulong)Api.RetrieveColumnAsUInt64(sesid, tableid, columnidSize);

            string fileType = GetFileExtension(fileName);

            long dateValue = (long)Api.RetrieveColumnAsInt64(sesid, tableid, columnidModifiedDate);
            DateTime fileDate = new DateTime(dateValue, DateTimeKind.Utc);
            fileDate = fileDate.AddYears(1600);

            byte[] sha1Bytes = Api.RetrieveColumn(sesid, tableid, columnidDigest);
            string sha1 = ByteArrayToString(sha1Bytes);

            byte[] sha256Bytes = Api.RetrieveColumn(sesid, tableid, columnidStrongDigest);
            string sha256 = ByteArrayToString(sha256Bytes);

            DownloadItem item = new DownloadItem(fileName, url, fileSize, fileType, fileDate.ToLocalTime(), sha1, sha256);

            return item;
        }

        private static string ByteArrayToString(byte[] byteArray)
        {
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:X2}", b);
            }

            return hex.ToString();
        }

        private string GetFileExtension(string fileName)
        {
            Regex regex = new Regex(@"(?<=.+)\.[a-z]{3}$");

            return regex.Match(fileName).Value;
        }
    }
}
