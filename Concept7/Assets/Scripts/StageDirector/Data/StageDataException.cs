using System;

public class StageDataException : InvalidOperationException
{
    public StageDataException() : base() { }
    public StageDataException(string message) : base(message) { }
    public StageDataException(string message, Exception innerException) : base(message, innerException) { }
}