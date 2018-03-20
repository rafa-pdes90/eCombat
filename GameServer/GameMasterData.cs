using System.Runtime.Serialization;

namespace GameServer
{
    [DataContract]
    public class GMFault
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Exception { get; set; }
    }
}
