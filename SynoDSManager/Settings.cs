using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SynoDSManager
{
    [DataContract]
    public class EmailSettings
    {
        [DataMember]
        public string smtpserver { get; set; }
        [DataMember]
        public int smtpport { get; set;  }
        [DataMember]
        public string username { get; set; }
        [DataMember]
        public string password { get; set; }
        [DataMember]
        public string toaddress { get; set; }
        [DataMember]    
        public string fromaddress { get; set; }
        [DataMember]
        public string toname { get; set; }
    }

    [DataContract]
    public class SynologySettings
    {
        [DataMember]
        public string username { get; set; }
        [DataMember]
        public string password { get; set; }
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string[] trackersToKeep { get; set; }
		[DataMember]
        public int maxDaysToKeep { get; set; }
    }

    [DataContract]
    public class Settings
    {
        [DataMember]
        public EmailSettings email { get; set; }
        [DataMember]
        public SynologySettings synology { get; set; }
        [DataMember]
        public string logLocation { get; set; }
    }
}
