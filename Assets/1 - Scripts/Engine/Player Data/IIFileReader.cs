using UnityEngine;

public interface IIFileReader
{

    object ReadFile( string locator );

}

public class LocalFileReader : IIFileReader
{
    public object ReadFile( string locator )
    {
        string file = UserFileIO.ReadTextUserDataFile( locator );
        return file;
    }
}
