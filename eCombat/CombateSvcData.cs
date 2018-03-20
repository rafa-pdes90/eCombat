using System.Runtime.Serialization;

namespace eCombat
{
    [DataContract]
    public class CombateSvcFault
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Exception { get; set; }
    }
}
