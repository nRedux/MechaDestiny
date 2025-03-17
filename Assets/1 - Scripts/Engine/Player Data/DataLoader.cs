using UnityEngine;

public class DataLoader
{
    private IIFileReader _reader;
    private IFileWriter 

    public DataLoader( IIFileReader reader )
    {
        _reader = reader;
    }

    private UserDataFile LoadUserData( string locator )
    {
        return _reader.ReadFile( locator ) as UserDataFile;
    }
}
