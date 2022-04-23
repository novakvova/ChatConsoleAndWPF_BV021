using System;
using System.IO;

namespace LibMessage
{
    public enum TypeMessage
    {
        Login,
        Logout,
        Message
    }
    public class ChatMessage
    {
        public TypeMessage MessageType;
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write((int)MessageType);
                    writer.Write(UserId);
                    writer.Write(UserName);
                    writer.Write(Text);
                }
                return m.ToArray();
            }
        }

        public static ChatMessage Desserialize(byte[] data)
        {
            ChatMessage result = new ChatMessage();
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    result.MessageType = (TypeMessage)reader.ReadInt32();
                    result.UserId = reader.ReadString();
                    result.UserName = reader.ReadString();
                    result.Text = reader.ReadString();
                }
            }
            return result;
        }
    }
}
