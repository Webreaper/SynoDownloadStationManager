using System;
using System.Linq;
using System.Collections.Generic;
using RestSharp;

namespace SynoDSManager
{

    namespace SynologyService
    {
        public class DSService
        {
            private ServerCertificateValidation certValidation = new ServerCertificateValidation();
            private readonly RestClient dsClient;
            private readonly SynologySettings settings;
            private string sid;

            public class SynoSID
            {
                public string sid { get; set; }
            }

            public class SynoTransfer
            {
                public long size_downloaded { get; set; }
                public long size_uploaded { get; set; }
            }

            public class SynoTracker
            {
                public string url { get; set; }
                public string status { get; set; }
                public int seeds { get; set; }
                public int peers { get; set; }
            }

            public class SynoFile
            {
                public string filename { get; set; }
                public long size { get; set; }
                public long size_downloaded { get; set; }
            }

            public class SynoDetail 
            {
                public string uri { get; set; }
                public DateTime create_time { get; set; }
            }

            public class SynoAdditional
            {
                public SynoDetail detail { get; set; }
                public SynoTransfer transfer { get; set; }
                public List<SynoTracker> tracker { get; set; }
                public List<SynoFile> file { get; set; }
            }

            public class SynoTask 
            {
                public string id { get; set; }  
                public string title { get; set; }  
                public string status { get; set; }  
                public string type { get; set; }  
                public SynoAdditional additional { get; set; }

				public override string ToString()
				{
					var tracker = additional.tracker != null ? additional.tracker.FirstOrDefault().url : "[none]";
					return string.Format("{0:dd-MMM-yyyy}: {1} [{2}]", additional.detail.create_time, title, 
					                     tracker);
				}
			}

            public class SynoDelete
            {
                public string id { get; set; }
                public int error { get; set; }
            }

            public class SynoTasks
            {
                public int total { get; set; }
                public List<SynoTask> tasks { get; set; }
            }

            public class SynologyResponse<T> where T : new()
            {
                public T data { get; set; }
                public bool success { get; set; }
                public int error { get; set; }
            }

            public DSService(SynologySettings synoSettings)
            {
                settings = synoSettings;
                dsClient = new RestClient(settings.url);
            }

            public bool SignIn()
            {
                bool success = false;

                var parms = new Dictionary<string, string>();

                parms.Add("api", "SYNO.API.Auth");
                parms.Add("version", "2");
                parms.Add("method", "login");
                parms.Add("account", settings.username);
                parms.Add("passwd", settings.password);
                parms.Add("session", "DownloadStation");
                parms.Add("format", "sid");

                var data = MakeRestRequest<SynoSID>("auth.cgi", parms);

                if (data != null)
                {
                    this.sid = data.sid;
                    success = true;
                }

                return success;
            }

            public IList<SynoTask> GetTasks()
            {
                var parms = new Dictionary<string, string>();


                parms.Add("api", "SYNO.DownloadStation.Task");
                parms.Add("_sid", this.sid);
                parms.Add("version", "1");
                parms.Add("method", "list");
                parms.Add("additional", "detail,transfer,file,tracker");

                var data = MakeRestRequest<SynoTasks>("DownloadStation/task.cgi", parms);

                if (data != null)
                    return data.tasks;

                return new List<SynoTask>();
            }

            public bool DeleteTask( string[] taskIds )
            {
                bool success = false;

                var parms = new Dictionary<string, string>();

                Utils.Log($"Deleting tasks: {string.Join(",", taskIds) }");

                parms.Add("api", "SYNO.DownloadStation.Task");
                parms.Add("_sid", this.sid);
                parms.Add("version", "1");
                parms.Add("method", "delete");
                parms.Add("id", string.Join( ",", taskIds));
                parms.Add("force_complete", false.ToString());

                var data = MakeRestRequest<List<SynoDelete>>("DownloadStation/task.cgi", parms);

                if (data != null )
                {
                    success = true;

                    foreach( var result in data )
                    {
                        if (result.error != 0)
                            Utils.Log($"Deletion failed for task {result.id}");
                    }
                }

                return success;
            }

            public T MakeRestRequest<T>( string requestMethod, IDictionary<string, string> parms ) where T : new()
            {
                var request = new RestRequest(requestMethod, Method.GET);
                // Dumb syno api returns the content type as plaintext, so we have to alter it.
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

                foreach (var kvp in parms)
                    request.AddParameter(kvp.Key, kvp.Value);

                try
                {
                    var queryResult = dsClient.Execute<SynologyResponse<T>>(request);

                    if (queryResult != null)
                    {
                        if (queryResult.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            Utils.Log("Error: {0} - {1}", queryResult.StatusCode, queryResult.Content);
                        }
                        else
                        {
                            var synoResponse = queryResult.Data;

                            if( synoResponse != null )
                            {
                                if( synoResponse.success )
                                    return synoResponse.data;
                                else
                                    Utils.Log("Request was not successful.");
                            }
                            else
                                Utils.Log("No response Data.");
                        }
                    }
                    else
                        Utils.Log("No valid queryResult.");
                }
                catch (Exception ex)
                {
                    Utils.Log("Exception: {0}: {1}", ex.Message, ex);
                }

                return default(T);
            }
        }
    }
}
