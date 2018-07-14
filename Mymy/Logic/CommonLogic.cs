using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace Mymy.Logic
{
    public class CommonLogic
    {
        /// <summary>
        /// StreamからDataTableに変換します
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        public static DataTable ConvertStreamToDataTable(StreamReader sr)
        {
            var testData = new DataTable();

            var line = sr.ReadLine();
            var headers = line.Split(',');

            foreach (var head in headers)
            {
                testData.Columns.Add(head);
            }
            // ストリームの末尾まで繰り返す
            while (!sr.EndOfStream)
            {
                string line2 = sr.ReadLine();
                string[] fields = line2.Split(',');

                for (int i = 0; i < fields.Length - 1; i++)
                {

                    if (fields[i].Length > 0 && fields[i].TrimStart()[0] == '"')
                    {
                        fields[i] = fields[i].TrimStart();

                        if (fields[i].TrimEnd()[fields[i].TrimEnd().Length - 1] == '"')
                        {
                            fields[i] = fields[i].TrimEnd();
                            fields[i] = fields[i].Remove(0, 1);
                            fields[i] = fields[i].Remove(fields[i].Length - 1, 1);
                            continue;
                        }

                        while (true)
                        {
                            if (i < fields.Length)
                            {
                                fields[i] = fields[i] + "," + fields[i + 1];

                                for (int k = i + 1; k < fields.Length - 1; k++)
                                {
                                    fields[k] = fields[k + 1];
                                }
                                fields[fields.Length - 1] = "";

                                if (fields[i][fields[i].Length - 1] == '"')
                                {
                                    fields[i] = fields[i].TrimEnd();
                                    fields[i] = fields[i].Remove(0, 1);
                                    fields[i] = fields[i].Remove(fields[i].Length - 1, 1);
                                    break;

                                }
                            }
                        }
                    }
                }

                DataRow dr = testData.NewRow();
                for (int i = 0; i < headers.Length; i++)
                {
                    dr[i] = fields[i];
                }
                testData.Rows.Add(dr);

            }
            return testData;
        }
    }
}