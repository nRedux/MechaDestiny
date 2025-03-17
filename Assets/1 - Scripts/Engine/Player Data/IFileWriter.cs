using UnityEngine;

public interface IFileWriter
{
    void WriteFile( string locator, object file );
}

public class LocalFileWriter : IFileWriter
{
    public object WriteFile( string locator, object data )
    {
        


    }
}
