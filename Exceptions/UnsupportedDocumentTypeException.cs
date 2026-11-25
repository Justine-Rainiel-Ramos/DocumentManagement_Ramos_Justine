namespace DocumentManagement.Exceptions
{
    public class UnsupportedDocumentTypeException : Exception
    {
        public UnsupportedDocumentTypeException(string message) : base(message)
        {
        }
    }
}