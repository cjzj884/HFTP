
using System.Collections.Generic;
using System.Diagnostics;
namespace HFTP
{
    /// <summary>
    /// 消息应用规则
    /// </summary>
    //  1）对于交易信息，适用TradeNotice
    //  2）对于系统信息，Error=出错-程序段退出不抛出异常，Warning=警告-程序段不退出，Information=消息-提示

    public enum MessageType
    { 
        Information,
        Warning,
        Error,
        TradeNotice
    }

    public class Message
    {
        public MessageType type = MessageType.Information;
        public string message;
        public string security;
        public bool isnew = true;

        public Message(MessageType type, string msg)
        {
            this.type = type;
            this.message = msg;
        }
        public string GetMessage()
        {
            this.isnew = false;
            return string.Format("[{0}:{1}]{2}", this.type.ToString(), this.security, this.message);
        }
    }

    public class MessageManager
    {
        #region 静态
        private static MessageManager _instance;
        public static MessageManager GetInstance()
        {
            if (_instance == null)
                _instance = new MessageManager();

            return _instance;
        }
        #endregion

        private List<Message> _messagelist = new List<Message>();
        public void Add(MessageType type, string msg)
        {
            this._messagelist.Add(new Message(type, msg));
        }

        private int currMsgRow = 0;
        public void GetNewMessages(ref string sysmsg, ref string trademsg)
        {
            int totalMsgRow = this._messagelist.Count;
            for (int i = currMsgRow; i < totalMsgRow; i++)
            {
                if (this._messagelist[i].isnew)
                {
                    string msg = this._messagelist[i].GetMessage() + "\n";
                    if (this._messagelist[i].type == MessageType.TradeNotice)
                        trademsg += msg;
                    else
                        sysmsg += msg;
                }
            }
            this.currMsgRow = totalMsgRow;
        }

        public void DebugPrint()
        {
            if (this._messagelist.Count > 0)
            {
                foreach (Message m in _messagelist)
                {
                    Debug.Print(m.GetMessage());
                }
            }
        }
    }
}
