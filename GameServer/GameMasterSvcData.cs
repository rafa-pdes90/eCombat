using System.Runtime.Serialization;

namespace GameServer
{
    [DataContract]
    public class GameMasterSvcFault
    {
        [DataMember]
        public string Operation { get; set; }

        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public string Code { get; set; }

        public GameMasterSvcFault(string operation, string reason, string code = "")
        {
            this.Operation = operation;
            this.Reason = reason;
            this.Code = code;
        }
    }
}
