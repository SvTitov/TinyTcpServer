using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace TinyService.Common
{
    public class MessageHandler
    {
        private StringBuilder _stringBuilder = new StringBuilder();

        public void HandleRawMessage(byte[] buffer)
        {
            var size = System.Array.IndexOf(buffer, (byte)0);
            var data = Encoding.UTF8.GetString(buffer, 0, size < 0 ? buffer.Length : size);

            _stringBuilder.Append(data);
        }

        public string GetMessage()
        {
            return _stringBuilder.ToString();
        }

        public void ClearCache()
        {
            _stringBuilder.Clear();
            _stringBuilder = new StringBuilder();
        }

        public bool IsCompletedMessage()
        {
            var builderString = _stringBuilder.ToString();

            return builderString.StartsWith("<START>") && (builderString.EndsWith("<END>") || builderString.EndsWith("<END>>"));
        }
    }
}