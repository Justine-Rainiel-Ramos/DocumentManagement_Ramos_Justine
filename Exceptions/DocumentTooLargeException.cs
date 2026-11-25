namespace DocumentManagement.Exceptions
{
    public class DocumentTooLargeException : Exception
    {
        public DocumentTooLargeException(string message) : base(message)
        {
        }
    }
}