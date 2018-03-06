using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;

namespace rafapdes90.combate.Model
{
    public class ChatMsg : NetMsg
    {
        public int MsgId { get; set; }
        public string Origem { get; set; }
        public string MsgContent { get; set; }

        public ChatMsg(int id, string origem, string content)
        {
            this.MsgId = id;
            this.Origem = origem;
            this.MsgContent = content;
        }
    }
}
