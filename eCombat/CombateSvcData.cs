using System.Runtime.Serialization;
using GalaSoft.MvvmLight;

namespace eCombat
{
    [DataContract]
    public class CombateSvcFault
    {
        [DataMember]
        public string Operation { get; set; }

        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public string Code { get; set; }

        public CombateSvcFault(string operation, string reason, string code = "")
        {
            this.Operation = operation;
            this.Reason = reason;
            this.Code = code;
        }
    }

    [DataContract]
    public class ChatMsg
    {
        [DataMember]
        public int MsgId { get; set; }

        [DataMember]
        public string MsgContent { get; set; }

        public bool IsSelfMessage { get; set; }
    }
}
