using System.Runtime.Serialization;

namespace GameServer
{
    [DataContract]
    public class GMFault
    {
        [DataMember]
        public string Operation { get; set; }

        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public string Code { get; set; }

        public GMFault(string operation, string reason, string code = "")
        {
            this.Operation = operation;
            this.Reason = reason;
            this.Code = code;
        }
    }
}
